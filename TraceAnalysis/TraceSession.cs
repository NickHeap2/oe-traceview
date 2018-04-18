using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceSession : TraceEntry
    {
        //public List<string> MessageLines { get; set; }

        public TraceSession(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            
        }
    }
}
