// See https://aka.ms/new-console-template for more information
using A2S;
using System.CommandLine;

namespace NZFTools;
internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var targetIP = new Option<string>(
            name: "--target",
            description: "IP Address of the server to query.",
            getDefaultValue: () => "103.62.49.18"
            )
        {
            IsRequired = true
        };
        targetIP.AddAlias("-t");

        var targetPort = new Option<int>(
            name: "--port",
            description: "Steam Query Port of the server. Always game port + 1",
            getDefaultValue: () => 2313
            )
        {
            IsRequired = true
        };
        targetPort.AddAlias("-p");

        var rootCommand = new RootCommand("NZF Tools CLI")
        {
            targetIP,
            targetPort
        };

        rootCommand.SetHandler(DoQuery, targetIP, targetPort);

        return await rootCommand.InvokeAsync(args);

    }

    private static void DoQuery(string targetIP, int targetPort)
    {
        string requestIP = targetIP;
        int requestPort = targetPort;
        int requestTimeout = 30;
        
        //TEMP: Make a document to store the output 
        string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ServerSessionLogs";

        //DEBUG
        Console.WriteLine($"Making Request to {requestIP} port {requestPort} with timeout {requestTimeout} seconds.");
        byte[] A2S_Response = A2STools.QueryPlayers(requestIP, requestPort, requestTimeout);

        //DEBUG
        Console.WriteLine($"DEBUG: Dumping raw response \n{BitConverter.ToString(A2S_Response)} ");
        Console.WriteLine($"Interpreting Response...");

        //Send the byteArray over to be interpreted and encoded to JSON
        string JSONout = Interpreter.InterpretA2SResponse(A2S_Response);

        using StreamWriter outputFile = new(Path.Combine(outputPath, "session_" + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".json"), true);
        outputFile.WriteLine(JSONout);
    }


}


