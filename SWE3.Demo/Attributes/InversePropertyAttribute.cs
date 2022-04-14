using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Attributes
{
    /// <summary>This attribute marks a property as an inverse property.</summary>
    public class InversePropertyAttribute : Attribute
    {
        public string inversePropertyName;

        public InversePropertyAttribute(string v)
        {
            this.inversePropertyName = v;
        }
    }
}
