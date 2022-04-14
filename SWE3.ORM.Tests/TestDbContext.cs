using SWE3.Demo;
using SWE3.Demo.Test;

namespace SWE3.ORM.Tests
{
    public class TestDbContext : DbContextBase
    {
        public TestDbContext(string connectionString = "") : base(connectionString)
        {
        }
        public DataSet<Course> Courses { get { return Get<Course>(); } }
        public DataSet<Teacher> Teachers { get { return Get<Teacher>(); } }
        public DataSet<Student> Students { get { return Get<Student>(); } }
        public DataSet<Person> Persons { get { return Get<Person>(); } }
    }
}
