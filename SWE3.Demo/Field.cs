using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;



namespace SWE3.Demo
{
    /// <summary>This class holds field metadata.</summary>
    public class Field
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="entity">Parent entity.</param>
        public Field(TableMetaData entity)
        {
            Entity = entity;
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the parent entity.</summary>
        public TableMetaData Entity
        {
            get; set;
        }


        /// <summary>Gets the type field..</summary>
        public MemberInfo FieldMember
        {
            get; internal set;
        }


        /// <summary>Gets the column name in table.</summary>
        public string ColumnName
        {
            get; internal set;
        }


        /// <summary>Gets the column database type.</summary>
        public Type ColumnType
        {
            get; internal set;
        }


        /// <summary>Gets the field type</summary>
        public Type FieldType
        {
            get
            {
                if (FieldMember is PropertyInfo) { return ((PropertyInfo)FieldMember).PropertyType; }

                throw new NotSupportedException("Member type not supported.");
            }
        }
        public String InversePropertyName { get; internal set; }
        public Boolean IsRequired { get; internal set; }
        public Field ReferencedField { get; internal set; }
        public enum RelationEnumeration { SimpleType, OneToMany, ManyToOne, ManyToMany }
        public RelationEnumeration RelationType { get; internal set; } // 0->simple type,1 -> one to many, 2 -> many to one, 3 -> many to many; // @TODO: Iman
        /// <summary>
        /// This is set only if the entity has a many to many relation to another entity.
        /// In such a case this value represents the table name of the join table.
        /// </summary>
        public string JoinTableName { get; set; }

        public bool AutoIncrement { get; set; }

        /// <summary>Gets if the column is a foreign key.</summary>
        public bool IsPrimaryKey
        {
            get; internal set;
        } = false;
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Returns a database column type equivalent for a field type value.</summary>
        /// <param name="value">Value.</param>
        /// <returns>Database type representation of the value.</returns>
        public object ToColumnType(object value)
        {
            if (FieldType == ColumnType) { return value; }

            if (value is bool)
            {
                if (ColumnType == typeof(int)) { return (((bool)value) ? 1 : 0); }
                if (ColumnType == typeof(short)) { return (short)(((bool)value) ? 1 : 0); }
                if (ColumnType == typeof(long)) { return (long)(((bool)value) ? 1 : 0); }
            }

            return value;
        }


        /// <summary>Returns a field type equivalent for a database column type value.</summary>
        /// <param name="value">Value.</param>
        /// <returns>Field type representation of the value.</returns>
        public object ToFieldType(object value)
        {
            if (FieldType == typeof(bool))
            {
                if (value is int) { return ((int)value != 0); }
                if (value is short) { return ((short)value != 0); }
                if (value is long) { return ((long)value != 0); }
            }

            if (FieldType == typeof(short)) { return Convert.ToInt16(value); }
            if (FieldType == typeof(int)) { return Convert.ToInt32(value); }
            if (FieldType == typeof(long)) { return Convert.ToInt64(value); }
            if (FieldType == typeof(DateTime)) { return Convert.ToDateTime(value); }

            if (FieldType.IsEnum) { return Enum.ToObject(FieldType, value); }

            return value;
        }


        /// <summary>Gets the field value.</summary>
        /// <param name="obj">Object.</param>
        public object GetValue(object obj)
        {
            if (FieldMember is PropertyInfo) { return ((PropertyInfo)FieldMember).GetValue(obj); }

            throw new NotSupportedException("Member type not supported.");
        }


        /// <summary>Sets the field value.</summary>
        /// <param name="obj">Object.</param>
        /// <param name="value">Value.</param>
        public void SetValue(object obj, object value)
        {
            if (FieldMember is PropertyInfo) { ((PropertyInfo)FieldMember).SetValue(obj, value); return; }

            throw new NotSupportedException("Member type not supported.");
        }

        // <summary>Sets the field value.</summary>
        /// <param name="obj">Object.</param>
        /// <param name="value">Value.</param>
        /// <param name="objects">Cached objects.</param>
        public void SetValue(object obj, object value,DbContextBase dbcontext, ICollection<object> objects = null)
        {
            if (FieldMember is PropertyInfo)
            {
                    if (RelationType == RelationEnumeration.ManyToMany  || RelationType == RelationEnumeration.OneToMany)
                    {
                        Type innerType = FieldType.GetGenericArguments()[0];
                        string sql = dbcontext.tableMetaDataCache._GetTableMetadata(innerType).GetSQL() + " WHERE " + ColumnName + " = :fk";
                        Tuple<string, object>[] parameters = new Tuple<string, object>[] { new Tuple<string, object>(":fk", Entity.PrimaryKeys[0].ToFieldType(value)) };
                        object rval;

                        if (this.RelationType == Field.RelationEnumeration.ManyToMany)
                        {
                            string RemoteColumName = Entity.TableName + "_" + Entity.PrimaryKeys[0].ColumnName;
                            sql = dbcontext.tableMetaDataCache._GetTableMetadata(innerType).GetSQL("T.") + " T WHERE EXISTS (SELECT * FROM " + JoinTableName + " X " +
                                                                    "WHERE X." + RemoteColumName + " = T." + dbcontext.tableMetaDataCache._GetTableMetadata(innerType).PrimaryKeys[0].ColumnName + " AND " +
                                                                    "X." + ColumnName + " = :fk)";
                        }

                        if (FieldType.Name.Contains("DataSet"))
                        {
                            Type genericDsType = typeof(DataSet<>).MakeGenericType(FieldType.GenericTypeArguments[0]);
                            rval = Activator.CreateInstance(genericDsType, dbcontext);
                            //rval = Activator.CreateInstance(FieldType, sql, parameters);
                        }
                        else
                        {
                            rval = Activator.CreateInstance(FieldType);
                           dbcontext._FillList(innerType, rval, sql, parameters, objects);
                        }
                        ((PropertyInfo)FieldMember).SetValue(obj, rval);
                    }
                    else
                    {
                        if (FieldType.Name.Contains("DataSet"))
                        {
                            Type innerType = FieldType.GetGenericArguments()[0];
                            ((PropertyInfo)FieldMember).SetValue(obj, Activator.CreateInstance(FieldType, dbcontext.tableMetaDataCache._GetTableMetadata(innerType).PrimaryKeys[0].ToFieldType(value)));
                        }
                        else
                        {
                            if (value.GetType() != FieldType) { value = dbcontext._CreateObject(FieldType, new object[] { dbcontext.tableMetaDataCache._GetTableMetadata(FieldType).PrimaryKeys[0].ToFieldType(value) }, objects); }
                            ((PropertyInfo)FieldMember).SetValue(obj, value);
                        }
                    }
                }
                else { ((PropertyInfo)FieldMember).SetValue(obj, value); }

                return;
            
        }
    }
}
