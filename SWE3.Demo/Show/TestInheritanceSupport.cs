using SWE3.Demo.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Show
{
    public class TestInheritanceSupport
    {
        public static void Show()
        {
            SampleDbContext w = new SampleDbContext();
            Console.WriteLine("\n--------- TestInheritanceSupport ---------");

            Console.WriteLine("Get first person with name Huber:");
            Person p = w.Persons.MyFirstOrDefault(x => x.Name == "Huber");
            Console.WriteLine(p.ID + " => " + p.FirstName + " " + p.Name);
            Console.WriteLine();

            Console.WriteLine("Get all people with name Huber:");
            foreach (Person i in w.Persons.MyWhere(x => x.Name == "Huber"))
            {
                Console.WriteLine(i.ID + ": [" + i.FirstName + " " + i.Name + "]");
            }
            Console.WriteLine();

            Console.WriteLine("Get all people (all students and teachers):");
            foreach (Person i in w.Persons)
            {
                Console.WriteLine(i.ID + ": [" + i.FirstName + " " + i.Name + "]");
            }
        }
    }
}
