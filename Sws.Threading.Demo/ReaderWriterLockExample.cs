using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sws.Threading.Extensions;

namespace Sws.Threading.Demo
{
    public class ReadLock : ILock
    {
        private readonly ReaderWriterLockSlim _readerWriterLockSlim;

        public ReadLock(ReaderWriterLockSlim readerWriterLockSlim)
        {
            if (readerWriterLockSlim == null)
            {
                throw new ArgumentNullException("readerWriterLockSlim");
            }

            _readerWriterLockSlim = readerWriterLockSlim;
        }

        public void Enter()
        {
            _readerWriterLockSlim.EnterReadLock();
        }

        public void Exit()
        {
            _readerWriterLockSlim.ExitReadLock();
        }

    }

    public class WriteLock : ILock
    {
        private readonly ReaderWriterLockSlim _readerWriterLockSlim;

        public WriteLock(ReaderWriterLockSlim readerWriterLockSlim)
        {
            if (readerWriterLockSlim == null)
            {
                throw new ArgumentNullException("readerWriterLockSlim");
            }

            _readerWriterLockSlim = readerWriterLockSlim;
        }

        public void Enter()
        {
            _readerWriterLockSlim.EnterWriteLock();
        }

        public void Exit()
        {
            _readerWriterLockSlim.ExitWriteLock();
        }

    }

    public class Container
    {
        public virtual int Value
        {
            get;
            set;
        }
    }

    public class Client
    {
        public void Main()
        {
            var container = new Container();

            var readerWriterLockSlim = new ReaderWriterLockSlim();

            // The proxied container will apply the semantics of a ReaderWriterLock to the Value property.

            container = container
                .ConfigureThreadSafeProxy().ForGetter(obj => obj.Value)
                .WithLockingObject(readerWriterLockSlim).WithLockFactory(lockingObject => new ReadLock(lockingObject as ReaderWriterLockSlim)).Build()
                .ConfigureThreadSafeProxy().ForSetter(obj => obj.Value)
                .WithLockingObject(readerWriterLockSlim).WithLockFactory(lockingObject => new WriteLock(lockingObject as ReaderWriterLockSlim)).Build();
        }
    }

}
