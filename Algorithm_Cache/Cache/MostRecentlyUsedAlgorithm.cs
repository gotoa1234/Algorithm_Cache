namespace Algorithm_Cache.Cache
{
    public class MostRecentlyUsedAlgorithmExecute
    {
        public void Execute()
        {
            // Wiki Example A B C D E C D B:
            var workerMRU = new MostRecentlyUsedAlgorithm(4);            
            workerMRU.Put("A", "A"); 
            workerMRU.Put("B", "B");
            workerMRU.Put("C", "C");
            workerMRU.Put("D", "D");
            workerMRU.Put("E", "E");
            workerMRU.Put("C", "C");
            workerMRU.Put("D", "D");
            workerMRU.Put("B", "B");

            // Print Result
            PrintCache();

            void PrintCache()
            {
                Console.WriteLine("\n=== Cache 內容 ===");
                Console.WriteLine($"容量: {workerMRU._capacity}, 目前數量: {workerMRU._cache.Count}");

                Console.WriteLine("\n_links (由最近到最舊):");
                foreach (var item in workerMRU._links)
                {
                    Console.WriteLine($"  Key: {item.key}, Value: {item.value}");
                }

                Console.WriteLine("\n_cache (Dictionary):");
                foreach (var kvp in workerMRU._cache)
                {
                    Console.WriteLine($"  Key: {kvp.Key}, Value: ({kvp.Value.Value.key}, {kvp.Value.Value.value})");
                }
                Console.WriteLine("==================\n");
            }
        }


    }

    public class MostRecentlyUsedAlgorithm
    {
        public readonly int _capacity;
        public readonly Dictionary<string, LinkedListNode<(string key, string value)>> _cache;
        public readonly LinkedList<(string key, string value)> _links;

        /// <summary>
        /// 1. 建構式 - 快取策略容量上限
        /// </summary>
        public MostRecentlyUsedAlgorithm(int capacity)
        {
            _capacity = capacity;
            _cache = new Dictionary<string, LinkedListNode<(string key, string value)>>();
            _links = new LinkedList<(string key, string value)>();
        }

        /// <summary>
        /// 2. 取值的處理 (同 LRU 處理) 
        /// </summary>
        public string Get(string key)
        {
            if (!_cache.ContainsKey(key))
                return string.Empty;

            var node = _cache[key];
            _links.Remove(node);
            _links.AddFirst(node);
            return node.Value.value;
        }

        /// <summary>
        /// 3. 存值的處理
        /// </summary>
        public void Put(string key, string value)
        {
            // 3-1. 存在的狀況同 LRU 
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
                // 3-2. [關鍵] MRU 與 LRU 關鍵差異在刪除的快取策略
                if (_cache.Count() >= _capacity)
                {
                    // MRU 刪除最近使用的 : 與 FRU 差異在此
                    var firstNode = _links.First;                    
                    _cache.Remove(firstNode.Value.key);                    
                    _links.RemoveFirst();
                }

                var newNode = _links.AddFirst((key, value));
                _cache[key] = newNode;
            }
        }

    }
}
