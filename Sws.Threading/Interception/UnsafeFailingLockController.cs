using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading.Interception
{
    public class UnsafeFailingLockController : ChainableLockControllerBase
    {
        public UnsafeFailingLockController(ILockController next)
            : base(next)
        {
        }

        protected override bool CanControlWithoutChaining(ILock theLock)
        {
            return true;
        }

        protected override void EnterWithoutChaining(ILock theLock, ref bool lockTaken)
        {
            theLock.Enter();
            lockTaken = true;
        }

        protected override void ExitWithoutChaining(ILock theLock)
        {
            theLock.Exit();
        }

    }
}
