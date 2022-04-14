using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo
{
    /// <summary>This class implements an empty cache.</summary>
    public class NullCache : Cache
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        internal protected NullCache()
        { }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets a cached object.</summary>
        /// <param name="pk">Primary key.</param>
        /// <returns>Object.</returns>
        public override object this[object pk]
        {
            get { return null; }
            set { }
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets if the cache contains an object.</summary>
        /// <param name="pk">Primary key.</param>
        /// <returns>Returns TRUE if the cache contains the object.</returns>
        public override bool Contains(object pk)
        {
            return false;
        }


        /// <summary>Deletes an object from the cache.</summary>
        /// <param name="pk">Primary key.</param>
        public override void Delete(object pk)
        { }
    }
}
