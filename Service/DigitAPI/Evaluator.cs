using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.MSR.CNTK.Extensibility.Managed;


namespace DigitAPI {
    public class Evaluator: IDisposable {
		#region types

		class ModelCache: ModelCacheBase<IEvaluateModelManagedF> {
			#region data

			private readonly Evaluator owner;

			#endregion


			#region creation and disposal

			public ModelCache(Evaluator owner, int maxCacheCount = DefaultMaxCacheCount): base(maxCacheCount) {
				// argument checks
				Debug.Assert(owner != null);

				// initialize members
				this.owner = owner;

				return;
			}

			#endregion


			#region overrides/overridables

			protected override IEvaluateModelManagedF CreateModelImpl() {
				return this.owner.CreateModel();
			}

			#endregion
		}

		#endregion


		#region data

		public const int ImageWidth = 28;

		public const int ImageHeight = 28;

		private static readonly IReadOnlyList<RotateFlipType> rotateFlipMap = new RotateFlipType[] {
			RotateFlipType.RotateNoneFlipNone,
			RotateFlipType.RotateNoneFlipX,
			RotateFlipType.Rotate180FlipNone,
			RotateFlipType.RotateNoneFlipY,
			RotateFlipType.Rotate270FlipY,
			RotateFlipType.Rotate90FlipNone,
			RotateFlipType.Rotate90FlipY,
			RotateFlipType.Rotate270FlipNone,
		};


		private readonly string baseDir;

		private readonly ModelCache modelCache;

		#endregion


		#region creation and disposal

		public Evaluator(string baseDir = null) {
			// argument checks
			if (string.IsNullOrEmpty(baseDir)) {
				baseDir = Path.GetDirectoryName(typeof(Evaluator).Module.FullyQualifiedName);
			}

			// initialize members
			this.baseDir = baseDir;
			this.modelCache = new ModelCache(this);

			return;
		}

		public void Dispose() {
			this.modelCache.Dispose();
		}

		#endregion


		#region methods

		/// <remarks>This method is thread-safe.</remarks>
		public List<float> Evaluate(Func<Bitmap> loadBitmap, Action<Bitmap> scanBitmap = null) {
			// argument checks
			if (loadBitmap == null) {
				throw new ArgumentNullException(nameof(loadBitmap));
			}
			// scanBitmap can be null

			// evaluate the image
			return Evaluate(GetInput(loadBitmap, scanBitmap));
		}


		/// <remarks>This method is thread-safe.</remarks>
		public List<float> Evaluate(Stream imageStream, bool closeStream = true, Action<Bitmap> scanBitmap = null) {
			// argument checks
			if (imageStream == null) {
				throw new ArgumentNullException(nameof(imageStream));
			}
			// scanBitmap can be null

			// evaluate the image
			Func<Bitmap> loadBitmap = () => {
				try {
					try {
						return new Bitmap(imageStream);
					} finally {
						if (closeStream) {
							imageStream.Dispose();
						}
					}
				} catch (ArgumentException) {
					throw new Exception("The specified resource is not acceptable as image.");
				}
			};
			return Evaluate(loadBitmap, scanBitmap);
		}

		/// <remarks>This method is thread-safe.</remarks>
		public List<float> Evaluate(string imageFilePath, Action<Bitmap> scanBitmap = null) {
			// argument checks
			if (string.IsNullOrEmpty(imageFilePath)) {
				throw new ArgumentNullException(nameof(imageFilePath));
			}
			// scanBitmap can be null

			// evaluate the image
			Func<Bitmap> loadBitmap = () => {
				try {
					return new Bitmap(imageFilePath);
				} catch (ArgumentException) {
					throw new Exception($"'{imageFilePath}' is not an acceptable image file.");
				}
			};
			return Evaluate(loadBitmap, scanBitmap);
		}

		public static void WriteOutput(TextWriter writer, List<float> output, bool singleLine = false) {
			// argument checks
			if (writer == null) {
				throw new ArgumentNullException(nameof(writer));
			}
			if (output == null) {
				throw new ArgumentNullException(nameof(output));
			}

			// write the output to the writer
			int i = 0;
			if (singleLine) {
				string separator = string.Empty;
				foreach (float value in output) {
					writer.Write($"{separator}\"{i++}\": {value.ToString("g")}");
					if (i == 1) {
						separator = ", ";
					}
				}
			} else {
				foreach (float value in output) {
					writer.WriteLine($"\"{i++}\": {value.ToString("g")}");
				}
			}

			return;
		}

		public static string OutputToString(List<float> output, bool singleLine = false) {
			using (TextWriter writer = new StringWriter()) {
				WriteOutput(writer, output, singleLine);
				return writer.ToString();
			}
		}

		#endregion


		#region privates

		private IEvaluateModelManagedF CreateModel() {
			// detect the path of the model definition
			string modelFilePath = Path.Combine(this.baseDir, "MNIST.model");

			// create a CNTK evaluate model for DigitAPI
			IEvaluateModelManagedF model = new IEvaluateModelManagedF();
			try {
				model.CreateNetwork($"modelPath=\"{modelFilePath}\"", deviceId: -1);
			} catch {
				model.Dispose();
				throw;
			}

			return model;
		}

