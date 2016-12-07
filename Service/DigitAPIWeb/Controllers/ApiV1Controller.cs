using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DigitAPI.Web.Models;


namespace DigitAPI.Web.Controllers {
	public class ApiV1Controller: ApiController {
		// POST "digit/v1.0/recognize"
		[HttpPost]
		public IHttpActionResult Recognize(RecognizeRequest request) {
			// argument checks
			if (request == null || string.IsNullOrEmpty(request.Url)) {
				throw API.CreateBadRequestException();
			}

			// call Recognize API
			RecognizeResponse response = new RecognizeResponse(API.Recognize(request.Url));
			return Ok(response);
		}
	}
}