using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceRun : TraceEntry
    {
        public string Procedure { get; set; }
        public string InternalProcedure { get; set; }
        public string Parameters{ get; set; }
        public bool Persistent { get; set; }
        public string ReturnValue { get; set; }
        public bool HasReturn { get; set; }
        public bool IsSuper { get; set; }
        public bool IsInternalProcedure { get; set; }

        public TraceRun(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            HasReturn = true;
            IsSuper = false;
            
            string[] parts = content.Split(' ');

            // check for run without a procedure context
            if (!parts[1].EndsWith(".r")
                && !parts[1].EndsWith(".p")
                && !parts[1].EndsWith(".w"))
            {
                this.IsInternalProcedure = true;
                this.Procedure = "";

                if (parts[1].Equals("SUPER", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.InternalProcedure = ""; // parts[2].Substring(1);
                    this.IsSuper = true;
                }
                else
                {
                    this.InternalProcedure = parts[1];
                }
            }
            else
            {
                this.IsInternalProcedure = false;
                this.Procedure = parts[1];
                this.InternalProcedure = "Main Block";
            }
            
            if (parts[2] == "in")
            {
                this.InternalProcedure = parts[1];
                this.Procedure = parts[3];
                this.Persistent = false;
            }
            else
            {
                if (parts[2] == "PERSIST")
                {
                    this.Persistent = true;
                }
                else
                {
                    this.Persistent = false;
                }
            }

            int istart = content.IndexOf('"');
            if (istart > 0)
            {
                int iend = content.LastIndexOf('"');
                //only 1 quote?
                if (iend == istart)
                {
                    iend = content.Length - 1;
                }

                if ((iend == (istart + 1)))
                {
                    this.Parameters = "";
                }
                else if ((iend - istart) <= 0)
                {
                    Console.WriteLine("(iend - istart) <= 0");
                    this.Parameters = "";
                }
                else
                {
                    istart++;
                    this.Parameters = content.Substring(istart, iend - istart);
                }
            }
        }
    }
}
