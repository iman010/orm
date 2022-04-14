using SWE3.Demo.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWE3.Demo.Test
{
    /// <summary>This is a teacher implementation (from School example).</summary>
    [entity(TableName = "STUDENTS")]
    public class Student : Person
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Gets or sets the teacher's hire date.</summary>
        public DateTime EnterDate { get; set; }

        [InverseProperty("students")]
        public virtual Course course { get; set; }
    }
}