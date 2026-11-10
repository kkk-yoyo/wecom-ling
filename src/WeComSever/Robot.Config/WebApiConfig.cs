using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.SelfHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Robot.Advice;

namespace Robot.Config;

public static class WebApiConfig
{
	public static void Register(this HttpSelfHostConfiguration config)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		((HttpConfiguration)config).Filters.Add((IFilter)(object)new GlobalExceptionFilter());
		((Collection<MediaTypeFormatter>)(object)((HttpConfiguration)config).Formatters).Clear();
		((Collection<MediaTypeFormatter>)(object)((HttpConfiguration)config).Formatters).Add((MediaTypeFormatter)new JsonMediaTypeFormatter());
		((BaseJsonMediaTypeFormatter)((HttpConfiguration)config).Formatters.JsonFormatter).SerializerSettings.Formatting = (Formatting)1;
		((BaseJsonMediaTypeFormatter)((HttpConfiguration)config).Formatters.JsonFormatter).SerializerSettings.ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver();
		Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
		((HttpConfiguration)config).Routes.Clear();
		HttpRouteCollectionExtensions.MapHttpRoute(((HttpConfiguration)config).Routes, "DefaultApi", "api/{controller}/{action}/{id}", (object)new
		{
			id = RouteParameter.Optional
		});
	}
}
