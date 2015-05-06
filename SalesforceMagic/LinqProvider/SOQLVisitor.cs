using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using SalesforceMagic.Extensions;

namespace SalesforceMagic.LinqProvider
{
    public static class SOQLVisitor
    {
        public static string ConvertToSOQL(Expression expression)
        {
            return VisitExpression(expression);
        }

        private static string VisitExpression(Expression expression, bool valueExpression = false)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    LambdaExpression lambda = Expression.Lambda(expression);
                    return VisitConstant(lambda.Compile().DynamicInvoke());
                case ExpressionType.Not:
                    return VisitExpression(Expression.NotEqual(((UnaryExpression)expression).Operand, Expression.Constant(true)));
                case ExpressionType.IsTrue:
                    return VisitBinary(expression as BinaryExpression, "=");
                case ExpressionType.IsFalse:
                    return VisitBinary(expression as BinaryExpression, "!=");
                case ExpressionType.GreaterThanOrEqual:
                    return VisitBinary(expression as BinaryExpression, ">=");
                case ExpressionType.LessThanOrEqual:
                    return VisitBinary(expression as BinaryExpression, "<=");
                case ExpressionType.LessThan:
                    return VisitBinary(expression as BinaryExpression, "<");
                case ExpressionType.GreaterThan:
                    return VisitBinary(expression as BinaryExpression, ">");
				case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return VisitBinary(expression as BinaryExpression, "AND");
				case ExpressionType.Or:
				case ExpressionType.OrElse:
					return VisitBinary(expression as BinaryExpression, "OR");
				case ExpressionType.Equal:
                    return VisitBinary(expression as BinaryExpression, "=");
                case ExpressionType.NotEqual:
                    return VisitBinary(expression as BinaryExpression, "!=");
                case ExpressionType.Lambda:
                    return VisitLambda(expression as LambdaExpression);
                case ExpressionType.MemberAccess:
                    return VisitMember(expression as MemberExpression, valueExpression);
                case ExpressionType.Constant:
                    return VisitConstant(expression as ConstantExpression);
                case ExpressionType.Convert:
                    return VisitMember(((UnaryExpression)expression).Operand as MemberExpression);
                default:
                    return null;
            }
        }

        private static string VisitBinary(BinaryExpression node, string opr)
        {
            if (node.Left.NodeType == ExpressionType.MemberAccess 
                && node.Left.Type == typeof(bool)
                && node.Right.NodeType == ExpressionType.Constant)
            {
                var right = ((ConstantExpression) node.Right).Value;
                return "(" + ((PropertyInfo)((MemberExpression)node.Left).Member).GetName() + " " + opr + " " + right + ")";
            }

            return "(" + VisitExpression(node.Left) + " " + opr + " " + VisitExpression(node.Right, true) + ")";
        }

        private static string VisitConstant(ConstantExpression node)
        {
            return VisitConstant(node.Value);
        }

        private static string VisitConstant(object value)
        {
            if (value is string)
                return "'" + ((string)value).Replace("'", "\\'") + "'";

            return value == null 
                ? "null" 
                : value.ToString();
        }

        private static string VisitLambda(LambdaExpression node)
        {
            return VisitExpression(node.Body);
        }

        private static string VisitMember(MemberExpression node, bool valueExpression = false)
        {
            if (node == null) return "null";
            if (node.Type == typeof(bool)) return ((PropertyInfo)node.Member).GetName() + " = True";
            if (node.Member is PropertyInfo && !valueExpression) return ((PropertyInfo)node.Member).GetName();
            if (node.Expression == null) throw new NullReferenceException();

            object value;
            ConstantExpression captureConst;
            
            if (node.Expression is ConstantExpression)
            {
                captureConst = (ConstantExpression)node.Expression;
                value = ((FieldInfo)node.Member).GetValue(captureConst.Value);
            }
            else
            {
                MemberExpression memberConst = (MemberExpression)node.Expression;
                captureConst = (ConstantExpression)memberConst.Expression;
                value = ((PropertyInfo)node.Member).GetValue(((FieldInfo)memberConst.Member).GetValue(captureConst.Value));
            }

            if (value is string) return "'" + ((string)value).Replace("'", "\\'") + "'";
            if (value is DateTime) return ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ssZ");
            if (value == null) return "null";
            if (value is int || value is float || value is decimal) return value.ToString();
            throw new InvalidDataException();
        }
    }
}