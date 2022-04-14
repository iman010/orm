using SWE3.Demo.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SWE3.Demo.Extensions;
using System.Data;

namespace SWE3.Demo
{
    public class ModelToSqlDDL
    {

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public ModelToSqlDDL(DbContextBase ctx)
        {
            this.ctx = ctx;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>Creates the database from the dbcontext.</summary>
        /// <param name="dbContext">DbContextBase.</param>
        /// <returns>SQL string.</returns>
        public string CreateDB(DbContextBase dbContext)
        {
            AllTableTypes.Clear();
            var tp = dbContext.GetType();
            var properties = tp.GetProperties();
            string SQL = "";
            foreach (var item in properties)
            {
                if (item.PropertyType.GetNameWithoutGenericInformation() == "DataSet")
                {
                    Type type = item.PropertyType.GenericTypeArguments[0]; //target type
                    if (!type.GetTypeInfo().IsAbstract)
                    {
                        AllTableTypes.Add(type);
                        HandleNestedObjects(type);
                        foreach (Type tabletype in AllTableTypes)
                        {
                            SQL += CreateDatabaseSqlCommandForTables(tabletype);
                        }
                    }
                }
            }
            return SQL;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Iterates over the nested classes in the RootQuery class and adds them to the hashset.</summary>
        /// <param name="tb">Type.</param>
        internal void HandleNestedObjects(Type tb)
        {
            var itemEntity = ctx.tableMetaDataCache._GetTableMetadata(tb);
            foreach (var field in itemEntity.Fields)
            {
                if (field.FieldType.IsSimpleType() == false && typeof(IEnumerable).IsAssignableFrom(field.FieldType) == false)
                {
                    //var e = _GetEntity(Activator.CreateInstance(field.ColumnType));
                    AllTableTypes.Add(field.ColumnType);
                    HandleNestedObjects(field.ColumnType);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType) == true && field.FieldType.IsSimpleType() == false)
                {
                    Type type = field.FieldType.GenericTypeArguments[0];
                    AllTableTypes.Add(type);
                }
            }
        }

        /// <summary>Retrieves the class name that has to be created and is responsible for overall creation process</summary>
        /// <param name="dbContext">DbContextBase.</param>
        /// <returns>string.</returns>

        /// <summary>Generates the SQL for normal (not custom) properties and adds it to the pk and fk SQL.</summary>
        /// <param name="propertyType">Type.</param>
        /// <returns>string.</returns>
        private string CreateDatabaseSqlCommandForTables(Type propertyType)
        {
            var itemEntity = ctx.tableMetaDataCache._GetTableMetadata(propertyType);
            string createManySQL = "";
            string createSQL = @"CREATE TABLE IF NOT EXISTS """ + itemEntity.TableName + @""" (";
            foreach (var field in itemEntity.Fields)
            {
                if (field.FieldType.IsSimpleType())
                {
                    //Normal properties
                    if (typeToDBType.ContainsKey(field.ColumnType.ToString()))
                    {
                        createSQL += @"""" + field.ColumnName + @""" ";

                        string dbColumnType = typeToDBType[field.ColumnType.ToString()];
                        if(dbColumnType.Equals("INTEGER") && field.AutoIncrement == false)
                        {
                            //Because SQLITE only applies AUTOINCREMENT to columns declared as INTEGER PRIMARY KEY not INT PRIMARY KEY
                            dbColumnType = "INT";
                        }
                        createSQL += dbColumnType;
                        
                        if (Attribute.IsDefined(field.FieldMember, typeof(requiredAttribute)))
                        {
                            createSQL += " NOT NULL ";
                        }

                        if (field.IsPrimaryKey)
                        {
                            createSQL += " PRIMARY KEY ";
                        }
                        createSQL += ",";
                    }
                    else if (field.FieldType.IsEnum)
                    {
                        //Enums handlen
                        createSQL += @"""" + field.ColumnName + @""" ";
                        createSQL += typeToDBType["System.Int32"] + ",";
                    }
                    else
                        throw new NotImplementedException();
                }
                else if (field.RelationType == Field.RelationEnumeration.ManyToMany)
                {
                    createManySQL += HandleManyToManyRelations(field, propertyType);
                }
            }