		private List<float> Evaluate(Dictionary<string, List<float>> input) {
			// argument checks
			Debug.Assert(input != null);

			// evaluate the input
			// Note that we allocate an instance of model per call,
			// because models are not thread-safe.
			List<float> output;
			IEvaluateModelManagedF model = this.modelCache.GetModel();
			try {
				output = model.Evaluate(input, "outputNodes");
			} catch {
				// don't return this model into the cache if something wrong
				IEvaluateModelManagedF temp = model;
				model = null;
				temp.Dispose();
				throw;
			} finally {
				if (model != null) {
					// return the model into the cache
					this.modelCache.ReleaseModel(model);
				}
			}

			return output;
		}

		private static Dictionary<string, List<float>> GetInput(Func<Bitmap> loadBitmap, Action<Bitmap> scanBitmap) {
			// argument checks
			Debug.Assert(loadBitmap != null);
			// scanBitmap can be null

			// build an input
			// Only the 'features' parameter is required.
			Dictionary<string, List<float>> input = new Dictionary<string, List<float>>(1);
			input.Add("features", GetFeatures(loadBitmap, scanBitmap));

			return input;
		}

		private static List<float> GetFeatures(Func<Bitmap> loadBitmap, Action<Bitmap> scanBitmap) {
			// argument checks
			Debug.Assert(loadBitmap != null);
			// scanBitmap can be null

			// get the bitmap adjusted for model input
			List<float> features;
			using (Bitmap bitmap = CreateAdjustedBitmap(loadBitmap, scanBitmap)) {
				Debug.Assert(bitmap.Width == ImageWidth && bitmap.Height == ImageHeight);

				// convert the bitmap to an array of float
				features = new List<float>(ImageWidth * ImageHeight);
				for (int y = 0; y < ImageHeight; ++y) {
					for (int x = 0; x < ImageWidth; ++x) {
						// make RGB value to grayscale, and reverse white and black colors
						// Note that the model was trained with images of white figure on black background.
						Color pixel = bitmap.GetPixel(x, y);
						float value = 255 - (pixel.R + pixel.G + pixel.B) / 3;
						features.Add(value);
					}
				}
			}
			Debug.Assert(features.Count == ImageWidth * ImageHeight);

			return features;
		}

		private static Bitmap CreateAdjustedBitmap(Func<Bitmap> loadBitmap, Action<Bitmap> scanBitmap) {
			// argument checks
			Debug.Assert(loadBitmap != null);
			// scanBitmap can be null

			// create a bitmap and adjust it if necessary
			Bitmap bitmap = loadBitmap();
			try {
				// normalize its orientation
				RotateFlipType rf = GetRotateFlipTypeToNormalize(bitmap);
				if (rf != RotateFlipType.RotateNoneFlipNone) {
					bitmap.RotateFlip(rf);
				}

				// perform custom task
				if (scanBitmap != null) {
					scanBitmap(bitmap);
				}

				// adjust its size
				if (bitmap.Width != ImageWidth || bitmap.Height != ImageHeight) {
					// replace bitmap with the resized one 
					Bitmap adjustedBitmap = new Bitmap(bitmap, ImageWidth, ImageHeight);
					try {
						bitmap.Dispose();
					} catch {
						// not fatal, continue
					}
					bitmap = adjustedBitmap;
				}
			} catch {
				bitmap.Dispose();
				throw;
			}

			return bitmap;
		}

		private static RotateFlipType GetRotateFlipTypeToNormalize(Bitmap bitmap) {
			// argument checks
			Debug.Assert(bitmap != null);

			// determine RotateFlipType from the Exif orientation of the bitmap
			// thanks to http://blog.ch3cooh.jp/entry/20111222/1324552051
			// and http://blog.shibayan.jp/entry/20140428/1398688687
			int id = 0x0112;    // Tag No. of Exif Orientation
			PropertyItem orientation = bitmap.PropertyItems.FirstOrDefault(pi => pi.Id == id);
			int mapIndex = (orientation == null)? 0: ((int)orientation.Value[0]) - 1;

			return (0 <= mapIndex && mapIndex < rotateFlipMap.Count)? rotateFlipMap[mapIndex]: RotateFlipType.RotateNoneFlipNone;
		}

		#endregion


		#region for internal test

#if DEBUG
		public void MultiThreadTest(string[] imageFilePaths) {
			try {
				// prepare inputs
				Dictionary<string, List<float>>[] inputs = imageFilePaths.Select(
					imageFilePath => GetInput(() => new Bitmap(imageFilePath), null)
				).ToArray();

				// call Eval() parallelly
				int count = 100;
				List<float>[] outputs = new List<float>[count];
				Action<int> call = (i) => {
					outputs[i] = this.Evaluate(inputs[i % inputs.Length]);
				};

				Parallel.For(0, count, call);

				// write outputs
				int index = 0;
				foreach (List<float> output in outputs) {
					Console.WriteLine($"[{index++}]");
					WriteOutput(Console.Out, output);
				}
			} catch (Exception exception) {
				Console.Error.WriteLine(exception);
			}
		}
#endif

		#endregion
	}
}
