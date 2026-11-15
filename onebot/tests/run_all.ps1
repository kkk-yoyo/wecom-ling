# run_all.ps1 -- one-shot test run:
#   1. build TestHost
#   2. start mock OneBot v11 server on 127.0.0.1:6399
#   3. run TestHost (all cases)
#   4. stop mock, collect logs, render TEST_RESULTS.md + results.json
$ErrorActionPreference = 'Stop'

$TestsDir = $PSScriptRoot
$LibDir   = Join-Path $TestsDir 'lib'
$BinDir   = Join-Path $TestsDir 'bin'
$LogDir   = Join-Path $TestsDir 'logs'
New-Item -ItemType Directory -Force $LogDir | Out-Null

$PythonCandidates = @(
    'C:\Users\Administrator\.astrbot_launcher\instances\38c868af-ee29-4389-9c27-c69e148f7200\venv\Scripts\python.exe',
    'C:\Users\Administrator\AppData\Local\Programs\Python\Python312\python.exe'
)

function Test-MockUp {
    try {
        $r = Invoke-RestMethod -Uri 'http://127.0.0.1:6399/ctl/ping' -TimeoutSec 2
        return [bool]$r.ok
    } catch { return $false }
}

# ---------------------------------------------------------------- 1. build
& (Join-Path $TestsDir 'build.ps1')
if ($LASTEXITCODE -ne 0) { throw "build failed" }

# ---------------------------------------------------------------- 2. python
$Py = $null
$PyVersion = $null
foreach ($c in $PythonCandidates) {
    if (-not (Test-Path $c)) { continue }
    $ver = & $c -c "import aiohttp, sys; print(sys.version.split()[0])" 2>$null
    if ($LASTEXITCODE -eq 0 -and $ver) {
        $Py = $c
        $PyVersion = ($ver | Select-Object -First 1)
        break
    }
}
if (-not $Py) { throw "no python interpreter with aiohttp found (candidates: $($PythonCandidates -join ', '))" }
Write-Host "python: $Py ($PyVersion)"

# clean generated logs (keep hand-extracted docs such as logs\wecom_doc.txt)
Get-ChildItem $LogDir -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '\.(log|jsonl)$' -or $_.Name -in @('results_raw.json','testhost_console.log') } |
    Remove-Item -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $BinDir 'onebot_v11.log') -Force -ErrorAction SilentlyContinue

# ------------------------------------------------- 3. stale mock cleanup
if (Test-MockUp) {
    Write-Host 'stale mock server detected on port 6399, shutting it down...'
    try { Invoke-RestMethod -Uri 'http://127.0.0.1:6399/ctl/shutdown' -Method Post -TimeoutSec 3 | Out-Null } catch {}
    Start-Sleep -Seconds 2
    if (Test-MockUp) { throw 'port 6399 is still occupied (not a mock_server.py we can stop)' }
}

# ---------------------------------------------------------------- 4. mock
$env:PYTHONIOENCODING = 'utf-8'
$mockOut = Join-Path $LogDir 'mock_stdout.log'
$mockErr = Join-Path $LogDir 'mock_stderr.log'
$mock = Start-Process -FilePath $Py -ArgumentList @((Join-Path $TestsDir 'mock_server.py')) `
    -WorkingDirectory $TestsDir -PassThru -WindowStyle Hidden `
    -RedirectStandardOutput $mockOut -RedirectStandardError $mockErr

$ready = $false
for ($i = 0; $i -lt 80; $i++) {
    if (Test-MockUp) { $ready = $true; break }
    if ($mock.HasExited) { break }
    Start-Sleep -Milliseconds 250
}
if (-not $ready) {
    if ($mock.HasExited) {
        $errText = Get-Content $mockErr -Raw -ErrorAction SilentlyContinue
        throw "mock server exited during startup: $errText"
    }
    Stop-Process -Id $mock.Id -Force -ErrorAction SilentlyContinue
    throw 'mock server did not become ready on http://127.0.0.1:6399'
}
Write-Host "mock server ready (pid=$($mock.Id))"

# ---------------------------------------------------------------- 5. TestHost
$code = 1
$consoleLog = Join-Path $LogDir 'testhost_console.log'
try {
    Push-Location $BinDir
    & (Join-Path $BinDir 'TestHost.exe') 2>&1 | Tee-Object -FilePath $consoleLog
    $code = $LASTEXITCODE
} catch {
    Write-Host "TestHost threw: $_"
    $code = 3
} finally {
    Pop-Location
    # politely stop the mock, then force if needed
    try { Invoke-RestMethod -Uri 'http://127.0.0.1:6399/ctl/shutdown' -Method Post -TimeoutSec 3 | Out-Null } catch {}
    Start-Sleep -Milliseconds 1200
    if (-not $mock.HasExited) { Stop-Process -Id $mock.Id -Force -ErrorAction SilentlyContinue }
    # collect shim evidence log from bin\
    Copy-Item (Join-Path $BinDir 'onebot_v11.log') (Join-Path $LogDir 'onebot_v11.log') -Force -ErrorAction SilentlyContinue
    Copy-Item $mockOut (Join-Path $LogDir 'mock_stdout.log') -Force -ErrorAction SilentlyContinue
    Copy-Item $mockErr (Join-Path $LogDir 'mock_stderr.log') -Force -ErrorAction SilentlyContinue
}

Write-Host "TestHost exit code: $code"

# ---------------------------------------------------------------- 6. report
& $Py (Join-Path $TestsDir 'render_report.py')
$renderCode = $LASTEXITCODE
if (-not (Test-Path (Join-Path $TestsDir 'TEST_RESULTS.md'))) {
    throw "render_report.py failed to produce TEST_RESULTS.md (exit $renderCode)"
}

Write-Host ''
Write-Host "RESULTS: $(Join-Path $TestsDir 'TEST_RESULTS.md')"
Write-Host "MACHINE: $(Join-Path $TestsDir 'results.json')"
exit $code
