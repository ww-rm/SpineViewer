using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdWriter
{
    internal static class PackBits
    {
        public static byte[] Encode(ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
                return [];

            var output = new List<byte>(data.Length + data.Length / 128 + 1);

            int i = 0;
            while (i < data.Length)
            {
                byte value = data[i];
                int runLength = 1;

                // 统计重复 run（最多 128）
                while (i + runLength < data.Length &&
                       runLength < 128 &&
                       data[i + runLength] == value)
                {
                    runLength++;
                }

                if (runLength >= 3)
                {
                    // replicate run
                    output.Add((byte)(1 - runLength)); // negative count
                    output.Add(value);
                    i += runLength;
                }
                else
                {
                    // literal run
                    int literalStart = i;
                    int literalLength = 0;

                    while (i < data.Length && literalLength < 128)
                    {
                        // 看看后面是否会形成一个 >=3 的重复 run
                        if (i + 2 < data.Length &&
                            data[i] == data[i + 1] &&
                            data[i] == data[i + 2])
                        {
                            break;
                        }

                        i++;
                        literalLength++;
                    }

                    output.Add((byte)(literalLength - 1)); // 0..127
                    for (int j = 0; j < literalLength; j++)
                    {
                        output.Add(data[literalStart + j]);
                    }
                }
            }

            return output.ToArray();
        }

        public static byte[] Decode(ReadOnlySpan<byte> data)
        {
            var output = new List<byte>();

            int i = 0;
            while (i < data.Length)
            {
                sbyte n = (sbyte)data[i];
                i++;

                if (n >= 0)
                {
                    // literal run: copy n+1 bytes
                    int count = n + 1;
                    for (int j = 0; j < count; j++)
                    {
                        output.Add(data[i++]);
                    }
                }
                else if (n != -128)
                {
                    // replicate run: repeat next byte (1 - n) times
                    int count = 1 - n;
                    byte value = data[i++];
                    for (int j = 0; j < count; j++)
                    {
                        output.Add(value);
                    }
                }
                // n == -128 : noop (do nothing)
            }

            return output.ToArray();
        }
    }
}
