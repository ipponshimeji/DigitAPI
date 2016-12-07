using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;


namespace DigitAPI.Web.Models {
	public class RecognizeResponse {
		#region data

		[JsonProperty("scores")]
		public Scores Scores { get; set; }

		#endregion


		#region creation and disposal

		public RecognizeResponse() {
			// initialize member
			this.Scores = new Scores();

			return;
		}

		public RecognizeResponse(IList<float> values) {
			// initialize member
			this.Scores = new Scores(values);

			return;
		}

		#endregion
	}
}
