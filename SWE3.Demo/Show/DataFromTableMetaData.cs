using SWE3.Demo.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Show
{
    public static class DataFromTableMetaData
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Implements the demonstration.</summary>
        public static void Show()
        {
            SampleDbContext myDbContext = new SampleDbContext();
            TableMetaData ent = myDbContext.tableMetaDataCache._GetTableMetadata(typeof(Teacher));

            Console.WriteLine("\n--------- DataFromTableMetaData ---------");
            Console.WriteLine("Entity Type: " + ent.EntityType);
            Console.WriteLine("Entity TableName: " + ent.TableName);
            Console.WriteLine("Entity Fields: ");
            foreach (Field i in ent.Fields)
            {
                Console.Write(i.FieldMember.Name + " => " + ent.TableName + "." + i.ColumnName);
                if (i.IsPrimaryKey) { Console.Write(" (pk)"); }
                Console.Write("\n");
            }

        }
    }
}
