using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;


namespace DigitAPI.Web.Models {
	public class Scores {
		#region data

		[JsonProperty("0")]
		public float Digit0 { get; set; }

		[JsonProperty("1")]
		public float Digit1 { get; set; }

		[JsonProperty("2")]
		public float Digit2 { get; set; }

		[JsonProperty("3")]
		public float Digit3 { get; set; }

		[JsonProperty("4")]
		public float Digit4 { get; set; }

		[JsonProperty("5")]
		public float Digit5 { get; set; }

		[JsonProperty("6")]
		public float Digit6 { get; set; }

		[JsonProperty("7")]
		public float Digit7 { get; set; }

		[JsonProperty("8")]
		public float Digit8 { get; set; }

		[JsonProperty("9")]
		public float Digit9 { get; set; }

		#endregion


		#region creation and disposal

		public Scores() {
			// initialize member
			this.Digit0 = 0F;
			this.Digit1 = 0F;
			this.Digit2 = 0F;
			this.Digit3 = 0F;
			this.Digit4 = 0F;
			this.Digit5 = 0F;
			this.Digit6 = 0F;
			this.Digit7 = 0F;
			this.Digit8 = 0F;
			this.Digit9 = 0F;

			return;
		}

		public Scores(IList<float> values) {
			// argument checks
			if (values == null) {
				throw new ArgumentNullException(nameof(values));
			}
			if (values.Count != 10) {
				throw new ArgumentException("Its count must be 10", nameof(values));
			}

			// initialize member
			this.Digit0 = values[0];
			this.Digit1 = values[1];
			this.Digit2 = values[2];
			this.Digit3 = values[3];
			this.Digit4 = values[4];
			this.Digit5 = values[5];
			this.Digit6 = values[6];
			this.Digit7 = values[7];
			this.Digit8 = values[8];
			this.Digit9 = values[9];

			return;
		}

		#endregion
	}
}