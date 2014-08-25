using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading.Interception
{
    public interface ILockController
    {
        /// <summary>
        /// Indicates whether or not this ILockController can control the specified lock.
        /// </summary>
        /// <param name="theLock"></param>
        /// <returns></returns>
        bool CanControl(ILock theLock);

        /// <summary>
        /// Tries to enter the lock if it can be controlled; behaviour is undefined otherwise, but may throw ArgumentException.
        /// </summary>
        /// <param name="theLock"></param>
        /// <param name="lockTaken"></param>
        void Enter(ILock theLock, ref bool lockTaken);

        /// <summary>
        /// Tries to exit the lock if it can be controlled; behaviour is undefined otherwise, but may throw ArgumentException.
        /// </summary>
        /// <param name="theLock"></param>
        void Exit(ILock theLock);
    }
}
