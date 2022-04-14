using SWE3.Demo.Test;

namespace SWE3.Demo.Show
{
    public class SampleDbContext : DbContextBase
    {
        public SampleDbContext(string connectionString = "Data Source=V17Test.db;Version=3;") : base(connectionString)
        {
        }
        public DataSet<Course> Courses { get { return Get<Course>(); } }
        public DataSet<Teacher> Teachers { get { return Get<Teacher>(); } }
        public DataSet<Student> Students { get { return Get<Student>(); } }
        public DataSet<Person> Persons { get { return Get<Person>(); } }

    }
}
