﻿using System;
using System.Collections.Generic;
using System.Configuration;
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
	public class API {
		#region data

		public const int MaxImageSize = 4 * 1024 * 1024;	// 4M

		private static Evaluator evaluator = new Evaluator();

		public static string Brand {
			get;
			private set;
		}

		public static string Copyright {
			get;
			private set;
		}

		#endregion


		#region creation and disposal

		public static void Initialize(string baseDir) {
			// create an evaluator
			evaluator = new Evaluator(baseDir);

			// read configurations
			Brand = "[MyBrand]";
			string value = ConfigurationManager.AppSettings["DigitAPI:Brand"];
			if (string.IsNullOrEmpty(value) == false) {
				Brand = value;
			}

			Copyright = string.Empty;
			value = ConfigurationManager.AppSettings["DigitAPI:Copyright"];
			if (string.IsNullOrEmpty(value) == false) {
				Copyright = value;
			}

			// debug log
			Debug.WriteLine($"{ nameof(API)}: initialized.");

			return;
		}

		public static void Uninitialize() {
			// dispose the evaluator
			Evaluator temp = evaluator;
			evaluator = null;
			temp.Dispose();

			// debug log
			Debug.WriteLine($"{nameof(API)}: uninitialized.");

			return;
		}

		#endregion


		#region methods

		public static List<float> RecognizeFromStream(Stream imageStream, bool closeStream, Action<Bitmap> scanBitmap) {
			// argument checks
			if (imageStream == null) {
				throw CreateBadRequestException();
			}

			return evaluator.Evaluate(imageStream, closeStream, scanBitmap);
		}

		public static List<float> RecognizeFromFile(string imageFilePath) {
			// argument checks
			if (string.IsNullOrEmpty(imageFilePath)) {
				throw CreateBadRequestException();
			}

			return evaluator.Evaluate(imageFilePath);
		}

		public static List<float> RecognizeFromUrl(string url) {
			// argument checks
			if (string.IsNullOrEmpty(url)) {
				throw new ArgumentNullException(nameof(url));
			}

			Func<Bitmap> loadBitmap = () => {
				// ToDo: should use Func<Task<Bitmap>> instead of Func<Bitmap> in Evaluation interface to support async?
				using (HttpClient httpClient = new HttpClient()) {
					// ToDo: size/content-type check
					// You cannot get imageStream.Length because this stream does not support seek.
					// Maybe I must check HttResponse directly.
					using (Stream imageStream = httpClient.GetStreamAsync(url).Result) {
						return new Bitmap(imageStream);
					}
				}
			};

			return evaluator.Evaluate(loadBitmap);
		}

		public static HttpResponseException CreateBadRequestException() {
			return new HttpResponseException(HttpStatusCode.BadRequest);
		}

		public static Exception CreateSizeException() {
			return new Exception("The image size must be less than 4M bytes.");
		}

		#endregion
	}
}