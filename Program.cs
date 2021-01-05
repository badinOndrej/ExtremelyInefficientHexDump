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
        /// <param name="outByte">output byte</param>
        static void writeByte(string outFilePath, byte outByte) {
            using(FileStream fs = File.Open(outFilePath, FileMode.Append)) {
                fs.WriteByte(outByte);
            }
        }

        /// <summary>Dumps file into hex output</summary>
        /// <param name="filePath">path to file</param>
        /// <param name="toScreen">output to screen (true) or to file (false)</param>
        static void dumpFile(string filePath, bool toScreen) {
            string outFilePath = filePath + ".hexdump";
            if(!toScreen) Console.WriteLine("Output file: \"" + outFilePath + "\"");
            using(FileStream fs = File.OpenRead(filePath)) {
                char[] charBuffer = new char[16];
                byte cBCount = 0;
                byte outByte;
                for(int i = 0; i < fs.Length; i++) {
                    outByte = (byte) fs.ReadByte();
                    if(toScreen) Console.Write(outByte.ToString("X2") + " ");
                    else writeString(outFilePath, outByte.ToString("X2") + " ");
                    if(outByte > 31) charBuffer[cBCount++] = Convert.ToChar(outByte);
                    else charBuffer[cBCount++] = '.';
                    if(cBCount == 15) {
                        cBCount = 0;
                        foreach(char c in charBuffer) {
                            if(toScreen) Console.Write(c);
                            else writeString(outFilePath, c.ToString());
                        }
                        if(toScreen) Console.WriteLine();
                        else writeString(outFilePath, "\n");
                    }
                }
                // after end of file, are there still characters in the buffer?
                if(cBCount > 0) {
                    for(int i = 0; i < 15 - cBCount; i++) {
                        if(toScreen) Console.Write("   ");
                        else writeString(outFilePath, "   ");
                    }
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
            if(!toScreen) File.Create(outFilePath).Close();
            using(FileStream fs = File.OpenRead(filePath)) {
                using(StreamReader sr = new StreamReader(fs)) {
                    string line;
                    while((line = sr.ReadLine()) != null) {
                        // hex data are in the first 45 characters of each line, then comes the ascii representation
                        line = line.Substring(0, 45);
                        // delete whitespaces
                        line = line.Replace(" ", "");
                        // split into chunks of 2
                        string[] arrOfHex = new string[line.Length / 2];
                        for(int i = 0; i < line.Length / 2; i++) {
                            arrOfHex[i] = line.Substring(i*2, 2);
                        }
                        // write to console
                        if(toScreen) {
                            for(int i = 0; i < arrOfHex.Length; i++) {
                                if(arrOfHex[0] == "EF" && arrOfHex[1] == "BB" && arrOfHex[2] == "BF" && i < 3) i = 3; // ignore unicode header, UTF-8
                                if(arrOfHex[0] == "FF" && arrOfHex[1] == "FE" && i < 2) i = 2; // ignore unicode header, UTF-16 LE
                                if(arrOfHex[0] == "FE" && arrOfHex[1] == "FF" && i < 2) i = 2; // ignore unicode header, UTF-16 BE
                                if(arrOfHex[0] == "FF" && arrOfHex[1] == "FE" && arrOfHex[2] == "00" && arrOfHex[3] == "00" && i < 4) i = 4; // ignore unicode header, UTF-32 LE
                                if(arrOfHex[0] == "00" && arrOfHex[1] == "00" && arrOfHex[2] == "FE" && arrOfHex[3] == "FF" && i < 4) i = 4; // ignore unicode header, UTF-32 BE
                                byte charCode = byte.Parse(arrOfHex[i], System.Globalization.NumberStyles.HexNumber); // parse hex string into byte
                                if(charCode == 0x0a) Console.WriteLine();   // acknowledge linefeed
                                if(charCode > 31) Console.Write(Convert.ToChar(charCode)); // otherwise ignore non-printable characters
                            }
                        }
                        // write to file
                        else {
                            foreach(string hex in arrOfHex) {
                                byte charCode = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber); // parse hex string into byte
                                writeByte(outFilePath, charCode); // write into file
                            }
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
