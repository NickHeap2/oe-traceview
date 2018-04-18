using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceInvokePropGet : TraceInvoke
    {
        public string PropertyName { get; set; }

        public TraceInvokePropGet(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {

            this.PropertyName = MethodName.Replace("propGet_", "");
            
        }
    }
}
