using System;
using System.Collections.Generic;
using static CSLox.TokenType;

namespace CSLox
{
    internal class Interpreter : Expr.Visitor<object>, Stmt.Visitor<VoidObject>
    {
        private LoxEnvironment loxEnvironment = new LoxEnvironment();

        internal void Interpret(IEnumerable<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr._operator.type)
            {
                case BANG:
                    return !IsTruthy(right);

                case MINUS:
                    CheckNumberOperand(expr._operator, right);
                    return -(double) right;
            }

            // Unreachable.
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return loxEnvironment.Get(expr.name);
        }

        private static void CheckNumberOperand(Token _operator, object operand)
        {
            if (operand is double)
            {
                return;
            }

            throw new RuntimeError(_operator, "Operand must be a number.");
        }

        private static void CheckNumberOperands(Token _operator, object left, object right)
        {
            if (left is double && right is double)
            {
                return;
            }

            throw new RuntimeError(_operator, "Operands must be numbers.");
        }

        private static bool IsTruthy(object _object)
        {
            if (_object == null)
            {
                return false;
            }

            if (_object is bool b)
            {
                return b;
            }

            return true;
        }

        private static bool IsEqual(object a, object b)
        {
            // nil is only equal to nil.
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        private static string Stringify(object _object)
        {
            if (_object == null)
            {
                return "nil";
            }

            return _object.ToString();
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        internal object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void ExecuteBlock(IEnumerable<Stmt> statements, LoxEnvironment loxEnvironment)
        {
            LoxEnvironment previous = this.loxEnvironment;

            try
            {
                this.loxEnvironment = loxEnvironment;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.loxEnvironment = previous;
            }
        }

        public VoidObject VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new LoxEnvironment(loxEnvironment));
            return null;
        }

        public VoidObject VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public VoidObject VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public VoidObject VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            loxEnvironment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);

            loxEnvironment.Assign(expr.name, value);
            return value;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr._operator.type)
            {
                case GREATER:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double) left > (double) right;
                case GREATER_EQUAL:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double) left >= (double) right;
                case LESS:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double) left < (double) right;
                case LESS_EQUAL:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double) left <= (double) right;
                case MINUS:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double) left - (double) right;
                case PLUS:
                    if (left is double d1 && right is double d2)
                    {
                        return d1 + d2;
                    }

                    if (left is string s1 && right is string s2)
                    {
                        return s1 + s2;
                    }

                    throw new RuntimeError(expr._operator,
                        "Operands must be two numbers or two strings.");
                case SLASH:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double) left / (double) right;
                case STAR:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double) left * (double) right;
                case BANG_EQUAL:
                    return !IsEqual(left, right);
                case EQUAL_EQUAL:
                    return IsEqual(left, right);
            }

            // Unreachable.
            return null;
        }
    }
}