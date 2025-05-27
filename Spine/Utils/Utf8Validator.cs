using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Utils
{
    /// <summary>
    /// UTF8 格式检测工具类
    /// </summary>
    public static class Utf8Validator
    {
        /// <summary>
        /// 判断某段数据是否是 UTF8 格式, 会忽略尾部不完整数据
        /// </summary>
        public static bool IsUtf8(byte[] data, int maxLength = 1024)
        {
            int length = Math.Min(data.Length, maxLength);

            int start = 0;
            if (length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            {
                start = 3;
            }

            int expectedContinuationBytes = 0;
            for (int i = start; i < length; i++)
            {
                byte currentByte = data[i];

                if (expectedContinuationBytes == 0)
                {
                    if ((currentByte & 0x80) == 0x00)
                    {
                        // 0xxxxxxx，ASCII 字符
                        continue;
                    }

                    // 计算需要的续字节数
                    int needed;
                    if ((currentByte & 0xE0) == 0xC0)
                    {
                        // 110xxxxx，1 个续字节
                        if (currentByte == 0xC0 || currentByte == 0xC1)
                            return false; // 避免过长编码
                        needed = 1;
                    }
                    else if ((currentByte & 0xF0) == 0xE0)
                    {
                        // 1110xxxx，2 个续字节
                        needed = 2;
                    }
                    else if ((currentByte & 0xF8) == 0xF0)
                    {
                        // 11110xxx，3 个续字节
                        if (currentByte > 0xF4)
                            return false; // 超出 Unicode 范围
                        needed = 3;
                    }
                    else
                    {
                        // 非法的起始字节
                        return false;
                    }

                    // 如果剩余字节不足以完成这个字符，就当作“尾部不完整”，跳出主循环
                    if (i + needed >= length)
                        break;

                    // 否则进入续字节检查
                    expectedContinuationBytes = needed;
                }
                else
                {
                    // 检查续字节（10xxxxxx）
                    if ((currentByte & 0xC0) != 0x80)
                        return false;
                    expectedContinuationBytes--;
                }
            }

            // 如果在跳出时，expectedContinuationBytes>0，说明我们跳过了一些尾部续字节，
            // 本着“忽略尾部不完整字符”的原则，仍然返回 true
            return expectedContinuationBytes == 0;
        }

        /// <summary>
        /// 判断某个文件是否是 UTF8 格式, 会忽略尾部不完整数据
        /// </summary>
        public static bool IsUtf8(string path, int maxLength = 1024)
        {
            using var stream = File.OpenRead(path);
            byte[] data = new byte[maxLength];
            var actualLength = stream.Read(data, 0, data.Length);
            return IsUtf8(data, actualLength);
        }
    }
}
