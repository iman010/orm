using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Exceptions
{
    public class InvalidPrimaryKeysException : Exception
    {
        public InvalidPrimaryKeysException()
        {
        }

        public InvalidPrimaryKeysException(string message) : base(message)
        {
        }

        public InvalidPrimaryKeysException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidPrimaryKeysException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
