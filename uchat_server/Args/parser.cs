using System.Net;
using System.Net.Sockets;

namespace uchat_server.Args;

public static class Parser
{
    private const string programName = "uchat_server";

    public static bool Parse(string[] args, out int port, out bool isDaemon)
    {
        port = -1;
        isDaemon = false;

        if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
        {
            PrintUsage();
            return false;
        }

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-d":
                case "--daemon":
                    isDaemon = true;
                    break;

                case "-p":
                case "--port":
                    if (i + 1 >= args.Length)
                    {
                        PrintError($"Option '{args[i]}' requires an argument.");
                        return false;
                    }

                    string portStr = args[i + 1];

                    if (!int.TryParse(portStr, out port) || port < 1 || port > 65535)
                    {
                        PrintError($"Invalid port number '{portStr}'. Must be 1-65535.");
                        return false;
                    }

                    if (!IsPortAvailable(port))
                    {
                        PrintError($"Port '{port}' is already in use or reserved.");
                        return false;
                    }

                    i++;
                    break;

                default:
                    PrintError($"Unrecognized option '{args[i]}'");
                    return false;
            }
        }

        if (port == -1)
        {
            PrintError("You must specify a port.");
            return false;
        }

        return true;
    }

    private static void PrintError(string message)
    {
        Console.WriteLine($"{programName}: {message}");
        Console.WriteLine($"Try '{programName} --help' for more information.");
    }

    private static void PrintUsage()
    {
        Console.WriteLine($@"
{programName}:
    Launches server for the uchat.

Usage:
    {programName} [options]

Options:
    -p, --port <num>    Specifies server port (Required)
    -d, --daemon        Starts server in background (detached)
    -h, --help          Prints this message

Example:
    {programName} -p 8100 -d");
    }

    static bool IsPortAvailable(int port) {
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
