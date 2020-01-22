using System;

namespace CSLox
{
    internal class Return : Exception
    {
        internal readonly object Value;

        internal Return(object value)
        {
            this.Value = value;
        }
    }
}