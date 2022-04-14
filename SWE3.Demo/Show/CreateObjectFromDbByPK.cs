using SWE3.Demo.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Show
{
    public static class CreateObjectFromDbByPK
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Implements the demonstration.</summary>
        public static void Show()
        {
            Console.WriteLine("\n--------- CreateObjectFromDbByPK ---------");
            SampleDbContext myDbContext = new SampleDbContext();
            Teacher t = myDbContext.GetObject<Teacher>("T1");
            Console.WriteLine(t.ID + " => " + t.Name);
        }
    }
}
