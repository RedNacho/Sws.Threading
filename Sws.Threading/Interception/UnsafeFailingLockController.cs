using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading.Interception
{
    public class UnsafeFailingLockController : ChainableLockControllerBase
    {
        public UnsafeFailingLockController(ChainableLockControllerBase next)
            : base(next)
        {
        }

        protected override bool CanControl(ILock theLock)
        {
            return true;
        }

        protected override void DoEnter(ILock theLock, ref bool lockTaken)
        {
            theLock.Enter();
            lockTaken = true;
        }

        protected override void DoExit(ILock theLock)
        {
            theLock.Exit();
        }

    }
}
