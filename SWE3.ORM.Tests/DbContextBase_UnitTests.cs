using NUnit.Framework;
using SWE3.Demo.Show;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SWE3.ORM.Tests
{
    public class DbContextBase_UnitTests
    {
        TestDbContext dbContext;
        [SetUp]
        public void CreateDB()
        {
            dbContext = new TestDbContext("Data Source=UnitTestsDB.db;Version=3;");
        }

        [Test]
        public void DbContextBase_Should_CreateDB()
        {
            dbContext.CreateDatabase();
            IDbCommand cmd = dbContext.Connection.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
            IDataReader rd = cmd.ExecuteReader();
            List<string> tablenames = new List<string>();
            while (rd.Read())
            {
                tablenames.Add(rd.GetString(0));
            }
            Assert.That(tablenames, Does.Contain("TEACHERS")); 
            Assert.That(tablenames, Does.Contain("STUDENTS"));
            Assert.That(tablenames, Does.Contain("COURSES"));
            Assert.That(tablenames, Does.Contain("COURSES_ClassRoomTeacher_TEACHERS_ClassRoomCourses"));
        }

        [Test]
        public void DbContextBase_Should_ClearDB()
        {
            dbContext.CreateDatabase();
            dbContext.ClearDataBase();

            IDbCommand cmd = dbContext.Connection.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
            IDataReader rd = cmd.ExecuteReader();
            List<string> tablenames = new List<string>();
            while (rd.Read())
            {
                tablenames.Add(rd.GetString(0));
            }
            Assert.AreEqual(0, tablenames.Count());
        }

        [Test]
        public void DbContextBase_Should_Insert_M_To_N()
        {
            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            Teacher t = InsertData();

            //Check if database contains data
            IDbCommand cmd = dbContext.Connection.CreateCommand();
            cmd.CommandText = "SELECT * from COURSES_ClassRoomTeacher_TEACHERS_ClassRoomCourses;";
            IDataReader rd = cmd.ExecuteReader();
            List<KeyValuePair<int, string>> data = new List<KeyValuePair<int, string>>();
            while (rd.Read())
            {
               data.Add(new KeyValuePair<int,string>(rd.GetInt32(0), rd.GetString(1)));
            }
            Assert.That(data, Does.Contain(new KeyValuePair<int,string>(1, "T2")));
            Assert.That(data, Does.Contain(new KeyValuePair<int, string>(1, "T3")));
        }


        [Test]
        public void DbContextBase_Should_Insert_One_To_Many()
        {
            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            Teacher t = InsertData();

            //Check if database contains data
            IDbCommand cmd = dbContext.Connection.CreateCommand();
            cmd.CommandText = "SELECT * from STUDENTS;";
            IDataReader rd = cmd.ExecuteReader();
            List<KeyValuePair<string,string>> data = new List<KeyValuePair<string, string>>();
            while (rd.Read())
            {
                data.Add(new KeyValuePair<string, string>(rd.GetValue(rd.GetOrdinal("ID")).ToString(), rd.GetValue(rd.GetOrdinal("course")).ToString()));
            }
            List<KeyValuePair<string, string>> expected = new List<KeyValuePair<string, string>>();
            expected.Add(new KeyValuePair<string, string>("S1", "1"));
            expected.Add(new KeyValuePair<string, string>("S2", "1"));
            Assert.AreEqual(expected, data);
        }


        [Test]
        public void DbContextBase_Should_Insert_Many_To_One()
        {
            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            Teacher t = InsertData();

            //Check if database contains data
            IDbCommand cmd = dbContext.Connection.CreateCommand();
            cmd.CommandText = "SELECT * from COURSES;";
            IDataReader rd = cmd.ExecuteReader();
            List<KeyValuePair<int, string>> data = new List<KeyValuePair<int, string>>();
            while (rd.Read())
            {
                data.Add(new KeyValuePair<int, string>(rd.GetInt32(rd.GetOrdinal("ID")), rd.GetValue(rd.GetOrdinal("ProfessorId")).ToString()));
            }
            Assert.That(data, Does.Contain(new KeyValuePair<int,string>(1, "T1")));
        }

        [Test]
        public void DbContextBase_Should_Insert_SimpleType_Data()
        {
            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            Teacher t = InsertData();

            //Check if database contains data
            IDbCommand cmd = dbContext.Connection.CreateCommand();
            cmd.CommandText = "SELECT * from COURSES;";
            IDataReader rd = cmd.ExecuteReader();
            List<KeyValuePair<int, string>> data = new List<KeyValuePair<int, string>>();
            while (rd.Read())
            {
                data.Add(new KeyValuePair<int, string>(rd.GetInt32(rd.GetOrdinal("HACTIVE")), rd.GetValue(rd.GetOrdinal("Name")).ToString()));
            }
            Assert.That(data, Does.Contain(new KeyValuePair<int, string>(1, "Turnen")));
        }

        [Test]
        public void DbContextBase_Should_Update_Data()
        {
            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            Teacher t = InsertData();

            //das führt noch zu Problemen
            //c.Name = "NeuesTurnen";
            //dbContext.Save(ref c);

            //Check if database contains data
            IDbCommand cmd = dbContext.Connection.CreateCommand();
            cmd.CommandText = "SELECT * from COURSES;";
            IDataReader rd = cmd.ExecuteReader();
            List<KeyValuePair<int, string>> data = new List<KeyValuePair<int, string>>();
            while (rd.Read())
            {
                data.Add(new KeyValuePair<int, string>(rd.GetInt32(rd.GetOrdinal("HACTIVE")), rd.GetValue(rd.GetOrdinal("Name")).ToString()));
            }
            Assert.That(data, Does.Contain(new KeyValuePair<int, string>(1, "Turnen")));
        }

        [Test]
        public void DbContextBase_Should_Lock_Object()
        {
            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            Teacher t = InsertData();

            //LOck object
            dbContext.Lock(t);

            //Check if table with locked object contains t
            IDbCommand cmd = dbContext.Connection.CreateCommand();
            cmd.CommandText = "SELECT * from LOCKS;";
            IDataReader rd = cmd.ExecuteReader();
            List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();
            while (rd.Read())
            {
                data.Add(new KeyValuePair<string, string>(rd.GetString(rd.GetOrdinal("Type_KEY")), rd.GetString(rd.GetOrdinal("OBJECT_ID"))));
            }
            Assert.That(data, Does.Contain(new KeyValuePair<string, string>("Teacher", "T1")));
        }

        [Test]
        public void DbContextBase_Should_Release_Lock_On_Object()
        {
            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            Teacher t = InsertData();

            //LOck object
            dbContext.Lock(t);
            dbContext.ReleaseLock(t);

            //Check if table with locked object contains t
            IDbCommand cmd = dbContext.Connection.CreateCommand();
            cmd.CommandText = "SELECT * from LOCKS;";
            IDataReader rd = cmd.ExecuteReader();
            List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();
            while (rd.Read())
            {
                data.Add(new KeyValuePair<string, string>(rd.GetString(rd.GetOrdinal("Type_KEY")), rd.GetString(rd.GetOrdinal("OBJECT_ID"))));
            }
            Assert.That(data, Does.Not.Contain(new KeyValuePair<string, string>("Teacher", "T1")));
        }

        [Test]
        public void DbContextBase_Should_Get_Object_By_PK()
        {
            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            Teacher t = InsertData();

            Teacher returnedTeacher = dbContext.GetObject<Teacher>("T1");
            Assert.AreEqual(t, returnedTeacher);
        }
        
        [Test]
        public void DbContextBase_Should_Track_Changes()
        {
            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            Teacher t = InsertData();

            Teacher returnedTeacher = dbContext.GetObject<Teacher>("T1");
            t.Salary += 20;

            Assert.AreEqual(true, dbContext.HasChanged(t));
        }

        public Teacher InsertData()
        {
            Teacher t = new Teacher()
            {
                ID = "T1",
                Name = "Lieb",
                FirstName = "Anna",
                BirthDate = new DateTime(),
                HireDate = new DateTime(2015, 1, 1),
                Gender = Gender.FEMALE,
                Salary = 3000,
                HeadCourses = null
            };
            dbContext.Save(ref t);

            List<Student> students = new List<Student>();
            students.Add(new Student()
            {
                ID = "S1",
                Name = "Gad",
                FirstName = "Iman",
                BirthDate = new DateTime(),
                EnterDate = new DateTime(),
                Gender = Gender.FEMALE,
            });
            students.Add(new Student()
            {
                ID = "S2",
                Name = "Huber",
                FirstName = "Tom",
                BirthDate = new DateTime(),
                EnterDate = new DateTime(),
                Gender = Gender.MALE,
            });
            dbContext.Save(ref students);

            List<Teacher> classTeachers = new List<Teacher>();
            classTeachers.Add(new Teacher()
            {
                ID = "T2",
                Name = "Huber",
                FirstName = "Tina",
                BirthDate = new DateTime(),
                HireDate = new DateTime(2021, 1, 1),
                ClassRoomCourses = null,
                HeadCourses = null,
                Gender = Gender.FEMALE,
            });
            classTeachers.Add(new Teacher()
            {
                ID = "T3",
                Name = "Fischer",
                FirstName = "Tim",
                BirthDate = new DateTime(),
                HireDate = new DateTime(2021, 1, 1),
                ClassRoomCourses = null,
                HeadCourses = null,
                Gender = Gender.MALE,
            });
            dbContext.Save(ref classTeachers);

            //Save the courses with its referneces
            Course c = new Course();
            c.Name = "Turnen";
            c.Active = true;
            c.Head = t;
            c.students = students;
            c.ClassRoomTeacher = classTeachers;
            dbContext.Save(ref c);

            return t;
        }
    }
}
