using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceInvoke : TraceEntry
    {
        public string MethodName { get; set; }
        public string FullClassName { get; set; }
        public string ClassName { get; set; }
        public string Parameters { get; set; }
        public bool HasReturn { get; set; }
        public string ReturnValue { get; set; }

        public TraceInvoke(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            string[] parts = content.Split(' ');
            this.MethodName = parts[1];

            if (parts[2] == "in")
            {
                this.FullClassName = parts[3];
            }
            else
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == "-")
                    {
                        this.FullClassName = parts[i + 1];
                        break;
                    }
                }
            }

            this.ClassName = this.FullClassName.Split('.').Last();

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
