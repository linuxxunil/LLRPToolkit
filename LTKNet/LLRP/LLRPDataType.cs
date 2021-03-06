
/*
 ***************************************************************************
 *  Copyright 2007 Impinj, Inc.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 ***************************************************************************
 */

/*
***************************************************************************
 * File Name:       LLRPDataType.cs
 *
 * Author:          Impinj
 * Organization:    Impinj
 * Date:            18 June, 2008
 *
 * Description:     This file contains supporting data types for encoding/
 *                  decoding LLRP messages/parameters
***************************************************************************
*/


using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Runtime.InteropServices;


namespace Org.LLRP.LTK.LLRPV1.DataType
{
    /// <summary>
    /// Parameter interface
    /// </summary>
    public interface IParameter
    {
        void ToBitArray(ref bool[] bit_array, ref int cursor);
    }

    /// <summary>
    /// LLRP parameter
    /// </summary>
    public abstract class Parameter : IParameter
    {
        protected UInt16 typeID;
        protected UInt16 length;
        protected bool tvCoding = false;

        public virtual void ToBitArray(ref bool[] bit_array, ref int cursor) { }
        public virtual Parameter FromBitArray(ref BitArray bit_array, ref int cursor, int length) { return null; }
        public virtual Parameter FromString(string xml) { return null; }

        public UInt16 TypeID { get { return typeID; } }
        public UInt16 Length { get { return length; } }
    }

    /// <summary>
    ///  Small Message ID class to hold a unique message ID number for packets
    /// </summary>
    public class MessageID
    {
        protected static object mutex = new object();
        protected static UInt32 sequence_num = 0;
        public static UInt32 getNewMessageID()
        {
            UInt32 retval;
            lock (mutex)
            {
                retval = sequence_num;
                sequence_num++;
            }
            return retval;
        }
    }

    /// <summary>
    /// LLRP Message
    /// </summary>
    public abstract class Message
    {
        protected UInt16 reserved;
        protected UInt16 version = 1;
        protected UInt16 msgType;
        protected UInt32 msgLen;
        protected UInt32 msgID;
        protected static UInt32 sequence_num = 0;

        public virtual bool[] ToBitArray() { return null; }
        public virtual Message FromBitArray(ref BitArray bit_array, ref int cursor, int length) { return null; }
        public virtual Message FromString(string xml) { return null; }

        public UInt32 MSG_ID { get { return msgID; } set { sequence_num = value; msgID = sequence_num; } }
        public UInt16 VERSION { get { return version; } set { version = value; } }
        public UInt32 Length { get { return msgLen; } set { msgLen = value; } }
        public UInt16 MSG_TYPE { get { return msgType; } set { msgType = value; } }
    }


    /// <summary>
    /// LLRPBitArray used to store "u1v" type
    /// </summary>
    [Serializable]
    public class LLRPBitArray
    {
        private List<bool> data = new List<bool>();

        /// <summary>
        /// Add bit to the array
        /// </summary>
        /// <param name="val"></param>
        public void Add(bool val)
        {
            data.Add(val);
        }

        public bool this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int Count
        {
            get { return data.Count; }
            set
            {
                if (data.Count > value)
                {
                    data.RemoveRange(value, data.Count - value);
                }
            }
        }

        /// <summary>
        /// Convert to bit array to Hex string
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            try
            {
                byte[] bD = Util.ConvertBitArrayToByteArray(data.ToArray());
                return Util.ConvertByteArrayToHexString(bD);
            }
            catch
            {
                return "code error!";
            }
        }

        /// <summary>
        /// Convert to Hex string in word order
        /// </summary>
        /// <returns></returns>
        public string ToHexWordString()
        {
            try
            {
                byte[] bD = Util.ConvertBitArrayToByteArray(data.ToArray());
                return Util.ConvertByteArrayToHexWordString(bD);
            }
            catch
            {
                return "code error!";
            }
        }

        /// <summary>
        /// Convert to decimal-based string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                byte[] bD = Util.ConvertBitArrayToByteArray(data.ToArray());

                string s = string.Empty;
                for (int i = 0; i < bD.Length; i++)
                {
                    s += Convert.ToInt16(bD[i]).ToString();
                    if (i + 1 < bD.Length)
                    {
                        s += " ";
                    }
                }

