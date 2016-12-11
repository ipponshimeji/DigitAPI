using System;
using System.Collections.Generic;
using System.Text;
using System.Web;


namespace DigitAPI.Web.Models {
	public class HomeIndexRequest {
		#region data

		public int SampleIndex { get; set; } = -1;

		public string RequestUrl { get; set; }

		public HttpPostedFileBase LocalFile { get; set; }
		
		public int ImageWidth { get; set; }

		public int ScrollPosition { get; set; }

		#endregion
	}
}