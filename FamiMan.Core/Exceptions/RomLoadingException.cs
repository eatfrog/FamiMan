using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core.Exceptions
{
    public class RomLoadingException : Exception
    {
        public RomLoadingException(string message) : base(message)
        {
        }
    }
}
