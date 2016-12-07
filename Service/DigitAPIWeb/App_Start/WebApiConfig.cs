using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;


namespace DigitAPI.Web {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {
            // Web API の設定およびサービス

            // Web API ルート
            config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "recognize",
				routeTemplate: "digit/v1.0/recognize",
				defaults: new { controller = "ApiV1", action = "Recognize" }
			);

			// We require only digit/v1.0/recognize
			/*
			config.Routes.MapHttpRoute(
              name: "DefaultApi",
              routeTemplate: "api/{controller}/{id}",
              defaults: new { id = RouteParameter.Optional }
            );
			*/
		}
	}
}
