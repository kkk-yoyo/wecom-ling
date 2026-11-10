using System;
using System.Collections.Generic;

namespace Robot.Common;

public class R<T>
{
	private Dictionary<string, object> map = new Dictionary<string, object>();

	public int Code { get; set; }

	public string Msg { get; set; }

	public T Data { get; set; }

	public long Time => DateTimeOffset.Now.ToUnixTimeSeconds();

	public static R<T> Success(T obj)
	{
		return new R<T>
		{
			Data = obj,
			Code = 200,
			Msg = "请求成功！"
		};
	}

	public static R<T> Success(T obj, string msg)
	{
		return new R<T>
		{
			Data = obj,
			Code = 200,
			Msg = msg
		};
	}

	public static R<T> Success(int code, T obj, string msg)
	{
		return new R<T>
		{
			Data = obj,
			Code = code,
			Msg = msg
		};
	}

	public static R<T> Warning(int code, string msg)
	{
		return new R<T>
		{
			Code = code,
			Msg = msg
		};
	}

	public static R<T> Error(string msg)
	{
		return new R<T>
		{
			Code = 500,
			Msg = msg
		};
	}

	public R<T> Add(string key, object value)
	{
		map[key] = value;
		return this;
	}
}
