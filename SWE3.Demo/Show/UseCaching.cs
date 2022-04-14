using SWE3.Demo.Show;
using SWE3.Demo.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Show
{
    public class UseCaching
    {
        public static void Show()
        {
            Console.WriteLine("\n--------- UseCaching ---------");
            SampleDbContext myDbContext = new SampleDbContext();
            myDbContext.CachingEnabled = false;
            Teacher t0 = myDbContext.GetObject<Teacher>("T1");
            Teacher t1 = myDbContext.GetObject<Teacher>("T1");
            Teacher t2 = myDbContext.GetObject<Teacher>("T1");

            Console.WriteLine("Caching disabled:");
            Console.WriteLine("t1 => " + t0.InstanceNumber.ToString());
            Console.WriteLine("t1 => " + t1.InstanceNumber.ToString());
            Console.WriteLine("t2 => " + t2.InstanceNumber.ToString());

            myDbContext.CachingEnabled = true;

            t0 = myDbContext.GetObject<Teacher>("T1");
            t1 = myDbContext.GetObject<Teacher>("T1");
            t2 = myDbContext.GetObject<Teacher>("T1");

            Console.WriteLine();
            Console.WriteLine("Caching enabled:");
            Console.WriteLine("t0 => " + t0.InstanceNumber.ToString());
            Console.WriteLine("t1 => " + t1.InstanceNumber.ToString());
            Console.WriteLine("t2 => " + t2.InstanceNumber.ToString());
        }
    }
}

