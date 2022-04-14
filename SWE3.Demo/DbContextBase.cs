using SWE3.Demo.Exceptions;
using SWE3.Demo.Extensions;
using SWE3.Demo.Proxies;
using SWE3.Demo.Test;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SWE3.Demo
{

    /// <summary>This class grants access to the demo framework.</summary>
    public class DbContextBase : IDisposable
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Constructos                                                                                               //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public DbContextBase(String connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                Connection = new SQLiteConnection(connectionString);
                Connection.Open();
            }
            tableMetaDataCache = new TableMetaDataCache(getAllModelTypes().ToList().AsReadOnly());
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Locks an object.</summary>
        /// <param name="obj">Object.</param>
        /// <exception cref="ObjectLockedException">Thrown when the object is locked.</exception>
        public void Lock(object obj)
        {
            IDbCommand cmd = this.Connection.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS LOCKS ( " +
                                "OWNER_KEY TEXT NOT NULL, " +
                                "TYPE_KEY TEXT NOT NULL, " +
                                "OBJECT_ID TEXT NOT NULL," +
                                "LOCKEDAT DATETIME DEFAULT (datetime(CURRENT_TIMESTAMP, 'localtime'))); " +
                              "CREATE UNIQUE INDEX IF NOT EXISTS UQ_LOCKS ON LOCKS(TYPE_KEY, OBJECT_ID);";
            cmd.ExecuteNonQuery();
            string owner = IsLockedBy(obj);
            if (owner != null && owner != OwnerKey) { throw new ObjectLockedException(owner); }
            cmd.CommandText = "INSERT INTO LOCKS (OWNER_KEY, TYPE_KEY, OBJECT_ID) VALUES (:own, :typ, :obj)";

            IDataParameter p = cmd.CreateParameter();
            p.ParameterName = ":own";
            p.Value = OwnerKey;
            cmd.Parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = ":typ";
            p.Value = obj.GetType().Name;
            cmd.Parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = ":obj";
            p.Value = tableMetaDataCache._GetTableMetadata(obj).PrimaryKeys[0].GetValue(obj).ToString();
            cmd.Parameters.Add(p);

            bool success = true;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                success = false;
            }
            cmd.Dispose();

            if (!success)
            {
                if (owner != null && owner != OwnerKey) { throw new ObjectLockedException(owner); }
            }
        }


        /// <summary>Returns the owner key that locks an object.</summary>
        /// <param name="obj">Object.</param>
        /// <returns>Returns TRUE if the object is locked (by another owner), otherwise returns NULL.</returns>
        public string IsLockedBy(object obj)
        {
            IDbCommand cmd = Connection.CreateCommand();
            //First delete records for objects that were locked for at least 5 minutes and then get the owner key of the object
            cmd.CommandText = "DELETE FROM LOCKS WHERE LOCKEDAT <= datetime('now', '-5 minutes','localtime');" +
                              "SELECT OWNER_KEY FROM LOCKS WHERE TYPE_KEY = :typ AND OBJECT_ID = :obj";

            IDataParameter p = cmd.CreateParameter();
            p.ParameterName = ":typ";
            p.Value = obj.GetType().Name;
            cmd.Parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = ":obj";
            p.Value = tableMetaDataCache._GetTableMetadata(obj).PrimaryKeys[0].GetValue(obj).ToString();
            cmd.Parameters.Add(p);

            string rval = (string)cmd.ExecuteScalar();
            cmd.Dispose();

            return rval;
        }


        /// <summary>Returns the if an object is locked.</summary>
        /// <param name="obj">Object.</param>
        /// <returns>Returns TRUE if the object is locked (by another owner), otherwise returns NULL.</returns>
        public bool IsLocked(object obj)
        {
            return (IsLockedBy(obj) != OwnerKey);
        }


        /// <summary>Releases a lock on an object.</summary>
        /// <param name="obj">Object.</param>
        public void ReleaseLock(object obj)
        {
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = "DELETE FROM LOCKS WHERE OWNER_KEY = :own AND TYPE_KEY = :typ AND OBJECT_ID = :obj";

            IDataParameter p = cmd.CreateParameter();
            p.ParameterName = ":own";
            p.Value = OwnerKey;
            cmd.Parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = ":typ";
            p.Value = obj.GetType().Name;
            cmd.Parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = ":obj";
            p.Value = tableMetaDataCache._GetTableMetadata(obj).PrimaryKeys[0].GetValue(obj).ToString();
            cmd.Parameters.Add(p);

            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        /// <summary>Deletes an object.</summary>
        /// <param name="obj">Object.</param>
        public void Delete(object obj)
        {
            TableMetaData ent = tableMetaDataCache._GetTableMetadata(obj);

            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = ("DELETE FROM " + ent.TableName + " WHERE ");
            IDataParameter p;

            for (int i = 0; i < ent.PrimaryKeys.Length; i++)
            {
                if (i > 0) { cmd.CommandText += " AND "; }
                cmd.CommandText += (ent.PrimaryKeys[i].ColumnName + " = " + (":" + ent.PrimaryKeys[i].ColumnName.ToLower() + "v"));

                p = cmd.CreateParameter();
                p.ParameterName = (":" + ent.PrimaryKeys[i].ColumnName.ToLower() + "v");
                p.Value = ent.PrimaryKeys[i].ToColumnType(ent.PrimaryKeys[i].GetValue(obj));
                cmd.Parameters.Add(p);
            }

            cmd.ExecuteNonQuery();
            cmd.Dispose();
            _GetCache(obj.GetType()).Delete(ent.PrimaryKeys[0].GetValue(obj));
        }



        /// <summary>Clears the cache.</summary>
        public void ClearCache()
        {
            _Caches = new Dictionary<Type, Cache>();
        }

        /// <summary>
        /// Creates the database
        /// </summary>
        public void CreateDatabase()
        {
            ModelToSqlDDL translateModelToSqlDDL = new ModelToSqlDDL(this);
            IDbCommand cmd = this.Connection.CreateCommand();
            string sqlCmd = translateModelToSqlDDL.CreateDB(this);
            cmd.CommandText = sqlCmd;
            cmd.ExecuteNonQuery();
        }

        /// <summary>Creates an instance by its primary keys.</summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="pks">Primary keys.</param>
        /// <returns>Object.</returns>
        public T GetObject<T>(params object[] pks)
        {
            return (T)_CreateObject(typeof(T), pks);
        }

        /// <summary>Returns an array of instances for an SQL query.</summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="sql">SQL query.</param>
        /// <returns>Instances.</returns>
        public T[] FromSQL<T>(string sql)
        {
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = sql;
            IDataReader re = cmd.ExecuteReader();

            List<T> rval = new List<T>();
            _FillList<T>(rval, re);
            return rval.ToArray();
        }


        protected DataSet<T> Get<T>()
        {
            var metaData = tableMetaDataCache._GetTableMetadata(typeof(T));
            return new DataSet<T>(this, (pk) =>
            {
                String sqlCode = $"DELETE FROM {metaData.TableName} WHERE {metaData.PrimaryKeys[0].ColumnName} = {pk}";
                IDbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = sqlCode;
                // cmd.ExecuteNonQuery();
            });
        }

        /// <summary>Gets a hash for this object.</summary>
        /// <param name="obj">Object.</param>
        /// <returns>Hash.</returns>
        public string GetHash(object obj)
        {
            var metaData = tableMetaDataCache._GetTableMetadata(obj.GetType());
            var fields = metaData.Fields.Where(f => f.RelationType == Field.RelationEnumeration.SimpleType || f.RelationType == Field.RelationEnumeration.ManyToOne).ToList();
            string rval = "";
            foreach (Field i in fields) { rval += (i.ColumnName + "=" + i.GetValue(obj).ToString() + ";"); }

            return Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(rval)));
        }

        /// <summary>Gets if an object has changed.</summary>
        /// <param name="obj">Object.</param>
        /// <returns>Returns TRUE if the object has changes, otherwise returns FALSE.</returns>
        public bool HasChanged(object obj)
        {
            if (!CachingEnabled) return true;

            var metaData = tableMetaDataCache._GetTableMetadata(obj.GetType());

            string ch = _GetCache(obj.GetType()).GetHash(metaData.PrimaryKeys[0].GetValue(obj));
            string lh = GetHash(obj);

            return (_GetCache(obj.GetType()).GetHash(metaData.PrimaryKeys[0].GetValue(obj)) != GetHash(obj));
        }
        /// <summary>
        /// Clears all data and tables from database
        /// </summary>
        public void ClearDataBase()
        {
            string sql = @"PRAGMA writable_schema = 1;
            DELETE FROM sqlite_master;
            PRAGMA writable_schema = 0;
            VACUUM;
            PRAGMA integrity_check; ";
            IDbCommand cmd = this.Connection.CreateCommand();
            cmd.CommandText = sql;
            int affectedLines = cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserts or updates an item in database
        /// </summary>
        /// <param name="item">T</param>
        public void Save<T>(ref T item)
        {
            TableMetaData itemEnt;
            if (DynamicProxy.typeMapper.ContainsKey(item))
            {
                var type = DynamicProxy.typeMapper[item];
                itemEnt = tableMetaDataCache._GetTableMetadata(type);
            }
            else
            {
                itemEnt = tableMetaDataCache._GetTableMetadata(item.GetType());
            }

            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = ("INSERT OR REPLACE INTO " + itemEnt.TableName + " (");

            string insert = "";
            IDataParameter p;
            var fields = itemEnt.Fields;
            int i = 0;

            foreach (Field field in fields.Where(field => field.IsPrimaryKey == false && field.RelationType != Field.RelationEnumeration.ManyToMany && field.RelationType != Field.RelationEnumeration.OneToMany || field.IsPrimaryKey == true && field.AutoIncrement == false))
            {
                if (i > 0) { cmd.CommandText += ", "; insert += ", "; }
                cmd.CommandText += field.ColumnName;
                insert += (":" + field.ColumnName.ToLower() + "v");

                p = cmd.CreateParameter();
                p.ParameterName = (":" + field.ColumnName.ToLower() + "v");

                if (field.RelationType == Field.RelationEnumeration.SimpleType)
                {
                    p.Value = field.ToColumnType(field.GetValue(item));
                }
                else if (field.RelationType == Field.RelationEnumeration.ManyToOne)
                {
                    var entProp = item.GetType().GetProperties().Where(x => x.Name.Equals(field.FieldMember.Name)).FirstOrDefault();
                    var propvalue = entProp.GetValue(item);

                    if (propvalue != null)
                    {
                        TableMetaData innerEnt = tableMetaDataCache._GetTableMetadata(field.FieldType);
                        var innerEntProp = innerEnt.EntityType.GetProperties().Where(x => x.Name.Equals(innerEnt.PrimaryKeys[0].FieldMember.Name)).FirstOrDefault();
                        p.Value = innerEntProp.GetValue(propvalue);
                        var dataset = field.FieldType.GetProperty(field.InversePropertyName).GetValue(propvalue);
                        if (dataset == null)
                        {
                            var dsType = innerEnt.Fields.Where(f => f.FieldMember.Name.Equals(field.InversePropertyName)).FirstOrDefault().FieldType;
                            Type genericDsType = typeof(DataSet<>).MakeGenericType(dsType.GenericTypeArguments[0]);

                            Type genericListType = typeof(List<>).MakeGenericType(dsType.GenericTypeArguments[0]);
                            IList newInnerCache = Activator.CreateInstance(genericListType) as IList;
                            newInnerCache.Add(item);

                            var newDataset = Activator.CreateInstance(genericDsType, newInnerCache, this);
                            entProp.PropertyType.GetProperty(field.InversePropertyName).SetValue(propvalue, newDataset);
                        }
                        else
                        {
                            var field_dataset = field.GetValue(item);
                            var dsType = innerEnt.Fields.Where(f => f.FieldMember.Name.Equals(field.InversePropertyName)).FirstOrDefault().FieldType;
                            if (DynamicProxy.typeMapper.ContainsKey(field_dataset) == false)
                            {
                                IList innerCache = (IList)dsType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name.Equals("cache")).FirstOrDefault().GetValue(field_dataset);
                                if (innerCache == null)
                                {
                                    Type genericListType = typeof(List<>).MakeGenericType(dsType.GenericTypeArguments[0]);
                                    innerCache = Activator.CreateInstance(genericListType) as IList;
                                }
                                innerCache.Add(item);
                                field.FieldType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name.Equals("cache")).FirstOrDefault().SetValue(field_dataset, innerCache);
                                entProp.PropertyType.GetProperty(field.InversePropertyName).SetValue(propvalue, field_dataset);
                            }
                        }
                        item.GetType().GetProperty(field.FieldMember.Name).SetValue(item, propvalue);
                    }
                }
                cmd.Parameters.Add(p);
                i++;
            }

            cmd.CommandText += ") VALUES (" + insert + "); select last_insert_rowid();";

            if (itemEnt.PrimaryKeys[0].AutoIncrement == true)
            {
                int lastId = Convert.ToInt32(cmd.ExecuteScalar());
                item.GetType().GetProperty(itemEnt.PrimaryKeys[0].FieldMember.Name).SetValue(item, lastId);
            }
            else
            {
                cmd.ExecuteNonQuery();
            }

            foreach (Field field in fields.Where(field => field.RelationType == Field.RelationEnumeration.OneToMany))
            {
                if (field.GetValue(item) != null)
                {
                    TableMetaData listEnt = tableMetaDataCache._GetTableMetadata(field.FieldType.GenericTypeArguments[0]);
                    var dataset = field.GetValue(item);
                    IList list = (IList)dataset.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name.Equals("cache")).FirstOrDefault().GetValue(dataset);
                    var ty = field.FieldType.GenericTypeArguments[0];
                    var pro = ty.GetProperty(field.InversePropertyName);
                    var proName = listEnt.Fields.Where(f => f.FieldMember.Name.Equals(field.InversePropertyName)).FirstOrDefault().ColumnName;

                    if (DynamicProxy.typeMapper.ContainsKey(item))
                    {
                        var t = DynamicProxy.typeMapper[item];
                    }

                    if (list != null)
                    {
                        foreach (var l in list)
                        {
                            var entProp = item.GetType().GetProperties().Where(x => x.Name.Equals(itemEnt.PrimaryKeys[0].FieldMember.Name)).FirstOrDefault();
                            var propvalue = entProp.GetValue(item);
                            field.FieldType.GenericTypeArguments[0].GetProperty(pro.Name).SetValue(l, item);

                            var innerEntProp = listEnt.EntityType.GetProperties().Where(x => x.Name.Equals(listEnt.PrimaryKeys[0].FieldMember.Name)).FirstOrDefault();
                            var pkValue = innerEntProp.GetValue(l);
                            string sql = ("UPDATE " + listEnt.TableName + " SET " + proName + " = " + ":" + field.ColumnName.ToLower() + "newValue" + " WHERE " + listEnt.PrimaryKeys[0].ColumnName + " = " + ":" + field.ColumnName.ToLower() + "pk" + ";");
                            cmd.CommandText = sql;
                            cmd.Parameters.Clear();
                            p = cmd.CreateParameter();
                            p.ParameterName = (":" + field.ColumnName.ToLower() + "pk");
                            p.Value = pkValue;
                            cmd.Parameters.Add(p);
                            p = cmd.CreateParameter();
                            p.ParameterName = (":" + field.ColumnName.ToLower() + "newValue");
                            p.Value = propvalue;
                            cmd.Parameters.Add(p);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            HandleManyToMany(fields, ref item);
            cmd.Dispose();
            _GetCache(item.GetType())[itemEnt.PrimaryKeys[0].GetValue(item)] = item;
        }

        /// <summary>
        /// Inserts or updates a list of items
        /// </summary>
        /// <param name="mylist">List<T></param>
        public void Save<T>(ref List<T> mylist) // Save<T>
        {
            foreach (var item in mylist)
            {
                TableMetaData ent = tableMetaDataCache._GetTableMetadata(item.GetType());

                IDbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = ("INSERT OR REPLACE INTO " + ent.TableName + " (");

                string insert = "";
                IDataParameter p;
                var fields = ent.Fields;
                int i = 0;

                foreach (Field field in fields.Where(field => field.IsPrimaryKey == false && field.RelationType != Field.RelationEnumeration.ManyToMany && field.RelationType != Field.RelationEnumeration.OneToMany || field.IsPrimaryKey == true && field.AutoIncrement == false))
                {
                    if (i > 0) { cmd.CommandText += ", "; insert += ", "; }
                    cmd.CommandText += field.ColumnName;
                    insert += (":" + field.ColumnName.ToLower() + "v");

                    p = cmd.CreateParameter();
                    p.ParameterName = (":" + field.ColumnName.ToLower() + "v");

                    if (field.RelationType == Field.RelationEnumeration.SimpleType)
                    {
                        p.Value = field.ToColumnType(field.GetValue(item));
                    }
                    else if (field.RelationType == Field.RelationEnumeration.ManyToOne)
                    {
                        var entProp = item.GetType().GetProperties().Where(x => x.Name.Equals(field.FieldMember.Name)).FirstOrDefault();
                        var propvalue = entProp.GetValue(item);

                        if (propvalue != null)
                        {
                            TableMetaData innerEnt = tableMetaDataCache._GetTableMetadata(field.FieldType);
                            var innerEntProp = innerEnt.EntityType.GetProperties().Where(x => x.Name.Equals(innerEnt.PrimaryKeys[0].FieldMember.Name)).FirstOrDefault();
                            p.Value = innerEntProp.GetValue(propvalue);
                        }
                    }
                    cmd.Parameters.Add(p);
                    i++;
                }

                cmd.CommandText += ") VALUES (" + insert + "); select last_insert_rowid();";

                if (ent.PrimaryKeys[0].AutoIncrement == true)
                {
                    int lastId = Convert.ToInt32(cmd.ExecuteScalar());
                    item.GetType().GetProperty(ent.PrimaryKeys[0].FieldMember.Name).SetValue(item, lastId);
                }
                else
                {
                    cmd.ExecuteNonQuery();
                }
                cmd.Dispose();

                foreach (Field field in fields.Where(field => field.RelationType == Field.RelationEnumeration.OneToMany))
                {
                    if (field.GetValue(item) != null)
                    {
                        TableMetaData listEnt = tableMetaDataCache._GetTableMetadata(field.FieldType.GenericTypeArguments[0]);
                        IList list = (IList)field.GetValue(item);
                        var ty = field.FieldType.GenericTypeArguments[0];
                        var propo = ty.GetProperty(field.InversePropertyName);
                        foreach (var l in list)
                        {
                            var entProp = item.GetType().GetProperties().Where(x => x.Name.Equals(ent.PrimaryKeys[0].FieldMember.Name)).FirstOrDefault();
                            var propvalue = entProp.GetValue(item);
                            field.FieldType.GenericTypeArguments[0].GetProperty(propo.Name).SetValue(l, item);
                            cmd.CommandText = ("UPDATE " + listEnt.TableName + " SET (" + propo + ") WHERE " + listEnt.PrimaryKeys[0].ColumnName + " = " + propvalue + ";");
                        }
                    }
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates an object from a database reader.
        /// </summary>
        /// <param name="t">Type</param>
        /// <param name="pks">IEnumerable<object></param>
        /// <param name="objects">ICollection<object></param>
        /// <returns>T</returns>
        internal object _CreateObject(Type t, IEnumerable<object> pks, ICollection<object> objects = null)
        {
            TableMetaData ent = tableMetaDataCache._GetTableMetadata(t);

            IDbCommand cmd = Connection.CreateCommand();

            string query = ent.GetSQL() + " WHERE ";

            for (int i = 0; i < ent.PrimaryKeys.Length; i++)
            {
                if (i > 0) { query += " AND "; }
                query += (ent.PrimaryKeys[i].ColumnName + " = " + (":pkv_" + i.ToString()));

                IDataParameter p = cmd.CreateParameter();
                p.ParameterName = (":pkv_" + i.ToString());
                p.Value = pks.ElementAt(i);
                cmd.Parameters.Add(p);
            }
            cmd.CommandText = query;

            object rval = null;
            IDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                rval = _CreateObject(t, re, objects);
            }

            re.Close();
            cmd.Dispose();

            if (rval == null) { throw new Exception("No data."); }
            return rval;
        }

        /// <summary>Creates an object from a database reader.</summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="re">Reader.</param>
        /// <param name="objects">Cached objects.</param>
        /// <returns>Object.</returns>
        internal object _CreateObject(Type t, IDataReader re, ICollection<object> objects = null)
        {
            object rval = _GetCachedObject(t, re, objects);

            if (rval == null)
            {
                if (objects == null) { objects = new List<object>(); }
                objects.Add(rval = Activator.CreateInstance(t));
            }
            else if (HasChanged(rval)) { return rval; }

            TableMetaData tmd = tableMetaDataCache._GetTableMetadata(t);

            foreach (Field i in tmd.PrimaryKeys)
            {
                i.SetValue(rval, i.ToFieldType(re.GetValue(re.GetOrdinal(i.ColumnName))));
            }

            foreach (Field i in tmd.Fields)
            {
                if (!i.IsPrimaryKey)
                {
                    if (i.RelationType == Field.RelationEnumeration.OneToMany || i.RelationType == Field.RelationEnumeration.ManyToMany)
                    {
                        i.SetValue(rval, i.ToFieldType(re.GetValue(re.GetOrdinal(i.Entity.PrimaryKeys[0].ColumnName))), this, objects);
                    }
                    else
                    {
                        i.SetValue(rval, i.ToFieldType(re.GetValue(re.GetOrdinal(i.ColumnName))), this, objects);
                    }
                }
            }

            _GetCache(t)[tmd.PrimaryKeys[0].GetValue(rval)] = rval;
            return rval;
        }

        /// <summary>Searches the cached objects for an object and returns it.</summary>
        /// <param name="t">Type.</param>
        /// <param name="re">Reader.</param>
        /// <param name="objects">Cached objects.</param>
        /// <returns>Returns the cached object that matches the current reader or NULL if no such object has been found.</returns>
        private object _GetCachedObject(Type t, IDataReader re, ICollection<object> objects)
        {
            TableMetaData tmd = tableMetaDataCache._GetTableMetadata(t);
            if (objects != null)
            {
                foreach (object i in objects)
                {
                    if (i.GetType() != t) continue;

                    bool found = true;
                    foreach (Field k in tmd.PrimaryKeys)
                    {
                        if (!k.GetValue(i).Equals(k.ToFieldType(re.GetValue(re.GetOrdinal(k.ColumnName))))) { found = false; break; }
                    }
                    if (found) { return i; }
                }
            }

            Field pk = tmd.PrimaryKeys[0];
            return _GetCache(t)[pk.ToFieldType(re.GetValue(re.GetOrdinal(pk.ColumnName)))]; ;
        }


        /// <summary>Creates an object from a database reader.</summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="re">Reader.</param>
        /// <returns>Object.</returns>
        internal Object _CreateObject(Type type, IDataReader re)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            object rval = dynamicProxyGenerator.CreateDynamicProxy(type, propertyName =>
            {
                return createDynamicProxyObject(propertyName, type, values);
            });
            foreach (Field i in tableMetaDataCache._GetTableMetadata(type).Fields)
            {
                if (i.FieldType.IsSimpleType())
                {
                    // TODO: Implement if datatype is DateTime
                    i.SetValue(rval, i.ToFieldType(re.GetValue(re.GetOrdinal(i.ColumnName))));
                }
            }
            for (int i = 0; i < re.FieldCount; i++)
            {
                string columnName = re.GetName(i);
                if (values.ContainsKey(columnName)) continue;
                object val = re.GetValue(i);
                values.Add(columnName, val);
            }
            return rval;
        }

        /// <summary>
        /// Returns also nested objects
        /// </summary>
        /// <returns></returns>
        private IList<Type> getAllModelTypes()
        {
            Queue<Type> queue = new Queue<Type>();
            var tp = this.GetType();
            var properties = tp.GetProperties();
            foreach (var item in properties)
            {
                if (item.PropertyType.GetNameWithoutGenericInformation() == "DataSet")
                {
                    Type type = item.PropertyType.GenericTypeArguments[0];
                    queue.Enqueue(type);
                }
            }
            return queue.ToList();
        }

        /// <summary>Creates a dynamic proxy object from a database reader.</summary>
        /// <param name="propertyName">string</param>#
        /// <param name="type">Type</param>
        /// <param name="values">Dictionary<string, object>.</param>
        /// <returns>Object.</returns>
        private Object createDynamicProxyObject(String propertyName, Type type, Dictionary<string, object> values)
        {
            TableMetaData currentClassMetaData = tableMetaDataCache._GetTableMetadata(type);
            Field currentColumn = currentClassMetaData.GetFieldInfoByMemberName(propertyName);

            Field referencedColumn = currentColumn.ReferencedField;
            TableMetaData referencedTableMetaData = referencedColumn.Entity;

            if (currentColumn.RelationType == Field.RelationEnumeration.OneToMany)
            {
                // creates list
                var returnType = referencedColumn.Entity.EntityType;
                // create query
                string pkColumnName = currentClassMetaData.PrimaryKeys[0].ColumnName;
                string sqlCmd = $"Select * FROM {referencedTableMetaData.TableName} WHERE {referencedColumn.ColumnName} = '{values[pkColumnName]}'";
                var nType = typeof(DataSet<>).MakeGenericType(returnType);

                Action<object> deleteCmd = a =>
                {
                    string deleteSqlCmd = $"DELETE FROM {referencedTableMetaData.TableName} WHERE {referencedTableMetaData.PrimaryKeys[0]} = {a}";
                    IDbCommand cmd = this.Connection.CreateCommand();
                    cmd.CommandText = sqlCmd;
                    cmd.ExecuteNonQuery();
                };

                return Activator.CreateInstance(nType, new object[] { this, deleteCmd });
            }
            else if (currentColumn.RelationType == Field.RelationEnumeration.ManyToOne)
            {
                string pk = currentColumn.ColumnName;
                string pkReferencedTableColumnName = referencedTableMetaData.PrimaryKeys[0].ColumnName;
                string sqlCmd = $"Select * FROM {referencedTableMetaData.TableName} WHERE {pkReferencedTableColumnName} = '{values[pk]}'";
                IDbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = sqlCmd;
                IDataReader rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    object c = _CreateObject(currentColumn.FieldType, rd);
                    return c;
                }
                return null; // no referenced item found in db
            }
            else if (currentColumn.RelationType == Field.RelationEnumeration.ManyToMany)
            {
                String tableName = currentColumn.JoinTableName;
                string pk = currentClassMetaData.PrimaryKeys[0].ColumnName;
                var returnType = referencedColumn.Entity.EntityType;

                string sqlCmd = $"Select b.* FROM {currentClassMetaData.TableName} a,{referencedTableMetaData.TableName} b,{tableName} c WHERE " +
                $"c.{currentClassMetaData.TableName}_{pk} = '{values[pk]}' AND " +
                $"a.{pk} = c.{currentClassMetaData.TableName}_{pk} AND " +
                $"b.{referencedTableMetaData.PrimaryKeys[0].ColumnName} = c.{referencedTableMetaData.TableName}_{referencedTableMetaData.PrimaryKeys[0].ColumnName}";

                var nType = typeof(DataSet<>).MakeGenericType(returnType);
                Action<object> deleteCmd = a =>
                {
                    string deleteSqlCmd = $"DELETE FROM {currentColumn.JoinTableName} WHERE " +
                    $"{currentClassMetaData.TableName}_{currentClassMetaData.PrimaryKeys[0].ColumnName} = {values[pk]} " +
                    $"AND {referencedTableMetaData.TableName}_{referencedTableMetaData.PrimaryKeys[0].ColumnName} = {a}";
                    IDbCommand cmd = this.Connection.CreateCommand();
                    cmd.CommandText = sqlCmd;
                    cmd.ExecuteNonQuery();
                };
                return Activator.CreateInstance(nType, new object[] { this, deleteCmd });
            }
            return null;
        }
        /// <summary>Creates an object from a database reader.</summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="re">Reader.</param>
        /// <returns>Object.</returns>
        internal Object _CreateObject(Type type, IDataReader re, string dbColumnPrefix)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            object rval = dynamicProxyGenerator.CreateDynamicProxy(type, propertyName =>
            {
                return createDynamicProxyObject(propertyName, type, values);
            });
            foreach (Field i in tableMetaDataCache._GetTableMetadata(type).Fields)
            {
                if (i.FieldType.IsSimpleType())
                {
                    // TODO: Implement if datatype is DateTime
                    var val = re.GetValue(re.GetOrdinal(dbColumnPrefix + "_" + i.ColumnName));
                    if (val != System.DBNull.Value)
                    {
                        i.SetValue(rval, i.ToFieldType(val));
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            for (int i = 0; i < re.FieldCount; i++)
            {
                string columnName = re.GetName(i);
                if (!columnName.StartsWith(dbColumnPrefix)) continue;
                columnName = columnName.Split('_')[0];
                if (values.ContainsKey(columnName)) continue;
                object val = re.GetValue(i);
                values.Add(columnName, val);
            }
            return rval;
        }




        /// <summary>Fills a list </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">List.</param>
        /// <param name="cmd">Command.</param>
        internal void _FillList<T>(IList<T> list, IDataReader re)
        {
            while (re.Read())
            {
                try
                {
                    list.Add((T)_CreateObject(typeof(T), re));
                }
                catch (Exception)
                {
                    Console.WriteLine("DataReader: Value 1 = " + re.GetValue(0) + " , Value 2 = " + re.GetValue(1));
                }
            }
            re.Close();
        }

        /// <summary>Fills a list </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">List.</param>
        /// <param name="re">Reader.</param>
        /// <param name="objects">Cached objects.</param>
        internal void _FillList<T>(ICollection<T> list, IDataReader re, ICollection<object> objects = null)
        {
            _FillList(typeof(T), list, re, objects);
        }


        /// <summary>Fills a list.</summary>
        /// <param name="t">Type.</param>
        /// <param name="list">List.</param>
        /// <param name="re">Reader.</param>
        /// <param name="objects">Cached objects.</param>
        internal void _FillList(Type t, object list, IDataReader re, ICollection<object> objects = null)
        {
            while (re.Read())
            {
                list.GetType().GetMethod("Add").Invoke(list, new object[] { _CreateObject(t, re, objects) });
            }
        }


        /// <summary>Fills a list.</summary>
        /// <param name="t">Type.</param>
        /// <param name="list">List.</param>
        /// <param name="sql">SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="objects">Cached objects.</param>
        internal void _FillList(Type t, object list, string sql, IEnumerable<Tuple<string, object>> parameters, ICollection<object> objects = null)
        {
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = sql;

            foreach (Tuple<string, object> i in parameters)
            {
                IDataParameter p = cmd.CreateParameter();
                p.ParameterName = i.Item1;
                p.Value = i.Item2;
                cmd.Parameters.Add(p);
            }

            IDataReader re = cmd.ExecuteReader();
            _FillList(t, list, re, objects);
            re.Close();
            re.Dispose();
            cmd.Dispose();
        }



        /// <summary>Gets the cache for a type.</summary>
        /// <param name="t">Type.</param>
        /// <returns>Cache.</returns>
        private Cache _GetCache(Type t)
        {
            if (!CachingEnabled) { return _NullCache; }

            if (!_Caches.ContainsKey(t))
            {
                _Caches.Add(t, new Cache(this));
            }

            return _Caches[t];
        }


        private void HandleManyToMany<T>(Field[] fields, ref T item)
        {
            TableMetaData itemEnt = tableMetaDataCache._GetTableMetadata(item.GetType());
            IDbCommand cmd = Connection.CreateCommand();
            IDataParameter p = cmd.CreateParameter();
            foreach (Field field in fields.Where(field => field.RelationType == Field.RelationEnumeration.ManyToMany))
            {
                if (field.GetValue(item) != null)
                {
                    TableMetaData listEnt = tableMetaDataCache._GetTableMetadata(field.FieldType.GenericTypeArguments[0]);
                    var dataset = field.GetValue(item);
                    IList list = (IList)dataset.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name.Equals("cache")).FirstOrDefault().GetValue(dataset);
                    Type ty = field.FieldType.GenericTypeArguments[0];
                    PropertyInfo pro = ty.GetProperty(field.InversePropertyName);
                    PropertyInfo itemPK = item.GetType().GetProperties().Where(x => x.Name.Equals(itemEnt.PrimaryKeys[0].FieldMember.Name)).FirstOrDefault();
                    var itemPKValue = itemPK.GetValue(item);
                    if (list != null)
                    {
                        foreach (var l in list)
                        {
                            PropertyInfo listPK = listEnt.EntityType.GetProperties().Where(x => x.Name.Equals(listEnt.PrimaryKeys[0].FieldMember.Name)).FirstOrDefault();
                            var listPkValue = listPK.GetValue(l);
                            string list_JoinTable_ColumName = listEnt.TableName + "_" + listEnt.PrimaryKeys[0].ColumnName;
                            string item_JoinTable_ColumName = itemEnt.TableName + "_" + itemEnt.PrimaryKeys[0].ColumnName;
                            string sql = "INSERT INTO " + field.JoinTableName + "(" + list_JoinTable_ColumName + "," + item_JoinTable_ColumName + ") " +
                                         "VALUES (:listPKValue, :itemPKValue);";
                            cmd.CommandText = sql;
                            cmd.Parameters.Clear();
                            p = cmd.CreateParameter();
                            p.ParameterName = ":listPKValue";
                            p.Value = listPkValue;
                            cmd.Parameters.Add(p);
                            p = cmd.CreateParameter();
                            p.ParameterName = ":itemPKValue";
                            p.Value = itemPKValue;
                            cmd.Parameters.Add(p);
                            cmd.ExecuteNonQuery();
                            //Add object in each item of list (other part of the manytomany relation)
                            var inversePropertyDataset = l.GetType().GetProperty(field.InversePropertyName).GetValue(l);
                            TableMetaData inversePropDataSetENTITY = tableMetaDataCache._GetTableMetadata(l);
                            if (inversePropertyDataset == null)
                            {
                                var dsType = inversePropDataSetENTITY.Fields.Where(f => f.FieldMember.Name.Equals(field.InversePropertyName)).FirstOrDefault().FieldType;
                                Type genericDsType = typeof(DataSet<>).MakeGenericType(dsType.GenericTypeArguments[0]);

                                Type genericListType = typeof(List<>).MakeGenericType(dsType.GenericTypeArguments[0]);
                                IList newInnerCache = Activator.CreateInstance(genericListType) as IList;
                                newInnerCache.Add(item);

                                var newDataset = Activator.CreateInstance(genericDsType, newInnerCache, this);
                                l.GetType().GetProperty(field.InversePropertyName).SetValue(l, newDataset);
                            }
                            else
                            {
                                var field_dataset = field.GetValue(item);
                                var dsType = inversePropDataSetENTITY.Fields.Where(f => f.FieldMember.Name.Equals(field.InversePropertyName)).FirstOrDefault().FieldType;
                                IList innerCache = (IList)dsType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name.Equals("cache")).FirstOrDefault().GetValue(field_dataset);
                                if (innerCache == null)
                                {
                                    Type genericListType = typeof(List<>).MakeGenericType(dsType.GenericTypeArguments[0]);
                                    innerCache = Activator.CreateInstance(genericListType) as IList;
                                }
                                innerCache.Add(item);
                                field.FieldType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name.Equals("cache")).FirstOrDefault().SetValue(field_dataset, innerCache);
                                l.GetType().GetProperty(field.InversePropertyName).SetValue(l, field_dataset);
                            }
                            dataset.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name.Equals("cache")).FirstOrDefault().SetValue(dataset, list);
                            item.GetType().GetProperty(field.FieldMember.Name).SetValue(item, dataset);//(zuerst das object wo der neue value gesetzt werden soll und dann der neue value)
                        }
                    }

                }
            }
            cmd.Dispose();
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Gets or sets if caching is enabled.</summary>
        public bool CachingEnabled { get; set; } = true;
        /// <summary>Gets or sets the database connection used by the framework.</summary>
        public IDbConnection Connection { get; set; }
        /// <summary>Gets this world's owner key.</summary>
        public string OwnerKey { get; } = new Random().Next(100000, 999999).ToString();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private properties                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal readonly TableMetaDataCache tableMetaDataCache;
        private readonly DynamicProxy dynamicProxyGenerator = new DynamicProxy();
        /// <summary>Caches.</summary>
        private Dictionary<Type, Cache> _Caches = new Dictionary<Type, Demo.Cache>();

        /// <summary>Empty cache.</summary>
        private readonly Cache _NullCache = new NullCache();

        #region Dispose
        private bool _disposed = false;
        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (Connection.State != ConnectionState.Closed)
                    Connection.Close();
            }
            _disposed = true;
        }
        #endregion
    }
}