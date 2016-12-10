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

		public ActionResult Index(HomeViewModel model) {
			// argument checks
			if (model == null) {
				throw API.CreateBadRequestException();
			}

			// call Recognize API
			try {
				if (0 <= model.SampleIndex) {
					// process sample images
					if (HomeViewModel.Samples.Length <= model.SampleIndex) {
						throw API.CreateBadRequestException();
					}
					string sampleFileName = HomeViewModel.Samples[model.SampleIndex];

					model.ImageUrl = $"/Content/{sampleFileName}";
					List<float> output = API.RecognizeFromFile(Server.MapPath(model.ImageUrl));
					model.SetResult(output);
					model.ImageText = Evaluator.OutputToString(output);

					Trace.TraceInformation($"Sample {model.SampleIndex}; {Evaluator.OutputToString(output, true)}");
				} else if (model.LocalFile != null) {
					// process uploaded file
					if (API.MaxImageSize < model.LocalFile.ContentLength) {
						throw API.CreateSizeException();
					}

					// copy stream to memory to create embedded image url
					Action<Bitmap> scanBitmap = (bitmap) => {
						int width = 600;
						if (0 < model.ImageWidth) {
							width = model.ImageWidth;
						}
						model.ImageUrl = GetEmbeddedImageUrl(bitmap, width);
					};

					// Set closeStream to true to dispose the memomry stream immediately after bitmap is created
					// It prevents from keeping large memory block long time.
					List<float> output = API.RecognizeFromStream(model.LocalFile.InputStream, true, scanBitmap);
					model.SetResult(output);
					model.ImageText = Evaluator.OutputToString(output);

					Trace.TraceInformation($"Uploaded File; {Evaluator.OutputToString(output, true)}");
				} else if (string.IsNullOrEmpty(model.RequestUrl) == false) {
					// process external resource
					model.ImageUrl = model.RequestUrl;

					List<float> output = API.RecognizeFromUrl(model.RequestUrl);
					model.SetResult(output);
					model.ImageText = Evaluator.OutputToString(output);

					Trace.TraceInformation($"External Resource; {Evaluator.OutputToString(output, true)}");
				} else {
					model.ImageUrl = null;
				}
			} catch (Exception exception) {
				model.ImageUrl = null;
				model.SetResult(exception);
			} finally {
				model.LocalFile = null;
			}

			return View(model);
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