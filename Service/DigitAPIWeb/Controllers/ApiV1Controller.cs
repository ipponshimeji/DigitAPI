using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			List<float> output = API.Recognize(request.Url);
			Trace.TraceInformation($"Uploaded File; {Evaluator.OutputToString(output, true)}");
			RecognizeResponse response = new RecognizeResponse(output);
			return Ok(response);
		}
	}
}