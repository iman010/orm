using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo.ExpressionVisitors
{
    public class ConsoleWriteVisitor : ExpressionVisitor
    {
        //public override Expression Visit(Expression node)
        //{
        //    Console.WriteLine($"Visited Visit，Content:{node.ToString()}");
        //    return base.Visit(node);
        //}

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {

            Console.WriteLine($"Visited VisitCatchBlock，Content:{node.ToString()}");
            return base.VisitCatchBlock(node);
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            Console.WriteLine($"Visited VisitElementInit，Content:{node.ToString()}");
            return base.VisitElementInit(node);
        }
        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {

            Console.WriteLine($"Visited VisitLabelTarget，Content:{node.ToString()}");
            return base.VisitLabelTarget(node);
        }
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {

            Console.WriteLine($"Visited VisitMemberAssignment，Content:{node.ToString()}");
            return base.VisitMemberAssignment(node);
        }
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {

            Console.WriteLine($"Visited VisitMemberBinding，Content:{node.ToString()}");
            return base.VisitMemberBinding(node);
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {

            Console.WriteLine($"Visited VisitMemberListBinding，Content:{node.ToString()}");
            return base.VisitMemberListBinding(node);
        }
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {

            Console.WriteLine($"Visited VisitMemberMemberBinding，Content:{node.ToString()}");
            return base.VisitMemberMemberBinding(node);
        }
        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            Console.WriteLine($"Visited VisitSwitchCase，Content:{node.ToString()}");
            return base.VisitSwitchCase(node);
        }
        protected override Expression VisitBlock(BlockExpression node)
        {
            Console.WriteLine($"Visited VisitBlock，Content:{node.ToString()}");
            return base.VisitBlock(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Console.WriteLine($"Visited VisitConditional，Content:{node.ToString()}");
            return base.VisitConditional(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Console.WriteLine($"Visited VisitConstant，Content:{node.ToString()}");
            return base.VisitConstant(node);
        }
        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            Console.WriteLine($"Visited VisitDebugInfo，Content:{node.ToString()}");
            return base.VisitDebugInfo(node);
        }
        protected override Expression VisitDefault(DefaultExpression node)
        {
            Console.WriteLine($"Visited VisitDefault，Content:{node.ToString()}");
            return base.VisitDefault(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            Console.WriteLine($"Visited VisitDynamic，Content:{node.ToString()}");
            return base.VisitDynamic(node);
        }
        protected override Expression VisitExtension(Expression node)
        {
            Console.WriteLine($"Visited VisitExtension，Content:{node.ToString()}");
            return base.VisitExtension(node);
        }
        protected override Expression VisitGoto(GotoExpression node)
        {
            Console.WriteLine($"Visited VisitGoto，Content:{node.ToString()}");
            return base.VisitGoto(node);
        }
        protected override Expression VisitIndex(IndexExpression node)
        {
            Console.WriteLine($"Visited VisitIndex，Content:{node.ToString()}");
            return base.VisitIndex(node);
        }
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            Console.WriteLine($"Visited VisitInvocation，Content:{node.ToString()}");
            return base.VisitInvocation(node);
        }
        protected override Expression VisitLabel(LabelExpression node)
        {
            Console.WriteLine($"Visited VisitLabel，Content:{node.ToString()}");
            return base.VisitLabel(node);
        }
        //protected override Expression VisitLambda<T>(Expression<T> node)
        //{
        //    Console.WriteLine($"Visited VisitLambda，Content:{node.ToString()}");
        //    return base.VisitLambda(node);
        //}

        protected override Expression VisitListInit(ListInitExpression node)
        {
            Console.WriteLine($"Visited VisitListInit，Content:{node.ToString()}");
            return base.VisitListInit(node);
        }
        protected override Expression VisitLoop(LoopExpression node)
        {
            Console.WriteLine($"Visited VisitLoop，Content:{node.ToString()}");
            return base.VisitLoop(node);
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            var n = base.VisitMember(node);
            Console.WriteLine($"Visited VisitMember，Content:{node.Member.Name}");
            return n;
        }
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            Console.WriteLine($"Visited VisitMemberInit，Content:{node.ToString()}");
            return base.VisitMemberInit(node);
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Console.WriteLine($"Visited VisitMethodCall，Content:{node.ToString()}");
            return base.VisitMethodCall(node);
        }
        protected override Expression VisitNew(NewExpression node)
        {
            Console.WriteLine($"Visited VisitNew，Content:{node.ToString()}");
            return base.VisitNew(node);
        }
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            Console.WriteLine($"Visited VisitNewArray，Content:{node.ToString()}");
            return base.VisitNewArray(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Console.WriteLine($"Visited VisitParameter，Content:{node.ToString()}");
            return base.VisitParameter(node);
        }
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            Console.WriteLine($"Visited VisitRuntimeVariables，Content:{node.ToString()}");
            return base.VisitRuntimeVariables(node);
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            Console.WriteLine($"Visited VisitSwitch，Content:{node.ToString()}");
            return base.VisitSwitch(node);
        }
        protected override Expression VisitTry(TryExpression node)
        {
            Console.WriteLine($"Visited VisitTry，Content:{node.ToString()}");
            return base.VisitTry(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Console.WriteLine($"Visited VisitTypeBinary，Content:{node.ToString()}");
            Visit(node.Left);
            Console.WriteLine($"Visited VisitTypeBinary，left done");
            Visit(node.Right);
            Console.WriteLine($"Visited VisitTypeBinary，right done");
            return node;
        }


        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            Console.WriteLine($"Visited VisitTypeBinary，Content:{node.ToString()}");
            return base.VisitTypeBinary(node);
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            Console.WriteLine($"Visited VisitUnary，Content:{node.ToString()}");
            return base.VisitUnary(node);
        }



    }
}
