using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceAnalysis;

namespace TraceConsole
{
    class Program
    {
        static TimeSpan threshold;
        static int Main(string[] args)
        {
            int indentLevel = 0;
            threshold = TimeSpan.FromMinutes(1);

            string filename = "";
            string outputFile = @"output.txt";
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: TraceConsole {input_file} {output_file} [marker_threshold]");
                    return -1;
                }
                if (args.Length >= 1)
                {
                    filename = args[0].ToString();
                }
                if (args.Length >= 2)
                {
                    outputFile = args[1].ToString();
                }
                if (args.Length >= 3)
                {
                    threshold = TimeSpan.FromSeconds(int.Parse(args[2]));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("!! Error parsing parameters: {0}", ex.Message);
                return 1;
            }

            Console.WriteLine("Analysing input file {0} to create output file {1}...", filename, outputFile);
            Console.WriteLine("Marker (=>) threshold set to {0}", threshold.TotalSeconds.ToString());
            if (!File.Exists(filename))
            {
                Console.WriteLine("!! Input file [{0}] was not found!", filename);
                return 2;

            }
            
            TraceAnal traceAnal = new TraceAnal();
            //traceAnal.Analyse(@"C:\workspaces\stp\rcode-05-01\Stocktrack Login.log");
            try
            {
                traceAnal.Analyse(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error analysing file: {0}\n{1}", ex.Message, ex.StackTrace);
            }
            Console.WriteLine("Parsed {0} lines of {1} to create {2} entries.", traceAnal.ParsedLines, traceAnal.InputLines, traceAnal.TraceHeaders.Count);
            TextWriter stdOut = Console.Out;
            StreamWriter writer = File.CreateText(outputFile);
            Console.SetOut(writer);


            //check for UI triggered pauses and clear the durations

            /*TraceHeader thThis = null;
            TraceHeader thNext = null;
            for (int i = 0; i < numHeaders; i++)
            {
                thThis = traceAnal.TraceHeaders[i];
                int iNext = i + 1;
                if (iNext < numHeaders)
                {
                    thNext = traceAnal.TraceHeaders[iNext];
                    if (thNext.TraceEntry.IsUITriggered
                        || thThis.TraceEntry.IsUITriggered)
                    {
                        thThis.Duration = TimeSpan.Zero;
                    }
                }
            }*/

            int numHeaders = traceAnal.TraceHeaders.Count;
            TraceHeader th = null;
            for (int i = 0; i < numHeaders; i++)
            //foreach (var th in traceAnal.TraceHeaders)
            {
                th = traceAnal.TraceHeaders[i];
                if (th.TraceEntryType == TraceEntryTypes.Configuration)
                {
                    Console.Write(th.TraceEntry.Content);
                }
                else
                {
                    if (th.TraceEntry is TraceStartupProcedure)
                    {
                        TraceStartupProcedure tsp = th.TraceEntry as TraceStartupProcedure;

                        LogTimes(th);
                        Indent(indentLevel);
                        indentLevel++;
                        Console.Write("Startup Procedure {0}.", tsp.Procedure);
                        Console.WriteLine();
                    }
                    else if (th.TraceEntry is TraceRun
                        || th.TraceEntry is TraceRunMain)
                    {
                        TraceRun tr = th.TraceEntry as TraceRun;

                        LogTimes(th);

                        Indent(indentLevel);
                        if ((!tr.IsInternalProcedure)
                            || tr.HasReturn)
                        {
                            indentLevel++;
                        }
                        Console.Write("Run ");
                        if (tr.InternalProcedure != null)
                        {
                            Console.Write("{0} [{1}]        <<{2}>>", tr.InternalProcedure, tr.Procedure, tr.Content);
                        }
                        else
                        {
                            Console.Write("{0}        <<{1}>>", tr.Procedure, tr.Content);
                        }
                        Console.WriteLine();
                    }
                    else if (th.TraceEntry is TraceDelete)
                    {
                        TraceDelete td = th.TraceEntry as TraceDelete;

                        LogTimes(th);

                        Indent(indentLevel);

                        indentLevel++;
                        Console.Write("Delete ");
                        Console.Write("{0} [{1}]        <<{2}>>", td.MethodName, td.ClassName, td.Content);
                        Console.WriteLine();
                    }
                    else if (th.TraceEntry is TraceFunc)
                    {
                        TraceFunc tf = th.TraceEntry as TraceFunc;

                        LogTimes(th);
                        Indent(indentLevel);
                        indentLevel++;

                        string function;
                        if (tf is TraceFuncPropGet)
                        {
                            Console.Write("Get Property ");
                            function = (tf as TraceFuncPropGet).PropertyName;
                        }
                        else if (tf is TraceFuncPropSet)
                        {
                            Console.Write("Set Property ");
                            function = (tf as TraceFuncPropSet).PropertyName;
                        }
                        else
                        {
                            Console.Write("Func ");
                            function = tf.Function;
                        }

                        if (tf.Parameters != null)
                        {
                            Console.Write("{0} [{1}]        <<{2}>>", function, tf.Parameters, tf.Content);
                        }
                        else
                        {
                            Console.Write("{0}        <<{1}>>", function, tf.Content);
                        }
                        Console.WriteLine();
                    }
                    else if (th.TraceEntry is TracePublish)
                    {
                        TracePublish tp = th.TraceEntry as TracePublish;

                        LogTimes(th);
                        Indent(indentLevel);
                        indentLevel++;
                        Console.Write("Publish ");
                        Console.Write("{0}        <<{1}>>", tp.EventName, tp.Content);

                        Console.WriteLine();
                    }
                    else if (th.TraceEntry is TraceReturn)
                    {
                        TraceReturn tr = th.TraceEntry as TraceReturn;

                        LogTimes(th);
                        if (tr.IsStaticReturn)
                        {
                            Indent(indentLevel);
                        }
                        else
                        {
                            Indent(indentLevel);
                        }
                        if ((!tr.IsUnlinkedReturn))
                        {
                            indentLevel--;
                        }

                        if (tr.IsStaticReturn)
                        {
                            Console.Write("Static Constructor ");
                        }
                        if (tr.IsUnlinkedReturn)
                        {
                            //Console.Write("UNLINKED ");
                        }

                        Console.Write("Return");

                        string returnFrom;
                        if (tr.IsPropertyReturn)
                        {
                            returnFrom = tr.PropertyName;
                        }
                        else
                        {
                            returnFrom = tr.ReturnFrom;
                        }

                        if (tr.InInternalProcedure != null)
                        {
                            Console.Write(" from {0} ", returnFrom);
                        }

                        if (tr.ReturnValue != null)
                        {
                            Console.Write("[{0}] ", tr.ReturnValue);
                        }
                        else
                        {
                            Console.Write("        <<{0}>>", tr.Content);
                        }
                        Console.WriteLine();
                    }
                    else if (th.TraceEntry is TraceSubscribe)
                    {
                        TraceSubscribe ts = th.TraceEntry as TraceSubscribe;
                        LogTimes(th);
                        Indent(indentLevel);
                        Console.Write("Subscribing to {0} in {1}", ts.EventName, ts.TargetName);
                        Console.WriteLine();
                    }
                    else if (th.TraceEntry is TraceNew)
                    {
                        TraceNew tn = th.TraceEntry as TraceNew;
                        LogTimes(th);
                        Indent(indentLevel);
                        indentLevel++;
                        Console.Write("New of {0} ({1})        <<{2}>>", tn.ClassName, tn.FullClassName, tn.Content);
                        Console.WriteLine();
                    }
                    else if (th.TraceEntry is TraceInvoke)
                    {
                        TraceInvoke ti = th.TraceEntry as TraceInvoke;

                        LogTimes(th);
                        Indent(indentLevel);
                        indentLevel++;

                        string methodName;
                        if (ti is TraceInvokePropGet)
                        {
                            Console.Write("Get Property ");
                            methodName = (ti as TraceInvokePropGet).PropertyName;
                        }
                        else if (ti is TraceInvokePropSet)
                        {
                            Console.Write("Set Property ");
                            methodName = (ti as TraceInvokePropSet).PropertyName;
                        }
                        else
                        {
                            Console.Write("Invoke ");
                            methodName = ti.MethodName;
                        }
                        Console.Write("{0} [{1}] ({2})       <<{3}>>", methodName, ti.ClassName, ti.Parameters, ti.Content);

                        Console.WriteLine();
                    }
                    //else if (th.TraceEntry is TraceDelete)
                    //{
                    //    TraceDelete td = th.TraceEntry as TraceDelete;

                    //    LogTimes(th);
                    //    Indent(indentLevel);

                    //    Console.Write("Delete ");
                    //    Console.Write("{0} [{1}]        <<{2}>>", td.MethodName, td.ClassName, td.Content);
                    //    Console.WriteLine();
                    //}
                    else if (th.TraceEntry is TraceSuper)
                    {
                        TraceSuper ts = th.TraceEntry as TraceSuper;

                        LogTimes(th);
                        Indent(indentLevel);
                        indentLevel++;
                        Console.Write("Super ");
                        Console.Write("{0} [{1}]        <<{2}>>", ts.ClassName, ts.FullClassName, ts.Content);
                        Console.WriteLine();
                    }
                    else if (th.TraceEntry is TraceMessageLine)
                    {
                        TraceMessageLine tm = th.TraceEntry as TraceMessageLine;

                        LogTimes(th);
                        Indent(indentLevel);
                        Console.Write("MessageLine ");
                        Console.Write("{0}", tm.Message);
                        Console.WriteLine();
                    }
                    else if (th.TraceEntry is ApplicationMessageLine)
                    {
                        ApplicationMessageLine tm = th.TraceEntry as ApplicationMessageLine;

                        LogTimes(th);
                        Indent(indentLevel);
                        Console.Write("ApplicationMessageLine ");
                        Console.Write("{0}", tm.Message);
                        Console.WriteLine();
                    }

                    else if (th.TraceEntry != null)
                    {
                        Console.WriteLine(th.TraceEntry.Content);
                    }
                }

            }

            Console.WriteLine("Total Lines: {0}, Lines Parsed: {1}, Unparsed: {2}", traceAnal.InputLines, traceAnal.ParsedLines, traceAnal.UnparsedLines);

            writer.Close();
            writer.Dispose();

            Console.SetOut(stdOut);
            Console.WriteLine("Complete.");

            Console.ReadLine();

            return 0;
        }

        private static void LogTimes(TraceHeader th)
        {
            Console.Write("{0}", th.OccurredAt);
            if (th.Duration == TimeSpan.Zero)
            {
                Console.Write("  |               ");
            }
            else
            {
                if (th.Duration > threshold)
                {
                    if (th.NextHeader != null
                        && th.NextHeader.TraceEntry.IsUITriggered)
                    {
                        Console.Write("@>");
                    }
                    else
                    {
                        Console.Write("=>");
                    }
                }
                else
                {
                    Console.Write("  ");
                }
                Console.Write("{0:dd\\.hh\\:mm\\:ss\\.fff} ", th.Duration);
            }
        }

        private static void Indent(int level)
        {
            for (int indent = 0; indent < level; indent++)
            {
                Console.Write("  ");
            }
        }
    }
}
