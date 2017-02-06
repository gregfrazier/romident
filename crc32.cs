using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomIdent2
{
    /// <summary>
    /// This code is everywhere on the net, I have no clue who wrote it.
    /// It's usually presented in C or C++ with the exact same comments.
    /// I'm pretty sure this is java code that was fixed up to compile in C#
    /// I assume it's public domain.
    /// </summary>
    public class crc32
    {
        private int[] iTable;

        public crc32()
        {
            this.iTable = new int[256];
            Init();
        }

        ///**
        // * Initialize the iTable aplying the polynomial used by PKZIP, WINZIP and Ethernet.
        // */
        private void Init()
        {
            // 0x04C11DB7 is the official polynomial used by PKZip, WinZip and Ethernet.
            int iPolynomial = 0x04C11DB7;

            // 256 values representing ASCII character codes.
            for (int iAscii = 0; iAscii <= 0xFF; iAscii++)
            {
                this.iTable[iAscii] = this.Reflect(iAscii, (byte)8) << 24;

                for (int i = 0; i <= 7; i++)
                {
                    if ((this.iTable[iAscii] & 0x80000000L) == 0) this.iTable[iAscii] = (this.iTable[iAscii] << 1) ^ 0;
                    else this.iTable[iAscii] = (this.iTable[iAscii] << 1) ^ iPolynomial;
                }
                this.iTable[iAscii] = this.Reflect(this.iTable[iAscii], (byte)32);
            }
        }

        ///**
        // * Reflection is a requirement for the official CRC-32 standard. Note that you can create CRC without it,
        // * but it won't conform to the standard.
        // *
        // * @param iReflect
        // *           value to apply the reflection
        // * @param iValue
        // * @return the calculated value
        // */
        private int Reflect(int iReflect, int iValue)
        {
            int iReturned = 0;
            // Swap bit 0 for bit 7, bit 1 For bit 6, etc....
            for (int i = 1; i < (iValue + 1); i++)
            {
                if ((iReflect & 1) != 0)
                {
                    iReturned |= (1 << (iValue - i));
                }
                iReflect >>= 1;
            }
            return iReturned;
        }

        ///**
        // * PartialCRC caculates the CRC32 by looping through each byte in sData
        // *
        // * @param lCRC
        // *           the variable to hold the CRC. It must have been initialize.
        // *           <p>
        // *           See fullCRC for an example
        // *           </p>
        // * @param sData
        // *           array of byte to calculate the CRC
        // * @param iDataLength
        // *           the length of the data
        // * @return the new caculated CRC
        // */
        public long CalculateCRC(long lCRC, byte[] sData, int iDataLength)
        {
            for (int i = 0; i < iDataLength; i++)
            {
                lCRC = (lCRC >> 8) ^ (long)(this.iTable[(int)(lCRC & 0xFF) ^ (int)(sData[i] & 0xff)] & 0xffffffffL);
            }
            return lCRC;
        }

        ///**
        // * Caculates the CRC32 for the given Data
        // *
        // * @param sData
        // *           the data to calculate the CRC
        // * @param iDataLength
        // *           then length of the data
        // * @return the calculated CRC32
        // */
        public long FullCRC(byte[] sData, int iDataLength)
        {
            long lCRC = 0xffffffffL;
            lCRC = this.CalculateCRC(lCRC, sData, iDataLength);
            return (lCRC /*& 0xffffffffL)*/^ 0xffffffffL);
        }

        ///**
        // * Calculates the CRC32 of a file
        // *
        // * @param sFileName
        // *           The complete file path
        // * @param context
        // *           The context to open the files.
        // * @return the calculated CRC32 or -1 if an error occurs (file not found).
        // */
        public long FileCRC(FileStream file)
        {
            long iOutCRC = 0xffffffffL;

            int iBytesRead = 0;
            int buffSize = 32 * 1024;
            try
            {
                byte[] data = new byte[buffSize];
                try
                {
                    while ((iBytesRead = file.Read(data, 0, buffSize)) > 0)
                    {
                        iOutCRC = this.CalculateCRC(iOutCRC, data, iBytesRead);
                    }
                    return (iOutCRC ^ 0xffffffffL);
                }
                catch (Exception)
                {
                    // Error reading file
                }
            }
            catch (Exception e)
            {
                // file not found
            }
            return -1L;
        }

        public byte[] ComputeHash(Stream file)
        {
            long iOutCRC = 0xffffffffL;

            int iBytesRead = 0;
            int buffSize = 32 * 1024;
            try
            {
                byte[] data = new byte[buffSize];
                try
                {
                    while ((iBytesRead = file.Read(data, 0, buffSize)) > 0)
                    {
                        iOutCRC = this.CalculateCRC(iOutCRC, data, iBytesRead);
                    }
                    return BitConverter.GetBytes((Int32)(iOutCRC ^ 0xffffffff));//iOutCRC ^ 0xffffffffL);
                }
                catch (Exception)
                {
                    // Error reading file
                }
            }
            catch (Exception e)
            {
                // file not found
            }
            return BitConverter.GetBytes(-1L);
        }
    }
}
