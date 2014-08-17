using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sws.Threading.Extensions;

namespace Sws.Threading.Demo
{
    public interface ICounter
    {
        int GetAndIncrement();
    }

    class Program
    {

        public class UnsafeCounter : ICounter
        {

            private int _counter;

            public int GetAndIncrement()
            {
                var currentCounter = _counter;
                Thread.Sleep(10);
                _counter++;
                return currentCounter;
            }

        }

        public class SafeCounterListener
        {

            private List<int> _counters = new List<int>();

            private readonly object _syncObject = new object();

            public void ReceiveCounter(int counter)
            {
                lock (_syncObject)
                {
                    if (_counters.Contains(counter))
                    {
                        Console.WriteLine("Duplicate counter!");
                    }
                    else
                    {
                        _counters.Add(counter);
                    }
                }
            }

        }

        static void Main(string[] args)
        {
            Console.WriteLine("Testing with unsafe counter");

            ICounter unsafeCounter = new UnsafeCounter();

            TestCounter(unsafeCounter);

            Console.WriteLine("Testing with safe counter (entire object threadsafed)");

            var safeCounter = unsafeCounter.ThreadSafeProxy();

            TestCounter(safeCounter);

            Console.WriteLine("Testing with safe counter (GetAndIncrement threadsafed)");

            safeCounter = unsafeCounter.ConfigureThreadSafeProxy().ForMember(counter => counter.GetAndIncrement()).Build();

            TestCounter(safeCounter);

            Console.WriteLine("Testing with safe counter (GetAndIncrement not threadsafed, should have problems...)");

            safeCounter = unsafeCounter.ConfigureThreadSafeProxy().NotForMember(counter => counter.GetAndIncrement()).Build();

            TestCounter(safeCounter);

            Console.WriteLine("Complete!  Press any key.");

            Console.ReadKey();
        }

        private static void TestCounter(ICounter counter)
        {
            var safeCounterListener = new SafeCounterListener();

            var actions = Enumerable.Range(0, 10).Select(index => (Action)(() =>
            {
                for (int iteration = 0; iteration < 100; iteration++)
                {
                    var nextCounter = counter.GetAndIncrement();
                    safeCounterListener.ReceiveCounter(nextCounter);
                }
            })).ToArray();

            Parallel.Invoke(actions);
        }

    }
}
