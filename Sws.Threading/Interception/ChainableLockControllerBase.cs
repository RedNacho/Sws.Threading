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
            if (CanControlWithoutChaining(theLock))
            {
                EnterWithoutChaining(theLock, ref lockTaken);
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
            if (CanControlWithoutChaining(theLock))
            {
                ExitWithoutChaining(theLock);
            }
            else
            {
                if (_next == null)
                {
                    throw new ArgumentException(ExceptionMessages.LockNotSupportedByLockController, "theLock");
                }

                _next.Exit(theLock);
            }
        }

        public bool CanControl(ILock theLock)
        {
            return CanControlWithoutChaining(theLock) || (_next != null && _next.CanControl(theLock)); 
        }

        protected abstract bool CanControlWithoutChaining(ILock theLock);

        protected abstract void EnterWithoutChaining(ILock theLock, ref bool lockTaken);

        protected abstract void ExitWithoutChaining(ILock theLock);
    }
}
