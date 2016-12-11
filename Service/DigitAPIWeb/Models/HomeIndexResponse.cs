using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;


namespace DigitAPI.Web.Models {
	public class HomeIndexResponse {
		#region data

		public static string[] Samples = new string[] {
			"sample1.png",
			"sample2.png",
			"sample3.png"
		};


		public HomeIndexRequest Request { get; }

		public string Result { get; set; } = null;

		public string ImageUrl { get; set; } = null;

		public string ImageText { get; set; } = string.Empty;

		#endregion


		#region creation and disposal

		public HomeIndexResponse(HomeIndexRequest request) {
			// argument checks
			if (request == null) {
				throw new ArgumentNullException(nameof(request));
			}

			// initialize member
			this.Request = request;

			return;
		}

		#endregion


		#region methods

		public void SetResult(List<float> output) {
			// argument checks
			if (output == null) {
				throw new ArgumentNullException(nameof(output));
			}
			if (output.Count != 10) {
				throw new ArgumentException("Its Count must be 10.", nameof(output));
			}

			// format the output into the Result
			StringBuilder buf = new StringBuilder();
			buf.AppendLine("Detection Result:");
			buf.AppendLine();
			buf.AppendLine("JSON:");
			buf.AppendLine("{");
			buf.AppendLine("  \"scores\": {");
			for (int i = 0; i < 10; ++i) {
				buf.AppendLine($"    \"{i}\": {output[i].ToString("g")}");
			}
			buf.AppendLine("  }");
			buf.AppendLine("}");

			this.Result = buf.ToString();
		}

		public void SetResult(Exception exception) {
			// argument checks
			if (exception == null) {
				throw new ArgumentNullException(nameof(exception));
			}

			// format the error into the Result
			this.Result = string.Concat("Detection Result:", Environment.NewLine, exception.Message);
		}

		#endregion
	}
}