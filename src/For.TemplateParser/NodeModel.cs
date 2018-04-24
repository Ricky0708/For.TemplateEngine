using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace For.TemplateParser
{
    [Serializable]
    internal class NodeModel
    {
        internal delegate object delgGetProperty(object instance);
        internal NodeType Type { get; set; }
        internal string NodeStringValue { get; set; }
        internal delgGetProperty NodeDelegateValue { get; set; }

    }
    internal enum NodeType
    {
        String,
        Property
    }
}
