using SWE3.Demo.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Show
{
    public class UpdateObject
    {

        public static void Show() {
            SampleDbContext myDbContext = new SampleDbContext();

            Console.WriteLine("\n--------- UpdateObject ---------");
            Teacher t = myDbContext.GetObject<Teacher>("T1");
            Console.WriteLine(t.ID + " => " + "Old name: " + t.Name);

            t.Name = "Lehner";
            myDbContext.Save(ref t);

            Console.WriteLine(t.ID + " => " + "New name: " + t.Name);
        }
    }
}
