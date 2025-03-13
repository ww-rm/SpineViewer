﻿using Microsoft.VisualBasic;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    /// <summary>
    /// SkeletonConverter 实现类标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SkeletonConverterImplementationAttribute : Attribute
    {
        public Version Version { get; }

        public SkeletonConverterImplementationAttribute(Version version)
        {
            Version = version;
        }
    }

    public abstract class SkeletonConverter
    {
        /// <summary>
        /// 实现类缓存
        /// </summary>
        private static readonly Dictionary<Version, Type> ImplementationTypes = [];

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static SkeletonConverter()
        {
            // 遍历并缓存标记了 SkeletonConverterImplementationAttribute 的类型
            var impTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(SkeletonConverter).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in impTypes)
            {
                var attr = type.GetCustomAttribute<SkeletonConverterImplementationAttribute>();
                if (attr is not null)
                {
                    if (ImplementationTypes.ContainsKey(attr.Version))
                        throw new InvalidOperationException($"Multiple implementations found: {attr.Version}");
                    ImplementationTypes[attr.Version] = type;
                }
            }
            Program.Logger.Debug("Find SkeletonConverter implementations: [{}]", string.Join(", ", ImplementationTypes.Keys));
        }

        /// <summary>
        /// 创建特定版本的 SkeletonConverter
        /// </summary>
        public static SkeletonConverter New(Version version)
        {
            if (!ImplementationTypes.TryGetValue(version, out var cvterType))
            {
                throw new NotImplementedException($"Not implemented version: {version}");
            }
            return (SkeletonConverter)Activator.CreateInstance(cvterType);
        }

        protected static readonly JsonWriterOptions jsonWriterOptions = new()
        {
            Indented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// 二进制转 Json 格式
        /// </summary>
        public abstract void BinaryToJson(string binPath, string jsonPath);

        /// <summary>
        /// Json 转二进制格式
        /// </summary>
        public abstract void JsonToBinary(string jsonPath, string binPath);

        protected class SkeletonReader
        {
            protected byte[] buffer = new byte[32];
            protected byte[] bytesBigEndian = new byte[8];
            public readonly List<string> StringTable = new(32);
            protected Stream input;

            public SkeletonReader(Stream input) { this.input = input; }
            public int Read()
            {
                int val = input.ReadByte();
                if (val == -1) throw new EndOfStreamException();
                return val;
            }
            public byte ReadByte() => (byte)Read();
            public byte ReadUByte() => (byte)Read();
            public sbyte ReadSByte() => (sbyte)ReadByte();
            public bool ReadBoolean() => Read() != 0;
            public float ReadFloat()
            {
                if (input.Read(bytesBigEndian, 0, 4) < 4) throw new EndOfStreamException();
                buffer[3] = bytesBigEndian[0];
                buffer[2] = bytesBigEndian[1];
                buffer[1] = bytesBigEndian[2];
                buffer[0] = bytesBigEndian[3];
                return BitConverter.ToSingle(buffer, 0);
            }
            public int ReadInt()
            {
                if (input.Read(bytesBigEndian, 0, 4) < 4) throw new EndOfStreamException();
                return (bytesBigEndian[0] << 24)
                     | (bytesBigEndian[1] << 16)
                     | (bytesBigEndian[2] << 8)
                     |  bytesBigEndian[3];
            }
            public long ReadLong()
            {
                if (input.Read(bytesBigEndian, 0, 8) < 8) throw new EndOfStreamException();
                return ((long)(bytesBigEndian[0]) << 56)
                    | ((long)(bytesBigEndian[1]) << 48)
                    | ((long)(bytesBigEndian[2]) << 40)
                    | ((long)(bytesBigEndian[3]) << 32)
                    | ((long)(bytesBigEndian[4]) << 24)
                    | ((long)(bytesBigEndian[5]) << 16)
                    | ((long)(bytesBigEndian[6]) << 8)
                    | (long)(bytesBigEndian[7]);
            }
            public int ReadVarInt(bool optimizePositive = true)
            {
                byte b = ReadByte();
                int val = b & 0x7F;
                if ((b & 0x80) != 0)
                {
                    b = ReadByte();
                    val |= (b & 0x7F) << 7;
                    if ((b & 0x80) != 0)
                    {
                        b = ReadByte();
                        val |= (b & 0x7F) << 14;
                        if ((b & 0x80) != 0)
                        {
                            b = ReadByte();
                            val |= (b & 0x7F) << 21;
                            if ((b & 0x80) != 0)
                                val |= (ReadByte() & 0x7F) << 28;
                        }
                    }
                }

                // 最低位是符号, 根据符号得到全 1 或全 0
                // 无符号右移, 符号按原样设置在最高位, 其他位与符号异或
                return optimizePositive ? val : (val >>> 1) ^ -(val & 1);
            }
            public string ReadString()
            {
                int byteCount = ReadVarInt();
                switch (byteCount)
                {
                    case 0: return null;
                    case 1: return "";
                }
                byteCount--;
                if (buffer.Length < byteCount) buffer = new byte[byteCount];
                ReadFully(buffer, 0, byteCount);
                return System.Text.Encoding.UTF8.GetString(buffer, 0, byteCount);
            }
            public string ReadStringRef()
            {
                int index = ReadVarInt();
                return index == 0 ? null : StringTable[index - 1];
            }
            public void ReadFully(byte[] buffer, int offset, int length)
            {
                while (length > 0)
                {
                    int count = input.Read(buffer, offset, length);
                    if (count <= 0) throw new EndOfStreamException();
                    offset += count;
                    length -= count;
                }
            }
        }

        protected class SkeletonWriter
        {
            protected byte[] buffer = new byte[32];
            protected byte[] bytesBigEndian = new byte[8];
            public readonly List<string> Strings = new(32);
            protected Stream output;

            public SkeletonWriter(Stream output) { this.output = output; }
            public void Write(int val) => output.WriteByte((byte)val);
            public void WriteByte(byte val) => output.WriteByte(val);
            public void WriteUByte(byte val) => output.WriteByte(val);
            public void WriteSByte(sbyte val) => output.WriteByte((byte)val);
            public void WriteBoolean(bool val) => output.WriteByte((byte)(val ? 1 : 0));
            public void WriteFloat(float val)
            {
                uint v = BitConverter.SingleToUInt32Bits(val);
                bytesBigEndian[0] = (byte)(v >> 24);
                bytesBigEndian[1] = (byte)(v >> 16);
                bytesBigEndian[2] = (byte)(v >> 8);
                bytesBigEndian[3] = (byte)v;
                output.Write(bytesBigEndian, 0, 4);
            }
            public void WriteInt(int val)
            {
                bytesBigEndian[0] = (byte)(val >> 24);
                bytesBigEndian[1] = (byte)(val >> 16);
                bytesBigEndian[2] = (byte)(val >> 8);
                bytesBigEndian[3] = (byte)val;
                output.Write(bytesBigEndian, 0, 4);
            }
            public void WriteLong(long val)
            {
                bytesBigEndian[0] = (byte)(val >> 56);
                bytesBigEndian[1] = (byte)(val >> 48);
                bytesBigEndian[2] = (byte)(val >> 40);
                bytesBigEndian[3] = (byte)(val >> 32);
                bytesBigEndian[4] = (byte)(val >> 24);
                bytesBigEndian[5] = (byte)(val >> 16);
                bytesBigEndian[6] = (byte)(val >> 8);
                bytesBigEndian[7] = (byte)val;
                output.Write(bytesBigEndian, 0, 8);
            }
            public void WriteVarInt(int val, bool optimizePositive = true)
            {
                // 有符号右移, 会变成全 1 或者全 0 符号
                // 其他位与符号异或, 符号按原样设置在最低位
                if (!optimizePositive) val = (val << 1) ^ (val >> 31);

                byte b = (byte)(val & 0x7F);
                val >>>= 7;
                if (val != 0)
                {
                    output.WriteByte((byte)(b | 0x80));
                    b = (byte)(val & 0x7F);
                    val >>>= 7;
                    if (val != 0)
                    {
                        output.WriteByte((byte)(b | 0x80));
                        b = (byte)(val & 0x7F);
                        val >>>= 7;
                        if (val != 0)
                        {
                            output.WriteByte((byte)(b | 0x80));
                            b = (byte)(val & 0x7F);
                            val >>>= 7;
                            if (val != 0)
                            {
                                output.WriteByte((byte)(b | 0x80));
                                b = (byte)(val & 0x7F);
                            }
                        }
                    }
                }
                output.WriteByte(b);
            }
            public void WriteString(string val)
            {
                if (val == null)
                {
                    WriteVarInt(0);
                    return;
                }
                if (val.Length == 0)
                {
                    WriteVarInt(1);
                    return;
                }
                int byteCount = System.Text.Encoding.UTF8.GetByteCount(val);
                WriteVarInt(byteCount + 1);
                if (buffer.Length < byteCount) buffer = new byte[byteCount];
                System.Text.Encoding.UTF8.GetBytes(val, 0, val.Length, buffer, 0);
                WriteFully(buffer, 0, byteCount);
            }
            public void WriteStringRef(List<string> strings, string val)
            {
                if (val is null)
                {
                    WriteVarInt(0);
                    return;
                }
                int index = strings.IndexOf(val);
                if (index < 0)
                {
                    strings.Add(val);
                    index = strings.Count - 1;
                }
                WriteVarInt(index + 1);
            }
            public void WriteFully(byte[] buffer, int offset, int length) => output.Write(buffer, offset, length);
        }

    }
}
