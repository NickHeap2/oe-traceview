using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class ApplicationMessageLine : TraceEntry
    {
        public string Message { get; set; }
        public ApplicationMessageLine(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            string[] parts = content.Split(' ');
            this.Message = content;
            this.IsUITriggered = true;
        }
    }
}
