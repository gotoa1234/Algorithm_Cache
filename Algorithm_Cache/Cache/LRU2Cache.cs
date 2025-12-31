namespace Algorithm_Cache.Cache
{
    public class LRU2Cache
    {
        private readonly Dictionary<string, Node> _cache; // 主快取
        private readonly static int LRU_K_Times = 2;// 冷數據閥值   
        private readonly LRU _HotData;// 熱數據 - LRU
        private readonly FIFO _ColdData;// 冷數據 - FIFO
        private readonly int _capacity;

        /// <summary>
        /// 1. 建構式 - 配置
        /// </summary>
        /// <param name="capacity"></param>
        public LRU2Cache(int capacity)
        {
            _capacity = capacity;
            _cache = new Dictionary<string, Node>();

            // 1-1. 配置冷熱數據比例：25% 冷數據，75% 熱數據
            int coldCapacity = Math.Max(1, capacity / 4);
            int hotCapacity = capacity - coldCapacity;

            // 1-2. 初始化冷熱數據上限
            _HotData = new LRU { LRUCapacity = hotCapacity };
            _ColdData = new FIFO { HistoryCapacity = coldCapacity };
        }

        /// <summary>
        /// 測試方法
        /// </summary>
        public void Execute()
        {
            var cache = new LRU2Cache(12);// 熱數據上限 : 9  + 冷數據上限 3

            cache.Put("A", "A");// 加入冷數據 [A]
            cache.Put("B", "B");// 加入冷數據 [B, A]
            cache.Put("C", "C");// 加入冷數據 [C, B, A]            
            cache.Put("D", "D");// 加入冷數據 [D, C, B] ，將 A 從冷數據移除，因為上限為 3
            var result = cache.Get("A");// Not Found 因為 A 不在冷熱數據中
            result = cache.Get("D");// 冷數據移除 [C, B] ， D 前往熱數據 [D]
            cache.Put("E", "E");// 加入冷數據 [E, C, B] ， 熱數據 [D]            
            result = cache.Get("B");// 冷數據移除 [E, C] ， B 前往熱數據 [B, D]
            cache.Put("E", "E2");// 冷數據移除 [C] ， E 前往熱數據 [E, B, D]

            // 輸出:  冷 :[C] , 熱 :[E, B, D]
        }

        /// <summary>
        /// 2. Get 獲取方法
        /// </summary>
        public string Get(string key)
        {
            if (!_cache.ContainsKey(key))
                return "Not Found!";

            // 2-1. 提升權重
            var node = _cache[key];
            PromoteNode(key, node);
            return node.Value;
        }

        /// <summary>
        /// 3. 設定鍵值
        /// </summary>
        public void Put(string key, string value)
        {
            // 3-1. 無空間直接結束
            if (_capacity == 0)
                return;

            // 3-2. 快取檢查，若被加入冷熱數據 中，皆可查詢到
            if (_cache.ContainsKey(key))
            {
                var node = _cache[key];
                node.Value = value;     
                
                // 3-3. 在冷數據中，重複 Put 的 Key 也可以升級成熱數據 
                // ※ Put 需依照應用情境，來決定是否提升權重
                PromoteNode(key, node);
            }
            else
            {
                // 3-3. 保存的數據達上限時，需要移除
                if (_cache.Count >= _capacity)
                {
                    Evict();
                }

                // 3-4. 新增節點到冷數據區
                var newNode = new Node
                {
                    Key = key,
                    Value = value,
                    AccessCount = 1,
                    FirstAccessTime = DateTime.Now,
                    SecondAccessTime = null
                };
                _cache[key] = newNode;

                // 3-5. 檢查冷數據區是否需要淘汰，達到上限時，移除最早加入的
                if (_ColdData.HistoryQueue.Count >= _ColdData.HistoryCapacity)
                {
                    var oldestKey = _ColdData.HistoryQueue.First.Value;
                    _ColdData.HistoryQueue.RemoveFirst();
                    _ColdData.HistoryNodes.Remove(oldestKey);
                    _cache.Remove(oldestKey);
                }

                var listNode = _ColdData.HistoryQueue.AddLast(key);
                _ColdData.HistoryNodes[key] = listNode;
            }
        }

        /// <summary>
        /// 4. 提升節點優先級
        /// </summary>
        private void PromoteNode(string key, Node node)
        {
            // 4-1. 提升訪問權重
            node.AccessCount++;

            // 4-2. LRU-K 這裡的 K 依照應用情境配置合理值，範例為 2 
            if (node.AccessCount == LRU_K_Times)
            {
                node.SecondAccessTime = DateTime.Now;

                // 4-3. 從冷數據區移除
                if (_ColdData.HistoryNodes.ContainsKey(key))
                {
                    _ColdData.HistoryQueue.Remove(_ColdData.HistoryNodes[key]);
                    _ColdData.HistoryNodes.Remove(key);
                }

                // 4-4. 檢查熱數據區容量，若達熱數據上限，要遵循 LRU 移除最久未被使用的
                if (_HotData.CacheQueue.Count >= _HotData.LRUCapacity)
                {
                    var lruKey = _HotData.CacheQueue.Last.Value;
                    _HotData.CacheQueue.RemoveLast();
                    _HotData.CacheNodes.Remove(lruKey);
                    _cache.Remove(lruKey);
                }

                // 4-5. 正式加入熱數據區 (完成 : 冷數據 -> 熱數據)
                var listNode = _HotData.CacheQueue.AddFirst(key);
                _HotData.CacheNodes[key] = listNode;
            }
            //4-6. 已被加入到熱數據
            else if (node.AccessCount > LRU_K_Times)
            {
                // 4-7. 遵循 LRU 在熱數據區內移到最前面
                if (_HotData.CacheNodes.ContainsKey(key))
                {
                    _HotData.CacheQueue.Remove(_HotData.CacheNodes[key]);
                    var listNode = _HotData.CacheQueue.AddFirst(key);
                    _HotData.CacheNodes[key] = listNode;
                }
            }
        }

        /// <summary>
        /// 5. 淘汰策略：優先淘汰 historyQueue (只訪問一次)，其次淘汰 cacheQueue 的 LRU
        /// </summary>
        private void Evict()
        {
            string keyToRemove;

            // 5-1. 優先淘汰冷數據區
            if (_ColdData.HistoryQueue.Count > 0)
            {
                keyToRemove = _ColdData.HistoryQueue.First.Value;
                _ColdData.HistoryQueue.RemoveFirst();
                _ColdData.HistoryNodes.Remove(keyToRemove);
            }
            // 5-2. 否則淘汰熱數據區的-最久未使用的 (LRU)
            else if (_HotData.CacheQueue.Count > 0)
            {
                keyToRemove = _HotData.CacheQueue.Last.Value;
                _HotData.CacheQueue.RemoveLast();
                _HotData.CacheNodes.Remove(keyToRemove);
            }
            else// 5-3. 沒有可淘汰的狀況
            {
                return; 
            }

            _cache.Remove(keyToRemove);
        }

        #region 內部類別

        /// <summary>
        /// FIFO 結構 (冷數據)
        /// </summary>
        private class FIFO
        {
            /// <summary>
            /// 快取空間筆數
            /// </summary>
            public int HistoryCapacity { get; set; }

            /// <summary>
            /// 歷史佇列節點
            /// </summary>
            public Dictionary<string, LinkedListNode<string>> HistoryNodes { get; set; } = new Dictionary<string, LinkedListNode<string>>();

            /// <summary>
            /// 只訪問過一次的佇列 (FIFO)
            /// </summary>
            public LinkedList<string> HistoryQueue { get; set; } = new LinkedList<string>();
        }

        /// <summary>
        /// LRU 結構 (熱數據)
        /// </summary>
        private class LRU
        {
            /// <summary>
            /// 快取空間筆數
            /// </summary>
            public int LRUCapacity { get; set; }

            /// <summary>
            /// 快取佇列節點
            /// </summary>
            public Dictionary<string, LinkedListNode<string>> CacheNodes { get; set; } = new Dictionary<string, LinkedListNode<string>>();

            /// <summary>
            /// 訪問過兩次以上的佇列 (LRU)
            /// </summary>
            public LinkedList<string> CacheQueue = new LinkedList<string>();
        }

        private class Node
        {
            public string Key { get; set; }
            public string Value { get; set; }

            /// <summary>
            /// 存取次數
            /// </summary>
            public int AccessCount { get; set; }

            /// <summary>
            /// 檢視用 - LinkedList 已有順序
            /// </summary>
            public DateTime FirstAccessTime { get; set; }

            /// <summary>
            /// 檢視用 - LinkedList 已有順序
            /// </summary>
            public DateTime? SecondAccessTime { get; set; }  
        }

        #endregion
    }
}
