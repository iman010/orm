using SWE3.Demo;
using SWE3.Demo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SWE3.ORM.Tests
{
    /// <summary>This class represents a course in the school model.</summary>
    [entity(TableName = "COURSES")]
    public class Course
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets the class ID.</summary>
        [pk(true)]
        public int ID { get; set; }

        /// <summary>Gets or sets the active flag.</summary>
        [field(ColumnName = "HACTIVE", ColumnType = typeof(int))]
        public bool Active { get; set; }

        /// <summary>Course name.</summary>
        [required]
        public string Name { get; set; }

        [InverseProperty("ClassRoomCourses")]
        public virtual DataSet<Teacher> ClassRoomTeacher { get; set; }

        [InverseProperty("HeadCourses")]
        [field("ProfessorId")]
        public virtual Teacher Head { get; set; }

        [InverseProperty("course")]
        public virtual DataSet<Student> students { get; set; }
    }
}
