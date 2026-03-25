using GoEngine;

namespace GoCli;

class Program
{

    static void Main(string[] args)
    {   

        bool debug;
        if (!checkArgs(args, out debug)) return;

        ConsoleInterface cli = new ConsoleInterface(debug);
        cli.Loop(); 
    }

    static bool checkArgs(string[] args, out bool debug)
    {
        debug = false;

        if (args.Length == 0)
            return true;

        if (args.Length > 1)
        {
            Console.WriteLine("Invalid arguments");
            return false;
        }

        string arg = args[0].ToLower();

        if (arg == "debug" || arg == "d")
        {
            debug = true;
            return true;
        }

        Console.WriteLine("Invalid argument");
        return false;
    }
}
