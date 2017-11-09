using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Framework.Utility.Container;
using Xunit;

namespace Framework.Tests.Utility.Container
{
    public class ConcurrentDictionaryTest
    {
        [Fact]
        public void ConcurrentDictionaryIsNotThreadSafe()
        {
            List<int> listRunCount = new List<int>();
            string key = "key";
            var i = 0;
            while (i++ < 1000 * 1000)
            { 
                var runCount = 0;
                ConcurrentDictionary<string,string> dic = new ConcurrentDictionary<string, string>();
                var task1 = Task.Run(() =>
                {
                    dic.GetOrAdd(key, (v) => { runCount++;
                        return key;
                    });
                });
                var task2 = Task.Run(() =>
                {
                    dic.GetOrAdd(key, (v) => { runCount++;
                        return key;
                    });
                });
                Task.WaitAll(task1, task2);
                listRunCount.Add(runCount);
            }
            
            Assert.Contains(listRunCount,t => t == 2);
            
        }
        
        
        
        [Fact]
        public void LazyConcurrentDictionaryIsThreadSafe()
        {
            List<int> listRunCount = new List<int>();
            string key = "key";
            var i = 0;
            while (i++ < 1000 * 1000)
            { 
                var runCount = 0;
                LazyConcurrentDictionary<string,string> dic = new LazyConcurrentDictionary<string, string>();
                var task1 = Task.Run(() =>
                {
                    dic.GetOrAdd(key, (v) => { runCount++;
                        return key;
                    });
                });
                var task2 = Task.Run(() =>
                {
                    dic.GetOrAdd(key, (v) => { runCount++;
                        return key;
                    });
                });
                Task.WaitAll(task1, task2);
                listRunCount.Add(runCount);
            }
            
            Assert.DoesNotContain(listRunCount,t => t == 2);
            
        }
        
        
        
    }
}