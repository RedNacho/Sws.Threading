using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading.Interception
{
    public abstract class ChainableLockControllerBase : ILockController
    {
        private readonly ILockController _next;

        public ChainableLockControllerBase(ILockController next)
        {
            _next = next;
        }

        public void Enter(ILock theLock, ref bool lockTaken)
        {
            if (CanControl(theLock))
            {
                DoEnter(theLock, ref lockTaken);
            }
            else
            {
                if (_next == null)
                {
                    throw new ArgumentException(ExceptionMessages.LockNotSupportedByLockController);
                }

                _next.Enter(theLock, ref lockTaken);
            }
        }

        public void Exit(ILock theLock)
        {
            if (CanControl(theLock))
            {
                DoExit(theLock);
            }
            else
            {
                if (_next == null)
                {
                    throw new ArgumentException(ExceptionMessages.LockNotSupportedByLockController);
                }

                _next.Exit(theLock);
            }
        }

        protected abstract bool CanControl(ILock theLock);

        protected abstract void DoEnter(ILock theLock, ref bool lockTaken);

        protected abstract void DoExit(ILock theLock);
    }
}
