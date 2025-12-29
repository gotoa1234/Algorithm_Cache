using Algorithm_Cache.Cache;

// 1. MRU
//var alMRU = new MostRecentlyUsedAlgorithmExecute();
//alMRU.Execute();

// 2. LRU-K K=2
var kLRU = new LRU2Cache(2);
kLRU.Execute();