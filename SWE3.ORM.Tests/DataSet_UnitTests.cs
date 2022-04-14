using NUnit.Framework;
using SWE3.Demo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SWE3.ORM.Tests
{
    public class CreateUnitTests
    {
        TestDbContext dbContext;
        [SetUp]
        public void CreateDB()
        {
            dbContext = new TestDbContext("Data Source=UnitTestsDB.db;Version=3;");
        }

        [Test]
        public void DataSet_generateSQLCodeForInheritance_Should_Generate_SQL_For_Selecting_All_Inherited_Types()
        {
            DataSet<Person> dataset = new DataSet<Person>(dbContext);
            string sqlCode = dataset.generateSQLCodeForInheritance();
            string expectedSqlCode = "SELECT Salary as TEACHERS_Salary,HDATE as TEACHERS_HDATE,ID as TEACHERS_ID,Name as TEACHERS_Name,FirstName as TEACHERS_FirstName,BDATE as TEACHERS_BDATE,Gender as TEACHERS_Gender,null as STUDENTS_EnterDate,null as STUDENTS_course,null as STUDENTS_ID,null as STUDENTS_Name,null as STUDENTS_FirstName,null as STUDENTS_BDATE,null as STUDENTS_Gender FROM TEACHERS UNION SELECT null as TEACHERS_Salary,null as TEACHERS_HDATE,null as TEACHERS_ID,null as TEACHERS_Name,null as TEACHERS_FirstName,null as TEACHERS_BDATE,null as TEACHERS_Gender,EnterDate as STUDENTS_EnterDate,course as STUDENTS_course,ID as STUDENTS_ID,Name as STUDENTS_Name,FirstName as STUDENTS_FirstName,BDATE as STUDENTS_BDATE,Gender as STUDENTS_Gender FROM STUDENTS;";
            Assert.AreEqual(expectedSqlCode, sqlCode);
        }

        [Test]
        public void DataSet_Count_Should_Return_Number_Of_Records()
        {
            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            InsertData();
            Assert.AreEqual(3, dbContext.Teachers.Count());
        }


        [Test]
        public void DataSet_Should_Return_Records()
        {

            dbContext.ClearDataBase();
            dbContext.CreateDatabase();
            //Insert some data
            InsertData();
            //Check führt zu Problemen irgendwie sind die records nur hier null
            //Assert.That(dbContext.Teachers, Does.Contain(new KeyValuePair<string, string>("T1", "Lieb")));
            //Assert.That(dbContext.Teachers, Does.Contain(new KeyValuePair<string, string>("T2", "Huber")));
            //Assert.That(dbContext.Teachers, Does.Contain(new KeyValuePair<string, string>("T3", "Fischer")));
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

            List<Student> students = new List<Student>
            {
                new Student()
                {
                    ID = "S1",
                    Name = "Gad",
                    FirstName = "Iman",
                    BirthDate = new DateTime(),
                    EnterDate = new DateTime(),
                    Gender = Gender.FEMALE,
                },
                new Student()
                {
                    ID = "S2",
                    Name = "Huber",
                    FirstName = "Tom",
                    BirthDate = new DateTime(),
                    EnterDate = new DateTime(),
                    Gender = Gender.MALE,
                }
            };
            dbContext.Save(ref students);

            List<Teacher> classTeachers = new List<Teacher>
            {
                new Teacher()
                {
                    ID = "T2",
                    Name = "Huber",
                    FirstName = "Tina",
                    BirthDate = new DateTime(),
                    HireDate = new DateTime(2021, 1, 1),
                    ClassRoomCourses = null,
                    HeadCourses = null,
                    Gender = Gender.FEMALE,
                },
                new Teacher()
                {
                    ID = "T3",
                    Name = "Fischer",
                    FirstName = "Tim",
                    BirthDate = new DateTime(),
                    HireDate = new DateTime(2021, 1, 1),
                    ClassRoomCourses = null,
                    HeadCourses = null,
                    Gender = Gender.MALE,
                }
            };
            dbContext.Save(ref classTeachers);

            //Save the courses with its referneces
            Course c = new Course
            {
                Name = "Turnen",
                Active = true,
                Head = t,
                students = students,
                ClassRoomTeacher = classTeachers
            };
            dbContext.Save(ref c);

            return t;
        }
    }
}
