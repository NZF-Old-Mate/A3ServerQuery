// See https://aka.ms/new-console-template for more information
using System;
using System.Text;
using System.IO;
using A2S;

internal class Program
{
    private static void Main(string[] args)
    {
        //IP Address of the server being queried 
        string requestIP = "14.1.30.99";
        //The Steam Query port is always the game port + 1
        int requestPort = 2318;
        //Wait time before giving up and closing the connection.
        int requestTimeout = 30;
        //TEMP: Make a document to store the output 
        string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        //DEBUG
        Console.WriteLine($"Making Request to {requestIP} port {requestPort} with timeout {requestTimeout} seconds.");
        byte[] A2S_Response = Query.QueryPlayers(requestIP, requestPort, requestTimeout);

        //DEBUG
        Console.WriteLine($"DEBUG: Dumping raw response \n {BitConverter.ToString(A2S_Response)} ");
        Console.WriteLine($"Interpreting Response...");

        //Send the byteArray over to be interpreted and encoded to JSON
        string JSONout = Interpreter.InterpretA2SResponse(A2S_Response);

        using StreamWriter outputFile = new(Path.Combine(outputPath, "outputTest.json"),true);
        outputFile.WriteLine(JSONout);




    }
}





