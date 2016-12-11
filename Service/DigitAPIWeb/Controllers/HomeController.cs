using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitAPI.Web.Models;


namespace DigitAPI.Web.Controllers {
	public class HomeController: Controller {
		#region action methods

		public ActionResult Index(HomeIndexRequest request) {
			// argument checks
			if (request == null) {
				throw API.CreateBadRequestException();
			}

			// call Recognize API
			HomeIndexResponse response = new HomeIndexResponse(request);
			try {
				if (0 <= request.SampleIndex) {
					// process sample images
					if (HomeIndexResponse.Samples.Length <= request.SampleIndex) {
						throw API.CreateBadRequestException();
					}
					string sampleFileName = HomeIndexResponse.Samples[request.SampleIndex];

					response.ImageUrl = $"/Content/{sampleFileName}";
					List<float> output = API.RecognizeFromFile(Server.MapPath(response.ImageUrl));
					response.SetResult(output);
					response.ImageText = Evaluator.OutputToString(output);

					Trace.TraceInformation($"Sample {request.SampleIndex}; {Evaluator.OutputToString(output, true)}");
				} else if (request.LocalFile != null) {
					// process uploaded file
					if (API.MaxImageSize < request.LocalFile.ContentLength) {
						throw API.CreateSizeException();
					}

					// copy stream to memory to create embedded image url
					Action<Bitmap> scanBitmap = (bitmap) => {
						int width = 600;
						if (0 < request.ImageWidth) {
							width = request.ImageWidth;
						}
						response.ImageUrl = GetEmbeddedImageUrl(bitmap, width);
					};

					// Set closeStream to true to dispose the memomry stream immediately after bitmap is created
					// It prevents from keeping large memory block long time.
					List<float> output = API.RecognizeFromStream(request.LocalFile.InputStream, true, scanBitmap);
					response.SetResult(output);
					response.ImageText = Evaluator.OutputToString(output);

					Trace.TraceInformation($"Uploaded File; {Evaluator.OutputToString(output, true)}");
				} else if (string.IsNullOrEmpty(request.RequestUrl) == false) {
					// process external resource
					response.ImageUrl = request.RequestUrl;

					List<float> output = API.RecognizeFromUrl(request.RequestUrl);
					response.SetResult(output);
					response.ImageText = Evaluator.OutputToString(output);

					Trace.TraceInformation($"External Resource; {Evaluator.OutputToString(output, true)}");
				} else {
					response.ImageUrl = null;
				}
			} catch (Exception exception) {
				response.ImageUrl = null;
				response.SetResult(exception);
			} finally {
				request.LocalFile = null;
			}

			return View(response);
		}

		#endregion


		#region privates

		private static string GetEmbeddedImageUrl(Bitmap bitmap, int width) {
			// argument checks
			Debug.Assert(bitmap != null);
			Debug.Assert(0 < width);

			using (MemoryStream memoryStream = new MemoryStream()) {
				// create a shrunk image
				int height;
				checked {
					// calculate height from the ratio of the width 
					height = (bitmap.Height * width) / bitmap.Width;
				}
				using (Bitmap preview = new Bitmap(bitmap, width, height)) {
					// save the shrunk bitmap as PNG
					preview.Save(memoryStream, ImageFormat.Png);
				}

				// convert the PNG image to base64 string
				Debug.Assert(memoryStream.Length < int.MaxValue);	// original image size is less than 4M
				return string.Concat(
					$"data:image/png;base64,",
					Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length)
				);
			}
		}

		#endregion
	}
}