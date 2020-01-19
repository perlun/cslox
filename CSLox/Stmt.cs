using System.Collections.Generic;

namespace CSLox
{
    internal abstract class Stmt
    {
        internal interface Visitor<R>
        {
            R VisitBlockStmt(Block stmt);
            R VisitExpressionStmt(Expression stmt);
            R VisitPrintStmt(Print stmt);
            R VisitVarStmt(Var stmt);
        }

        internal class Block : Stmt
        {
            internal readonly List<Stmt> statements;

            internal Block(List<Stmt> statements) {
                this.statements = statements;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }
        }

        internal class Expression : Stmt
        {
            internal readonly Expr expression;

            internal Expression(Expr expression) {
                this.expression = expression;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        internal class Print : Stmt
        {
            internal readonly Expr expression;

            internal Print(Expr expression) {
                this.expression = expression;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }

        internal class Var : Stmt
        {
            internal readonly Token name;
            internal readonly Expr initializer;

            internal Var(Token name, Expr initializer) {
                this.name = name;
                this.initializer = initializer;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }

        internal abstract R Accept<R>(Visitor<R> visitor);
    }
}
