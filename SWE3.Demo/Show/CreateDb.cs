using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Show
{
    class CreateDb
    {
        public static void Show()
        {
            SampleDbContext emptyWorld = new SampleDbContext("Data Source=V17Test.db;Version=3;");
            emptyWorld.CreateDatabase();
        }
    }
}
