namespace CSLox
{
    internal abstract class Expr
    {
        internal interface Visitor<R>
        {
            R VisitBinaryExpr(Binary expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitUnaryExpr(Unary expr);
        }

        internal class Binary : Expr
        {
            internal readonly Expr left;
            internal readonly Token _operator;
            internal readonly Expr right;

            internal Binary(Expr left, Token _operator, Expr right) {
                this.left = left;
                this._operator = _operator;
                this.right = right;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }
        }

        internal class Grouping : Expr
        {
            internal readonly Expr expression;

            internal Grouping(Expr expression) {
                this.expression = expression;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }
        }

        internal class Literal : Expr
        {
            internal readonly object value;

            internal Literal(object value) {
                this.value = value;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }
        }

        internal class Unary : Expr
        {
            internal readonly Token _operator;
            internal readonly Expr right;

            internal Unary(Token _operator, Expr right) {
                this._operator = _operator;
                this.right = right;
            }

            internal override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }
        }

        internal abstract R Accept<R>(Visitor<R> visitor);
    }
}
