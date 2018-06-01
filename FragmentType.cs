using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace magic.html
{
    internal enum FragmentType
    {
        Html=1,
        Begin,
        Text,
        End,
        DocDeclare,
        Comment,
        Whitespace
    }
}
