using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineRuntime38;
using System.Text.Json;
using System.Text.Json.Nodes;
using SpineRuntime38.Attachments;

namespace SpineViewer.Spine.Implementations.SkeletonConverter
{
    [SkeletonConverterImplementation(Version.V38)]
    class SkeletonConverter38 : SpineViewer.Spine.SkeletonConverter
    {
        private SkeletonReader reader = null;
        private bool nonessential = false;
        private JsonObject root = null;

        protected override JsonObject ReadBinary(string binPath)
        {
            using var input = File.OpenRead(binPath);
            reader = new(input);

            var result = root = [];
            root["skeleton"] = ReadSkeleton();
            ReadStrings();
            root["bones"] = ReadBones();
            root["slots"] = ReadSlots();
            root["ik"] = ReadIK();
            root["transform"] = ReadTransform();
            root["path"] = ReadPath();
            root["skins"] = ReadSkins();
            root["events"] = ReadEvents();
            root["aimations"] = ReadAnimations();

            reader = null;
            nonessential = false;
            root = null;

            return result;
        }

        private JsonObject ReadSkeleton()
        {
            JsonObject skeleton = [];
            skeleton["hash"] = reader.ReadString();
            skeleton["spine"] = reader.ReadString();
            skeleton["x"] = reader.ReadFloat();
            skeleton["y"] = reader.ReadFloat();
            skeleton["width"] = reader.ReadFloat();
            skeleton["height"] = reader.ReadFloat();
            nonessential = reader.ReadBoolean();
            if (nonessential)
            {
                skeleton["fps"] = reader.ReadFloat();
                skeleton["images"] = reader.ReadString();
                skeleton["audio"] = reader.ReadString();
            }
            return skeleton;
        }

        private void ReadStrings()
        {
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
                reader.StringTable.Add(reader.ReadString());
        }

        private JsonArray ReadBones()
        {
            JsonArray bones = [];
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                if (i != 0) data["parent"] = bones[reader.ReadVarInt()]["name"].GetValue<string>();
                data["rotation"] = reader.ReadFloat();
                data["x"] = reader.ReadFloat();
                data["y"] = reader.ReadFloat();
                data["scaleX"] = reader.ReadFloat();
                data["scaleY"] = reader.ReadFloat();
                data["shearX"] = reader.ReadFloat();
                data["shearY"] = reader.ReadFloat();
                data["length"] = reader.ReadFloat();
                data["transform"] = SkeletonBinary.TransformModeValues[reader.ReadVarInt()].ToString();
                data["skin"] = reader.ReadBoolean();
                if (nonessential) reader.ReadInt();
                bones.Add(data);
            }
            return bones;
        }

