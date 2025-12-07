using System.Net;
using System.Net.Sockets;

namespace uchat_server.Args;

public static class Parser {
    static string program = "uchat_server";

    public static bool Parse(string[] args, out int port) {
        Option h = new Option("--help", "-h", "Prints this message"); 
        Option p = new Option("--port", "-p", "Specifies server port to connect to"); 
        if (args.Length == 0) {
            PrintUsage();
        } else if (h.HasSameName(args[0])) {
            PrintUsage();
        } else if (p.HasSameName(args[0])) {
            if (args.Length == 1) {
                Console.WriteLine($"{program}: option '{args[0]}' has too few arguments");
                Console.WriteLine($"Try '{program} --help' for more information.");
            } else if (int.TryParse(args[1], out port)) {
                if (!IsPortAvailible(port)) {
                    Console.WriteLine($"{program}: port '{args[1]}' is occupied or reserved");
                    Console.WriteLine($"Try using another one.");
                } else {
                    return true;
                }
            } else {
                Console.WriteLine($"{program}: argument '{args[1]}' for option '{args[0]}' is incorrect");
                Console.WriteLine($"Try '{program} --help' for more information.");
            }
        } else {
            Console.WriteLine($"{program}: unrecognized option '{args[0]}'");
            Console.WriteLine($"Try '{program} --help' for more information.");
        }

        port = -1;
        return false;
    }

    static void PrintUsage() {
        Console.WriteLine(@"uchat_server:
    Launches server for the uchat.
Usage:
    uchat_server [options]
Options:
    --port    -p    Specifies server port to connect to
    --help    -h    Prints this message
Example:
    uchat_server -p 8100");
    }

    static bool IsPortAvailible(int port) {
        TcpListener l = new TcpListener(IPAddress.Loopback, port);
        try {
            l.Start();
        } catch {
            return false;
        }
        l.Stop();
        return true;
    }
}
