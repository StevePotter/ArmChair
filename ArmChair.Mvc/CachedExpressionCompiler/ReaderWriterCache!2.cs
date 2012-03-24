namespace Microsoft.Web.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal abstract class ReaderWriterCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _cache;
        private readonly ReaderWriterLockSlim _rwLock;

        protected ReaderWriterCache() : this(null)
        {
        }

        protected ReaderWriterCache(IEqualityComparer<TKey> comparer)
        {
            this._rwLock = new ReaderWriterLockSlim();
            this._cache = new Dictionary<TKey, TValue>(comparer);
        }

        protected TValue FetchOrCreateItem(TKey key, Func<TValue> creator)
        {
            TValue local4;
            this._rwLock.EnterReadLock();
            try
            {
                TValue local;
                if (this._cache.TryGetValue(key, out local))
                {
                    return local;
                }
            }
            finally
            {
                this._rwLock.ExitReadLock();
            }
            TValue local2 = creator();
            this._rwLock.EnterWriteLock();
            try
            {
                TValue local3;
                if (this._cache.TryGetValue(key, out local3))
                {
                    return local3;
                }
                this._cache[key] = local2;
                local4 = local2;
            }
            finally
            {
                this._rwLock.ExitWriteLock();
            }
            return local4;
        }

        protected Dictionary<TKey, TValue> Cache
        {
            get
            {
                return this._cache;
            }
        }
    }
}

