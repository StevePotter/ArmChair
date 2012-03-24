namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Collections;

    internal class HashCodeCombiner
    {
        private long _combinedHash64 = 0x1505L;

        public void AddEnumerable(IEnumerable e)
        {
            if (e == null)
            {
                this.AddInt32(0);
            }
            else
            {
                int i = 0;
                foreach (object obj2 in e)
                {
                    this.AddObject(obj2);
                    i++;
                }
                this.AddInt32(i);
            }
        }

        public void AddFingerprint(ExpressionFingerprint fingerprint)
        {
            if (fingerprint != null)
            {
                fingerprint.AddToHashCodeCombiner(this);
            }
            else
            {
                this.AddInt32(0);
            }
        }

        public void AddInt32(int i)
        {
            this._combinedHash64 = ((this._combinedHash64 << 5) + this._combinedHash64) ^ i;
        }

        public void AddObject(object o)
        {
            int i = (o != null) ? o.GetHashCode() : 0;
            this.AddInt32(i);
        }

        public int CombinedHash
        {
            get
            {
                return this._combinedHash64.GetHashCode();
            }
        }
    }
}

