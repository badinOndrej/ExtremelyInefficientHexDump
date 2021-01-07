# ExtremelyInefficientHexDump
Extremely inefficient command line hex dump program  
Do not use for... Actually, don't use, full stop. Or do. I'm not telling you what to do, I'm not your mother.  
v0.3.inf slightly faster but still unusable for large files.

## Build requirements
.NET 5.0 SDK (https://dotnet.microsoft.com/download/dotnet/5.0)

## Build instructions
`git clone https://github.com/badinOndrej/ExtremelyInefficientHexDump`  
`cd ExtremelyInefficientHexDump`  
`dotnet build` for debug or `dotnet publish` for release or `dotnet run` to build debug & run

## Runtime requirements
.NET 5.0 Runtime (see above)

## Usage
`hexDump [path] [s|f]`  
s - output to screen  
f - output to file  
If no arguments are given, program will ask for required info.

If output is set to file, a .hexdump file is created.  
If output is set to file and input is a .hexdump file, reconstructs the original file from the hexdump.  
If output is set to screen, prints output to the command line window.
