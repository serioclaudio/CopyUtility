using System;
using System.IO;

namespace FileCopyUtility_001
{
    class Program
    {
        static void Main(string[] args)
        {
            DoTest1(args);
        }

        static void DoTest1(string[] args)
        {
            if (args.Length == 0 )
            {
                Console.WriteLine("  Error:");
                Console.WriteLine("    Process File must be specified as a command-line parameter.");
                Console.WriteLine("    Correct usage: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " <ProcessFile.txt>");
                Console.WriteLine("      where <ProcessFile.txt> is the file containing the configuration tags for the application.");
                return;
            }

            cFileProcess cfp = new cFileProcess(args[0]);

            return;
        }
    }
}
