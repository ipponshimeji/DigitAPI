using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;


namespace DigitAPI.Web {
    public class MvcApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

			// The following settings are required for CNTK evaluation.
			// see https://github.com/Microsoft/CNTK/wiki/Evaluate-a-model-in-an-Azure-WebApi
			string pathValue = Environment.GetEnvironmentVariable("PATH");
			string domainBaseDir = AppDomain.CurrentDomain.BaseDirectory;
			string cntkPath = domainBaseDir + @"bin\";
			pathValue += ";" + cntkPath;
			Environment.SetEnvironmentVariable("PATH", pathValue);

			// initialize DigitAPI class
			DigitAPI.Initialize(cntkPath);
		}

		protected void Application_End() {
			// uninitialize DigitAPI class
			DigitAPI.Uninitialize();
		}
	}
}
