using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Robot.Common;
using Robot.Utils;

namespace Robot.Advice;

public class GlobalExceptionFilter : IExceptionFilter, IFilter
{
	public bool AllowMultiple => false;

	public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Expected O, but got Unknown
		TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
		JsonSerializerSettings val = new JsonSerializerSettings
		{
			ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver()
		};
		if (actionExecutedContext.Exception is DownloadException)
		{
			actionExecutedContext.Response = HttpRequestMessageExtensions.CreateErrorResponse(actionExecutedContext.Request, HttpStatusCode.InternalServerError, actionExecutedContext.Exception.Message);
			actionExecutedContext.Response.Content = new StringContent(JsonConvert.SerializeObject((object)R<object>.Error(actionExecutedContext.Exception.Message), val), Encoding.UTF8, "application/json");
			taskCompletionSource.SetResult(null);
			return taskCompletionSource.Task;
		}
		if (actionExecutedContext.Exception is CustomException)
		{
			actionExecutedContext.Response = HttpRequestMessageExtensions.CreateErrorResponse(actionExecutedContext.Request, HttpStatusCode.OK, "403 Forbidden");
			actionExecutedContext.Response.Content = new StringContent(JsonConvert.SerializeObject((object)R<object>.Warning(403, actionExecutedContext.Exception.Message), val), Encoding.UTF8, "application/json");
			taskCompletionSource.SetResult(null);
			return taskCompletionSource.Task;
		}
		LogException(actionExecutedContext.Exception);
		actionExecutedContext.Response = HttpRequestMessageExtensions.CreateErrorResponse(actionExecutedContext.Request, HttpStatusCode.InternalServerError, "Internal Server Error");
		actionExecutedContext.Response.Content = new StringContent(JsonConvert.SerializeObject((object)R<object>.Error("服务器正忙,请稍后重试!"), val), Encoding.UTF8, "application/json");
		taskCompletionSource.SetResult(null);
		return taskCompletionSource.Task;
	}

	private void LogException(Exception exception)
	{
		Logger.Error("服务端异常:", exception);
	}
}
