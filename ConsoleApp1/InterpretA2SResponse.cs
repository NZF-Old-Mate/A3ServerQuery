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
 * New, better plan: Return the username and current date/time as JSON. 
 *  { "playerData": [ { "a3ProfileName" : "username", "dateTime" : "dd,MM,yyyy,hh,mm" } ] }
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

