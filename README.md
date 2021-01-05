# ExtremelyInefficientHexDump
Extremely inefficient command line hex dump program

# Build requirements
.NET 5.0 SDK (https://dotnet.microsoft.com/download/dotnet/5.0)

# Usage
`hexDump [path] [s|f]`  
s - output to screen  
f - output to file  
If no arguments are given, program will ask for required info.

If output is set to file, a .hexdump file is created.  
If output is set to file and input is a .hexdump file, reconstructs the original file from the hexdump.  
If output is set to screen, prints output to the command line window.
