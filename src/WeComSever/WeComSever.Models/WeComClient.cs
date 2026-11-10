using System;
using System.ComponentModel;

namespace WeComSever.Models;

public class WeComClient : INotifyPropertyChanged
{
	private string _clientId;

	private string _weChatName;

	private string _weChatId;

	private string _avatar;

	private string _mobile;

	private string _corpId;

	private string _acctId;

	private int _pid;

	private bool _isLoggedIn;

	private bool _isOnline;

	private DateTime _connectedTime;

	public string ClientId
	{
		get
		{
			return _clientId;
		}
		set
		{
			if (_clientId != value)
			{
				_clientId = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClientId"));
			}
		}
	}

	public string WeChatName
	{
		get
		{
			return _weChatName;
		}
		set
		{
			if (_weChatName != value)
			{
				_weChatName = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WeChatName"));
			}
		}
	}

	public string WeChatId
	{
		get
		{
			return _weChatId;
		}
		set
		{
			if (_weChatId != value)
			{
				_weChatId = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WeChatId"));
			}
		}
	}

	public string Avatar
	{
		get
		{
			return _avatar;
		}
		set
		{
			if (_avatar != value)
			{
				_avatar = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Avatar"));
			}
		}
	}

	public string Mobile
	{
		get
		{
			return _mobile;
		}
		set
		{
			if (_mobile != value)
			{
				_mobile = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Mobile"));
			}
		}
	}

	public string CorpId
	{
		get
		{
			return _corpId;
		}
		set
		{
			if (_corpId != value)
			{
				_corpId = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CorpId"));
			}
		}
	}

	public string AcctId
	{
		get
		{
			return _acctId;
		}
		set
		{
			if (_acctId != value)
			{
				_acctId = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AcctId"));
			}
		}
	}

	public int Pid
	{
		get
		{
			return _pid;
		}
		set
		{
			if (_pid != value)
			{
				_pid = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Pid"));
			}
		}
	}

	public bool IsLoggedIn
	{
		get
		{
			return _isLoggedIn;
		}
		set
		{
			if (_isLoggedIn != value)
			{
				_isLoggedIn = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsLoggedIn"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusText"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusColor"));
			}
		}
	}

	public bool IsOnline
	{
		get
		{
			return _isOnline;
		}
		set
		{
			if (_isOnline != value)
			{
				_isOnline = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsOnline"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusText"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusColor"));
			}
		}
	}

	public DateTime ConnectedTime
	{
		get
		{
			return _connectedTime;
		}
		set
		{
			if (_connectedTime != value)
			{
				_connectedTime = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectedTime"));
			}
		}
	}

	public string StatusText
	{
		get
		{
			if (!IsOnline)
			{
				return "离线";
			}
			if (!IsLoggedIn)
			{
				return "未登录";
			}
			return "在线";
		}
	}

	public string StatusColor
	{
		get
		{
			if (!IsOnline)
			{
				return "#EF4444";
			}
			if (!IsLoggedIn)
			{
				return "#F59E0B";
			}
			return "#10B981";
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public WeComClient()
	{
		ConnectedTime = DateTime.Now;
		IsOnline = true;
		IsLoggedIn = false;
	}

	public WeComClient(string clientId, string weChatName = "", string weChatId = "")
		: this()
	{
		ClientId = clientId;
		WeChatName = weChatName;
		WeChatId = weChatId;
	}
}
