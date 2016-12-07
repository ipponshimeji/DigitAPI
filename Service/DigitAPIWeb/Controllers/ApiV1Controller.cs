using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DigitAPI.Web.Models;


namespace DigitAPI.Web.Controllers {
	public class ApiV1Controller: ApiController {
		// POST "digit/v1.0/recognize"
		[HttpPost]
		public IHttpActionResult Recognize(RecognizeRequest request) {
			return Ok(DigitAPI.Recognize(request));
		}
	}
}