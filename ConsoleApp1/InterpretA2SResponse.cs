/* Author: [NZF] Old Mate
 * 
 * Takes a byte array and interprets it according to the following: 
 *
 *
 *  [0] to [4] are always FF FF FF FF 44, denoting an A2S_PLAYER packet.
 *  [5] shows the number of players present at the time of the request
 *  [6]... is the player information as:
 *      [6] index of player chunk, uint8
 *      [7]... player name, null terminated string, UTF-8
 *      [?]... player score, int32 
 *      [?]... Duration, float, time in seconds of current session (float32)
 *
 * Currently dumps the interpreted info to console.
 * Planned: Divvy up the response into [player handle, time played, current time], discard score, pass data over to storage.
 *
 *
 *
 */


using System;
using System.Text;

namespace A2S
{
    public class Interpreter
    {
        public static void InterpretA2SResponse(byte[] rawResponse)
        {
            int counter = 6; //Ignore header, start at byte 7
            while (counter < rawResponse.Length)         
            {
                
                Console.WriteLine($"Index: {rawResponse[counter]}");
                counter++; //advance 1

                string? utf8string = ReadNullTerminatedString(rawResponse, ref counter);
                if (utf8string == null) //Catch any issues with reading the username
                {
                    Console.WriteLine("ERROR: could not interpret null terminated string, exiting");
                    //return "ERROR: could not interpret null terminated string, exiting";
                }
                Console.Write($"\tUsername: {utf8string}\n");

                //float playerScore = BitConverter.ToInt32(rawResponse, counter); //Score               
                //Console.Write($"\tScore: {playerScore.ToString()}");
                counter += 4; //Advance 4 bytes
                
                //float playerDuration = BitConverter.ToSingle(rawResponse, counter); //Time on server
                //Console.Write($"\tDuration: {playerDuration.ToString()} \n");
                counter += 4; //Advance 4 bytes 

                //counter++; //skip the terminating byte, loop.
            }
            
        }

        static string? ReadNullTerminatedString(byte[] rawResponse, ref int counter)
        {
            int start = counter;
            while (counter < rawResponse.Length && rawResponse[counter] != 0) //Check each byte, if it's an 0xFF then stop and proceed, otherwise advance the counter and check again 
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

