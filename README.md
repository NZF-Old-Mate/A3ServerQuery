# A3ServerQuery
Status: ~~Barely functional, if you're feeling generous.~~ Barebones, but functional. WIP.

Known:
- ~~Fails to receive full data from the servers being queried. Don't yet know why.~~ fixed, PEBKAC error, was aiming at the wrong port.
- ~~Sometimes receives only 1 packet of 8B, sometimes will get more. Investigating.~~ fixed, see above
- ~~OB1 problems in A2SInterpreter~~ fixed
- ~~Documentation for A2S+Arma3 (and just A2S in general) is hot garbage.~~ Not fixed, but nothing I can do about that.
- JSON output is weirdly formatted - this is a placeholder until data handling is implemented
- No handling for empty responses

## New and Improved

- Updated to .NET 7.0
- Removed extraneous iterations from A2SInterpreter, solving the Ob1 issues.
- Removed interpretation of Duration and Score from A2SInterpreter, as they are not needed.
- A2SInterpreter now encodes the usernames and current date/time into a JSON object containing an array of JSON objects and returns this as a string. 
- The program will now create a file in My Documents called outputTest.json to store the output. If outputTest.json already exists it will append to it. This is a temporary measure for now until some actual data handling can be built.
- Cleaned up a ton of unused code and unnecessary comments.

## 8/9/23 CLI Update
- Implemented a barebones CLI
-  --target or -t to designate the target IP, defaults to NZF Operations Server
-  --port or -p to designate the target port, defaults to 2313

## 10/9/23 Refactor and cleanup
- Removed a bunch of unnecessary code
- Condensed Query and A2SInterpreter into a single file for portability, since Interpreter's function is always used with QueryPlayers()
- Query class is now called A2STools class in preparation for adding more query types. Interpeter remains the same. 

## TODO
- Actual data handling/storage
  - Ideally, will read existing .json, modify/append where required, then update the file.
  - Aiming for a DB-less storage solution.
- ~~Ability to accept IP, port, and output location as arguments via console.~~
- Implement ability to take commands from, and send output suitable for, a Discord bot.
