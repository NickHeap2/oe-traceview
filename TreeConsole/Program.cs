using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceAnalysis;

namespace TreeConsole
{
    class Program
    {
        static TimeSpan threshold;
        static int Main(string[] args)
        {
            threshold = TimeSpan.FromMinutes(1);

            string filename = @"D:\workspaces\oe-traceview\input.log";
            string outputFile = @"C:\temp\tree.txt";
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: TreeConsole {input_file} {output_file} [marker_threshold]");
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


            OutputTree(traceAnal.TraceEntryTree);


            Console.WriteLine("Total Lines: {0}, Lines Parsed: {1}, Unparsed: {2}", traceAnal.InputLines, traceAnal.ParsedLines, traceAnal.UnparsedLines);

            writer.Close();
            writer.Dispose();

            Console.SetOut(stdOut);
            Console.WriteLine("Complete.");

            Console.ReadLine();
            return 0;
        }

        private static void OutputTree(TraceEntry traceEntryTree)
        {
            OutputEntry(traceEntryTree, 0);
        }

        private static void OutputEntry(TraceEntry traceEntryTree, int indent)
        {
            OutputIndent(indent);
            Console.WriteLine("|{0}", traceEntryTree.Content);

            indent++;
            foreach (TraceEntry child in traceEntryTree.Children)
            {
                OutputEntry(child, indent);
            }
        }

        private static void OutputIndent(int level)
        {
            for (int indent = 0; indent < level; indent++)
            {
                Console.Write("  ");
            }
        }
    }
}
