
namespace PlayCEASharp.RequestManagement
{
    /// <summary>
    /// Provides a way to cache and retrive single instances of objects when they are identified by an id.
    /// </summary>
    /// <typeparam name="T">The type of object being cached.</typeparam>
    internal class GenericCache<T>
    {
        /// <summary>
        /// The backing cache for the objects.
        /// </summary>
        private readonly Dictionary<string, T> cache = new Dictionary<string, T>();

        /// <summary>
        /// Function to generate a new object when not present in the cache.
        /// </summary>
        private Func<string, T> constructor;

        /// <summary>
        /// Creates a new cache with the given constructor for creating objects.
        /// </summary>
        /// <param name="constructor"></param>
        internal GenericCache(Func<string, T> constructor) {
            this.constructor = constructor;
        }

        /// <summary>
        /// Gets an object with the given id.
        /// If the id is not present in cache, creates the object and caches it.
        /// </summary>
        /// <param name="id">The unique id for the obejct.</param>
        /// <returns>The object with the given id.</returns>
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
