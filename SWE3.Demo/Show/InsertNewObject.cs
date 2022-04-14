using SWE3.Demo.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.Show
{
    public class InsertNewObject
    {
        public static void Show()
        {
            SampleDbContext myDbContext = new SampleDbContext();
            myDbContext.CreateDatabase();
            //Save all refernces of course object
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
            myDbContext.Save(ref t);

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
            myDbContext.Save(ref students);

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
            myDbContext.Save(ref classTeachers);

            //Save the courses with its referneces
            Course c = new Course
            {
                Name = "Turnen",
                Active = true,
                Head = t,
                students = students,
                ClassRoomTeacher = classTeachers
            };
            myDbContext.Save(ref c);
        }
    }
}
