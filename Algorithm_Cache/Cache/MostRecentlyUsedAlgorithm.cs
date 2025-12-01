namespace Algorithm_Cache.Cache
{
    public class MostRecentlyUsedAlgorithmExecute
    {
        public void Execute()
        {
            //Example A B C D E C D B:
        }
    }

    public class MostRecentlyUsedAlgorithm
    {
        private readonly int _capacity;
        private readonly Dictionary<string, LinkedListNode<(string key, int value)>> _cache;
        private readonly LinkedList<(string key, int value)> _links;

        public MostRecentlyUsedAlgorithm(int capacity)
        {
            
        }

        public int Get(int key)
        {
            return 0;
        }


        public void Put(int key, int value)
        {
        }

    }
}
