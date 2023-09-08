// See https://aka.ms/new-console-template for more information
using System;
using System.Text;
using A2S;

internal class Program
{
    private static void Main(string[] args)
    {
        string requestIP = "103.212.224.189";
        int requestPort = 2303;
        int requestTimeout = 30;


        Console.WriteLine($"Making Request to {requestIP} port {requestPort} with timeout {requestTimeout} seconds.");
        byte[] A2S_Response = Query.QueryPlayers(requestIP, requestPort, requestTimeout);
        Console.WriteLine($"DEBUG: Dumping raw response \n {BitConverter.ToString(A2S_Response)} ");
        Console.WriteLine($"Interpreting Response...");
        Interpreter.InterpretA2SResponse(A2S_Response);
    }
}





