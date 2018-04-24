using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace For.TemplateParser
{

    internal class NodeModel
    {
        internal delegate object delgGetProperty(object instance);
        public NodeType Type { get; set; }
        public string NodeStringValue { get; set; }
        public delgGetProperty NodeDelegateValue { get; set; }

    }
    internal enum NodeType
    {
        String,
        Property
    }
}
