using SWE3.Demo.Attributes;
using SWE3.Demo.Exceptions;
using SWE3.Demo.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SWE3.Demo
{
    /// <summary>This class holds entity metadata.</summary>
    public sealed class TableMetaData
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="t">Type.</param>
        public TableMetaData(Type t)
        {

            entityAttribute tattr = (entityAttribute)t.GetCustomAttribute(typeof(entityAttribute));
            if ((tattr == null) || (string.IsNullOrWhiteSpace(tattr.TableName)))
            {
                TableName = t.Name;
            }
            else { TableName = tattr.TableName; }

            EntityType = t;
            List<Field> fields = new List<Field>();
            List<Field> pks = new List<Field>();

            foreach (PropertyInfo i in t.GetProperties())
            {
                //Check if type is supported in or mapper e.g. dictionaries aren't supported
                if (i.PropertyType.IsMapAbleType() == false)
                {
                    throw new NotSupportedException($"Type {i.PropertyType} isn't supported.");
                }

                if (i.PropertyType.ShouldTypeBeVirtual() == true && !i.GetAccessors()[0].IsVirtual)
                {
                    throw new Exception($"Property {i.Name} in class {t.Name} has to be virtual.");
                }
                if ((ignoreAttribute)i.GetCustomAttribute(typeof(ignoreAttribute)) != null)
                {
                    continue;
                }

                Field field = new Field(this);

                fieldAttribute fattr = (fieldAttribute)i.GetCustomAttribute(typeof(fieldAttribute));
                InversePropertyAttribute ipa = (InversePropertyAttribute)i.GetCustomAttribute(typeof(InversePropertyAttribute));
                if (fattr != null)
                {
                    field.ColumnName = (fattr?.ColumnName ?? i.Name);
                    field.ColumnType = (fattr?.ColumnType ?? i.PropertyType);

                    if (fattr is pkAttribute) { 
                        pks.Add(field);
                        field.IsPrimaryKey = true;
                        pkAttribute pkattr = (pkAttribute)i.GetCustomAttribute(typeof(pkAttribute));
                        if(field.ColumnType.IsInteger()){
                            if (pkattr.AutoIncrementIsSet == true)
                            {
                                field.AutoIncrement = pkattr.AutoIncrement;
                            }
                            else
                            {
                                field.AutoIncrement = true;
                            }
                        }else if(!field.ColumnType.IsInteger() && pkattr.AutoIncrement){
                            throw new InvalidPrimaryKeysException($"Can't set autoincrement for Primary key property {i.Name} of class {t.Name}. Autoincrement is only allowed for integers.");
                        }
                    }
                }
                else
                {
                    field.ColumnName = i.Name;
                    field.ColumnType = i.PropertyType;
                }
                if (ipa != null)
                {
                    field.InversePropertyName = ipa.inversePropertyName;
                }
                field.FieldMember = i;

                field.IsRequired = Attribute.IsDefined(i, typeof(requiredAttribute));
                fields.Add(field);
            }

            Fields = fields.ToArray();
            if(pks.Count == 1)
            {
                //Check primary key
                if(pks[0].FieldType.IsSimpleType() == false)
                {
                    throw new InvalidPrimaryKeysException($"{pks[0].FieldMember.Name} can't be a primary key because its of type is {pks[0].FieldType}." +
                                                          $" PK can't be an Enumerable or custom class");
                }
                PrimaryKeys = pks.ToArray();
            }else if(pks.Count < 1)
            {
                throw new InvalidPrimaryKeysException($"Class {t.Name} has no primary key. Each class has to have one primary key.");
            }
            else if(pks.Count > 1)
            {
                throw new InvalidPrimaryKeysException($"Class {t.Name} has {pks.Count} primary keys. Each class can only have one primary key.");
            }

        }

        /// <summary>Gets the entity SQL.</summary>
        /// <param name="prefix">Prefix.</param>
        /// <returns>SQL string.</returns>
        public string GetSQL(string prefix = null)
        {
            if (prefix == null) { prefix = ""; }
            string rval = "SELECT ";
            var fields = this.Fields.Where(f => f.RelationType == Field.RelationEnumeration.SimpleType || f.RelationType == Field.RelationEnumeration.ManyToOne).ToList();
            for (int i = 0; i < fields.Count; i++)
            {
                if (i > 0) { rval += ", "; }
                rval += prefix.Trim() + fields[i].ColumnName;
            }
            rval += (" FROM " + TableName);

            return rval;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the primary keys.</summary>
        public Field[] PrimaryKeys
        {
            get; internal set;
        }


        /// <summary>Gets or sets the entity type.</summary>
        public Type EntityType
        {
            get; set;
        }


        /// <summary>Gets the table name.</summary>
        public string TableName
        {
            get; private set;
        }

        public Field GetFieldInfoByMemberName(String fieldName)
        {
            return Fields.FirstOrDefault(x => x.FieldMember.Name.Equals(fieldName));
        }
        public Field GetFieldInfoByColumnName(String columnName)
        {
            return Fields.FirstOrDefault(x => x.ColumnName.Equals(columnName));
        }

        /// <summary>Gets the entity fields.</summary>
        public Field[] Fields
        {
            get; private set;
        }
    }
}
