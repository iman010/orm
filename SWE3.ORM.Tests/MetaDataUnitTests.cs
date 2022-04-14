using NUnit.Framework;
using SWE3.Demo;
using SWE3.Demo.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SWE3.Demo.Field;

namespace SWE3.ORM.Tests
{
    public class MetaDataUnitTests
    {
        TestDbContext emptyWorld;
        [SetUp]
        public void CreateDB()
        {
            emptyWorld = new TestDbContext(null);
        }

        [Test]
        public void InitMetaData_Should_Set_InversePropertyName() {
            var entity = emptyWorld.tableMetaDataCache._Entities
                 .Where(x => x.Key.Name.Equals("Course"))
                 .FirstOrDefault().Value;
            Assert.IsNotNull(entity);
            var field = entity.Fields.Where(x => x.FieldMember.Name.Equals("ClassRoomTeacher")).FirstOrDefault();
            Assert.AreEqual("ClassRoomCourses", field.InversePropertyName);
        }

        [Test]
        public void InitMetaData_Should_Set_ReferencedField()
        {
             var entity = emptyWorld.tableMetaDataCache._Entities
                 .Where(x => x.Key.Name.Equals("Course"))
                 .FirstOrDefault().Value;
            var field = entity.Fields
                        .Where(x => x.FieldMember.Name.Equals("ClassRoomTeacher"))
                        .FirstOrDefault();
            var otherentity = emptyWorld.tableMetaDataCache._Entities
                              .Where(x => x.Key.Name.Equals("Teacher"))
                              .FirstOrDefault().Value;
            var referencedfield = otherentity.Fields
                                .Where(x => x.FieldMember.Name.Equals("ClassRoomCourses"))
                                .FirstOrDefault();
            Assert.AreEqual(referencedfield, field.ReferencedField);
        }

        [Test]
        public void InitMetaData_Should_Set_JoinTableName_On_ManyToMany()
        {
            var entity = emptyWorld.tableMetaDataCache._Entities
                 .Where(x => x.Key.Name.Equals("Course"))
                 .FirstOrDefault().Value;
            var field = entity.Fields
                        .Where(x => x.FieldMember.Name.Equals("ClassRoomTeacher"))
                        .FirstOrDefault();
            var otherentity = emptyWorld.tableMetaDataCache._Entities
                              .Where(x => x.Key.Name.Equals("Teacher"))
                              .FirstOrDefault().Value;
            var referencedfield = otherentity.Fields
                                .Where(x => x.FieldMember.Name.Equals("ClassRoomCourses"))
                                .FirstOrDefault();
            Assert.AreEqual("COURSES_ClassRoomTeacher_TEACHERS_ClassRoomCourses", field.JoinTableName);
            Assert.AreEqual("COURSES_ClassRoomTeacher_TEACHERS_ClassRoomCourses", referencedfield.JoinTableName);
        }

        [Test]
        public void InitMetaData_Should_Set_Relationtype_SimpleType()
        {
            var entity = emptyWorld.tableMetaDataCache._Entities
                 .Where(x => x.Key.Name.Equals("Course"))
                 .FirstOrDefault().Value;
            var field = entity.Fields
                        .Where(x => x.FieldMember.Name.Equals("ID"))
                        .FirstOrDefault();
            Assert.AreEqual(RelationEnumeration.SimpleType, field.RelationType);
        }

        [Test]
        public void InitMetaData_Should_Set_Relationtype_OneToMany()
        {
            var entity = emptyWorld.tableMetaDataCache._Entities
                 .Where(x => x.Key.Name.Equals("Course"))
                 .FirstOrDefault().Value;
            var field = entity.Fields
                        .Where(x => x.FieldMember.Name.Equals("Head"))
                        .FirstOrDefault();
            Assert.AreEqual(RelationEnumeration.ManyToOne, field.RelationType);
        }

        [Test]
        public void InitMetaData_Should_Set_Relationtype_ManyToOne()
        {
            var entity = emptyWorld.tableMetaDataCache._Entities
                 .Where(x => x.Key.Name.Equals("Teacher"))
                 .FirstOrDefault().Value;
            var field = entity.Fields
                        .Where(x => x.FieldMember.Name.Equals("HeadCourses"))
                        .FirstOrDefault();
            Assert.AreEqual(RelationEnumeration.OneToMany, field.RelationType);
        }

        [Test]
        public void InitMetaData_Should_Set_Relationtype_ManyToMany()
        {
            var entity = emptyWorld.tableMetaDataCache._Entities
                 .Where(x => x.Key.Name.Equals("Course"))
                 .FirstOrDefault().Value;
            var field = entity.Fields
                        .Where(x => x.FieldMember.Name.Equals("ClassRoomTeacher"))
                        .FirstOrDefault();
            Assert.AreEqual(RelationEnumeration.ManyToMany, field.RelationType);
        }

        [Test]
        public void TableMetaData_Should_Throw_Exception_For_Not_MapAble_Types_Of_Properties()
        {
            var ex = Assert.Throws<NotSupportedException>(() => new TableMetaData(typeof(NotMapable_Course)));
            Assert.That(ex.Message, Is.EqualTo("Type System.Collections.Generic.Dictionary`2[System.Int32,System.String] isn't supported."));
        }

        [Test]
        public void TableMetaData_Should_Throw_Exception_On_Missing_PK()
        {
            var ex = Assert.Throws<InvalidPrimaryKeysException>(() => new TableMetaData(typeof(MissingPK_Course)));
            Assert.That(ex.Message, Is.EqualTo("Class MissingPK_Course has no primary key. Each class has to have one primary key."));
        }

        [Test]
        public void TableMetaData_Should_Throw_Exception_For_Missing_Virtual_On_ICollection()
        {
            var ex = Assert.Throws<Exception>(() => new TableMetaData(typeof(MissingVirtual_Course)));
            Assert.That(ex.Message, Is.EqualTo("Property OnlineTeacher in class MissingVirtual_Course has to be virtual."));
        }
    }
}
