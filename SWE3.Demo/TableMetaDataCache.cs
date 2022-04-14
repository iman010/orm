using SWE3.Demo.Attributes;
using SWE3.Demo.Exceptions;
using SWE3.Demo.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SWE3.Demo
{
    /// <summary>
    /// Represents a class for caching table meta data.
    /// </summary>
    internal class TableMetaDataCache
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="types">IReadOnlyCollection<Type>.</param>
        public TableMetaDataCache(IReadOnlyCollection<Type> types)
        {
            if (types is null)
            {
                throw new ArgumentNullException(nameof(types));
            }
            initMetaData(types);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private methods                                                                                          //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
       
        /// <summary>Sets Metadata (Relationtype, JoinTableName, ReferencedField, InverseProperty) for all properties in all entities.</summary>
        /// <param name="types">IReadOnlyCollection<Type>.</param>
        private void initMetaData(IReadOnlyCollection<Type> types)
        {
            types.ToList().ForEach(x =>
            {
                _Entities.Add(x, new TableMetaData(x));
            });
            //RelationType und (JoinTableName und InverseProeprty) zuweisen
            foreach (var entity in _Entities)
            {
                var props = entity.Value.Fields.Where(prop => prop.InversePropertyName != null);
                foreach (var p in props)
                {
                    var fieldType = p.FieldType;

                    TableMetaData otherentity;
                    Field inverseProperty = null;
                    if (typeof(IEnumerable).IsAssignableFrom(fieldType))
                    {
                        otherentity = _Entities.Where(x => x.Key.Equals(fieldType.GenericTypeArguments[0])).FirstOrDefault().Value;
                        if (otherentity != null)
                        {
                            inverseProperty = otherentity.Fields.Where(f => f.FieldMember.Name == p.InversePropertyName).FirstOrDefault();
                        }
                        else
                        {
                            throw new ArgumentException($"Cant find entity with name {fieldType.GenericTypeArguments[0]}.");
                        }
                    }
                    else
                    {
                        otherentity = _Entities.Where(x => x.Key.Equals(fieldType)).FirstOrDefault().Value;
                        if (otherentity != null)
                        {
                            inverseProperty = otherentity.Fields.Where(f => f.ColumnName == p.InversePropertyName).FirstOrDefault();
                        }
                    }

                    var prop = otherentity.EntityType.GetProperties().Where(property => property.Name == p.InversePropertyName).FirstOrDefault();

                    if (prop == null)
                    {
                        throw new InversePropertyException($"Can't set inverseproperty for property {p.FieldMember.Name} " +
                                                           $"because property with that name " +
                                                           $"doesn't exist " +
                                                           $"in class {fieldType.GenericTypeArguments[0]}.");
                    }
                    else if (Attribute.IsDefined(prop, typeof(InversePropertyAttribute)) == false)
                    {
                        throw new InversePropertyException($"Can't set inverseproperty for property {p.FieldMember.Name} " +
                                                           $"because inverse property attribute isn't set for property {prop.Name} " +
                                                           $"in class {fieldType.GenericTypeArguments[0]}.");
                    }
                    else if (p.FieldType.IsSimpleType())
                    {
                        throw new InversePropertyException($"Can't set inverseproperty for property {p.FieldMember.Name} " +
                                                           $"because inverse property isn't an enumerable or object.");
                    }

                    p.ReferencedField = inverseProperty;

                    // many to many relation
                    if (typeof(IEnumerable).IsAssignableFrom(fieldType) && typeof(IEnumerable).IsAssignableFrom(inverseProperty.FieldType))
                    {
                        p.RelationType = Field.RelationEnumeration.ManyToMany;
                        /*
                         * The name of the join table contains the name of the two tables and corresponding properties.
                         * I wanted the names of the two tables in the join table name to be alphabetically sorted to 
                         * always have the same name regardless of which table/entity is being handled currently. 
                         * First the two table names are compared to see which one is earlier in the alphabet and then that one is put at the beginning of the name. 
                         * Entity: A -> Property: id1
                         * Entity: B -> Property: id2
                         * Possible name: A_id1_B_id2
                         * Possible name: B_id2_A_id1
                         * -> Chosen name: A_id1_B_id2 => because 'A' < 'B'
                        */
                        var m = otherentity.TableName.CompareTo(entity.Value.TableName);
                        if (m == -1)
                        {
                            string joinTable = otherentity.TableName + "_" + p.InversePropertyName + "_" + entity.Value.TableName + "_" + p.FieldMember.Name;
                            p.JoinTableName = joinTable;
                        }
                        else if (m == 1)
                        {
                            string joinTable = entity.Value.TableName + "_" + p.ColumnName + "_" + otherentity.TableName + "_" + p.InversePropertyName;
                            p.JoinTableName = joinTable;
                        }
                        else
                        {
                            throw new NotImplementedException("Reference to own table not possible.");
                        }
                    }
                    else if (!typeof(IEnumerable).IsAssignableFrom(fieldType) && typeof(IEnumerable).IsAssignableFrom(inverseProperty.FieldType))
                    {
                        p.RelationType = Field.RelationEnumeration.ManyToOne;
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(fieldType) && !typeof(IEnumerable).IsAssignableFrom(inverseProperty.FieldType))
                    {
                        p.RelationType = Field.RelationEnumeration.OneToMany;
                    }
                }
                props = entity.Value.Fields.Where(prop => prop.FieldType.IsSimpleType() == true);

                foreach (var p in props)
                {
                    _Entities.Where(x => x.Key.Name == entity.Key.Name).FirstOrDefault().Value.Fields.Where(f => f.FieldMember.Name == p.FieldMember.Name).FirstOrDefault().RelationType = Field.RelationEnumeration.SimpleType;
                }
            }
        }

        /// <summary>
        /// Returns the table meta data for the given object. If the item is not in the cache it will be computed and stored in the cache.
        /// </summary>
        /// <param name="o">object</param>
        /// <returns>meta data</returns>
        internal TableMetaData _GetTableMetadata(object o)
        {
            Type t = null;
            if (o is Type)
            {
                t = (Type)o;
            }
            else { t = o.GetType(); }

            if (_Entities.ContainsKey(t))
                return _Entities[t];
            else if (t.Name.Contains("Proxy"))
            {
                    TableMetaData ent = _Entities.Where(entity => entity.Key.Name.Equals(t.Name.Replace("Proxy", ""))).FirstOrDefault().Value;
                    return ent;
            }
            throw new ArgumentException("Type is unknown.");
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Entities.</summary>
        public Dictionary<Type, TableMetaData> _Entities = new Dictionary<Type, TableMetaData>();
    }
}