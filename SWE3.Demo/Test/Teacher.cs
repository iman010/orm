using SWE3.Demo.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWE3.Demo.Test
{
    /// <summary>This is a teacher implementation (from School example).</summary>
    [entity(TableName = "TEACHERS")]
    public class Teacher : Person
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets the teacher's salary.</summary>
        public int Salary { get; set; }

        [field(ColumnName = "HDATE")]
        /// <summary>Gets or sets the teacher's hire date.</summary>
        public DateTime HireDate { get; set; }

        [InverseProperty("ClassRoomTeacher")]
        public virtual DataSet<Course> ClassRoomCourses { get; set; }

        [InverseProperty("Head")]
        public virtual DataSet<Course> HeadCourses { get; set; }
    }
}