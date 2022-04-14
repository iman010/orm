using SWE3.Demo.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Show
{
    public class LockObject
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Implements the demonstration.</summary>
        public static void Show()
        {
            SampleDbContext myDbContext = new SampleDbContext();

            Teacher t = myDbContext.GetObject<Teacher>("T2");

            myDbContext.Lock(t);
            t.Salary += 10;
            myDbContext.Save(ref t);
            myDbContext.ReleaseLock(t);
        }
    }
}
