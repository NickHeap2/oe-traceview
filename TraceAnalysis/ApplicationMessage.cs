using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class ApplicationMessage : TraceEntry
    {
        //public List<string> MessageLines { get; set; }

        public ApplicationMessage(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            
        }
    }
}
