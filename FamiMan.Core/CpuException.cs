using System;
using System.Runtime.Serialization;

namespace FamiMan.Core
{
    [Serializable]
    public class CpuException : Exception
    {
        public CpuException()
        {
        }

        public CpuException(string message) : base(message)
        {
        }

        public CpuException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CpuException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}