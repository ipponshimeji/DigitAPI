using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DigitAPI;
using DigitAPI.Web.Models;


namespace DigitAPI.Web {
	public class DigitAPI {
		#region data

		private static Evaluator evaluator = new Evaluator();

		#endregion


		#region creation and disposal

		public static void Initialize(string baseDir) {
			// create an evaluator
			evaluator = new Evaluator(baseDir);
			Debug.WriteLine($"{ nameof(DigitAPI)}: initialized.");

			return;
		}

		public static void Uninitialize() {
			// dispose the evaluator
			Evaluator temp = evaluator;
			evaluator = null;

			temp.Dispose();
			Debug.WriteLine($"{nameof(DigitAPI)}: uninitialized.");

			return;
		}

		#endregion


		#region methods

		public static RecognizeResponse Recognize(RecognizeRequest request) {
			// argument checks
			if (request == null || string.IsNullOrEmpty(request.Url)) {
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
			}

			Func<Bitmap> loadBitmap = () => {
				using (HttpClient httpClient = new HttpClient()) {
					var task = httpClient.GetStreamAsync(request.Url);
					task.Wait();
					using (Stream imageStream = task.Result) {
						return new Bitmap(imageStream);
					}
				}
			};

			List<float> output = evaluator.Evaluate(loadBitmap);
			return new RecognizeResponse(output);
		}

		#endregion
	}
}