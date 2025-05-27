using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Utils
{
    /// <summary>
    /// 二进制骨骼文件写
    /// </summary>
    public class BinaryWriter(Stream output)
    {
        private readonly Stream _output = output;
        private byte[] _buffer = new byte[32];
        private readonly byte[] _bytesBigEndian = new byte[8];
        private readonly List<string> _stringTable = new(32);

        /// <summary>
        /// 构造 Writer, 但是继承其他 Writer 的字符串表
        /// </summary>
        public BinaryWriter(Stream output, BinaryWriter writer) : this(output)
        {
            _stringTable.AddRange(writer._stringTable);
        }

        public void Write(int val) => _output.WriteByte((byte)val);

        public void WriteByte(byte val) => _output.WriteByte(val);

        public void WriteUByte(byte val) => _output.WriteByte(val);

        public void WriteSByte(sbyte val) => _output.WriteByte((byte)val);

        public void WriteBoolean(bool val) => _output.WriteByte((byte)(val ? 1 : 0));

        public void WriteFloat(float val)
        {
            uint v = BitConverter.SingleToUInt32Bits(val);
            _bytesBigEndian[0] = (byte)(v >> 24);
            _bytesBigEndian[1] = (byte)(v >> 16);
            _bytesBigEndian[2] = (byte)(v >> 8);
            _bytesBigEndian[3] = (byte)v;
            _output.Write(_bytesBigEndian, 0, 4);
        }

        public void WriteInt(int val)
        {
            _bytesBigEndian[0] = (byte)(val >> 24);
            _bytesBigEndian[1] = (byte)(val >> 16);
            _bytesBigEndian[2] = (byte)(val >> 8);
            _bytesBigEndian[3] = (byte)val;
            _output.Write(_bytesBigEndian, 0, 4);
        }

        public void WriteLong(long val)
        {
            _bytesBigEndian[0] = (byte)(val >> 56);
            _bytesBigEndian[1] = (byte)(val >> 48);
            _bytesBigEndian[2] = (byte)(val >> 40);
            _bytesBigEndian[3] = (byte)(val >> 32);
            _bytesBigEndian[4] = (byte)(val >> 24);
            _bytesBigEndian[5] = (byte)(val >> 16);
            _bytesBigEndian[6] = (byte)(val >> 8);
            _bytesBigEndian[7] = (byte)val;
            _output.Write(_bytesBigEndian, 0, 8);
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
                _output.WriteByte((byte)(b | 0x80));
                b = (byte)(val & 0x7F);
                val >>>= 7;
                if (val != 0)
                {
                    _output.WriteByte((byte)(b | 0x80));
                    b = (byte)(val & 0x7F);
                    val >>>= 7;
                    if (val != 0)
                    {
                        _output.WriteByte((byte)(b | 0x80));
                        b = (byte)(val & 0x7F);
                        val >>>= 7;
                        if (val != 0)
                        {
                            _output.WriteByte((byte)(b | 0x80));
                            b = (byte)(val & 0x7F);
                        }
                    }
                }
            }
            _output.WriteByte(b);
        }

        public void WriteString(string? val)
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
            if (_buffer.Length < byteCount) _buffer = new byte[byteCount];
            System.Text.Encoding.UTF8.GetBytes(val, 0, val.Length, _buffer, 0);
            WriteFully(_buffer, 0, byteCount);
        }

        public void WriteStringRef(string? val)
        {
            if (val is null)
            {
                WriteVarInt(0);
                return;
            }
            int index = _stringTable.IndexOf(val);
            if (index < 0)
            {
                _stringTable.Add(val);
                index = _stringTable.Count - 1;
            }
            WriteVarInt(index + 1);
        }

        public void WriteFully(byte[] buffer, int offset, int length) => _output.Write(buffer, offset, length);
    }
}
