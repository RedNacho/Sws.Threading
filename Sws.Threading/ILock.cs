using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading
{

    /// <summary>
    /// Abstraction for a thread lock (e.g. a Mutex).
    /// </summary>
    public interface ILock
    {
        /// <summary>
        /// Enters the lock.
        /// </summary>
        void Enter();

        /// <summary>
        /// Exits the lock.
        /// </summary>
        void Exit();
    }
}
