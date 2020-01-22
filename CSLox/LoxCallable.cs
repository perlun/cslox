using System.Collections.Generic;

namespace CSLox
{
    internal interface ILoxCallable
    {
        object Call(Interpreter interpreter, List<object> arguments);
        int Arity();
    }
}