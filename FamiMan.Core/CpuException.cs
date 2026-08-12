using System;

namespace FamiMan.Core
{
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
    }
}