            createSQL += GetForeignKeys(propertyType);
            //createSQL += GetPrimaryKey(itemEntity);
            //Beistrich am Ende entfernen
            createSQL = createSQL.Remove(createSQL.Length - 1);
            createSQL += ");";
            createSQL += createManySQL;
            return createSQL;
        }

        /// <summary>Generates the SQL for the join table of two properties.</summary>
        /// <param name="enumerableField">Field</param>
        /// <param name="entityType">Type</param>
        /// <returns>string.</returns>
        private string HandleManyToManyRelations(Field enumerableField, Type entityType)
        {
            Type enumerableType = enumerableField.FieldType.GenericTypeArguments[0];
            var itemEntity = ctx.tableMetaDataCache._GetTableMetadata(enumerableType);
            foreach (var field in itemEntity.Fields)
            {
                // @TODO: outsource logic to Field class
                if (typeof(IEnumerable).IsAssignableFrom(field.FieldType) == true && field.FieldType.IsSimpleType() == false)
                {
                    Type currentFieldType = field.FieldType.GenericTypeArguments[0];
                    var inversePropertyFieldName = field.ReferencedField.FieldMember.Name;
                    if (currentFieldType.Equals(entityType) == true && field.ReferencedField != null && inversePropertyFieldName.Equals(enumerableField.ColumnName))
                    {
                        string tablename = enumerableField.JoinTableName;
                        string createSQL = @"CREATE TABLE IF NOT EXISTS """ + tablename + @""" (";
                        string fkSQL = "";
                        string pkSQL = "PRIMARY KEY(";
                        foreach (var pk in enumerableField.Entity.PrimaryKeys)
                        {
                            string columnName = enumerableField.Entity.TableName + "_" + pk.ColumnName;
                            createSQL += @"""" + columnName + @""" ";
                            createSQL += typeToDBType[pk.ColumnType.ToString()];
                            if (pk.IsRequired)
                            {
                                createSQL += " NOT NULL ";
                            }
                            createSQL += ",";
                            pkSQL += columnName + ",";
                            fkSQL += @",FOREIGN KEY(""" + columnName + @""") REFERENCES """ + enumerableField.Entity.TableName + @"""(""" + pk.ColumnName + @""")";
                        }
                        foreach (var pk in itemEntity.PrimaryKeys)
                        {
                            string columnName = itemEntity.TableName + "_" + pk.ColumnName;
                            createSQL += @"""" + columnName + @""" ";
                            createSQL += typeToDBType[pk.ColumnType.ToString()];
                            if (pk.IsRequired)
                            {
                                createSQL += " NOT NULL ";
                            }
                            createSQL += ",";
                            pkSQL += columnName + ",";
                            fkSQL += @",FOREIGN KEY(""" + columnName + @""") REFERENCES """ + pk.Entity.TableName + @"""(""" + pk.ColumnName + @""")";
                        }
                        pkSQL = pkSQL.Remove(pkSQL.Length - 1);
                        pkSQL += ")";
                        createSQL += pkSQL + fkSQL + ");";
                        return createSQL;
                    }
                }
            }
            // @TODO: handle ManyToOne Relations
            return "";
        }

        /// <summary>Generates the SQL for the foreignkeys for the current class.</summary>
        /// <param name="PropertyType">Type.</param>
        /// <returns>string.</returns>
        private string GetForeignKeys(Type PropertyType)
        {
            var tbm = ctx.tableMetaDataCache._GetTableMetadata(PropertyType);
            var props = PropertyType.GetProperties().Where(prop => prop.GetCustomAttribute(typeof(ignoreAttribute)) == null);
            props = props.Where(prop => tbm.Fields.Where(x => x.FieldMember.Name.Equals(prop.Name)).FirstOrDefault().RelationType == Field.RelationEnumeration.ManyToOne)
                        .Where(prop => !prop.PropertyType.IsSimpleType());

            if (props.Count() > 0)
            {
                string constraintsSQL = "";
                string columnSQL = "";
                foreach (var p in props)
                {
                    var refTableMetaData = ctx.tableMetaDataCache._GetTableMetadata(PropertyType);
                    var currentTable_fk = refTableMetaData.GetFieldInfoByMemberName(p.Name).ColumnName;
                    var referncedTable = refTableMetaData.GetFieldInfoByMemberName(p.Name).ReferencedField.Entity;
                    var referencedTable_pk = referncedTable.PrimaryKeys[0].ColumnName;
                    var referencedTable_name = referncedTable.TableName;

                    columnSQL += @"""" + currentTable_fk + @""" ";
                    columnSQL += typeToDBType[referencedTable_pk.GetType().ToString()] + ",";
                    constraintsSQL += @"FOREIGN KEY(""" + currentTable_fk + @""") REFERENCES """ + referencedTable_name + @"""(""" + referencedTable_pk + @"""),";
                }
                columnSQL += constraintsSQL;
                return columnSQL;
            }
            else
            {
                //throw new NotImplementedException();
                return "";
            }
        }

        /// <summary>Generates the SQL for the primarykeys for the current class.</summary>
        /// <param name="itemEntity">Entity</param>
        /// <returns>string.</returns>
        private string GetPrimaryKey(TableMetaData itemEntity)
        {
            string pkSQL = "";
            if (itemEntity.PrimaryKeys.Length > 0)
            {
                pkSQL += "PRIMARY KEY(";
                foreach (var pk in itemEntity.PrimaryKeys)
                {
                    pkSQL += @"""" + pk.ColumnName + @""",";
                }
                pkSQL = pkSQL.Remove(pkSQL.Length - 1);
                pkSQL += "),";
            }
            return pkSQL;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public HashSet<Type> AllTableTypes { get; set; } = new HashSet<Type>();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly Dictionary<string, string> typeToDBType = new Dictionary<string, string>()
        {
            {"System.Int16", "INTEGER" },
            {"System.Int32", "INTEGER" },
            {"System.Int64", "INTEGER" },
            {"System.SByte", "INTEGER" },
            {"System.Byte", "INTEGER" },
            {"System.Boolean", "boolean" },
            {"System.DateTime", "datetime" },
            {"System.DateTimeOffset", "datetimeoffset" },
            {"System.Decimal", "NUMERIC" },
            {"System.Double", "REAL" },
            {"System.Guid", "GUID" },
            {"System.Byte[]", "BLOB" },
            {"System.Single", "REAL" },
            {"System.String", "TEXT" },
            {"System.Char", "TEXT" },
            {"System.TimeSpan", "time" }
        };
        private readonly DbContextBase ctx;
    }
}
