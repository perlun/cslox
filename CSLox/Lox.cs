using System;
using System.IO;
using System.Text;

namespace CSLox
{
    public static class Lox
    {
        private static bool hadError = false;

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
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
            byte[] bytes = File.ReadAllBytes(path);
            Run(Encoding.UTF8.GetString(bytes));

            // Indicate an error in the exit code.
            if (hadError)
            {
                Environment.Exit(65);
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
            var scanner = new Scanner(source);

            var tokens = scanner.ScanTokens();

            // For now, just print the tokens.
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        internal static void Error(int line, String message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, String where, String message)
        {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            hadError = true;
        }
    }
}