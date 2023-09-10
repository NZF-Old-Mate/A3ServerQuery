/* Author: [NZF] Old Mate
 *
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
 *
 *
 */




using System.Net;
using System.Net.Sockets;
using System.Text;

namespace A2S
{
    public class Query
    {
        //1 - Send a U packet with no other information to request a challenge 
        static readonly byte[] Request = { 0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF }; //yyyyUyyyy

        public byte[]? A2S_Response;

        public static byte[] QueryPlayers(string address, int port, int timeout)
        {
            //Store the endpoint as an IPEndPoint object
            var endPoint = new IPEndPoint(IPAddress.Parse(address), Convert.ToUInt16(port));

            //Create a new UdpClient
            using var udpClient = new UdpClient();

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

            //Create an empty byte array to return in case of an error
            byte[] ErrorArray = { };

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
                    //string str = BitConverter.ToString(A2S_Response);
                    //Console.WriteLine(str);

                    //Return the response in byte array form
                    return A2S_Response;
                }
                else
                {
                    //Bad response, log error, close out
                    Console.WriteLine($"ERROR: Unexpected response, expected Length 9 and type A, got Length {response.Length} and type {response[4]}");
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

            string currentDateTime = DateTime.UtcNow.ToString("[dd,MM,yy,hh,mm]");
            //Output is an Object called PlayerData that contains an array of  { username , current DateTimeUTC } 
            string outputJSON = $"{{ \"PlayerData\" :[";
            int counter = 6; //Ignore header, start at byte 7
            while (counter < rawResponse.Length)
            {

                counter++; //advance 1

                string? utf8string = ReadNullTerminatedString(rawResponse, ref counter);
                if (utf8string == null) //Catch any issues with reading the username
                {
                    Console.WriteLine("ERROR: could not interpret null terminated string, exiting");
                    //return "ERROR: could not interpret null terminated string, exiting";
                }
                //Dirty AF built-in JSON encoder
                // { arma3ProfileName : currentDateTime }
                // outputs as
                //      { a3ProfileName :"bobsmith69420", DateTime :"dd,MM,yy,hh,mm" }, 
                string outJsonLine = $"{{ \"a3ProfileName\" :\"{utf8string}\", \"DateTime\" :\"{currentDateTime}\"}},";
                //DEBUG
                Console.WriteLine(outJsonLine);
                outputJSON += outJsonLine;


                counter += 8; //Advance 8 bytes, we don't care about the rest of the chunk.
            }
            //Trim off the trailing comma, close the array, close the object.
            outputJSON = outputJSON.TrimEnd(',');
            outputJSON += $"]}}";

            //DEBUG
            //Console.WriteLine(outputJSON);

            return outputJSON;

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

    }

}
