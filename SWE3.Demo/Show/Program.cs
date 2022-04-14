using SWE3.Demo.ExpressionVisitors;
using SWE3.Demo.FluentApi;
using SWE3.Demo.Test;
using System;
using System.Linq.Expressions;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SWE3.Demo.Show
{
    class Program
    {
        /// <summary>Main entry point.</summary>
        /// <param name="args">Arguments</param>
        static void Main(string[] args)
        {
            CreateDb.Show();
            InsertNewObject.Show();
            DataFromTableMetaData.Show();
            CreateObjectFromDbByPK.Show();
            ChangeTracking.Show();
            UseCaching.Show();
            UpdateObject.Show();
            TestInheritanceSupport.Show();
            SQLToFrameworkObjects.Show();
            DeleteObject.Show();
            LockObject.Show();
            Console.ReadKey();
        }
    }
}
