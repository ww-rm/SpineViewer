using Microsoft.VisualBasic;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Encodings.Web;
using SpineViewer.Utils;

namespace SpineViewer.Spine
{
    /// <summary>
    /// SkeletonConverter 基类, 使用静态方法 New 来创建具体版本对象
    /// </summary>
    public abstract class SkeletonConverter : ImplementationResolver<SkeletonConverter, SpineImplementationAttribute, SpineVersion>
    {
        /// <summary>
        /// 创建特定版本的 SkeletonConverter
        /// </summary>
        public static SkeletonConverter New(SpineVersion version) => New(version, []);

        /// <summary>
        /// Json 格式控制
        /// </summary>
        private static readonly JsonWriterOptions jsonWriterOptions = new()
        {
            Indented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// 读取二进制骨骼文件并构造 Json 对象
        /// </summary>
        public abstract JsonObject ReadBinary(string binPath);

        /// <summary>
        /// 将 Json 对象写入二进制骨骼文件
        /// </summary>
        public abstract void WriteBinary(JsonObject root, string binPath, bool nonessential = false);

        /// <summary>
        /// 读取 Json 对象
        /// </summary>
        public virtual JsonObject ReadJson(string jsonPath)
        {
            using var input = File.OpenRead(jsonPath);
            if (JsonNode.Parse(input) is JsonObject root)
                return root;
            else
                throw new InvalidDataException($"{jsonPath} is not a valid json object");
        }

        /// <summary>
        /// 写入 Json 对象
        /// </summary>
        public virtual void WriteJson(JsonObject root, string jsonPath)
        {
            using var output = File.Create(jsonPath);
            using var writer = new Utf8JsonWriter(output, jsonWriterOptions);
            root.WriteTo(writer);
        }

        /// <summary>
        /// 读取骨骼文件
        /// </summary>
        public JsonObject Read(string path)
        {
            try
            {
                return ReadBinary(path);
            }
            catch
            {
                try
                {
                    return ReadJson(path);
                }
                catch
                {
                    // 都不行就报错
                    throw new InvalidDataException($"Unknown skeleton file format {path}");
                }
            }
        }

        /// <summary>
        /// 转换到目标版本
        /// </summary>
        public abstract JsonObject ToVersion(JsonObject root, SpineVersion version);

        /// <summary>
        /// 二进制骨骼文件读
        /// </summary>
        public class BinaryReader
        {
            protected byte[] buffer = new byte[32];
            protected byte[] bytesBigEndian = new byte[8];
            public readonly List<string> StringTable = new(32);
            protected Stream input;

            public BinaryReader(Stream input) { this.input = input; }
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
                     | bytesBigEndian[3];
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
            public string? ReadString()
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
            public string? ReadStringRef()
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

        /// <summary>
        /// 二进制骨骼文件写
        /// </summary>
        protected class BinaryWriter
        {
            protected byte[] buffer = new byte[32];
            protected byte[] bytesBigEndian = new byte[8];
            public readonly List<string> StringTable = new(32);
            protected Stream output;

            public BinaryWriter(Stream output) { this.output = output; }
            public void Write(byte val) => output.WriteByte(val);
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
                if (buffer.Length < byteCount) buffer = new byte[byteCount];
                System.Text.Encoding.UTF8.GetBytes(val, 0, val.Length, buffer, 0);
                WriteFully(buffer, 0, byteCount);
            }
            public void WriteStringRef(string? val)
            {
                if (val is null)
                {
                    WriteVarInt(0);
                    return;
                }
                int index = StringTable.IndexOf(val);
                if (index < 0)
                {
                    StringTable.Add(val);
                    index = StringTable.Count - 1;
                }
                WriteVarInt(index + 1);
            }
            public void WriteFully(byte[] buffer, int offset, int length) => output.Write(buffer, offset, length);                      
        }
        public static JsonObject V4XToV38(JsonObject root, bool keep, string originVersion)
        {
            JsonObject data = root.DeepClone().AsObject();

            if (keep) data["reserved"] = new JsonObject();

            //skeleton
            data["skeleton"]["spine"] = "3.8.76";
            if (keep) data["skeleton"]["reserved"] = new JsonObject()
            {
                ["spine"] = originVersion,
            };

            //bones
            if (originVersion.StartsWith("4.2")) //emm,先这样吧
            {
                if (data.TryGetPropertyValue("bones", out var bones))
                {
                    foreach (JsonObject bone in bones.AsArray())
                    {
                        if (bone.TryGetPropertyValue("inherit", out var inherit))
                        {
                            bone.Remove("inherit");
                            bone["transform"] = (string)inherit;
                        }
                    }
                }
            }


            //transform
            if (data.TryGetPropertyValue("transform", out var transforms))
            {
                foreach (JsonObject transform in transforms.AsArray())
                {
                    JsonObject reservedItem = new JsonObject();
                    if (transform.TryGetPropertyValue("mixRotate", out var mixRotate))
                    {
                        transform["rotateMix"] = (float)mixRotate;
                        transform.Remove("mixRotate");
                        //if (keep) reservedItem["mixRotate"] = (float)mixRotate;
                    }
                    //判断mixX是否被修改来确定mixY还原还是取mixX.下同
                    if (transform.TryGetPropertyValue("mixX", out var mixX))
                    {
                        transform["translateMix"] = (float)mixX;
                        data.Remove("mixX");
                        if (keep) reservedItem["mixX"] = (float)mixX;
                    }
                    if (transform.TryGetPropertyValue("mixY", out var mixY))
                    {
                        transform.Remove("mixY");
                        if (keep) reservedItem["mixY"] = (float)mixY;
                    }
                    if (transform.TryGetPropertyValue("mixScaleX", out var mixScaleX))
                    {
                        transform["scaleMix"] = (float)mixScaleX;
                        transform.Remove("mixScaleX");
                        if (keep) reservedItem["mixScaleX"] = (float)mixScaleX;
                    }
                    if (transform.TryGetPropertyValue("mixScaleY", out var mixScaleY))
                    {
                        transform.Remove("mixScaleY");
                        if (keep) reservedItem["mixScaleY"] = (float)mixScaleY;
                    }
                    if (transform.TryGetPropertyValue("mixShearY", out var mixShearY))
                    {
                        transform["shearMix"] = (float)mixShearY;
                        transform.Remove("mixShearY");
                        //if (keep) reservedItem["mixShearY"] = (float)mixShearY;
                    }
                    if (reservedItem.Count > 0) transform["reserved"] = reservedItem;
                }
            }

            //path
            if (data.TryGetPropertyValue("path", out var path))
            {
                JsonObject reservedPath = [];
                data["reserved"]["path"] = reservedPath;
                for (int i = path.AsArray().Count - 1; i >= 0; i--)
                {
                    JsonObject _data = path.AsArray()[i].AsObject();
                    if (_data.TryGetPropertyValue("spacingMode", out var spacing) && ((string)spacing).ToLower() == "proportional")
                    {
                        path.AsArray().RemoveAt(i);
                        reservedPath[(string)_data["name"]] = new JsonObject(_data);//先加入，到时候再决定是否保留
                    }
                }

            }

            //physics
            if (data.TryGetPropertyValue("physics", out var physics))
            {
                data.Remove("physics");
                if (keep) data["reserved"]["physics"] = physics.DeepClone().AsArray();
            }

            //skin
            if (data.TryGetPropertyValue("skins", out var skins))
            {
                foreach (JsonObject data1 in skins.AsArray())
                {
                    JsonObject reserved = [];
                    if (data1.TryGetPropertyValue("path", out var path1))
                    {
                        JsonArray reservedPathName = [];
                        foreach (string pathName in data1.AsArray())
                        {
                            if (data["reserved"]["path"].AsObject().ContainsKey(pathName))
                            {
                                data1.Remove(pathName);
                                if (keep) reservedPathName.Add((string)pathName);
                            }
                        }
                        if (reservedPathName.Count > 0) reserved["path"] = reservedPathName;
                    }                    
                    if (data1.TryGetPropertyValue("physics", out var physics1))
                    {
                        data1.Remove("physics");
                        if (keep) reserved["physics"] = physics1.DeepClone().AsArray();
                    }
                    if (keep) data1["reserved"] = reserved;
                }
            }

            //animation

            if (data.TryGetPropertyValue("animations", out var animations))
            {
                foreach (var (name, _animation) in animations.AsObject())
                {
                    JsonObject animation = _animation.AsObject();
                    JsonObject reservedAnimation = [];

                    //<---slot
                    if (animation.TryGetPropertyValue("slots", out var slots))
                    {
                        JsonObject newSlots = [];
                        foreach (var (slotName, _slot) in slots.AsObject())
                        {
                            JsonObject slotData = _slot.AsObject();
                            JsonObject reserved = [];
                            JsonObject newSlotData = [];
                            newSlots[slotName] = newSlotData;
                            foreach (var (timelineName, _timelines) in slotData)
                            {
                                var timelines = _timelines.AsArray();
                                if (timelineName == "attachment")
                                {
                                    newSlotData[timelineName] = timelines.DeepClone().AsArray();
                                }
                                if (timelineName == "aplha")
                                {
                                    //slotData.Remove(timelineName);
                                    if (keep) reserved[timelineName] = timelines.DeepClone().AsArray();
                                }
                                //理论上来说，颜色的几种timeline是互斥的，但我没办法实操验证.
                                //如果遇到同一个slottimes下有多种颜色的timeline，
                                //这段代码只会保留onecolor和twocolor最后设置的一个
                                else if (timelineName == "rgba")
                                {
                                    //slotData.Remove(timelineName);                                    
                                    if (keep) reserved["colorType"] = timelineName;
                                    foreach (JsonObject timeline in timelines)
                                    {
                                        processCurve(timeline, keep);
                                    }
                                    newSlotData["color"] = timelines.DeepClone().AsArray();
                                }
                                else if (timelineName == "rgb")
                                {
                                    //slotData.Remove(timelineName);
                                    if (keep) reserved["colorType"] = timelineName;

                                    foreach (JsonObject timeline in timelines)
                                    {
                                        if (timeline.TryGetPropertyValue("color", out var color))
                                        {
                                            timeline["color"] = (string)color + "ff";
                                        }
                                        processCurve(timeline, keep);
                                    }
                                    newSlotData["color"] = timelines.DeepClone().AsArray();
                                }
                                else if (timelineName == "rgba2")
                                {
                                    //slotData.Remove(timelineName);                                    
                                    if (keep) reserved["colorType"] = timelineName;
                                    foreach (JsonObject timeline in timelines)
                                    {
                                        processCurve(timeline, keep);
                                    }
                                    newSlotData["twoColor"] = timelines.DeepClone().AsArray();
                                }
                                else if (timelineName == "rgb2")
                                {
                                    //slotData.Remove(timelineName);
                                    if (keep) reserved["colorType"] = timelineName;

                                    foreach (JsonObject timeline in timelines)
                                    {
                                        if (timeline.TryGetPropertyValue("light", out var light))
                                        {
                                            timeline["light"] = (string)light + "ff";
                                        }
                                        processCurve(timeline, keep);
                                    }
                                    newSlotData["twoColor"] = timelines.DeepClone().AsArray();
                                }
                            }
                            if (reserved.Count > 0) slotData["reserved"] = reserved;
                        }
                        animation["slots"] = newSlots;
                    }
                    //--->slot

                    //<---bone
                    if (animation.TryGetPropertyValue("bones", out var bones))
                    {
                        JsonObject newBones = [];
                        JsonObject reserved = [];
                        foreach (var (boneName, _bone) in bones.AsObject())
                        {
                            JsonObject boneData = _bone.AsObject();
                            reserved[boneName] = new JsonObject();
                            //bool adaptedTranslate = false;
                            //bool adaptedScale = false;
                            //bool adaptedShear = false;                           
                            JsonObject newBoneData = [];
                            newBones[boneName] = newBoneData;

                            foreach (var (timelineName, _timelines) in boneData.AsObject())
                            {
                                var timelines = _timelines.AsArray();
                                if (timelineName == "rotate")
                                {
                                    foreach (JsonObject timeline in timelines)
                                    {
                                        if (timeline.TryGetPropertyValue("value", out var value))
                                        {
                                            timeline.Remove("value");
                                            timeline["angle"] = (float)value;
                                        }
                                        processCurve(timeline, keep);
                                    }
                                    newBoneData[timelineName] = timelines.DeepClone().AsArray();
                                }
                                else if (timelineName == "translate" || timelineName == "scale" || timelineName == "shear")
                                {
                                    foreach (JsonObject timeline in timelines)
                                    {
                                        processCurve(timeline, keep);
                                    }
                                    newBoneData[timelineName] = timelines.DeepClone().AsArray();
                                }
                                //理论上来说translate和x,y是互斥的。如果translate存在或者x和y同时存在，则把x和y去掉。否则尝试把单独的x或者y转成translate。
                                else if (timelineName == "translatex")
                                {
                                    if (boneData.ContainsKey("translate"))
                                    {
                                        if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                    }
                                    else
                                    {
                                        if (boneData.ContainsKey("translatey"))
                                        {
                                            if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                        }
                                        else
                                        {
                                            foreach (JsonObject timeline in timelines)
                                            {
                                                if (timeline.TryGetPropertyValue("value", out var value))
                                                {
                                                    timeline.Remove("value");
                                                    timeline["x"] = (float)value;
                                                    timeline["y"] = 0f;
                                                }
                                                processCurve(timeline, keep);
                                            }
                                            newBoneData["translate"] = timelines.DeepClone().AsArray();
                                        }
                                    }
                                }
                                else if (timelineName == "translatey")
                                {
                                    //boneData.Remove(timelineName);
                                    if (boneData.ContainsKey("translate"))
                                    {
                                        if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                    }
                                    else
                                    {
                                        if (boneData.ContainsKey("translatex"))
                                        {
                                            if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                        }
                                        else
                                        {
                                            foreach (JsonObject timeline in timelines)
                                            {
                                                if (timeline.TryGetPropertyValue("value", out var value))
                                                {
                                                    timeline.Remove("value");
                                                    timeline["x"] = 0f;
                                                    timeline["y"] = (float)value;
                                                }
                                                processCurve(timeline, keep);
                                            }
                                            newBoneData["translate"] = timelines.DeepClone().AsArray();
                                        }
                                    }
                                }
                                else if (timelineName == "scalex")
                                {
                                    if (boneData.ContainsKey("scale"))
                                    {
                                        if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                    }
                                    else
                                    {
                                        if (boneData.ContainsKey("scaley"))
                                        {
                                            if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                        }
                                        else
                                        {
                                            foreach (JsonObject timeline in timelines)
                                            {
                                                if (timeline.TryGetPropertyValue("value", out var value))
                                                {
                                                    timeline.Remove("value");
                                                    timeline["x"] = (float)value;
                                                    timeline["y"] = 0f;
                                                }
                                                processCurve(timeline, keep);
                                            }
                                            newBoneData["scale"] = timelines.DeepClone().AsArray();
                                        }
                                    }
                                }
                                else if (timelineName == "scaley")
                                {
                                    //boneData.Remove(timelineName);
                                    if (boneData.ContainsKey("scale"))
                                    {
                                        if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                    }
                                    else
                                    {
                                        if (boneData.ContainsKey("scalex"))
                                        {
                                            if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                        }
                                        else
                                        {
                                            foreach (JsonObject timeline in timelines)
                                            {
                                                if (timeline.TryGetPropertyValue("value", out var value))
                                                {
                                                    timeline.Remove("value");
                                                    timeline["x"] = 0f;
                                                    timeline["y"] = (float)value;
                                                }
                                                processCurve(timeline, keep);
                                            }
                                            newBoneData["scale"] = timelines.DeepClone().AsArray();
                                        }
                                    }
                                }
                                else if (timelineName == "shearx")
                                {
                                    if (boneData.ContainsKey("shear"))
                                    {
                                        if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                    }
                                    else
                                    {
                                        if (boneData.ContainsKey("sheary"))
                                        {
                                            if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                        }
                                        else
                                        {
                                            foreach (JsonObject timeline in timelines)
                                            {
                                                if (timeline.TryGetPropertyValue("value", out var value))
                                                {
                                                    timeline.Remove("value");
                                                    timeline["x"] = (float)value;
                                                    timeline["y"] = 0f;
                                                }
                                                processCurve(timeline, keep);
                                            }
                                            newBoneData["shear"] = timelines.DeepClone().AsArray();
                                        }
                                    }
                                }
                                else if (timelineName == "sheary")
                                {
                                    //boneData.Remove(timelineName);
                                    if (boneData.ContainsKey("shear"))
                                    {
                                        if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                    }
                                    else
                                    {
                                        if (boneData.ContainsKey("shearx"))
                                        {
                                            if (keep) reserved[boneName][timelineName] = timelines.DeepClone().AsArray();
                                        }
                                        else
                                        {
                                            foreach (JsonObject timeline in timelines)
                                            {
                                                if (timeline.TryGetPropertyValue("value", out var value))
                                                {
                                                    timeline.Remove("value");
                                                    timeline["x"] = 0f;
                                                    timeline["y"] = (float)value;
                                                }
                                                processCurve(timeline, keep);
                                            }
                                            newBoneData["shear"] = timelines.DeepClone().AsArray();
                                        }
                                    }
                                }

                            }
                        }
                        //if (boneData.Count > 0) animation["bones"][boneName] = boneData;
                        //if (reserved.Count > 0) boneData["reserved"] = reserved;
                        //if (reserved.Count > 0) newBoneData["reserved"] = reserved;

                        animation["bones"] = newBones;
                        reservedAnimation["bones"] = reserved;


                    }
                    //--->bone

                    //<---ik
                    if (animation.TryGetPropertyValue("ik", out var iks))
                    {
                        foreach (var (ikName, _ik) in iks.AsObject())
                        {
                            foreach (var _timelines in _ik.AsArray())
                            {
                                var timelines = _timelines.AsArray();
                                foreach (JsonObject timeline in timelines)
                                {
                                    processCurve(timeline, keep);
                                }
                            }
                            //if (reserved.Count > 0) ikData["reserved"] = reserved;
                        }
                    }
                    //--->ik

                    //<---transform
                    if (animation.TryGetPropertyValue("transform", out var transforms1))
                    {
                        foreach (var (transformName, _transform) in transforms1.AsObject())
                        {
                            //JsonObject reserved = [];
                            foreach (var _timelines in _transform.AsArray())
                            {
                                var timelines = _timelines.AsArray();
                                foreach (JsonObject timeline in timelines)
                                {
                                    JsonObject reservedItem = new JsonObject();
                                    if (timeline.TryGetPropertyValue("mixRotate", out var mixRotate))
                                    {
                                        timeline["rotateMix"] = (float)mixRotate;
                                        timeline.Remove("mixRotate");
                                    }
                                    if (timeline.TryGetPropertyValue("mixX", out var mixX))
                                    {
                                        timeline["translateMix"] = (float)mixX;
                                        timeline.Remove("mixX");
                                        if (keep) reservedItem["mixX"] = (float)mixX;
                                    }
                                    if (timeline.TryGetPropertyValue("mixY", out var mixY))
                                    {
                                        timeline.Remove("mixY");
                                        if (keep) reservedItem["mixY"] = (float)mixY;
                                    }
                                    if (timeline.TryGetPropertyValue("mixScaleX", out var mixScaleX))
                                    {
                                        timeline["scaleMix"] = (float)mixScaleX;
                                        timeline.Remove("mixScaleX");
                                        if (keep) reservedItem["mixScaleX"] = (float)mixScaleX;
                                    }
                                    if (timeline.TryGetPropertyValue("mixScaleY", out var mixScaleY))
                                    {
                                        timeline.Remove("mixScaleY");
                                        if (keep) reservedItem["mixScaleY"] = (float)mixScaleY;
                                    }
                                    if (timeline.TryGetPropertyValue("mixShearY", out var mixShearY))
                                    {
                                        timeline["shearMix"] = (float)mixShearY;
                                        timeline.Remove("mixShearY");
                                    }
                                    processCurve(timeline, keep);
                                }
                            }
                            //if (reserved.Count > 0) transformData["reserved"] = reserved;
                        }
                    }

                    //<---path
                    if (animation.TryGetPropertyValue("path", out var path1))
                    {
                        var reservedPath = data["reserved"]["path"].AsObject();
                        JsonObject reserved = new JsonObject();
                        JsonObject newPathData = [];
                        foreach (var (pathName, _path) in path1.AsObject())
                        {
                            if (reservedPath.ContainsKey((string)pathName))
                            {
                                //path1.AsObject().Remove(pathName);
                                if (keep) reserved[pathName] = _path.DeepClone().AsObject();
                            }
                            else
                            {
                                foreach (var (timelineName, _timelines) in _path.AsObject())
                                {
                                    if (timelineName == "mix")
                                    {
                                        var timelines = _timelines.AsArray();
                                        foreach (JsonObject timeline in timelines)
                                        {
                                            if (timeline.TryGetPropertyValue("mixRotate", out var mixRotate))
                                            {
                                                timeline.Remove("mixRotate");
                                                timeline["rotateMix"] = (float)mixRotate;
                                            }
                                            if (timeline.TryGetPropertyValue("mixX", out var mixX))
                                            {
                                                timeline.Remove("mixX");
                                                timeline["translateMix"] = (float)mixX;
                                            }
                                            if (timeline.TryGetPropertyValue("mixY", out var mixY))
                                            {
                                                timeline.Remove("mixY");
                                                if (!timeline.ContainsKey("translateMix"))
                                                {
                                                    timeline["translateMix"] = (float)mixY;
                                                }
                                                else if (keep)
                                                {
                                                    timeline["reserved"] = new JsonObject()
                                                    {
                                                        ["mixY"] = (float)mixY,
                                                    };
                                                }
                                            }
                                        }
                                    }
                                }
                                newPathData[pathName] = _path.DeepClone().AsObject();
                            }
                        }
                        animation["path"] = newPathData;
                    }
                    //--->path

                    //<---physics
                    if (animation.TryGetPropertyValue("physics", out var physics1))
                    {
                        animation.Remove("physics");
                        if (keep) reservedAnimation["physics"] = physics1.DeepClone().AsObject();
                    }
                    //--->physics

                    //<---attachments
                    if (animation.TryGetPropertyValue("attachment", out var attachments))
                    {
                        JsonObject reservedAttachment = new JsonObject();
                        foreach (var (skinName, _skins) in attachments.AsObject())
                        {
                            reservedAttachment[skinName] = new JsonObject();
                            foreach (var (slotName, _slot) in _skins.AsObject())
                            {
                                reservedAttachment[skinName][slotName] = new JsonObject();
                                JsonObject slotData = _slot.AsObject();
                                foreach (var (attachmentName, _attachment) in slotData)
                                {
                                    JsonObject attachmentData = _attachment.AsObject();
                                    //JsonArray deformList = [];
                                    foreach (var (timelineName, _timelines) in attachmentData)
                                    {
                                        if (timelineName == "deform")
                                        {
                                            slotData[attachmentName] = _timelines.DeepClone().AsArray();
                                            //deformList.Add(_timelines.DeepClone().AsArray());
                                        }
                                        else if (timelineName == "sequence" && keep)
                                        {
                                            reservedAttachment[skinName][slotName][attachmentName] = new JsonObject()
                                            {
                                                ["sequence"] = _timelines.DeepClone().AsArray(),
                                            };
                                        }
                                    }
                                }
                            }
                        }
                        if (keep) reservedAnimation["attachment"] = reservedAttachment;
                        animation.Remove("attachment");
                        animation["deform"] = attachments;
                    }
                    if (reservedAnimation.Count > 0) animation["reserved"] = reservedAnimation;
                    //--->attachments
                }
            }
            if (!keep) data.Remove("reserved");//移除path

            return data;
        }

        private static void processCurve(JsonObject timeline, bool keep)
        {
            //emm,3.8版本的曲线只有一条,故在还原的时候通过检查前四个是否相等
            //来判断是否对曲线进行了修改，从而判断恢复原先数据还是用修改后的数据
            //覆盖所有的curve数据
            if (timeline.TryGetPropertyValue("curve", out var curve) && curve.GetValueKind() == JsonValueKind.Array)
            {
                curve = curve.AsArray();
                if (keep) timeline["reserved"] = new JsonObject()
                {
                    ["curve"] = curve.DeepClone().AsArray()
                };
                timeline["curve"] = (float)curve[0];
                timeline["c2"] = (float)curve[1];
                timeline["c3"] = (float)curve[2];
                timeline["c4"] = (float)curve[3];
            }
        }
    }
}
