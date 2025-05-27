using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Utils
{
    /// <summary>
    /// 二进制骨骼文件读
    /// </summary>
    public class BinaryReader(Stream input)
    {
        private readonly Stream _input = input;
        private byte[] _buffer = new byte[32];
        private readonly byte[] _bytesBigEndian = new byte[8];
        private readonly List<string> _stringTable = new(32);

        public int Read()
        {
            int val = _input.ReadByte();
            if (val == -1) throw new EndOfStreamException();
            return val;
        }

        public byte ReadByte() => (byte)Read();

        public byte ReadUByte() => (byte)Read();

        public sbyte ReadSByte() => (sbyte)ReadByte();

        public bool ReadBoolean() => Read() != 0;

        public float ReadFloat()
        {
            if (_input.Read(_bytesBigEndian, 0, 4) < 4) throw new EndOfStreamException();
            _buffer[3] = _bytesBigEndian[0];
            _buffer[2] = _bytesBigEndian[1];
            _buffer[1] = _bytesBigEndian[2];
            _buffer[0] = _bytesBigEndian[3];
            return BitConverter.ToSingle(_buffer, 0);
        }

        public int ReadInt()
        {
            if (_input.Read(_bytesBigEndian, 0, 4) < 4) throw new EndOfStreamException();
            return (_bytesBigEndian[0] << 24)
                 | (_bytesBigEndian[1] << 16)
                 | (_bytesBigEndian[2] << 8)
                 | _bytesBigEndian[3];
        }

        public long ReadLong()
        {
            if (_input.Read(_bytesBigEndian, 0, 8) < 8) throw new EndOfStreamException();
            return ((long)(_bytesBigEndian[0]) << 56)
                | ((long)(_bytesBigEndian[1]) << 48)
                | ((long)(_bytesBigEndian[2]) << 40)
                | ((long)(_bytesBigEndian[3]) << 32)
                | ((long)(_bytesBigEndian[4]) << 24)
                | ((long)(_bytesBigEndian[5]) << 16)
                | ((long)(_bytesBigEndian[6]) << 8)
                | (long)(_bytesBigEndian[7]);
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

        public string? ReadString()
        {
            int byteCount = ReadVarInt();
            switch (byteCount)
            {
                case 0: return null;
                case 1: return "";
            }
            byteCount--;
            if (_buffer.Length < byteCount) _buffer = new byte[byteCount];
            ReadFully(_buffer, 0, byteCount);
            return Encoding.UTF8.GetString(_buffer, 0, byteCount);
        }

        public string? ReadStringRef()
        {
            int index = ReadVarInt();
            return index == 0 ? null : _stringTable[index - 1];
        }

        public void ReadFully(byte[] buffer, int offset, int length)
        {
            while (length > 0)
            {
                int count = _input.Read(buffer, offset, length);
                if (count <= 0) throw new EndOfStreamException();
                offset += count;
                length -= count;
            }
        }
    }
}
