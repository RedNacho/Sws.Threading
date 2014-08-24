using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Sws.Threading
{
    public class LockFailureException : Exception, ISerializable
    {

        public LockFailureException() : base() { }

        public LockFailureException(string message) : base(message) { }

        public LockFailureException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public LockFailureException(string message, Exception innerException) : base(message, innerException) { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }

    }
}
