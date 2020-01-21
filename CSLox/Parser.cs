using System;
using System.Collections.Generic;
using static CSLox.TokenType;
using static CSLox.ParseErrorType;

namespace CSLox
{
    internal class Parser
    {
        private class ParseError : Exception
        {
            public ParseErrorType? ParseErrorType { get; }

            public ParseError(ParseErrorType? parseErrorType)
            {
                ParseErrorType = parseErrorType;
            }
        }

        private readonly List<Token> tokens;
        private int current = 0;

        internal Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        internal IEnumerable<Stmt> ParseStatements()
        {
            var statements = new List<Stmt>();

            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        internal Expr ParseExpression()
        {
            try
            {
                return Expression();
            }
            catch (ParseError)
            {
                return null;
            }
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(VAR)) return VarDeclaration();

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt Statement()
        {
            if (Match(PRINT)) return PrintStatement();
            if (Match(LEFT_BRACE)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(EQUAL))
            {
                initializer = Expression();
            }

            Consume(SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(SEMICOLON, "Expect ';' after expression.", MISSING_TRAILING_SEMICOLON);
            return new Stmt.Expression(expr);
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Expr Assignment()
        {
            Expr expr = Equality();

            if (Match(EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Expr.Variable variable)
                {
                    Token name = variable.name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(BANG_EQUAL, EQUAL_EQUAL))
            {
                Token _operator = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, _operator, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Addition();

            while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                Token _operator = Previous();
                Expr right = Addition();
                expr = new Expr.Binary(expr, _operator, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(MINUS, PLUS))
            {
                Token _operator = Previous();
                Expr right = Multiplication();
                expr = new Expr.Binary(expr, _operator, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            Expr expr = Unary();

            while (Match(SLASH, STAR))
            {
                Token _operator = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, _operator, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(BANG, MINUS))
            {
                Token _operator = Previous();
                Expr right = Unary();
                return new Expr.Unary(_operator, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (Match(FALSE)) return new Expr.Literal(false);
            if (Match(TRUE)) return new Expr.Literal(true);
            if (Match(NIL)) return new Expr.Literal(null);

            if (Match(NUMBER, STRING))
            {
                return new Expr.Literal(Previous().literal);
            }

            if (Match(IDENTIFIER))
            {
                return new Expr.Variable(Previous());
            }

            if (Match(LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private Token Consume(TokenType type, string message, ParseErrorType? parseErrorType = null)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), message, parseErrorType);
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd())
            {
                return false;
            }

            return Peek().type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                current++;
            }

            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().type == EOF;
        }

        private Token Peek()
        {
            return tokens[current];
        }

        private Token Previous()
        {
            return tokens[current - 1];
        }

        private static ParseError Error(Token token, string message, ParseErrorType? parseErrorType = null)
        {
            Lox.ParseError(token, message, parseErrorType);
            return new ParseError(parseErrorType);
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().type == SEMICOLON)
                {
                    return;
                }

                switch (Peek().type)
                {
                    case CLASS:
                    case FUN:
                    case VAR:
                    case FOR:
                    case IF:
                    case WHILE:
                    case PRINT:
                    case RETURN:
                        return;
                }

                Advance();
            }
        }
    }
}