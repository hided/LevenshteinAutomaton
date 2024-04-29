using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevenshteinAutomaton.Performance
{
    internal struct SequentialNode
    {
        public int ChildStartPos;
        public byte ChildCount;
        public byte KeyChar;
        public bool IsEnd;
    }
}
