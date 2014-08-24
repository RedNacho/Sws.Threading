using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading.Interception
{
    public class SafeFailingLockController : ChainableLockControllerBase
    {
        public SafeFailingLockController(ILockController next) : base(next)
        {
        }

        protected override bool CanControl(ILock theLock)
        {
            return theLock is ISafeFailingLock;
        }

        protected override void DoEnter(ILock theLock, ref bool lockTaken)
        {
            (theLock as ISafeFailingLock).Enter(ref lockTaken);
        }

        protected override void DoExit(ILock theLock)
        {
            theLock.Exit();
        }
    }
}
