using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSLox
{
    public static class Lox
    {
        private static readonly Interpreter interpreter = new Interpreter();

        private static bool hadError;
        private static bool hadRuntimeError;

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cslox [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            Run(Encoding.UTF8.GetString(bytes));

            // Indicate an error in the exit code.
            if (hadError)
            {
                Environment.Exit(65);
            }

            if (hadRuntimeError)
            {
                Environment.Exit(70);
            }
        }

        private static void RunPrompt()
        {
            for (;;)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                hadError = false;
            }
        }

        private static void Run(string source)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return;
            }

            var scanner = new Scanner(source);

            var tokens = scanner.ScanTokens();

            // For now, just print the tokens.
            var parser = new Parser(tokens);
            IEnumerable<Stmt> statements = parser.ParseStatements();

            // Stop if there was a syntax error.
            if (!hadError)
            {
                interpreter.Interpret(statements);
            }
            else
            {
                // This was not a valid set of statements. But is it perhaps a valid expression? The parser is now
                // at EOF and since we don't currently have any form of "rewind" functionality, the easiest approach
                // is to just create a new parser at this point.
                parser = new Parser(tokens);
                Expr expression = parser.ParseExpression();

                if (expression == null)
                {
                    // Likely not a valid expression. Errors are presumed to have been handled at this point, so we
                    // can just return.
                    return;
                }

                object result = interpreter.Evaluate(expression);

                if (result != null)
                {
                    Console.WriteLine(result);
                }
            }
        }

        internal static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        internal static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine($"{error.Message}\n" +
                              $"[line {error.token.line}]");
            hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            hadError = true;
        }

        private static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        internal static void ParseError(Token token, string message, ParseErrorType? parseErrorType)
        {
            if (parseErrorType == ParseErrorType.MISSING_TRAILING_SEMICOLON)
            {
                // These errors are ignored; we will get them all them when we try to parse expressions as
                // statements.
                hadError = true;
                return;
            }

            Error(token, message);
        }
    }
}