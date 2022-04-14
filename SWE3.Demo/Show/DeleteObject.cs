using SWE3.Demo.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Show
{
    public class DeleteObject
    {
        public static void Show()
        {
            SampleDbContext myDbContext = new SampleDbContext();
            myDbContext.CreateDatabase();
            Teacher t = myDbContext.GetObject<Teacher>("T3");
            Console.WriteLine("\n--------- DeleteObject ---------");
            Console.WriteLine(t.ID + " => " + t.FirstName + " " + t.Name);
            myDbContext.Delete(t);
            try
            {
                Teacher t1 = myDbContext.GetObject<Teacher>("T3");
            }
            catch (Exception ex)
            { 
                if(ex.Message.Equals("No data."))
                {
                    Console.WriteLine("Deleted object with ID " + t.ID + " successfully");
                }
            }
        }
    
    }
}
