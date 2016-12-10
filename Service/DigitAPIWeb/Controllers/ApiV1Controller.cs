using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using DigitAPI.Web.Models;


namespace DigitAPI.Web.Controllers {
	public class ApiV1Controller: ApiController {
		#region action methods

		[Route("digit/v1.0/recognize")]
		[HttpPost]
		public IHttpActionResult Recognize() {
			HttpContent content = this.Request.Content;

			// call Recognize API depending on the Content-Type
			List<float> output;
			switch (content.Headers.ContentType.MediaType) {
				case "application/json":
					output = RecognizeFromJson(content.ReadAsStringAsync().Result);
					break;
				case "application/octet-stream":
					output = RecognizeFromStream(content.ReadAsStreamAsync().Result);
					break;
				default:
					throw API.CreateBadRequestException();
			}

			// build a response
			RecognizeResponse response = new RecognizeResponse(output);

			return Ok(response);
		}

		#endregion


		#region privates

		private static List<float> RecognizeFromJson(string json) {
			// deserialize the request
			RecognizeRequest request = JsonConvert.DeserializeObject<RecognizeRequest>(json);
			if (request == null || string.IsNullOrEmpty(request.Url)) {
				throw API.CreateBadRequestException();
			}

			// call Recognize API
			List<float> output = API.RecognizeFromUrl(request.Url);

			// log the result
			Trace.TraceInformation($"API (application/json); {Evaluator.OutputToString(output, true)}");

			return output;
		}

		private static List<float> RecognizeFromStream(Stream stream) {
			// argument checks
			Debug.Assert(stream != null);

			// call Recognize API
			List<float> output = API.RecognizeFromStream(stream, true, null);

			// log the result
			Trace.TraceInformation($"API (application/octet-stream); {Evaluator.OutputToString(output, true)}");

			return output;
		}

		#endregion
	}
}