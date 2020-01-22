using System.Collections.Generic;

namespace CSLox
{
    class LoxFunction : ILoxCallable
    {
        private readonly Stmt.Function declaration;
        private readonly LoxEnvironment closure;

        internal LoxFunction(Stmt.Function declaration, LoxEnvironment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new LoxEnvironment(closure);

            for (int i = 0; i < declaration._params.Count; i++)
            {
                environment.Define(declaration._params[i].lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
                return null;
            }
            catch (Return returnValue)
            {
                return returnValue.Value;
            }
        }

        public int Arity()
        {
            return declaration._params.Count;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }
    }
}