                return s;
            }
            catch
            {
                return "code error!";
            }
        }

        /// <summary>
        /// Convert a binary string to LLRPBitArray
        /// </summary>
        /// <param name="str">Binary string, for example: "1110010001"</param>
        /// <returns></returns>
        public static LLRPBitArray FromBinString(string str)
        {
            LLRPBitArray myBitArray = new LLRPBitArray();

            for (int i = 0; i < str.Length; i++)myBitArray.Add((str[i] == '1') ? true : false);

            return myBitArray;
        }

        /// <summary>
        /// Convert a Hex string to LLRPBitArray
        /// </summary>
        /// <param name="str">Hex string, for example: "EEFF"</param>
        /// <returns></returns>
        public static LLRPBitArray FromString(string str)
        {
            return LLRPBitArray.FromHexString(str);
        }

        /// <summary>
        /// Convert a Hex string to LLRPBitArray
        /// </summary>
        /// <param name="str">Hex string, for example: "EEFF"</param>
        /// <returns></returns>
        public static LLRPBitArray FromHexString(string str)
        {
            string s_temp = Util.ConvertHexStringToBinaryString(str);

            return LLRPBitArray.FromBinString(s_temp);
        }
    }

    ///// <summary>
    ///// Enumeration Array
    ///// </summary>
    //[Serializable]
    //public class EnumByteArray
    //{
    //    private List<byte> data = new List<byte>();
    //    Type t;

    //    public EnumByteArray(Type t)
    //    {
    //        this.t = t;
    //    }

    //    public void Add(byte val)
    //    {
    //        data.Add(val);
    //    }

    //    public byte this[int index]
    //    {
    //        get { return data[index]; }
    //        set { data[index] = value; }
    //    }

    //    public int Count
    //    {
    //        get { return data.Count; }
    //    }

    //    /// <summary>
    //    /// Convert to enumeration string
    //    /// </summary>
    //    /// <returns></returns>
    //    public string ToString()
    //    {
    //        string str = string.Empty;
    //        foreach (byte b in data)
    //        {
    //            str += Enum.GetName(t, 1) + " ";
    //        }
    //        return str;
    //    }
    //    /// <summary>
    //    /// Convert a enumeration string to byte array.
    //    /// </summary>
    //    /// <param name="str"></param>
    //    /// <returns></returns>
    //    public static ByteArray FromString(string str)
    //    {
    //        ByteArray bA = new ByteArray();
    //        str = str.Trim();

    //        string[] s = str.Split(new char[] { ',', ' ' });

    //        byte[] bD = new byte[s.Length];

    //        for (int i = 0; i < s.Length; i++)
    //        {
    //            try { bA.Add((byte)((int)Enum.Parse(t, s[i], true))); }
    //            catch { bA.Add(0); }
    //        }

    //        return bA;
    //    }

    //    /// <summary>
    //    /// Convert to byte array
    //    /// </summary>
    //    /// <returns></returns>
    //    public byte[] ToArray()
    //    {
    //        return data.ToArray();
    //    }
    //}



    /// <summary>
    /// Byte Array used to store "u8v" "endofbytes"
    /// </summary>
    [Serializable]
    public class ByteArray
    {
        private List<byte> data = new List<byte>();

        public void Add(byte val)
        {
            data.Add(val);
        }


        public byte this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int Count
        {
            get { return data.Count; }
        }

        /// <summary>
        /// Convert to Hex string
        /// </summary>
        /// <returns></returns>
        public string ToHexWordString()
        {
            return Util.ConvertByteArrayToHexWordString(data.ToArray());
        }

        /// <summary>
        /// Convert to Hex string
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            return Util.ConvertByteArrayToHexString(data.ToArray());
        }

        /// <summary>
        /// Convert a Hex string to byte array.
        /// </summary>
        /// <param name="str">Hex string. For example: "EE FF" or "EEFF" or "EE,FF"</param>
        /// <returns></returns>
        public static ByteArray FromHexString(string str)
        {
            ByteArray bA = new ByteArray();
            str = str.Trim();

            string[] s = Util.SplitString(str, new char[] { ',', ' ', '\t', '\n', '\r' }, 2);

            byte[] bD = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                try { bA.Add((byte)Byte.Parse(s[i], System.Globalization.NumberStyles.HexNumber)); }
                catch{bA.Add(0);}
            }

            return bA;
        }

        /// <summary>
        /// Convert a Dec string to byte array.
        /// </summary>
        /// <param name="str">Dec string. For example: "0 255" or "12,111"</param>
        /// <returns></returns>
        public static ByteArray FromString(string str)
        {
            ByteArray bA = new ByteArray();
            str = str.Trim();

            string[] s = str.Split(new char[] { ',', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            byte[] bD = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                try { bA.Add((byte)Byte.Parse(s[i], System.Globalization.NumberStyles.Integer)); }
                catch { bA.Add(0); }
            }

            return bA;
        }

        /// <summary>
        /// Convert a enumeration string to byte array.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ByteArray FromString(string str, Type t)
        {
            ByteArray bA = new ByteArray();
            str = str.Trim();

            string[] s = str.Split(new char[] { ',', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            byte[] bD = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                try { bA.Add((byte)((int)Enum.Parse(t, s[i], true))); }
                catch { bA.Add(0); }
            }

            return bA;
        }

        /// <summary>
        /// Convert to dec. based string. space is applied to each number
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                byte[] bD = data.ToArray();

                string s = string.Empty;
                for (int i = 0; i < bD.Length; i++) s += Convert.ToInt16(bD[i]).ToString() + " ";

                return s;
            }
            catch
            {
                return "code error!";
            }
        }

        /// <summary>
        /// Convert to enumeration string
        /// </summary>
        /// <returns></returns>
        public string ToString(Type t)
        {
            string str = string.Empty;
            foreach (byte b in data)
            {
                str += Enum.GetName(t, 1) + " ";
            }
            return str;
        }

        /// <summary>
        /// Convert to byte array
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return data.ToArray();
        }
    }

    /// <summary>
    /// Byte Array used to store "s8v"
    /// </summary>
    [Serializable]
    public class SignedByteArray
    {
        private List<sbyte> data = new List<sbyte>();

        public void Add(sbyte val)
        {
            data.Add(val);
        }


        public sbyte this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int Count
        {
            get { return data.Count; }
        }

        /// <summary>
        /// Convert to Hex string
        /// </summary>
        /// <returns></returns>
        public string ToHexWordString()
        {
            return Util.ConvertSignedByteArrayToHexWordString(data.ToArray());
        }

        /// <summary>
        /// Convert to Hex string
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            return Util.ConvertSignedByteArrayToHexString(data.ToArray());
        }

        /// <summary>
        /// Convert a Hex string to byte array.
        /// </summary>
        /// <param name="str">Hex string. For example: "EE FF" or "EEFF" or "EE,FF"</param>
        /// <returns></returns>
        public static SignedByteArray FromHexString(string str)
        {
            SignedByteArray bA = new SignedByteArray();
            str = str.Trim();

            string[] s = Util.SplitString(str, new char[] { ',', ' ', '\t', '\n', '\r' }, 2);

            sbyte[] bD = new sbyte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                try { bA.Add((sbyte)SByte.Parse(s[i], System.Globalization.NumberStyles.HexNumber)); }
                catch { bA.Add(0); }
            }

            return bA;
        }

        /// <summary>
        /// Convert a Dec string to byte array.
        /// </summary>
        /// <param name="str">Dec string. For example: "0 255" or "12,111"</param>
        /// <returns></returns>
        public static SignedByteArray FromString(string str)
        {
            SignedByteArray bA = new SignedByteArray();
            str = str.Trim();

            string[] s = str.Split(new char[] { ',', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            sbyte[] bD = new sbyte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                try { bA.Add((sbyte)SByte.Parse(s[i], System.Globalization.NumberStyles.Integer)); }
                catch { bA.Add(0); }
            }

            return bA;
        }

        /// <summary>
        /// Convert a enumeration string to byte array.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static SignedByteArray FromString(string str, Type t)
        {
            SignedByteArray bA = new SignedByteArray();
            str = str.Trim();

            string[] s = str.Split(new char[] { ',', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            sbyte[] bD = new sbyte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                try { bA.Add((sbyte)((int)Enum.Parse(t, s[i], true))); }
                catch { bA.Add(0); }
            }

            return bA;
        }

        /// <summary>
        /// Convert to dec. based string. space is applied to each number
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                sbyte[] bD = data.ToArray();

                string s = string.Empty;
                for (int i = 0; i < bD.Length; i++) s += Convert.ToInt16(bD[i]).ToString() + " ";

                return s;
            }
            catch
            {
                return "code error!";
            }
        }

        /// <summary>
        /// Convert to enumeration string
        /// </summary>
        /// <returns></returns>
        public string ToString(Type t)
        {
            string str = string.Empty;
            foreach (sbyte b in data)
            {
                str += Enum.GetName(t, 1) + " ";
            }
            return str;
        }

        /// <summary>
        /// Convert to byte array
        /// </summary>
        /// <returns></returns>
        public sbyte[] ToArray()
        {
            return data.ToArray();
        }
    }

    /// <summary>
    /// used to store "u16v"
    /// </summary>
    [Serializable]
    public class UInt16Array
    {
        public List<UInt16> data = new List<ushort>();

        public void Add(UInt16 val)
        {
            data.Add(val);
        }
        public UInt16 this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int Count
        {
            get { return data.Count; }
        }

        /// <summary>
        /// Convert to Hex string
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            string s = string.Empty;

            for (int i = 0; i < data.Count; i++)
            {
                UInt16 hD = (UInt16)(data[i] >> 8);
                UInt16 lD = (UInt16)(data[i] & 0x00FF);

                s += hD.ToString("X2") + lD.ToString("X2");
                if (i + 1 < data.Count)
                {
                    s += " ";
                }
            }

            return s;
        }

        /// <summary>
        /// Convert to Hex string in word order
        /// </summary>
        /// <returns></returns>
        public string ToHexWordString()
        {
            string s = string.Empty;

            for (int i = 0; i < data.Count; i++)
            {
                UInt16 hD = (UInt16)(data[i] >> 8);
                UInt16 lD = (UInt16)(data[i] & 0x00FF);

                s += hD.ToString("X2") + lD.ToString("X2");
                if (i + 1 < data.Count)
                {
                    s += " ";
                }
            }

            return s;
        }

        /// <summary>
        /// Convert to dec. based string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                UInt16[] bD = data.ToArray();

                string s = string.Empty;
                for (int i = 0; i < bD.Length; i++)
                {
                    s += Convert.ToUInt16(bD[i]).ToString();
                    if (i + 1 < bD.Length)
                    {
                        s += " ";
                    }
                }

                return s;
            }
            catch
            {
                return "code error!";
            }
        }

        /// <summary>
        /// Convert Hex string to UInt16Array.
        /// </summary>
        /// <param name="str">Hex string. For example: "EEFFEEFF" or "EEFF EEFF" or "EEFF,EEFF"</param>
        /// <returns></returns>
        public static UInt16Array FromHexString(string str)
        {
            str = str.Trim();
            UInt16Array Arr = new UInt16Array();

            if (str != string.Empty)
            {
                string[] s = Util.SplitString(str, new char[] { ',', ' ', '\t', '\n', '\r' }, 4);
                for (int i = 0; i < s.Length; i++)
                {
                    try
                    {
                        if (s[i] != string.Empty)
                            Arr.Add(Convert.ToUInt16(s[i], 16));
                    }
                    catch
                    {
                    }
                }
            }

            return Arr;
        }
        /// <summary>
        /// Convert Dec string to UInt16Array.
        /// </summary>
        /// <param name="str">dec string. For example: "12333 12334" or "23333,12331"</param>
        /// <returns></returns>
        public static UInt16Array FromString(string str)
        {
            str = str.Trim();
            UInt16Array Arr = new UInt16Array();

            if (str != string.Empty)
            {
                string[] s = str.Split(new char[] { ',', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < s.Length; i++)
                {
                    try
                    {
                        if (s[i] != string.Empty)
                            Arr.Add(Convert.ToUInt16(s[i], 10));
                    }
                    catch
                    {
                    }
                }
            }

            return Arr;
        }
    }

    /// <summary>
    /// used to store "s16v"
    /// </summary>
    [Serializable]
    public class Int16Array
    {
        public List<Int16> data = new List<short>();

        public void Add(Int16 val)
        {
            data.Add(val);
        }
        public Int16 this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int Count
        {
            get { return data.Count; }
        }

        /// <summary>
        /// Convert to Hex string
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            string s = string.Empty;

            for (int i = 0; i < data.Count; i++)
            {
                UInt16 hD = (UInt16)(data[i] >> 8);
                UInt16 lD = (UInt16)(data[i] & 0x00FF);

                s += hD.ToString("X2") + lD.ToString("X2");
                if (i + 1 < data.Count)
                {
                    s += " ";
                }
            }

            return s;
        }

        /// <summary>
        /// Convert to Hex string in word order
        /// </summary>
        /// <returns></returns>
        public string ToHexWordString()
        {
            string s = string.Empty;

            for (int i = 0; i < data.Count; i++)
            {
                UInt16 hD = (UInt16)(data[i] >> 8);
                UInt16 lD = (UInt16)(data[i] & 0x00FF);

                s += hD.ToString("X2") + lD.ToString("X2");
                if (i + 1 < data.Count)
                {
                    s += " ";
                }
            }

            return s;
        }

        /// <summary>
        /// Convert to dec. based string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                Int16[] bD = data.ToArray();

                string s = string.Empty;
                for (int i = 0; i < bD.Length; i++)
                {
                    s += Convert.ToInt16(bD[i]).ToString();
                    if (i + 1 < bD.Length)
                    {
                        s += " ";
                    }
                }

                return s;
            }
            catch
            {
                return "code error!";
            }
        }

        /// <summary>
        /// Convert Hex string to Int16Array.
        /// </summary>
        /// <param name="str">Hex string. For example: "EEFFEEFF" or "EEFF EEFF" or "EEFF,EEFF"</param>
        /// <returns></returns>
        public static Int16Array FromHexString(string str)
        {
            str = str.Trim();
            Int16Array Arr = new Int16Array();

            if (str != string.Empty)
            {
                string[] s = Util.SplitString(str, new char[] { ',', ' ', '\t', '\n', '\r' }, 4);
                for (int i = 0; i < s.Length; i++)
                {
                    try
                    {
                        if (s[i] != string.Empty)
                            Arr.Add(Convert.ToInt16(s[i], 16));
                    }
                    catch
                    {
                    }
                }
            }

            return Arr;
        }
        /// <summary>
        /// Convert Dec string to Int16Array.
        /// </summary>
        /// <param name="str">dec string. For example: "12333 12334" or "23333,12331"</param>
        /// <returns></returns>
        public static Int16Array FromString(string str)
        {
            str = str.Trim();
            Int16Array Arr = new Int16Array();

            if (str != string.Empty)
            {
                string[] s = str.Split(new char[] { ',', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < s.Length; i++)
                {
                    try
                    {
                        if (s[i] != string.Empty)
                            Arr.Add(Convert.ToInt16(s[i], 10));
                    }
                    catch
                    {
                    }
                }
            }

            return Arr;
        }
    }

    /// <summary>
    /// Used to store "u32v"
    /// </summary>
    [Serializable]
    public class UInt32Array
    {
        public List<UInt32> data = new List<UInt32>();

        public void Add(UInt32 val)
        {
            data.Add(val);
        }

        public UInt32 this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int Count
        {
            get { return data.Count; }
        }

        /// <summary>
        /// Convert to Hex string
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            string s = string.Empty;

            for (int i = 0; i < data.Count; i++)
            {
                UInt16 hD = (UInt16)(data[i] >> 16);
                UInt16 lD = (UInt16)(data[i]& 0x0000FFFF);

                UInt16 d1 = (UInt16)(hD >> 8);
                UInt16 d2 = (UInt16)(hD & 0x00FF);
                UInt16 d3 = (UInt16)(lD >> 8);
                UInt16 d4 = (UInt16)(lD & 0x00FF);

                s += d1.ToString("X2") + d2.ToString("X2") + d3.ToString("X2") + d4.ToString("X2");
            }

            return s;
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                UInt32[] bD = data.ToArray();

                string s = string.Empty;
                for (int i = 0; i < bD.Length; i++)
                {
                    s += Convert.ToUInt32(bD[i]).ToString();
                    if (i + 1 < bD.Length)
                    {
                        s += " ";
                    }
                }

                return s;
            }
            catch
            {
                return "code error!";
            }
        }

        /// <summary>
        /// Convert Hex string to UInt32Array
        /// </summary>
        /// <param name="str">Hex string. For example: "EEFFEEFFEEFFEEFF" or "EEFFEEFF EEFFEEFF" or "EEFFEEFF, EEFFEEFF"</param>
        /// <returns></returns>
        public static UInt32Array FromHexString(string str)
        {
            str = str.Trim();
            UInt32Array Arr = new UInt32Array();
            string[] s = Util.SplitString(str, new char[] { ',', ' ', '\t', '\n', '\r' }, 8);

            for (int i = 0; i < s.Length; i++)
            {
                try
                {
                    Arr.Add(Convert.ToUInt32(s[i], 16));
                }
                catch
                {
                }
            }

            return Arr;
        }

        /// <summary>
        /// Convert Dec string to UInt32Array
        /// </summary>
        /// <param name="str">Dec string. For example: "1233214234 1234234234" or "21342222, 1234234234"</param>
        /// <returns></returns>
        public static UInt32Array FromString(string str)
        {
            str = str.Trim();
            UInt32Array Arr = new UInt32Array();
            string[] s = str.Split(new char[] { ',', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < s.Length; i++)
            {
                try
                {
                    Arr.Add(Convert.ToUInt32(s[i], 10));
                }
                catch
                {
                }
            }

            return Arr;
        }
    }

    /// <summary>
    /// Used to store "u32v"
    /// </summary>
    [Serializable]
    public class Int32Array
    {
        public List<Int32> data = new List<Int32>();

        public void Add(Int32 val)
        {
            data.Add(val);
        }

        public Int32 this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int Count
        {
            get { return data.Count; }
        }

        /// <summary>
        /// Convert to Hex string
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            string s = string.Empty;

            for (int i = 0; i < data.Count; i++)
            {
                UInt16 hD = (UInt16)(data[i] >> 16);
                UInt16 lD = (UInt16)(data[i] & 0x0000FFFF);

                UInt16 d1 = (UInt16)(hD >> 8);
                UInt16 d2 = (UInt16)(hD & 0x00FF);
                UInt16 d3 = (UInt16)(lD >> 8);
                UInt16 d4 = (UInt16)(lD & 0x00FF);

                s += d1.ToString("X2") + d2.ToString("X2") + d3.ToString("X2") + d4.ToString("X2");
            }

            return s;
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                Int32[] bD = data.ToArray();

                string s = string.Empty;
                for (int i = 0; i < bD.Length; i++)
                {
                    s += Convert.ToInt32(bD[i]).ToString();
                    if (i + 1 < bD.Length)
                    {
                        s += " ";
                    }
                }

                return s;
            }
            catch
            {
                return "code error!";
            }
        }

        /// <summary>
        /// Convert Hex string to UInt32Array
        /// </summary>
        /// <param name="str">Hex string. For example: "EEFFEEFFEEFFEEFF" or "EEFFEEFF EEFFEEFF" or "EEFFEEFF, EEFFEEFF"</param>
        /// <returns></returns>
        public static Int32Array FromHexString(string str)
        {
            str = str.Trim();
            Int32Array Arr = new Int32Array();
            string[] s = Util.SplitString(str, new char[] { ',', ' ' }, 8);

            for (int i = 0; i < s.Length; i++)
            {
                try
                {
                    Arr.Add(Convert.ToInt32(s[i], 16));
                }
                catch
                {
                }
            }

            return Arr;
        }

        /// <summary>
        /// Convert Dec string to UInt32Array
        /// </summary>
        /// <param name="str">Dec string. For example: "1233214234 1234234234" or "21342222, 1234234234"</param>
        /// <returns></returns>
        public static Int32Array FromString(string str)
        {
            str = str.Trim();
            Int32Array Arr = new Int32Array();
            string[] s = str.Split(new char[] { ',', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < s.Length; i++)
            {
                try
                {
                    Arr.Add(Convert.ToInt32(s[i], 10));
                }
                catch
                {
                }
            }

            return Arr;
        }
    }

    /// <summary>
    /// For future use
    /// </summary>
    public class U96
    {
        private UInt16[] data;

        public override string ToString()
        {
            return string.Format("{0:4X}{1:4X}{2:4X}{3:4X}{4:4X}{5:4X}", data[0], data[1], data[2], data[3], data[4], data[5]);
        }
        public U96()
        {
            data = new ushort[6];
        }
    }

    /// <summary>
    /// Used to store "u2"
    /// </summary>
    public class TwoBits
    {
        private bool[] bits = new bool[] { false, false };

        public TwoBits()
        {
            bits = new bool[] { false, false };
        }

        /// <summary>
        /// Construct a TwoBits from high bit and low bit.
        /// </summary>
        /// <param name="bit1">High bit</param>
        /// <param name="bit2">Low bit</param>
        public TwoBits(bool bit1, bool bit2)
        {
            bits = new bool[2];
            bits[0] = bit1;
            bits[1] = bit2;
        }

        /// <summary>
        /// Convert UInt16 to TwoBits
        /// </summary>
        /// <param name="data"></param>
        public TwoBits(UInt16 data)
        {
            bits[0] = ((data & 0x0002) == 2) ? true : false;
            bits[1] = ((data & 0x0001) == 1) ? true : false;
        }

        /// <summary>
        /// Convert TwoBits to UInt16
        /// </summary>
        /// <returns></returns>
        public UInt16 ToInt()
        {
            return (UInt16)((UInt16)(bits[0] ? 2 : 0) + (UInt16)(bits[1] ? 1 : 0));
        }

        public bool this[int index]
        {
            get
            {
                if (index > 1) throw new Exception("Index is out of range!");
                return bits[index];
            }
            set
            {
                if (index > 1) throw new Exception("Index is out of range!");
                bits[index] = value;
            }
        }

        /// <summary>
        /// Convert string to UInt16
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static TwoBits FromString(string str)
        {
            try
            {
                UInt16 data = Convert.ToUInt16(str);
                return new TwoBits(data);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convert TwoBits to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            UInt16 data = (UInt16)((bits[0] ? 2 : 0) + (bits[1] ? 1 : 0));
            return data.ToString();
        }
    }

    /// <summary>
    /// Used for general XML parser
    /// </summary>
    public class LLRPMessageTypePair
    {
        public object msg;
        public ENUM_LLRP_MSG_TYPE type;

        public LLRPMessageTypePair(object msg, ENUM_LLRP_MSG_TYPE type)
        {
            this.msg = msg;
            this.type = type;
        }
    }

    /// <summary>
    /// Custom Parameter ArrayList
    /// </summary>
    public class CustomParameterArrayList
    {
        private List<ICustom_Parameter> list;

        public CustomParameterArrayList()
        {
            list = new List<ICustom_Parameter>();
        }

        /// <summary>
        /// Length of the list
        /// </summary>
        public int Length
        {
            get { return list.Count; }
        }

        /// <summary>
        /// Length of the list
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }

        /// <summary>
        /// Overriden the add method to enforce type checking
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Add(ICustom_Parameter value)
        {
            list.Add(value);
            return list.Count;
        }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ICustom_Parameter this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }
    }

    /// <summary>
    /// LLRP parameter array.
    /// </summary>
    public class ParamArrayList
    {
        List<IParameter> data;

        /// <summary>
        /// Constructor
        /// </summary>
        public ParamArrayList()
        {
            data = new List<IParameter>();
        }

        /// <summary>
        /// Add bit to the array
        /// </summary>
        /// <param name="val"></param>
        public void Add(IParameter val)
        {
            data.Add(val);
        }

        public IParameter this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        /// <summary>
        /// Return total elements count
        /// </summary>
        public int Count
        {
            get { return data.Count; }
        }

        /// <summary>
        /// Return total elements length
        /// </summary>
        public int Length
        {
            get { return data.Count; }
        }
    }

    public class UTCDateTime
    {
        private UInt64 microseconds;

        public UTCDateTime(string utcstring)
        {

        }

    }

}



