using System;
using System.Collections.Generic;
using SWE3.Demo.Test;



namespace SWE3.Demo.Show
{
    /// <summary>This implementation shows how to create framework objects from native SQL.</summary>
    public static class SQLToFrameworkObjects
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Implements the demonstration.</summary>
        public static void Show()
        {
            Console.WriteLine("\n--------- SQLToFrameworkObjects ---------");
      
            SampleDbContext w = new SampleDbContext();
            foreach (Teacher i in w.Teachers)
            {
               Console.WriteLine(i.Name);
            }
            Console.WriteLine();

            Course c = w.Courses.MyFirstOrDefault(x => x.Head.ID == "T1");
            int id2 = c.ID;

            Console.WriteLine("All Courses with id = 1");
            foreach (Course i in w.Courses.MyWhere(x => x.ID == 1))
            {
                Console.WriteLine(i.ID + ": [" + i.Name + "]");
            }
            Console.WriteLine();

            Console.WriteLine("All Teachers with id = T1");
            foreach (Teacher i in w.Teachers.MyWhere(x => x.ID == "T1"))
            {
                Console.WriteLine(i.ID + ": [" + i.Name + "]");
            }
            Console.WriteLine();

            Console.WriteLine("Teachers count");
            Console.WriteLine(w.Teachers.Count());
            Console.WriteLine();

            Console.WriteLine("Teachers count with id = T1 (variable in lambda instead constant T1)");
            string id = "T1";
            Console.WriteLine(w.Teachers.MyWhere(x => x.ID == id).Count());
            Console.WriteLine();

            Console.WriteLine("Teachers count with id = T1 (constant in lambda for T1)");
            Console.WriteLine(w.Teachers.MyWhere(x => x.ID == "T1").Count());
            Console.WriteLine();

            Console.WriteLine("Teachers count with hiredate year 2021");
            Console.WriteLine(w.Teachers.MyWhere(x => x.HireDate.Year == 2021).Count());
            Console.WriteLine();

            Console.WriteLine("Teachers count with hiredate year 2015");
            Console.WriteLine(w.Teachers.MyWhere(x => x.HireDate.Year == 2015).Count());
            Console.WriteLine();

            Console.WriteLine("Get all courses that \"T1\" teaches");
            foreach (Course i in w.Courses.MyWhere(x => x.Head.ID == "T1"))
            {
                Console.WriteLine(i.ID + ": [" + i.Name + "]");
            }
        }
    }
}
