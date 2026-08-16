using System;

namespace FamiMan.Core.Exceptions
{
    [Serializable]
    internal class ApuException : Exception
    {
        public ApuException()
        {
        }

        public ApuException(string message) : base(message)
        {
        }

        public ApuException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}