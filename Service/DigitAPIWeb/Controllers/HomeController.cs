using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
				if (model.LocalFile != null) {
					// process uploaded file
					if (API.MaxImageSize < model.LocalFile.ContentLength) {
						throw API.CreateSizeException();
					}

					// copy stream to memory to create embedded image url
					MemoryStream stream = CopyToMemoryStream(model.LocalFile);
					try {
						model.ImageUrl = GetEmbeddedImageUrl(model.LocalFile, stream);
					} catch {
						stream.Dispose();
						throw;
					}

					// Set closeStream to true to dispose the memomry stream immediately after bitmap is created
					// It prevents from keeping large memory block long time.
					model.SetResult(API.Recognize(stream, closeStream: true));
				} else if (string.IsNullOrEmpty(model.RequestUrl) == false) {
					// process external resource
					model.ImageUrl = model.RequestUrl;
					model.SetResult(API.Recognize(model.RequestUrl));
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

		private static MemoryStream CopyToMemoryStream(HttpPostedFileBase uploadedFile) {
			// argument checks
			Debug.Assert(uploadedFile != null);

			MemoryStream stream = new MemoryStream(uploadedFile.ContentLength);
			try {
				uploadedFile.InputStream.CopyTo(stream);
			} catch {
				stream.Dispose();
				throw;
			}

			return stream;
		}

		private static string GetEmbeddedImageUrl(HttpPostedFileBase uploadedFile, MemoryStream stream) {
			// argument checks
			Debug.Assert(uploadedFile != null);
			Debug.Assert(stream != null);
			Debug.Assert(stream.Length < int.MaxValue);

			// convert the stream content to base64 string
			return string.Concat(
				$"data:{uploadedFile.ContentType};base64,",
				Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length)
			);
		}

		#endregion
	}
}