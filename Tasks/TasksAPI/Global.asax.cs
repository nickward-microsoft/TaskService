using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace TaskService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var appInsightsKey = System.Web.Configuration.WebConfigurationManager.AppSettings["AppInsightsKey"];
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = appInsightsKey;
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
