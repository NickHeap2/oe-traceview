using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceQueryPlan : TraceEntry
    {
        public TraceQueryPlan(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            
        }
    }
}
