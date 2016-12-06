using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace DigitAPI {
	public abstract class ModelCacheBase<TModel>: IDisposable where TModel: class, IDisposable {
		#region data

		public const int DefaultMaxCacheCount = 8;


		private readonly object cacheLocker = new object();

		private readonly int maxCacheCount;

		private Stack<TModel> cache = new Stack<TModel>();

		#endregion


		#region properties

		public int MaxCacheCount {
			get {
				return this.maxCacheCount;
			}
		}

		#endregion


		#region creation and disposal

		protected ModelCacheBase(int maxCacheCount) {
			// initialize members
			this.maxCacheCount = maxCacheCount;

			return;
		}

		public virtual void Dispose() {
			// clear the cache
			Stack<TModel> cache;
			lock (this.cacheLocker) {
				cache = this.cache;
				this.cache = null;
			}

			// dispose the models
			if (cache != null) {
				while (0 < cache.Count) {
					try {
						cache.Pop().Dispose();
					} catch {
						// continue
					}
				}
				cache.Clear();
			}

			return;
		}

		#endregion


		#region methods

		public TModel GetModel() {
			TModel model = null;

			// try to retrieve a cached model
			lock (this.cacheLocker) {
				var cache = this.cache;
				if (cache == null) {
					throw new ObjectDisposedException(null);
				}
				if (0 < cache.Count) {
					model = cache.Pop();
				}
			}

			// if no model is available, create new one
			if (model == null) {
				model = CreateModelImpl();
			}

			return model;
		}

		public void ReleaseModel(TModel model) {
			// argument checks
			if (model == null) {
				throw new ArgumentNullException(nameof(model));
			}

			// try to store the model into the cache
			lock (this.cacheLocker) {
				if (this.cache.Count < this.maxCacheCount) {
					this.cache.Push(model);
					model = null;
				}
			}

			// if enough models are cached, dispose this one
			if (model != null) {
				model.Dispose();
			}

			return;
		}

		#endregion


		#region overridables

		protected abstract TModel CreateModelImpl();

		#endregion
	}
}
