using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using System.Collections;
using System.Collections.Generic;

namespace Framework.Tests
{
    public interface Idata
    {
        
    }

    public class Data : Idata
    {
        public string a;
    }
    public class Tests
    {
        [Fact]
        public void Test1()
        {
            //var asyncResult = new WarmupAsyncResult((obj) =>
            //{
            //    Thread.Sleep(10 * 1000);

            //}, "123");
            var a = testYied(0);
            List<object> list = new List<object>();
            foreach (var item in a)
            {
                list.Add(item);
            }
        }
        public IEnumerable testYied(int i)
        {
            if (i == 0)
                yield return new int[5];
            yield return new string[5];

        }

        [Fact]
        public void Test2()
        {
           var str=  JsonConvert.SerializeObject(new Data() {a = "123"});
        }
        
        /// <summary>
        /// AsyncResult for "on hold" request (resumes when "Completed()" is called)
        /// </summary>
        private class WarmupAsyncResult : IAsyncResult {
            private readonly EventWaitHandle _eventWaitHandle = new AutoResetEvent(false/*initialState*/);
            private readonly AsyncCallback _cb;
            private readonly object _asyncState;
            private bool _isCompleted;

            public WarmupAsyncResult(AsyncCallback cb, object asyncState) {
                _cb = cb;
                _asyncState = asyncState;
                _isCompleted = false;
            }

            public void Completed() {
                _isCompleted = true;
                _eventWaitHandle.Set();
                _cb(this);
            }

            bool IAsyncResult.CompletedSynchronously {
                get { return false; }
            }

            bool IAsyncResult.IsCompleted {
                get { return _isCompleted; }
            }

            object IAsyncResult.AsyncState {
                get { return _asyncState; }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle {
                get { return _eventWaitHandle; }
            }
        }
    }
}