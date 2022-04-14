using SWE3.Demo.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Show
{
    /// <summary>This class shows change tracking.</summary>
    public static class ChangeTracking
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Implements the demonstration.</summary>
        public static void Show()
        {
            SampleDbContext myDbContext = new SampleDbContext();

            Console.WriteLine("\n--------- ChangeTracking ---------");

            Teacher t = myDbContext.GetObject<Teacher>("T1");
            Console.WriteLine(t.FirstName);

            Console.WriteLine("Hash for teacher with id " + t.ID + ": " + myDbContext.GetHash(t));

            bool xxx = myDbContext.HasChanged(t);
            Console.WriteLine("Teacher with id " + t.ID + " has changed after getting from db: " + xxx);

            t.Salary += 20;
            bool xxy = myDbContext.HasChanged(t);
            Console.WriteLine("Teacher with id " + t.ID + " has changed after changing salary: " + xxy);

            myDbContext.Save(ref t);
            bool xxz = myDbContext.HasChanged(t);
            Console.WriteLine("Teacher with id " + t.ID + " has changed after saving changes to db: " + xxz);
        }
    }
}
