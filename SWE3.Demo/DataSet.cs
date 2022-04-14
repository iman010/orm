using SWE3.Demo.ExpressionVisitors;
using SWE3.Demo.Extensions;
using SWE3.Demo.FluentApi;
using SWE3.Demo.Proxies;
using SWE3.Demo.Test;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SWE3.Demo
{
    public partial class DataSet<T> : IEnumerable<T>
    {
        public bool IsReadOnly => false;
        private readonly Action<object> deleteCommand;
        private DbContextBase dbContext;
        private List<T> cache = null;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Constructos                                                                                               //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private DataSet(List<T> list)
        {
            this.cache = list;
        }

        public DataSet(List<T> list, DbContextBase dbContext)
        {
            this.dbContext = dbContext;
            this.cache = list;
        }

        public DataSet(DbContextBase dbContext)
        {
            this.dbContext = dbContext;
        }

        public DataSet(DbContextBase dbContext, Action<object> deleteCommand)
        {
            this.dbContext = dbContext;
            this.deleteCommand = deleteCommand;
            init();
        }

        public static implicit operator DataSet<T>(List<T> list)
        {
            return new DataSet<T>(list);//@TODO
        }

        void init()
        {
            visitor = new WhereExpressionVisitor(dbContext);
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (cache != null)
            {
                foreach (var item in cache)
                {
                    yield return item;
                }
            }
            else
            {
                // @NEW
                IDbCommand cmd = dbContext.Connection.CreateCommand();
                if (!typeof(T).GetTypeInfo().IsAbstract)
                {
                    cmd.CommandText = "Select * FROM " + generateSqlCodeWithoutSelectPart();
                    IDataReader rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        T myobject = default(T);
                        try
                        {
                            myobject = (T)dbContext._CreateObject(typeof(T), rd);
                        }
                        catch (Exception)
                        {
                            Console.Write("Exception occured while trying to create object.");
                        }
                        yield return myobject;
                    }
                }
                else{
                    cmd.CommandText = generateSqlCodeWithoutSelectPart();
                    IDataReader rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        yield return this.TryCreateObject(rd);
                    }
                }
            }
        }
        public void CopyTo(T[] array, int arrayIndex) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns the records that match the where condition
        /// </summary>
        /// <param name="pred">Expression<predicate></param>
        /// <returns>Dataset</returns>
        public DataSet<T> MyWhere(Expression<Predicate<T>> pred)
        {
            visitor.Reset();
            visitor.Visit(pred);
            this.queryParts = visitor.GetNames();
            if (!String.IsNullOrWhiteSpace(WhereCondition))
                WhereCondition += " AND ";
            WhereCondition += visitor.SqlCode;
            if (typeof(T).GetTypeInfo().IsAbstract)
            {
                ModifyWhereConditionForInheritance();
            }
            return this;
        }

        /// <summary>
        /// Returns the first record of table
        /// </summary>
        /// <param name="pred">Expression<predicate></param>
        /// <returns>T</returns>
        public T MyFirstOrDefault(Expression<Predicate<T>> pred)
        {
            var x = MyWhere(pred).ToList();
            if (x == null)
                return default(T);
            foreach (var item in x)
            {
                return item;
            }
            return default(T);
        }

        /// <summary>
        /// Generates the select sql code for the inherited types
        /// </summary>
        /// <returns>string</returns>
        internal string generateSQLCodeForInheritance()
        {
            var tp = dbContext.GetType();
            var properties = tp.GetProperties();
            List<Type> derivedTypes = new List<Type>();
            foreach (var item in properties)
            {
                if (item.PropertyType.GetNameWithoutGenericInformation() == "DataSet")
                {
                    Type type = item.PropertyType.GenericTypeArguments[0];
                    if (!type.GetTypeInfo().IsAbstract && typeof(T).IsAssignableFrom(type))
                    {
                        derivedTypes.Add(type);
                    }
                }
            }
            Dictionary<TableMetaData, List<Field>> derivedTypesTableMetaData = new Dictionary<TableMetaData, List<Field>>();
            foreach (var type in derivedTypes)
            {
                var metaData = dbContext.tableMetaDataCache._GetTableMetadata(type);
                var fields = metaData.Fields.Where(f => f.RelationType == Field.RelationEnumeration.SimpleType || f.RelationType == Field.RelationEnumeration.ManyToOne).ToList();
                derivedTypesTableMetaData.Add(metaData, fields);
            }
            string sql = "";
            int i = 0;
            foreach (var tbm in derivedTypesTableMetaData)
            {
                if (i > 0)
                {
                    sql += " UNION ";
                }
                sql += "SELECT ";
                int j = 0;
                foreach (var innerTbm in derivedTypesTableMetaData)
                {
                    var fields = innerTbm.Value;
                    if (i == j)
                    {
                        foreach (Field field in fields)
                        {
                            sql += field.ColumnName + " as " + innerTbm.Key.TableName + "_" + field.ColumnName + ",";
                        }
                    }
                    else
                    {
                        foreach (Field field in fields)
                        {
                            sql += "null as " + innerTbm.Key.TableName + "_" + field.ColumnName + ",";
                        }
                    }
                    j++;
                    if (j == derivedTypesTableMetaData.Count)
                    {
                        sql = sql.Remove(sql.Length - 1);
                    }
                }
                sql += " FROM " + tbm.Key.TableName;
                if (WhereConditionsInheritance.ContainsKey(tbm.Key.TableName))
                {
                    sql += " WHERE " + WhereConditionsInheritance[tbm.Key.TableName];
                }
                i++;
            }
            return sql + ";";
        }
        /// <summary>
        /// Returns the number of records
        /// </summary>
        /// <returns>long</returns>
        public virtual long Count()
        {
            if (!typeof(T).GetTypeInfo().IsAbstract)
            {
                IDbCommand cmd = dbContext.Connection.CreateCommand();
                cmd.CommandText = "Select Count(*) FROM " + generateSqlCodeWithoutSelectPart();
                var count = cmd.ExecuteScalar();
                return (long)count;
            }
            return 0;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Tries to create an object by trying to create an object of datareade values with all types in dbcontext
        /// </summary>
        /// <param name="rd">IDataReader</param>
        /// <returns>T</returns>
        private T TryCreateObject(IDataReader rd)
        {
            var tp = dbContext.GetType();
            var properties = tp.GetProperties();
            List<Type> derivedTypes = new List<Type>();
            foreach (var item in properties)
            {
                if (item.PropertyType.GetNameWithoutGenericInformation() == "DataSet")
                {
                    Type type = item.PropertyType.GenericTypeArguments[0];
                    if (!type.GetTypeInfo().IsAbstract && typeof(T).IsAssignableFrom(type))
                    {
                        derivedTypes.Add(type);
                    }
                }
            }
            foreach (Type type in derivedTypes)
            {
                try
                {
                    T myobject = (T)dbContext._CreateObject(type, rd, dbContext.tableMetaDataCache._GetTableMetadata(type).TableName);
                    if (myobject != null)
                    {
                        return myobject;
                    }
                }
                catch (IndexOutOfRangeException)
                {

                }
            }
            return default(T);
        }
        /// <summary>
        /// Modifies the where condition to match the aliases of the columns
        /// </summary>
        private void ModifyWhereConditionForInheritance()
        {
            var tp = dbContext.GetType();
            var properties = tp.GetProperties();
            List<Type> derivedTypes = new List<Type>();
            foreach (var item in properties)
            {
                if (item.PropertyType.GetNameWithoutGenericInformation() == "DataSet")
                {
                    Type type = item.PropertyType.GenericTypeArguments[0];
                    if (!type.GetTypeInfo().IsAbstract && typeof(T).IsAssignableFrom(type))
                    {
                        derivedTypes.Add(type);
                    }
                }
            }
            Dictionary<TableMetaData, List<Field>> derivedTypesTableMetaData = new Dictionary<TableMetaData, List<Field>>();
            foreach (var type in derivedTypes)
            {
                var metaData = dbContext.tableMetaDataCache._GetTableMetadata(type);
                var fields = metaData.Fields.Where(f => f.RelationType == Field.RelationEnumeration.SimpleType || f.RelationType == Field.RelationEnumeration.ManyToOne).ToList();
                derivedTypesTableMetaData.Add(metaData, fields);
            }
            foreach (var tbm in derivedTypesTableMetaData)
            {
                foreach (Field f in tbm.Value)
                {
                    if (WhereCondition.Contains(f.ColumnName) && WhereConditionsInheritance.ContainsKey(tbm.Key.TableName) == false)
                    {
                        WhereConditionsInheritance.Add(tbm.Key.TableName, WhereCondition.Replace(f.ColumnName, tbm.Key.TableName + "_" + f.ColumnName));
                    }
                }
            }
        }
        /// <summary>
        /// Generates the sql code
        /// </summary>
        /// <returns>string</returns>
        private String generateSqlCodeWithoutSelectPart()
        {
            if (!typeof(T).GetTypeInfo().IsAbstract)
            {
                string tablesNames = string.Join(",", queryParts.Select(x => x.TableNameInDB).ToArray().Where(s => !string.IsNullOrEmpty(s)));
                tablesNames = tablesNames != "" ? tablesNames : dbContext.tableMetaDataCache._GetTableMetadata(typeof(T)).TableName;
                return tablesNames + (JoinCondition != null ? " WHERE " + JoinCondition : "")
                                    + ((WhereCondition != null && JoinCondition == null) ? " WHERE " + WhereCondition : "")
                                    + ((WhereCondition != null && JoinCondition != null) ? " AND " + WhereCondition : "");
            }
            else
            {
                return generateSQLCodeForInheritance();
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private String WhereCondition { get; set; }
        private Dictionary<string, string> WhereConditionsInheritance { get; set; } = new Dictionary<string, string>();
        private String JoinCondition { get; set; }
        private List<String> TableNames { get; set; } = new List<string>();

        private List<QueryPart> queryParts = new List<QueryPart>();

        private WhereExpressionVisitor visitor;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