        private JsonArray ReadSlots()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray slots = [];
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["bone"] = bones[reader.ReadVarInt()]["name"].GetValue<string>();
                data["color"] = reader.ReadInt().ToString("x8"); // 0xrrggbbaa -> rrggbbaa
                int dark = reader.ReadInt();
                if (dark != -1) data["dark"] = dark.ToString("x6"); // 0x00rrggbb -> rrggbb
                data["attachment"] = reader.ReadStringRef();
                data["blend"] = ((BlendMode)reader.ReadVarInt()).ToString();
                slots.Add(data);
            }
            return slots;
        }

        private JsonArray ReadIK()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray ik = [];
            int count = reader.ReadVarInt();
            for (int i = 0, bonesCount; i < count; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                JsonArray bonesArray = []; bonesCount = reader.ReadVarInt();
                for (int j = 0; j < bonesCount; j++) bonesArray.Add(bones[reader.ReadVarInt()]["name"].GetValue<string>());
                data["bones"] = bonesArray;
                data["target"] = bones[reader.ReadVarInt()]["name"].GetValue<string>();
                data["mix"] = reader.ReadFloat();
                data["softness"] = reader.ReadFloat();
                data["bendPositive"] = reader.ReadSByte() > 0;
                data["compress"] = reader.ReadBoolean();
                data["stretch"] = reader.ReadBoolean();
                data["uniform"] = reader.ReadBoolean();
                ik.Add(data);
            }
            return ik;
        }

        private JsonArray ReadTransform()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray transform = [];
            int count = reader.ReadVarInt();
            for (int i = 0, bonesCount; i < count; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                JsonArray bonesArray = []; bonesCount = reader.ReadVarInt();
                for (int j = 0; j < bonesCount; j++) bonesArray.Add(bones[reader.ReadVarInt()]["name"].GetValue<string>());
                data["bones"] = bonesArray;
                data["target"] = bones[reader.ReadVarInt()]["name"].GetValue<string>();
                data["local"] = reader.ReadBoolean();
                data["relative"] = reader.ReadBoolean();
                data["rotation"] = reader.ReadFloat();
                data["x"] = reader.ReadFloat();
                data["y"] = reader.ReadFloat();
                data["scaleX"] = reader.ReadFloat();
                data["scaleY"] = reader.ReadFloat();
                data["shearY"] = reader.ReadFloat();
                data["rotateMix"] = reader.ReadFloat();
                data["translateMix"] = reader.ReadFloat();
                data["scaleMix"] = reader.ReadFloat();
                data["shearMix"] = reader.ReadFloat();
                transform.Add(data);
            }
            return transform;
        }

        private JsonArray ReadPath()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray path = [];
            int count = reader.ReadVarInt();
            for (int i = 0, bonesCount; i < count; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                JsonArray bonesArray = []; bonesCount = reader.ReadVarInt();
                for (int j = 0; j < bonesCount; j++) bonesArray.Add(bones[reader.ReadVarInt()]["name"].GetValue<string>());
                data["bones"] = bonesArray;
                data["target"] = bones[reader.ReadVarInt()]["name"].GetValue<string>();
                data["positionMode"] = ((PositionMode)reader.ReadVarInt()).ToString();
                data["spacingMode"] = ((SpacingMode)reader.ReadVarInt()).ToString();
                data["rotateMode"] = ((RotateMode)reader.ReadVarInt()).ToString();
                data["rotation"] = reader.ReadFloat();
                data["position"] = reader.ReadFloat();
                data["spacing"] = reader.ReadFloat();
                data["rotateMix"] = reader.ReadFloat();
                data["translateMix"] = reader.ReadFloat();
                path.Add(data);
            }
            return path;
        }

        private JsonArray ReadSkins()
        {
            JsonArray skins = [];

            // default skin
            if (ReadSkin(true) is JsonObject data)
                skins.Add(data);

            // other skins
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
                skins.Add(ReadSkin());

            return skins;
        }

        private JsonObject? ReadSkin(bool isDefault = false)
        {
            JsonObject skin = [];
            int count = 0;
            if (isDefault)
            {
                // 这里固定有一个给 default 的 count 值, 算是占位符, 如果是 0 则表示没有 default 的 skin
                count = reader.ReadVarInt();
                if (count <= 0) return null;
                skin["name"] = "default";
            }
            else
            {
                skin["name"] = reader.ReadStringRef();

                JsonArray bones = root["bones"].AsArray();
                JsonArray bonesArray = []; count = reader.ReadVarInt();
                for (int i = 0; i < count; i++) bonesArray.Add(bones[reader.ReadVarInt()]["name"].GetValue<string>());
                skin["bones"] = bonesArray;

                JsonArray ik = root["ik"].AsArray();
                JsonArray ikArray = []; count = reader.ReadVarInt();
                for (int i = 0; i < count; i++) ikArray.Add(ik[reader.ReadVarInt()]["name"].GetValue<string>());
                skin["ik"] = ikArray;

                JsonArray transform = root["transform"].AsArray();
                JsonArray transformArray = []; count = reader.ReadVarInt();
                for (int i = 0; i < count; i++) transformArray.Add(transform[reader.ReadVarInt()]["name"].GetValue<string>());
                skin["transform"] = transformArray;

                JsonArray path = root["path"].AsArray();
                JsonArray pathArray = []; count = reader.ReadVarInt();
                for (int i = 0; i < count; i++) pathArray.Add(path[reader.ReadVarInt()]["name"].GetValue<string>());
                skin["path"] = pathArray;

                count = reader.ReadVarInt();
            }

            JsonArray slots = root["slots"].AsArray();
            JsonObject attachments = [];
            for (int i = 0; i < count; i++)
            {
                JsonObject data = [];
                attachments[slots[reader.ReadVarInt()]["name"].GetValue<string>()] = data;
                for (int j = 0, attachmentCount = reader.ReadVarInt(); j < attachmentCount; j++)
                {
                    var attachmentName = reader.ReadStringRef();
                    var attachment = ReadAttachment(attachmentName);
                    if (attachment is not null) data[attachmentName] = attachment;
                }
            }
            skin["attachments"] = attachments;

            return skin;
        }

        private JsonObject? ReadAttachment(string keyName)
        {
            JsonArray slots = root["slots"].AsArray();
            JsonObject attachment = [];
            int vertexCount = 0;

            string name = reader.ReadStringRef() ?? keyName;
            var type = (AttachmentType)reader.ReadByte();
            attachment["name"] = name;
            attachment["type"] = type.ToString();
            switch (type)
            {
                case AttachmentType.Region:
                    attachment["path"] = reader.ReadStringRef();
                    attachment["rotation"] = reader.ReadFloat();
                    attachment["x"] = reader.ReadFloat();
                    attachment["y"] = reader.ReadFloat();
                    attachment["scaleX"] = reader.ReadFloat();
                    attachment["scaleY"] = reader.ReadFloat();
                    attachment["width"] = reader.ReadFloat();
                    attachment["height"] = reader.ReadFloat();
                    attachment["color"] = reader.ReadInt().ToString("x8");
                    return attachment;
                case AttachmentType.Boundingbox:
                    vertexCount = reader.ReadVarInt();
                    attachment["vertexCount"] = vertexCount;
                    attachment["vertices"] = ReadVertices(vertexCount);
                    if (nonessential) reader.ReadInt();
                    return attachment;
                case AttachmentType.Mesh:
                    attachment["path"] = reader.ReadStringRef();
                    attachment["color"] = reader.ReadInt().ToString("x8");
                    vertexCount = reader.ReadVarInt();
                    attachment["uvs"] = ReadFloatArray(vertexCount << 1);
                    attachment["triangles"] = ReadShortArray();
                    attachment["vertices"] = ReadVertices(vertexCount);
                    attachment["hull"] = reader.ReadVarInt();
                    if (nonessential)
                    {
                        attachment["edges"] = ReadShortArray();
                        attachment["width"] = reader.ReadFloat();
                        attachment["height"] = reader.ReadFloat();
                    }
                    return attachment;
                case AttachmentType.Linkedmesh:
                    attachment["path"] = reader.ReadStringRef();
                    attachment["color"] = reader.ReadInt().ToString("x8");
                    attachment["skin"] = reader.ReadStringRef();
                    attachment["parent"] = reader.ReadStringRef();
                    attachment["deform"] = reader.ReadBoolean();
                    if (nonessential)
                    {
                        attachment["width"] = reader.ReadFloat();
                        attachment["height"] = reader.ReadFloat();
                    }
                    // 补充缺失的必需 key
                    attachment["uvs"] = new JsonArray();
                    attachment["triangles"] = new JsonArray();
                    attachment["vertices"] = new JsonArray();
                    return attachment;
                case AttachmentType.Path:
                    attachment["closed"] = reader.ReadBoolean();
                    attachment["constantSpeed"] = reader.ReadBoolean();
                    vertexCount = reader.ReadVarInt();
                    attachment["vertexCount"] = vertexCount;
                    attachment["vertices"] = ReadVertices(vertexCount);
                    attachment["lengths"] = ReadFloatArray(vertexCount / 3);
                    if (nonessential) reader.ReadInt();
                    return attachment;
                case AttachmentType.Point:
                    attachment["rotation"] = reader.ReadFloat();
                    attachment["x"] = reader.ReadFloat();
                    attachment["y"] = reader.ReadFloat();
                    if (nonessential) reader.ReadInt();
                    return attachment;
                case AttachmentType.Clipping:
                    attachment["end"] = slots[reader.ReadVarInt()]["name"].GetValue<string>();
                    vertexCount = reader.ReadVarInt();
                    attachment["vertexCount"] = vertexCount;
                    attachment["vertices"] = ReadVertices(vertexCount);
                    if (nonessential) reader.ReadInt();
                    return attachment;
            }
            return null;
        }

        private JsonObject ReadEvents()
        {
            JsonObject events = [];
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
            {
                JsonObject data = [];
                events[reader.ReadStringRef()] = data;
                data["int"] = reader.ReadVarInt(false);
                data["float"] = reader.ReadFloat();
                data["string"] = reader.ReadString();
                string audio = reader.ReadString();
                if (audio is not null)
                {
                    data["audio"] = audio;
                    data["volume"] = reader.ReadFloat();
                    data["balance"] = reader.ReadFloat();
                }
            }
            return events;
        }

        private JsonObject ReadAnimations()
        {
            JsonObject animations = [];
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
            {
                throw new NotImplementedException();
            }
            return animations;
        }

        public JsonArray ReadFloatArray(int length)
        {
            JsonArray array = [];
            for (int i = 0; i < length; i++)
                array.Add(reader.ReadFloat());
            return array;
        }

        public JsonArray ReadShortArray()
        {
            JsonArray array = [];
            int length = reader.ReadVarInt();
            for (int i = 0; i < length; i++)
                array.Add((reader.ReadByte() << 8) | reader.ReadByte());
            return array;
        }

        public JsonArray ReadVertices(int vertexCount)
        {
            JsonArray vertices = [];
            if (!reader.ReadBoolean())
                return ReadFloatArray(vertexCount << 1);

            for (int i = 0; i < vertexCount; i++)
            {
                int bonesCount = reader.ReadVarInt();
                vertices.Add(bonesCount);
                for (int j = 0; j < bonesCount; j++)
                {
                    vertices.Add(reader.ReadVarInt());
                    vertices.Add(reader.ReadFloat());
                    vertices.Add(reader.ReadFloat());
                    vertices.Add(reader.ReadFloat());
                }
            }
            return vertices;
        }

        protected override void WriteBinary(JsonObject root, string binPath)
        {
            throw new NotImplementedException();
        }

        //public void WriteFloatArray(float[] array)
        //{
        //    foreach (var i in array)
        //        writer.WriteFloat(i);
        //}

        //public void WriteShortArray(int[] array)
        //{
        //    foreach (var i in array)
        //    {
        //        writer.WriteByte((byte)(i >> 8)); 
        //        writer.WriteByte((byte)i);
        //    }
        //}
    }
}
