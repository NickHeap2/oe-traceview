using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceMessageLine : TraceEntry
    {
        public string Message { get; set; }
        public TraceMessageLine(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            string[] parts = content.Split(' ');
            this.Message = content;
            this.IsUITriggered = true;
        }
    }
}
