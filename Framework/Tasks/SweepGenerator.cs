using System;
using System.Threading;
using System.Timers;
using Framework.Exceptions;
using Framework.Logging;
using Timer = System.Timers.Timer;

namespace Framework.Tasks
{
//    public interface ISweepGenerator : ISingletonDependency
//    {
//        void Activate();
//        void Terminate();
//    }
//
//    public class SweepGenerator : ISweepGenerator, IDisposable
//    {
//        private readonly IWorkContextAccessor _workContextAccessor;
//        private readonly Timer _timer;
//
//        public SweepGenerator(IWorkContextAccessor workContextAccessor)
//        {
//            _workContextAccessor = workContextAccessor;
//            _timer = new Timer();
//            _timer.Elapsed += Elapsed;
//            Logger = NullLogger.Instance;
//            Interval = TimeSpan.FromMinutes(1);
//        }
//
//        public ILogger Logger { get; set; }
//
//        public TimeSpan Interval
//        {
//            get => TimeSpan.FromMilliseconds(_timer.Interval);
//            set => _timer.Interval = value.TotalMilliseconds;
//        }
//
//        public void Activate()
//        {
//            lock (_timer)
//            {
//                _timer.Start();
//            }
//        }
//
//        public void Terminate()
//        {
//            lock (_timer)
//            {
//                _timer.Stop();
//            }
//        }
//
//        private void Elapsed(object sender, ElapsedEventArgs e)
//        {
//            // current implementation disallows re-entrancy
//            if (!Monitor.TryEnter(_timer))
//                return;
//
//            try
//            {
//                if (_timer.Enabled) DoWork();
//            }
//            catch (Exception ex)
//            {
//                if (ex.IsFatal()) throw;
//
//                Logger.Warning(ex, "Problem in background tasks");
//            }
//            finally
//            {
//                Monitor.Exit(_timer);
//            }
//        }
//
//        public void DoWork()
//        {
//            using (var scope = _workContextAccessor.CreateWorkContextScope())
//            {
//                // resolve the manager and invoke it
//                var manager = scope.Resolve<IBackgroundService>();
//                manager.Sweep();
//            }
//        }
//
//        public void Dispose()
//        {
//            _timer.Dispose();
//        }
//    }
}