using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Exceptions
{
    public class InversePropertyException : Exception
    {
        public InversePropertyException()
        {
        }

        public InversePropertyException(string message) : base(message)
        {
        }

        public InversePropertyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InversePropertyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
