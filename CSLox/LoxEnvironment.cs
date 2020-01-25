using System.Collections.Generic;

namespace CSLox
{
    public class LoxEnvironment
    {
        readonly LoxEnvironment enclosing;

        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public LoxEnvironment(LoxEnvironment enclosing = null)
        {
            this.enclosing = enclosing;
        }

        internal void Define(string name, object value)
        {
            values[name] = value;
        }

        internal object GetAt(int distance, string name)
        {
            return Ancestor(distance).values[name];
        }

        internal void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).values[name.lexeme] = value;
        }

        private LoxEnvironment Ancestor(int distance)
        {
            LoxEnvironment environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.enclosing;
            }

            return environment;
        }

        internal object Get(Token name)
        {
            if (values.ContainsKey(name.lexeme))
            {
                return values[name.lexeme];
            }

            // Fall-back to the enclosing scope if the variable isn't found in the current scope.
            if (enclosing != null)
            {
                return enclosing.Get(name);
            }

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        internal void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name,
                "Undefined variable '" + name.lexeme + "'.");
        }
    }
}