using SWE3.Demo.Extensions;
using SWE3.Demo.FluentApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SWE3.Demo.ExpressionVisitors
{
    public class WhereExpressionVisitor : ExpressionVisitor
    {

        private static readonly Dictionary<ExpressionType, String> operatorToSQL = new Dictionary<ExpressionType, string>()
        {
            {ExpressionType.AndAlso,"AND" },
            {ExpressionType.OrElse,"OR" },
            {ExpressionType.Equal,"=" },
            {ExpressionType.NotEqual,"!=" },
            {ExpressionType.GreaterThan,">"},
            {ExpressionType.GreaterThanOrEqual,">="},
            {ExpressionType.LessThan,"<"},
            {ExpressionType.LessThanOrEqual,"<="}
        };
        private readonly DbContextBase world;
        private String sqlCode = "";
        public String SqlCode
        {
            get
            {
                return sqlCode;
            }
        }
        public void Reset()
        {
            sqlCode = "";
        }


        protected override Expression VisitBinary(BinaryExpression node)
        {
            sqlCode += "(";
            Visit(node.Left);
            if (operatorToSQL.ContainsKey(node.NodeType))
                sqlCode += " " + operatorToSQL[node.NodeType] + " ";
            else
                throw new NotImplementedException();
            Visit(node.Right);
            sqlCode += ")";
            return node;
        }
        private Boolean lambdaParameter = false;
        private List<string> members = new List<string>();

        public List<QueryPart> queryParts = new List<QueryPart>();

        public WhereExpressionVisitor(DbContextBase world)
        {
            this.world = world;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            QueryPart rootQueryPart = new QueryPart()
            {
                Name = node.Type.Name,
                TableNameInDB = world.tableMetaDataCache._GetTableMetadata(node.Type).TableName,
                type = node.Type
            };
            if (!this.queryParts.Contains(rootQueryPart))
            {
                this.queryParts.Add(rootQueryPart);
            }

            HandleColumnNameAttribute(node.Type);
            lambdaParameter = true;
            return base.VisitParameter(node);
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.Type == typeof(DateTime) &&
                (node.Expression as MemberExpression).Expression.NodeType == ExpressionType.Parameter)
            {
                string memberName = (node.Expression as MemberExpression).Member.Name;
                members.Add(memberName);
                if (node.Member.Name == "Day")
                {
                    sqlCode += $"CAST(strftime('%D',{memberName}) AS INTEGER)";
                }
                else if (node.Member.Name == "Month")
                {
                    sqlCode += $"CAST(strftime('%M',{memberName}) AS INTEGER)";
                }
                else if (node.Member.Name == "Year")
                {
                    sqlCode += $"CAST(strftime('%Y',{memberName}) AS INTEGER)";
                }
                return node;
            }
            else
            {
                var x = base.VisitMember(node);
                if (lambdaParameter == false && node.Expression.NodeType == ExpressionType.Constant)
                {
                    handleConstant(GetMemberConstant(node).Value);
                    return x;
                }
                else if (lambdaParameter == true)
                {
                    ModifyWhereCondition(node);
                    members.Add(node.Member.Name);
                    lambdaParameter = false;
                    return x;
                }
                else
                {
                    ModifyWhereCondition(node);
                    members.Add(node.Member.Name);
                    return x;
                }

                //throw new NotImplementedException();
            }
        }
        private static ConstantExpression GetMemberConstant(MemberExpression node)
        {
            object value;
            if (node.Member.MemberType == MemberTypes.Field)
            {
                value = GetFieldValue(node);
            }
            else if (node.Member.MemberType == MemberTypes.Property)
            {
                value = GetPropertyValue(node);
            }
            else
            {
                throw new NotSupportedException();
            }
            return Expression.Constant(value, node.Type);
        }
        private static object GetFieldValue(MemberExpression node)
        {
            var fieldInfo = (FieldInfo)node.Member;

            var instance = (node.Expression == null) ? null : TryEvaluate(node.Expression).Value;

            return fieldInfo.GetValue(instance);
        }
        private static object GetPropertyValue(MemberExpression node)
        {
            var propertyInfo = (PropertyInfo)node.Member;

            var instance = (node.Expression == null) ? null : TryEvaluate(node.Expression).Value;

            return propertyInfo.GetValue(instance, null);
        }
        private static ConstantExpression TryEvaluate(Expression expression)
        {

            if (expression.NodeType == ExpressionType.Constant)
            {
                return (ConstantExpression)expression;
            }
            throw new NotSupportedException();

        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            handleConstant(node.Value);
            return base.VisitConstant(node);
        }

        private void handleConstant(Object val)
        {
            if (val is string)
            {
                sqlCode += $"'{val}'";
            }
            else if (val is int)
            {
                sqlCode += $"{val}";
            }
            else if (val is bool)
            {
                sqlCode += (Convert.ToBoolean(val) ? 1 : 0).ToString();
            }
        }

        private void HandleColumnNameAttribute(Type parameterType)
        {
            TableMetaData enty = new TableMetaData(parameterType);
            foreach (string member in members)
            {
                Field currentField = enty.Fields.Where(x => x.FieldMember.Name.Equals(member)).FirstOrDefault();
                if (currentField != null && currentField.ColumnName != currentField.FieldMember.Name)
                {
                    this.sqlCode = this.sqlCode.Replace(currentField.FieldMember.Name, currentField.ColumnName);
                }
            }
        }

        public List<QueryPart> GetNames()
        {
            TableMetaData enty = new TableMetaData(this.queryParts[0].type);
            foreach (string member in members)
            {
                Field currentField = enty.Fields.Where(x => x.FieldMember.Name.Equals(member)).FirstOrDefault();
                if (currentField != null)
                {
                    if (currentField.FieldType.IsSimpleType() == false)
                    {
                        TableMetaData currentEntity = world.tableMetaDataCache._GetTableMetadata(currentField.FieldType);
                        this.queryParts.Add(new QueryPart()
                        {
                            Name = member,
                            TableNameInDB = currentEntity.TableName,
                            type = currentEntity.EntityType
                        });
                    }
                    else
                    {
                        this.queryParts.Add(new QueryPart()
                        {
                            Name = member,
                            TableNameInDB = "",
                            type = currentField.FieldType
                        });
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return queryParts;
        }

        public void ModifyWhereCondition(MemberExpression node)
        {
            if (node.Type.IsSimpleType())
            {
                sqlCode += node.Member.Name;
            }
            else
            {
                sqlCode += world.tableMetaDataCache._GetTableMetadata(node.Type).TableName + ".";
            }
        }
    }
}