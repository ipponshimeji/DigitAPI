using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;


namespace DigitAPI.Web.Models {
	public class HomeViewModel {
		#region data

		public string RequestUrl { get; set; }

		public HttpPostedFileBase LocalFile { get; set; }
		
		public string Result { get; set; }

		public int ImageWidth { get; set; }

		public string ImageUrl { get; set; }

		public string ImageText { get; set; }

		public int ScrollPosition { get; set; }

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