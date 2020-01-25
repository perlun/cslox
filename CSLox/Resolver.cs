using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CSLox
{
    class Resolver : Expr.Visitor<VoidObject>, Stmt.Visitor<VoidObject>
    {
        private readonly Stack<IDictionary<string, bool>> scopes = new Stack<IDictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;

        private readonly Interpreter interpreter;

        internal Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        internal void Resolve(IEnumerable<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                Resolve(statement);
            }
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (IsEmpty(scopes)) return;

            // This adds the variable to the innermost scope so that it shadows any outer one and so that we know the
            // variable exists. We mark it as “not ready yet” by binding its name to false in the scope map. Each value
            // in the scope map means “is finished being initialized”.
            var scope = scopes.Peek();

            if (scope.ContainsKey(name.lexeme))
            {
                Lox.Error(name, "Variable with this name already declared in this scope.");
            }

            scope[name.lexeme] = false;
        }

        private static bool IsEmpty(ICollection stack)
        {
            return stack.Count == 0;
        }

        private void Define(Token name)
        {
            if (IsEmpty(scopes)) return;

            // We set the variable’s value in the scope map to true to mark it as fully initialized and available for
            // use. It’s alive!
            scopes.Peek()[name.lexeme] = true;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                // TODO: rewrite this for performance, since scopes.ElementAt() is much more inefficient on .NET
                // TODO: than the Java counterpart.
                if (scopes.ElementAt(i).ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                    return;
                }
            }

            // Not found. Assume it is global.                   
        }

        public VoidObject VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public VoidObject VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public VoidObject VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);

            foreach (Expr argument in expr.arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public VoidObject VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);
            return null;
        }

        public VoidObject VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public VoidObject VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public VoidObject VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);
            return null;
        }

        public VoidObject VisitVariableExpr(Expr.Variable expr)
        {
            if (!IsEmpty(scopes) &&
                scopes.Peek()[expr.name.lexeme] == false)
            {
                Lox.Error(expr.name,
                    "Cannot read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        public VoidObject VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        public VoidObject VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();

            foreach (Token param in function._params)
            {
                Declare(param);
                Define(param);
            }

            Resolve(function.body);
            EndScope();

            currentFunction = enclosingFunction;
        }

        public VoidObject VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        public VoidObject VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public VoidObject VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmt.keyword, "Cannot return from top-level code.");
            }

            if (stmt.value != null)
            {
                Resolve(stmt.value);
            }

            return null;
        }

        public VoidObject VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }

            Define(stmt.name);
            return null;
        }

        public VoidObject VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        private enum FunctionType
        {
            NONE,
            FUNCTION
        }
    }
}