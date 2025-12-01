using System.Linq;

namespace Algorithm_Cache.Cache
{
    public class MostRecentlyUsedAlgorithmExecute
    {
        public void Execute()
        {
            //Example A B C D E C D B:
            var lRUCache = new MostRecentlyUsedAlgorithm(2);

            lRUCache.Put("A", "A"); // cache is {1=1}
            lRUCache.Put("B", "B"); // cache is {1=1, 2=2}
            var t = lRUCache.Get("A");    // return 1
            lRUCache.Put("C","C"); // LRU key was 2, evicts key 2, cache is {1=1, 3=3}
            t = lRUCache.Get("B");    // returns -1 (not found)
            lRUCache.Put("D", "D"); // LRU key was 1, evicts key 1, cache is {4=4, 3=3}
            t = lRUCache.Get("A");    // return -1 (not found)
            t = lRUCache.Get("C");    // return 3
            t = lRUCache.Get("D");    // return 4
        }
    }

    public class MostRecentlyUsedAlgorithm
    {
        private readonly int _capacity;
        private readonly Dictionary<string, LinkedListNode<(string key, string value)>> _cache;
        private readonly LinkedList<(string key, string value)> _links;

        public MostRecentlyUsedAlgorithm(int capacity)
        {
            _capacity = capacity;
            _cache = new Dictionary<string, LinkedListNode<(string key, string value)>>();
            _links = new LinkedList<(string key, string value)>();
        }

        public string Get(string key)
        {
            if (!_cache.ContainsKey(key))
                return string.Empty;

            var node = _cache[key];
            _links.Remove(node);
            _links.AddFirst(node);
            return node.Value.value;
        }


        public void Put(string key, string value)
        {
            if (_cache.ContainsKey(key))
            {                
                var node = _cache[key];
                _cache.Remove(node.Value.key);
                _links.Remove(node);

                var newNode = _links.AddFirst((key, value));
                _cache[key] = newNode;
            }
            else 
            {
                if (_cache.Count() >= _capacity)
                {
                    var lastNode = _links.Last;                    
                    _cache.Remove(lastNode.Value.key);
                    _links.RemoveLast();                
                }

                var newNode = _links.AddFirst((key, value));
                _cache[key] = newNode;
            }
        }

    }
}
