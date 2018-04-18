using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceStartupProcedure : TraceEntry
    {
        //public List<string> MessageLines { get; set; }
        public string Procedure { get; set; }
        public string InternalProcedure { get; set; }

        public TraceStartupProcedure(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            this.Procedure = content;
            this.InInternalProcedure = "Main Block";
        }
    }
}
