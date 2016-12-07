using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace DigitAPI.Web.Models {
	public class RecognizeRequest {
		#region data

		[JsonProperty("url")]
		public string Url { get; set; }

		#endregion
	}
}