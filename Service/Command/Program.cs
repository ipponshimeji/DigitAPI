using System;
using System.Collections.Generic;
using DigitAPI;

namespace DigitAPI.Command {
	class Program {
		static void Main(string[] args) {
			if (args.Length < 1) {
				Console.WriteLine("Usage: digit <image file path>");
				return;
			}
			string filePath = args[0];

			try {
				List<float> output;
				using (Evaluator evaluator = new Evaluator()) {
					output = evaluator.Evaluate(filePath);
				}

				Console.WriteLine(filePath);
				Evaluator.WriteOutput(Console.Out, output);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}
	}
}
