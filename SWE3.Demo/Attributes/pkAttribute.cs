using System;



namespace SWE3.Demo
{
    /// <summary>This attribute marks a property as a primary key field.</summary>
    public class pkAttribute : fieldAttribute
    {
        public pkAttribute()
        {
            AutoIncrementIsSet = false;
        }

        public pkAttribute(bool autoincrement)
        {
            this.AutoIncrement = autoincrement;
            this.AutoIncrementIsSet = true;
        }

        public bool AutoIncrement { get; set; }
        public bool AutoIncrementIsSet { get; set; }
    }
}
