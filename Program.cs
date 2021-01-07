using System;
using System.IO;

namespace hexDump
{
    class Program
    {
        /// <summary>Writes string outString into file at outFilePath</summary>
        /// <param name="outFilePath">output file path</param>
        /// <param name="outString">output string</param>
        static void writeString(string outFilePath, string outString) {
            using(FileStream fs = File.Open(outFilePath, FileMode.Append)) {
                using(StreamWriter sw = new StreamWriter(fs)) {
                    sw.Write(outString);
                }
            }
        }

        /// <summary>Writes byte outByte into file at outFilePath</summary>
        /// <param name="outFilePath">output file path</param>
        /// <param name="outByteBuffer">output byte buffer</param>
        static void writeByte(string outFilePath, byte[] outByteBuffer) {
            using(FileStream fs = File.Open(outFilePath, FileMode.Append)) {
                fs.Write(outByteBuffer);
            }
        }

        /// <summary>Dumps file into hex output</summary>
        /// <param name="filePath">path to file</param>
        /// <param name="toScreen">output to screen (true) or to file (false)</param>
        static void dumpFile(string filePath, bool toScreen) {
            // set up output file path & create file if needed
            string outFilePath = filePath + ".hexdump";
            if(!toScreen) {
                Console.WriteLine("Output file: \"" + outFilePath + "\"");
                File.Create(outFilePath).Close();
            }
            using(FileStream fs = File.OpenRead(filePath)) {
                char[] charBuffer = new char[16]; // character buffer for ascii output
                byte cBCount = 0;                 // with counter
                byte outByte;                     // output variable
                string lineBuffer = "";           // line buffer for speeding up file operations slightly
                // go through the file reading bytes
                for(int i = 0; i < fs.Length; i++) {
                    outByte = (byte) fs.ReadByte();
                    // convert to two-digit hex and output
                    lineBuffer += outByte.ToString("X2") + " ";
                    // convert to ascii and add to buffer
                    if(outByte > 31) charBuffer[cBCount++] = Convert.ToChar(outByte);
                    else charBuffer[cBCount++] = '.';
                    // once buffer is full, write ascii output and new line
                    if(cBCount == charBuffer.Length) {
                        cBCount = 0;
                        foreach(char c in charBuffer) {
                            lineBuffer += c.ToString();
                        }
                        lineBuffer += "\n";
                        if(toScreen) Console.Write(lineBuffer);
                        else writeString(outFilePath, lineBuffer);
                        lineBuffer = "";
                    }
                }
                // after end of file, are there still characters in the buffer?
                if(cBCount > 0) {
                    // output remainder of line buffer (doesn't contain ascii output)
                    if(toScreen) Console.Write(lineBuffer);
                    else writeString(outFilePath, lineBuffer);
                    // fix spacing
                    for(int i = 0; i < charBuffer.Length - cBCount; i++) {
                        if(toScreen) Console.Write("   ");
                        else writeString(outFilePath, "   ");
                    }
                    // output remainder of character buffer
                    for(int i = 0; i < cBCount; i++) {
                            if(toScreen) Console.Write(charBuffer[i]);
                            else writeString(outFilePath, charBuffer[i].ToString());
                        }
                        if(toScreen) Console.WriteLine();
                }
            }
        }

        /// <summary>Reconstructs file from hex input</summary>
        /// <param name="filePath">path to file</param>
        /// <param name="toScreen">output to screen (true) or to file (false)</param>
        static void reconstructFile(string filePath, bool toScreen) {
            // create output file if needed
            string outFilePath = filePath.Substring(0, filePath.Length - Path.GetExtension(filePath).Length);
            if(!toScreen) {
                Console.WriteLine("Output file: \"" + outFilePath + "\"");
                File.Create(outFilePath).Close();
            }
            using(FileStream fs = File.OpenRead(filePath)) {
                using(StreamReader sr = new StreamReader(fs)) {
                    string line;
                    while((line = sr.ReadLine()) != null) {
                        // hex data are in the first 48 characters of each line, then comes the ascii representation
                        line = line.Substring(0, 48);
                        // delete whitespaces
                        line = line.Replace(" ", "");
                        // split into chunks of 2
                        string[] arrOfHex = new string[line.Length / 2];
                        for(int i = 0; i < line.Length / 2; i++) {
                            arrOfHex[i] = line.Substring(i*2, 2);
                        }
                        // write to console
                        if(toScreen) {
                            string lineBuffer = "";
                            for(int i = 0; i < arrOfHex.Length; i++) {
                                if(arrOfHex[0] == "EF" && arrOfHex[1] == "BB" && arrOfHex[2] == "BF" && i < 3) i = 3; // ignore unicode header, UTF-8
                                if(arrOfHex[0] == "FF" && arrOfHex[1] == "FE" && i < 2) i = 2; // ignore unicode header, UTF-16 LE
                                if(arrOfHex[0] == "FE" && arrOfHex[1] == "FF" && i < 2) i = 2; // ignore unicode header, UTF-16 BE
                                if(arrOfHex[0] == "FF" && arrOfHex[1] == "FE" && arrOfHex[2] == "00" && arrOfHex[3] == "00" && i < 4) i = 4; // ignore unicode header, UTF-32 LE
                                if(arrOfHex[0] == "00" && arrOfHex[1] == "00" && arrOfHex[2] == "FE" && arrOfHex[3] == "FF" && i < 4) i = 4; // ignore unicode header, UTF-32 BE
                                byte charCode = byte.Parse(arrOfHex[i], System.Globalization.NumberStyles.HexNumber); // parse hex string into byte
                                if(charCode == 0x0a) lineBuffer += Convert.ToChar(charCode).ToString();   // acknowledge linefeed
                                if(charCode > 31) lineBuffer += Convert.ToChar(charCode).ToString(); // otherwise ignore non-printable characters
                            }
                            Console.Write(lineBuffer);
                        }
                        // write to file (buffer for each line of source file, probably a bit faster)
                        else {
                            byte[] lineBuffer = new byte[arrOfHex.Length];
                            for(int i = 0; i < arrOfHex.Length; i++) {
                                // parse hex string into byte
                                lineBuffer[i] = byte.Parse(arrOfHex[i], System.Globalization.NumberStyles.HexNumber);
                            }
                            writeByte(outFilePath, lineBuffer);
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            // get file path
            string filePath;
            // from arg if available
            if(args.Length >= 1) filePath = args[0];
            // if not then from console input
            else {
                Console.Write("Path to file: ");
                filePath = Console.ReadLine();
            }
            // dump or reconstruct?
            bool toDump = true;
            if(Path.GetExtension(filePath) == ".hexdump") toDump = false; // if file is a hexdump then reconstruct
            // to new file or to console
            bool toScreen;
            // from arg if available
            if(args.Length == 2) {
                toScreen = true;
                if(args[1] == "f") toScreen = false;
            // otherwise from console input
            } else {
                Console.Write("To (S)creen or to (F)ile? ");
                ConsoleKeyInfo sOrF = Console.ReadKey();
                toScreen = false;
                if(sOrF.Key == ConsoleKey.S) toScreen = true;
            }
            // if dump then dump
            if(toDump) { 
                Console.WriteLine("\nDumping file \"" + filePath + "\":\n");
                dumpFile(filePath, toScreen);
            // else reconstruct
            } else {
                Console.WriteLine("\nReconstructing file \"" + filePath + "\":\n");
                reconstructFile(filePath, toScreen);
            }
        }
    }
}
