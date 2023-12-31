﻿/* Author: [NZF] Old Mate
 *
 *  QUERY
 *  Send an A2S Query, recieve the response from an Arma 3 server, and store the response as a byte array
 *  
 *  A2S Queries are UDP packets 9 bytes in size. Byte 4 denotes the packet type:
 *      - U denotes an A2S_PLAYER request
 *      - A denotes a challenge
 *      - D denotes A2S_PLAYER data
 *      
 *  Challenge/Response structure is:
 *      1 - Send a U packet with no additional information 
 *          0xFF 0xFF 0xFF 0xFF 0x55 0xFF 0xFF 0xFF 0xFF
 *          
 *      2 - Recieve an A packet with the challenge number
 *          0xFF 0xFF 0xFF 0xFF 0x41 0xAA 0xBB 0xCC 0xDD
 *          
 *      3 - Send a U packet with the given challenge number
 *          0xFF 0xFF 0xFF 0xFF 0x55 0xAA 0xBB 0xCC 0xDD
 *          
 *      4 - Recieve a D packet containing player information
 *          0xFF 0xFF 0xFF 0xFF 0x44 ...
 *  
 *  =======================================================================================================================
 *  
 *  INTERPRETER
 *  Interprets the output of QueryPlayers() (a byte array), extracts the relevant information, 
 *  and returns a json string containing said information.
 *  
 *  According to Valve's documentation, A2S query responses follow the format
 *  FF FF FF FF 44 ?? 00 ?? ??...  
 *  FF FF FF FF 44 is the header, 0x44 (D) denotes A2S Player data
 *  Byte 5 is the number of players currently online and effectively the number of player chunks 
 *  present in the payload. Theoretically A2S allows for multicasting if a payload is too large to 
 *  fit one packet but that is an edge case as far as this tool is concerned, thus handling multicast
 *  packets is not implemented.
 *  Byte 6 onwards is a player chunk, in the format:
 *          [Index (int)][Player Username (Null-terminated String)] [Score (double)] [Length of Session in seconds (float)]
 *  
 *  InterpretA2SResponse returns a JSON-formatted string with structure:
 *      {"PlayerData": {"a3ProfileName" : "User Profile Name" , "DateTime" : "[dd.MM,yy,mm,ss]" }, ... } *  
 *
 */




using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DataStorage;

namespace A2S
{
    public class A2STools
    {
        public static byte[] QueryPlayers(string address, int port, int timeout)
        {
            //Store the endpoint as an IPEndPoint object
            var endPoint = new IPEndPoint(IPAddress.Parse(address), Convert.ToUInt16(port));

            //Create a new UdpClient
            using var udpClient = new UdpClient();
            
            //Create a U packet with no other information to request a challenge 
            byte[] Request = { 0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF }; //yyyyUyyyy
            //Send a packet comprised of our predefined request, the length of the request, and the address to send it to
            udpClient.Send(Request, Request.Length, endPoint);
            //DEBUG: print request info to console
            string reqstr = BitConverter.ToString(Request);
            Console.WriteLine($"Sending message: {reqstr}");

            //Start listening for the response, terminate if no response heard before the timeout period elapses
            var asyncResponse = udpClient.BeginReceive(null, null);
            asyncResponse.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeout));

            //Create a byte array to hold the final response
            byte[] A2S_Response;

            //Create byte array to return in case of an error
            byte[] ErrorArray = { 0x6F, 0x68, 0x20, 0x64, 0x65, 0x61, 0x72, 0x21 }; //oh dear!

            //Start processing the recieved packet once it has completely arrived
            if (asyncResponse.IsCompleted)
            {
                //Store the response as a byte array
                byte[] response = udpClient.EndReceive(asyncResponse, ref endPoint);

                //Check to see if the response is a challenge. The challenge will have the format yyyyAabcd where abcd is the challenge number
                if (response.Length == 9 && response[4] == 0x41)
                {
                    //DEBUG: Print the response to console
                    string chalstr = BitConverter.ToString(response);
                    Console.WriteLine($"Challenge Recieved: {chalstr}");

                    //Complete the challenge/response procedure by sending yyyyUabcd
                    response[4] = 0x55;
                    udpClient.Send(response, response.Length, endPoint);
                    //DEBUG: Print challenge response to console
                    string chalstr2 = BitConverter.ToString(response);
                    Console.WriteLine($"Challenge Response: {chalstr2}");

                    //Store the final response as a byte array
                    A2S_Response = udpClient.Receive(ref endPoint);

                    //DEBUG print response to console as raw bytes
                    Console.WriteLine($"Recieved response of length {A2S_Response.Length}");

                    //Return the response in byte array form
                    return A2S_Response;
                }
                else
                {
                    //Bad response, log error, close out
                    Console.WriteLine($"ERROR: Unexpected response, expected Length 9 and type A, got Length {response.Length} and type {response[4]}. \n Dumping bad response: \n {response.ToString}");
                    udpClient.Close();
                    return ErrorArray;
                }

            }
            else
            {
                //Process never completed, log error, close out
                Console.WriteLine("ERROR: Incomplete response");
                udpClient.Close();
                return ErrorArray;
            }

        }

    }

    public class Interpreter
    {
        public static string InterpretA2SResponse(byte[] rawResponse)
        {
            //Returns a json-serialized object comprised of the current date/time and a list of usernames
            int counter = 6; //Ignore header, start at byte 7
            Session session = new Session();
            session.SessionDateTime = DateTime.UtcNow;
            session.OnlineUsersA3ProfileNames = new List<string>();
            while (counter < rawResponse.Length)
            {
                counter++; //advance 1
                string? A3ProfileName = ReadNullTerminatedString(rawResponse, ref counter);
                if (A3ProfileName == null) 
                {
                    Console.WriteLine("Null profile Name"); 
                } 
                else 
                {
                    session.OnlineUsersA3ProfileNames.Add(A3ProfileName); 
                }
                counter += 8; //Advance 8 bytes, we don't care about the rest of the chunk.
            }
            //DEBUG: Pretty-print json output to make it human-readable
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(session, options);
        }

        static string? ReadNullTerminatedString(byte[] rawResponse, ref int counter)
        {
            int start = counter;
            while (counter < rawResponse.Length && rawResponse[counter] != 0) //Check each byte, if it's an 0xFF then breakout, otherwise advance the counter and check again 
            {
                counter++;
            }
            if (counter < rawResponse.Length) //Encode all bytes between the start point and the current position of the counter as UTF8 string
            {
                string utf8string = Encoding.ASCII.GetString(rawResponse, start, counter - start);
                counter++;
                return utf8string;
            }
            return null; //Something went wrong
        }

        //ADD LATER - Parse and interpret queries, parse and interpret JSON.

    }

}
