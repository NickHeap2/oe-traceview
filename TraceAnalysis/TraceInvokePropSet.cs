using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceInvokePropSet : TraceInvoke
    {
        public string PropertyName { get; set; }

        public TraceInvokePropSet(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {

            this.PropertyName = MethodName.Replace("propSet_", "");
            
        }
    }
}
