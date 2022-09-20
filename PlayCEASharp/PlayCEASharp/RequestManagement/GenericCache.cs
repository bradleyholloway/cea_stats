
namespace PlayCEASharp.RequestManagement
{
    internal class GenericCache<T>
    {
        private readonly Dictionary<string, T> cache = new Dictionary<string, T>();

        private Func<string, T> constructor;

        internal GenericCache(Func<string, T> constructor) {
            this.constructor = constructor;
        }

        internal T Get(string id)
        {
            T o;
            if (this.cache.ContainsKey(id))
            {
                o = this.cache[id];
            }
            else
            {
                Dictionary<string, T> cache = this.cache;
                lock (cache)
                {
                    if (this.cache.ContainsKey(id))
                    {
                        o = this.cache[id];
                    }
                    else
                    {
                        T n = this.constructor(id);
                        this.cache[id] = n;
                        o = n;
                    }
                }
            }
            return o;
        }
    }
}
