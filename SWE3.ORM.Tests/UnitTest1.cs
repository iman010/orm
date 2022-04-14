using NUnit.Framework;
using SWE3.Demo;
using System.Linq;

namespace SWE3.ORM.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TableMetaData_Should_Get_TableName()
        {
            var tmd = TableMetaDataCache._GetTableMetadata(new Teacher());
            Assert.AreEqual("TEACHERS", tmd.TableName);
        }

        [Test]
        public void TableMetaData_Should_Get_PrimaryKeys()
        {
            var tmd = TableMetaDataCache._GetTableMetadata(new Teacher());
            Assert.AreEqual("ID", tmd.PrimaryKeys[0].ColumnName);
        }

        [Test]
        public void TableMetaData_Should_Get_EntityFields()
        {
            var tmd = TableMetaDataCache._GetTableMetadata(new Teacher());
            Assert.AreEqual(9, tmd.Fields.Length);
        }

        [Test]
        public void TableMetaData_Should_Get_Field_ColumnData()
        {
            var tmd = TableMetaDataCache._GetTableMetadata(new Teacher());
            Assert.That(tmd.Fields[0].ColumnType.ToString(), Does.Contain("Int"));
            Assert.AreEqual(tmd.Fields[0].ColumnName, "Salary");
        }

        [Test]
        public void TableMetaData_Should_Get_EntityType()
        {
            var tmd = TableMetaDataCache._GetTableMetadata(new Teacher());
            var t = new Teacher();
            Assert.AreEqual(tmd.Fields[0].Entity.EntityType, t.GetType());
        }

        [Test]
        public void ModelToSqlDDL_HandleNestedObjects_Should_Add_All_Nested_Objects()
        {
            ModelToSqlDDL translateModelToSqlDDL = new ModelToSqlDDL();
            var c = new Course();
            translateModelToSqlDDL.HandleNestedObjects(c.GetType());
            Assert.That(translateModelToSqlDDL.AllTableTypes, Does.Contain(new Teacher().GetType()));
            Assert.That(translateModelToSqlDDL.AllTableTypes, Does.Contain(new Course().GetType()));
        }

        [Test]
        public void ModelToSqlDDL_GetPrimaryKey_Should_Return_PK_SQL()
        {
            ModelToSqlDDL translateModelToSqlDDL = new ModelToSqlDDL();
            var c = new Course();
            var c_entity = TableMetaDataCache._GetTableMetadata(c);
            Assert.AreEqual(translateModelToSqlDDL.GetPrimaryKey(c_entity), "PRIMARY KEY(\"ID\"),");
        }

        [Test]
        public void ModelToSqlDDL_GetForeignKeys_Should_Return_FK_SQL()
        {
            ModelToSqlDDL translateModelToSqlDDL = new ModelToSqlDDL();
            var c = new Course();
            Assert.AreEqual(translateModelToSqlDDL.GetForeignKeys(c.GetType()), "\"ProfessorId\" TEXT,FOREIGN KEY(\"ProfessorId\") REFERENCES \"TEACHERS\"(\"ID\"),");
        }

        [Test]
        public void ModelToSqlDDL_HandleManyToManyRelationships_Should_Return_SQL_For_JoiningTable()
        {
            ModelToSqlDDL translateModelToSqlDDL = new ModelToSqlDDL();
            var c = new Course();
            var c_field = TableMetaDataCache._GetTableMetadata(c).Fields.Where(f => f.ColumnName.Equals("OnlineTeacher")).FirstOrDefault();
            Assert.AreEqual(translateModelToSqlDDL.HandleManyToManyRelations(c_field, c.GetType()),
                "CREATE TABLE IF NOT EXISTS \"COURSES_OnlineTeacher_TEACHERS_OnlineCourses\" " +
                 "(\"COURSES_ID\" TEXT,\"TEACHERS_ID\" TEXT,PRIMARY KEY(COURSES_ID,TEACHERS_ID)," + 
                 "FOREIGN KEY(\"COURSES_ID\") REFERENCES \"COURSES\"(\"ID\"),FOREIGN KEY(\"TEACHERS_ID\") REFERENCES \"COURSES\"(\"ID\"));"
            );
        }

        [Test]
        public void ModelToSqlDDL_CreateDatabaseSqlCommandForTables_Should_Return_SQL_For_All_Tables()
        {

            ModelToSqlDDL translateModelToSqlDDL = new ModelToSqlDDL();
            var c = new Course();
            Assert.AreEqual(translateModelToSqlDDL.CreateDatabaseSqlCommandForTables(c.GetType()),
                "CREATE TABLE IF NOT EXISTS \"COURSES\" " +
                "(\"ID\" TEXT,\"HACTIVE\" INTEGER,\"Name\" TEXT NOT NULL ,\"ProfessorId\" TEXT," + 
                "FOREIGN KEY(\"ProfessorId\") REFERENCES \"TEACHERS\"(\"ID\")," + 
                "PRIMARY KEY(\"ID\"));" + 
                
                "CREATE TABLE IF NOT EXISTS \"COURSES_OnlineTeacher_TEACHERS_OnlineCourses\" " + 
                "(\"COURSES_ID\" TEXT,\"TEACHERS_ID\" TEXT,PRIMARY KEY(COURSES_ID,TEACHERS_ID)," + 
                "FOREIGN KEY(\"COURSES_ID\") REFERENCES \"COURSES\"(\"ID\")," + 
                "FOREIGN KEY(\"TEACHERS_ID\") REFERENCES \"COURSES\"(\"ID\"));" +
                
                "CREATE TABLE IF NOT EXISTS \"COURSES_ClassRoomTeacher_TEACHERS_ClassRoomCourses\" " + 
                "(\"COURSES_ID\" TEXT,\"TEACHERS_ID\" TEXT,PRIMARY KEY(COURSES_ID,TEACHERS_ID)," + 
                "FOREIGN KEY(\"COURSES_ID\") REFERENCES \"COURSES\"(\"ID\")," + 
                "FOREIGN KEY(\"TEACHERS_ID\") REFERENCES \"COURSES\"(\"ID\"));"
            );
        }
    }
}