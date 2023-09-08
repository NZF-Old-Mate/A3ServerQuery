# A3ServerQuery
~~Status: Barely functional, if you're feeling generous.~~


Known:
- ~~Fails to receive full data from the servers being queried. Don't yet know why.~~ fixed, PEBKAC error, was aiming at the wrong port.
- ~~Sometimes receives only 1 packet of 8B, sometimes will get more. Investigating.~~ fixed, see above
- ~~OB1 problems in A2SInterpreter~~ fixed
- ~~Documentation for A2S+Arma3 (and just A2S in general) is hot garbage.~~ Not fixed, but nothing I can do about that.

## New and Improved

- Updated to .NET 7.0
- Removed extraneous iterations from A2SInterpreter, solving the Ob1 issues.
- Removed interpretation of Duration and Score from A2SInterpreter, as they are not needed.
- A2SInterpreter now encodes the usernames and current date/time into a JSON object containing an array of JSON objects and returns this as a string. 
- The program will now create a file in My Documents called outputTest.json to store the output. If outputTest.json already exists it will append to it. This is a temporary measure for now until some actual data handling can be built.
- Cleaned up a ton of unused code and unnecessary comments.

## TODO
- Actual data handling/storage
  - Ideally, will read existing .json, modify/append where required, then update the file. 
- Ability to accept IP, port, and output location as arguments via console.
