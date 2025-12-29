using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithm_Cache.Cache
{
    public class LRU2Cache
    {
        private readonly int _capacity;// 快取空間筆數
        private readonly Dictionary<int, Node> _cache; // 主快取

        // 冷數據 - FIFO
        private readonly Dictionary<int, LinkedListNode<int>> _historyNodes; // 歷史佇列節點
        private readonly LinkedList<int> _historyQueue; // 只訪問過一次的佇列 (FIFO)

        // 熱數據 - LRU
        private readonly Dictionary<int, LinkedListNode<int>> _cacheNodes; // 快取佇列節點
        private readonly LinkedList<int> _cacheQueue; // 訪問過兩次以上的佇列 (LRU)

        public LRU2Cache(int capacity)
        {
            _capacity = capacity;
            _cache = new Dictionary<int, Node>();
            _historyNodes = new Dictionary<int, LinkedListNode<int>>();
            _historyQueue = new LinkedList<int>();

            _cacheNodes = new Dictionary<int, LinkedListNode<int>>();
            _cacheQueue = new LinkedList<int>();
        }

        /// <summary>
        /// 測試方法
        /// </summary>
        public void Execute()
        {
            var cache = new LRU2Cache(3);

            cache.Put(1, 1);  // [1]_history
            cache.Put(2, 2);  // [1,2]_history
            cache.Put(3, 3);  // [1,2,3]_history

            var t = cache.Get(1); // 1, 提升到 cache: []_history, [1]_cache
            cache.Put(4, 4);      // 淘汰 2 (history 中最早): [3,4]_history, [1]_cache

            t = cache.Get(3);     // 3, 提升到 cache: [4]_history, [3,1]_cache
            t = cache.Get(1);     // 1, 保持在 cache 前面: [4]_history, [1,3]_cache

            cache.Put(5, 5);      // 淘汰 4 (history): [5]_history, [1,3]_cache
            t = cache.Get(3);     // 3, [5]_history, [3,1]_cache

            cache.Put(6, 6);      // 容量滿，淘汰 cache 的 LRU (1): [5,6]_history, [3]_cache

            // 輸出: 1, 3, 1, 3
        }

        /// <summary>
        /// 獲取值
        /// </summary>
        public int Get(int key)
        {
            if (!_cache.ContainsKey(key))
                return -1;

            var node = _cache[key];
            PromoteNode(key, node);
            return node.Value;
        }

        /// <summary>
        /// 設定鍵值
        /// </summary>
        public void Put(int key, int value)
        {
            if (_capacity == 0)
                return;

            // 已存在的 key
            if (_cache.ContainsKey(key))
            {
                var node = _cache[key];
                node.Value = value;
                PromoteNode(key, node);
            }
            else
            {
                // 需要淘汰
                if (_cache.Count >= _capacity)
                {
                    Evict();
                }

                // 新增節點，首次訪問放入 historyQueue
                var newNode = new Node
                {
                    Key = key,
                    Value = value,
                    AccessCount = 1,
                    FirstAccessTime = DateTime.Now,
                    SecondAccessTime = null
                };
                _cache[key] = newNode;

                var listNode = _historyQueue.AddLast(key);
                _historyNodes[key] = listNode;
            }
        }

        /// <summary>
        /// 提升節點優先級
        /// </summary>
        private void PromoteNode(int key, Node node)
        {
            node.AccessCount++;

            // 第一次訪問 -> 第二次訪問：從 historyQueue 移到 cacheQueue
            if (node.AccessCount == 2)
            {
                node.SecondAccessTime = DateTime.Now;

                // 從 historyQueue 移除
                if (_historyNodes.ContainsKey(key))
                {
                    _historyQueue.Remove(_historyNodes[key]);
                    _historyNodes.Remove(key);
                }

                // 加入 cacheQueue (最前面 = 最近使用)
                var listNode = _cacheQueue.AddFirst(key);
                _cacheNodes[key] = listNode;
            }
            // 已經在 cacheQueue 中，移到最前面
            else if (node.AccessCount > 2)
            {
                if (_cacheNodes.ContainsKey(key))
                {
                    _cacheQueue.Remove(_cacheNodes[key]);
                    var listNode = _cacheQueue.AddFirst(key);
                    _cacheNodes[key] = listNode;
                }
            }
        }

        /// <summary>
        /// 淘汰策略：優先淘汰 historyQueue (只訪問一次)，其次淘汰 cacheQueue 的 LRU
        /// </summary>
        private void Evict()
        {
            int keyToRemove;

            // 優先淘汰只訪問過一次的頁面 (FIFO)
            if (_historyQueue.Count > 0)
            {
                keyToRemove = _historyQueue.First.Value;
                _historyQueue.RemoveFirst();
                _historyNodes.Remove(keyToRemove);
            }
            // 否則淘汰 cacheQueue 中最久未使用的 (LRU)
            else
            {
                keyToRemove = _cacheQueue.Last.Value;
                _cacheQueue.RemoveLast();
                _cacheNodes.Remove(keyToRemove);
            }

            _cache.Remove(keyToRemove);
        }

        /// <summary>
        /// 節點資料結構
        /// </summary>
        private class Node
        {
            public int Key { get; set; }
            public int Value { get; set; }
            public int AccessCount { get; set; }
            public DateTime FirstAccessTime { get; set; }// 檢視用 - LinkedList 已有順序
            public DateTime? SecondAccessTime { get; set; }// 檢視用 - LinkedList 已有順序
        }
    }
}
