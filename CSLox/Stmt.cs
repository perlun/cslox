using System.Collections.Generic;

namespace CSLox
{
    internal abstract class Stmt
    {
        internal interface Visitor<R>
        {
            R VisitBlockStmt(Block stmt);
            R VisitExpressionStmt(Expression stmt);
            R VisitFunctionStmt(Function stmt);
            R VisitIfStmt(If stmt);
            R VisitPrintStmt(Print stmt);
            R VisitReturnStmt(Return stmt);
            R VisitVarStmt(Var stmt);
            R VisitWhileStmt(While stmt);
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

        internal class Function : Stmt
        {
            internal readonly Token name;
            internal readonly List<Token> _params;
            internal readonly List<Stmt> body;

            internal Function(Token name, List<Token> _params, List<Stmt> body) {
                this.name = name;
                this._params = _params;
                this.body = body;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }
        }

        internal class If : Stmt
        {
            internal readonly Expr condition;
            internal readonly Stmt thenBranch;
            internal readonly Stmt elseBranch;

            internal If(Expr condition, Stmt thenBranch, Stmt elseBranch) {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
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

        internal class Return : Stmt
        {
            internal readonly Token keyword;
            internal readonly Expr value;

            internal Return(Token keyword, Expr value) {
                this.keyword = keyword;
                this.value = value;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitReturnStmt(this);
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

        internal class While : Stmt
        {
            internal readonly Expr condition;
            internal readonly Stmt body;

            internal While(Expr condition, Stmt body) {
                this.condition = condition;
                this.body = body;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }
        }

        internal abstract R Accept<R>(Visitor<R> visitor);
    }
}
