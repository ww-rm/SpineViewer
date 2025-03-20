using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineRuntime38;
using System.Text.Json;
using System.Text.Json.Nodes;
using SpineRuntime38.Attachments;
using System.Globalization;

namespace SpineViewer.Spine.Implementations.SkeletonConverter
{
    [SkeletonConverterImplementation(Version.V38)]
    class SkeletonConverter38 : SpineViewer.Spine.SkeletonConverter
    {
        private BinaryReader reader = null;
        private JsonObject root = null;
        private bool nonessential = false;

        private readonly List<JsonObject> idx2event = [];

        public override JsonObject ReadBinary(string binPath)
        {
            var root = new JsonObject();
            using var input = File.OpenRead(binPath);

            this.root = root;
            reader = new(input);

            ReadSkeleton();
            ReadStrings();
            ReadBones();
            ReadSlots();
            ReadIK();
            ReadTransform();
            ReadPath();
            ReadSkins();
            ReadEvents();
            ReadAnimations();

            reader = null;
            this.root = null;

            idx2event.Clear();

            return root;
        }

        private void ReadSkeleton()
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
            root["skeleton"] = skeleton;
        }

        private void ReadStrings()
        {
            for (int n = reader.ReadVarInt(); n > 0; n--)
                reader.StringTable.Add(reader.ReadString());
        }

        private void ReadBones()
        {
            JsonArray bones = [];
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                if (i > 0) data["parent"] = (string)bones[reader.ReadVarInt()]["name"];
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
            root["bones"] = bones;
        }

        private void ReadSlots()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray slots = [];
            for (int n = reader.ReadVarInt(); n > 0; n--)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["bone"] = (string)bones[reader.ReadVarInt()]["name"];
                data["color"] = reader.ReadInt().ToString("x8"); // 0xrrggbbaa -> rrggbbaa
                int dark = reader.ReadInt();
                if (dark != -1) data["dark"] = dark.ToString("x6"); // 0x00rrggbb -> rrggbb
                data["attachment"] = reader.ReadStringRef();
                data["blend"] = ((BlendMode)reader.ReadVarInt()).ToString();
                slots.Add(data);
            }
            root["slots"] = slots;
        }

        private void ReadIK()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray ik = [];
            for (int n = reader.ReadVarInt(); n > 0; n--)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                data["bones"] = ReadNames(bones);
                data["target"] = (string)bones[reader.ReadVarInt()]["name"];
                data["mix"] = reader.ReadFloat();
                data["softness"] = reader.ReadFloat();
                data["bendPositive"] = reader.ReadSByte() > 0;
                data["compress"] = reader.ReadBoolean();
                data["stretch"] = reader.ReadBoolean();
                data["uniform"] = reader.ReadBoolean();
                ik.Add(data);
            }
            root["ik"] = ik;
        }

        private void ReadTransform()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray transform = [];
            for (int n = reader.ReadVarInt(); n > 0; n--)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                data["bones"] = ReadNames(bones);
                data["target"] = (string)bones[reader.ReadVarInt()]["name"];
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
            root["transform"] = transform;
        }

        private void ReadPath()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray path = [];
            for (int n = reader.ReadVarInt(); n > 0; n--)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                data["bones"] = ReadNames(bones);
                data["target"] = (string)bones[reader.ReadVarInt()]["name"];
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
            root["path"] = path;
        }

        private void ReadSkins()
        {
            JsonArray skins = [];

            // default skin
            if (ReadSkin(true) is JsonObject data)
                skins.Add(data);

            // other skins
            for (int n = reader.ReadVarInt(); n > 0; n--)
                skins.Add(ReadSkin());

            root["skins"] = skins;
        }

        private JsonObject? ReadSkin(bool isDefault = false)
        {
            JsonObject skin = [];
            int slotCount;
            if (isDefault)
            {
                // 这里固定有一个给 default 的 count 值, 算是占位符, 如果是 0 则表示没有 default 的 skin
                skin["name"] = "default";
                slotCount = reader.ReadVarInt();
                if (slotCount <= 0) return null;
            }
            else
            {
                skin["name"] = reader.ReadStringRef();
                skin["bones"] = ReadNames(root["bones"].AsArray());
                skin["ik"] = ReadNames(root["ik"].AsArray());
                skin["transform"] = ReadNames(root["transform"].AsArray()); ;
                skin["path"] = ReadNames(root["path"].AsArray()); ;
                slotCount = reader.ReadVarInt();
            }

            JsonArray slots = root["slots"].AsArray();
            JsonObject skinAttachments = [];
            while (slotCount-- > 0)
            {
                JsonObject slotAttachments = [];
                skinAttachments[(string)slots[reader.ReadVarInt()]["name"]] = slotAttachments;
                for (int attachmentCount = reader.ReadVarInt(); attachmentCount > 0; attachmentCount--)
                {
                    var attachmentKey = reader.ReadStringRef();
                    slotAttachments[attachmentKey] = ReadAttachment(attachmentKey);
                }
            }
            skin["attachments"] = skinAttachments;

            return skin;
        }

        private JsonObject ReadAttachment(string keyName)
        {
            JsonArray slots = root["slots"].AsArray();
            JsonObject attachment = [];
            int vertexCount;
            string path;

            string name = reader.ReadStringRef() ?? keyName;
            var type = (AttachmentType)reader.ReadByte();
            attachment["name"] = name;
            attachment["type"] = type.ToString();
            switch (type)
            {
                case AttachmentType.Region:
                    path = reader.ReadStringRef();
                    if (path is not null) attachment["path"] = path;
                    attachment["rotation"] = reader.ReadFloat();
                    attachment["x"] = reader.ReadFloat();
                    attachment["y"] = reader.ReadFloat();
                    attachment["scaleX"] = reader.ReadFloat();
                    attachment["scaleY"] = reader.ReadFloat();
                    attachment["width"] = reader.ReadFloat();
                    attachment["height"] = reader.ReadFloat();
                    attachment["color"] = reader.ReadInt().ToString("x8");
                    break;
                case AttachmentType.Boundingbox:
                    vertexCount = reader.ReadVarInt();
                    attachment["vertexCount"] = vertexCount;
                    attachment["vertices"] = ReadVertices(vertexCount);
                    if (nonessential) reader.ReadInt();
                    break;
                case AttachmentType.Mesh:
                    path = reader.ReadStringRef();
                    if (path is not null) attachment["path"] = path;
                    attachment["color"] = reader.ReadInt().ToString("x8");
                    vertexCount = reader.ReadVarInt();
                    attachment["uvs"] = ReadFloatArray(vertexCount << 1); // vertexCount = uvs.Length
                    attachment["triangles"] = ReadShortArray();
                    attachment["vertices"] = ReadVertices(vertexCount);
                    attachment["hull"] = reader.ReadVarInt();
                    if (nonessential)
                    {
                        attachment["edges"] = ReadShortArray();
                        attachment["width"] = reader.ReadFloat();
                        attachment["height"] = reader.ReadFloat();
                    }
                    break;
                case AttachmentType.Linkedmesh:
                    path = reader.ReadStringRef();
                    if (path is not null) attachment["path"] = path;
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
                    break;
                case AttachmentType.Path:
                    attachment["closed"] = reader.ReadBoolean();
                    attachment["constantSpeed"] = reader.ReadBoolean();
                    vertexCount = reader.ReadVarInt();
                    attachment["vertexCount"] = vertexCount;
                    attachment["vertices"] = ReadVertices(vertexCount);
                    attachment["lengths"] = ReadFloatArray(vertexCount / 3);
                    if (nonessential) reader.ReadInt();
                    break;
                case AttachmentType.Point:
                    attachment["rotation"] = reader.ReadFloat();
                    attachment["x"] = reader.ReadFloat();
                    attachment["y"] = reader.ReadFloat();
                    if (nonessential) reader.ReadInt();
                    break;
                case AttachmentType.Clipping:
                    attachment["end"] = (string)slots[reader.ReadVarInt()]["name"];
                    vertexCount = reader.ReadVarInt();
                    attachment["vertexCount"] = vertexCount;
                    attachment["vertices"] = ReadVertices(vertexCount);
                    if (nonessential) reader.ReadInt();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid attachment type: {type}");
            }
            return attachment;
        }

        private void ReadEvents()
        {
            idx2event.Clear();
            JsonObject events = [];
            for (int n = reader.ReadVarInt(); n > 0; n--)
            {
                JsonObject data = [];
                var name = reader.ReadStringRef();
                events[name] = data;
                data["name"] = name; // 额外增加的, 方便后面查找
                data["int"] = reader.ReadVarInt(false);
                data["float"] = reader.ReadFloat();
                data["string"] = reader.ReadString();
                if (reader.ReadString() is string audio)
                {
                    data["audio"] = audio;
                    data["volume"] = reader.ReadFloat();
                    data["balance"] = reader.ReadFloat();
                }
                idx2event.Add(data);
            }
            root["events"] = events;
        }

        private void ReadAnimations()
        {
            JsonObject animations = [];
            for (int n = reader.ReadVarInt(); n > 0; n--)
            {
                JsonObject data = [];
                animations[reader.ReadString()] = data;
                if (ReadSlotTimelines() is JsonObject slots) data["slots"] = slots;
                if (ReadBoneTimelines() is JsonObject bones) data["bones"] = bones;
                if (ReadIKTimelines() is JsonObject ik) data["ik"] = ik;
                if (ReadTransformTimelines() is JsonObject transform) data["transform"] = transform;
                if (ReadPathTimelines() is JsonObject path) data["path"] = path;
                if (ReadDeformTimelines() is JsonObject deform) data["deform"] = deform;
                if (ReadDrawOrderTimelines() is JsonArray draworder) data["drawOrder"] = draworder;
                if (ReadEventTimelines() is JsonArray events) data["events"] = events;
            }
            root["animations"] = animations;
        }

        private JsonObject? ReadSlotTimelines()
        {
            JsonArray slots = root["slots"].AsArray();
            JsonObject slotTimelines = [];

            for (int slotCount = reader.ReadVarInt(); slotCount > 0; slotCount--)
            {
                JsonObject timeline = [];
                slotTimelines[(string)slots[reader.ReadVarInt()]["name"]] = timeline;
                for (int timelineCount = reader.ReadVarInt(); timelineCount > 0; timelineCount--)
                {
                    JsonArray frames = [];
                    var type = reader.ReadByte();
                    var frameCount = reader.ReadVarInt();
                    switch (type)
                    {
                        case SkeletonBinary.SLOT_ATTACHMENT:
                            timeline["attachment"] = frames;
                            while (frameCount-- > 0)
                            {
                                frames.Add(new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["name"] = reader.ReadStringRef(),
                                });
                            }
                            break;
                        case SkeletonBinary.SLOT_COLOR:
                            timeline["color"] = frames;
                            while (frameCount-- > 0)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["color"] = reader.ReadInt().ToString("x8"),
                                };
                                if (frameCount > 0) ReadCurve(o);
                                frames.Add(o);
                            }
                            break;
                        case SkeletonBinary.SLOT_TWO_COLOR:
                            timeline["twoColor"] = frames;
                            while (frameCount-- > 0)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["light"] = reader.ReadInt().ToString("x8"),
                                    ["dark"] = reader.ReadInt().ToString("x6"),
                                }; 
                                if (frameCount > 0) ReadCurve(o);
                                frames.Add(o);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Invalid slot timeline type: {type}");
                    }
                }
            }

            return slotTimelines.Count > 0 ? slotTimelines : null;
        }

        private JsonObject? ReadBoneTimelines()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonObject boneTimelines = [];

            for (int boneCount = reader.ReadVarInt(); boneCount > 0; boneCount--)
            {
                JsonObject timeline = [];
                boneTimelines[(string)bones[reader.ReadVarInt()]["name"]] = timeline;
                for (int timelineCount = reader.ReadVarInt(); timelineCount > 0; timelineCount--)
                {
                    JsonArray frames = [];
                    var type = reader.ReadByte();
                    var frameCount = reader.ReadVarInt();
                    switch (type)
                    {
                        case SkeletonBinary.BONE_ROTATE:
                            timeline["rotate"] = frames;
                            while (frameCount-- > 0)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["angle"] = reader.ReadFloat(),
                                };
                                if (frameCount > 0) ReadCurve(o);
                                frames.Add(o);
                            }
                            break;
                        case SkeletonBinary.BONE_TRANSLATE:
                            timeline["translate"] = frames;
                            while (frameCount-- > 0)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["x"] = reader.ReadFloat(),
                                    ["y"] = reader.ReadFloat(),
                                };
                                if (frameCount > 0) ReadCurve(o);
                                frames.Add(o);
                            }
                            break;
                        case SkeletonBinary.BONE_SCALE:
                            timeline["scale"] = frames;
                            while (frameCount-- > 0)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["x"] = reader.ReadFloat(),
                                    ["y"] = reader.ReadFloat(),
                                };
                                if (frameCount > 0) ReadCurve(o);
                                frames.Add(o);
                            }
                            break;
                        case SkeletonBinary.BONE_SHEAR:
                            timeline["shear"] = frames;
                            while (frameCount-- > 0)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["x"] = reader.ReadFloat(),
                                    ["y"] = reader.ReadFloat(),
                                };
                                if (frameCount > 0) ReadCurve(o);
                                frames.Add(o);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Invalid bone timeline type: {type}");
                    }
                }
            }

            return boneTimelines.Count > 0 ? boneTimelines : null;
        }

        private JsonObject? ReadIKTimelines()
        {
            JsonArray ik = root["ik"].AsArray();
            JsonObject ikTimelines = [];

            for (int ikCount = reader.ReadVarInt(); ikCount > 0; ikCount--)
            {
                JsonArray frames = [];
                ikTimelines[(string)ik[reader.ReadVarInt()]["name"]] = frames;
                for (int frameCount = reader.ReadVarInt(); frameCount > 0; frameCount--)
                {
                    var o = new JsonObject()
                    {
                        ["time"] = reader.ReadFloat(),
                        ["mix"] = reader.ReadFloat(),
                        ["softness"] = reader.ReadFloat(),
                        ["bendPositive"] = reader.ReadSByte() > 0,
                        ["compress"] = reader.ReadBoolean(),
                        ["stretch"] = reader.ReadBoolean(),
                    };
                    if (frameCount > 1) ReadCurve(o);
                    frames.Add(o);
                }
            }

            return ikTimelines.Count > 0 ? ikTimelines : null;
        }

        private JsonObject? ReadTransformTimelines()
        {
            JsonArray transform = root["transform"].AsArray();
            JsonObject transformTimelines = [];

            for (int transformCount = reader.ReadVarInt(); transformCount > 0; transformCount--)
            {
                JsonArray frames = [];
                transformTimelines[(string)transform[reader.ReadVarInt()]["name"]] = frames;
                for (int frameCount = reader.ReadVarInt(); frameCount > 0; frameCount--)
                {
                    var o = new JsonObject()
                    {
                        ["time"] = reader.ReadFloat(),
                        ["rotateMix"] = reader.ReadFloat(),
                        ["translateMix"] = reader.ReadFloat(),
                        ["scaleMix"] = reader.ReadFloat(),
                        ["shearMix"] = reader.ReadFloat(),
                    };
                    if (frameCount > 1) ReadCurve(o);
                    frames.Add(o);
                }
            }

            return transformTimelines.Count > 0 ? transformTimelines : null;
        }

        private JsonObject? ReadPathTimelines()
        {
            JsonArray path = root["path"].AsArray();
            JsonObject pathTimelines = [];

            for (int pathCount = reader.ReadVarInt(); pathCount > 0; pathCount--)
            {
                JsonObject timeline = [];
                pathTimelines[(string)path[reader.ReadVarInt()]["name"]] = timeline;
                for (int timelineCount = reader.ReadVarInt(); timelineCount > 0; timelineCount--)
                {
                    JsonArray frames = [];
                    var type = reader.ReadByte();
                    var frameCount = reader.ReadVarInt();
                    switch (type)
                    {
                        case SkeletonBinary.PATH_POSITION:
                            timeline["position"] = frames;
                            while (frameCount-- > 0)
                            {
                                frames.Add(new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["position"] = reader.ReadFloat(),
                                });
                            }
                            break;
                        case SkeletonBinary.PATH_SPACING:
                            timeline["spacing"] = frames;
                            while (frameCount-- > 0)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["spacing"] = reader.ReadFloat(),
                                };
                                if (frameCount > 0) ReadCurve(o);
                                frames.Add(o);
                            }
                            break;
                        case SkeletonBinary.PATH_MIX:
                            timeline["mix"] = frames;
                            while (frameCount-- > 0)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["rotateMix"] = reader.ReadFloat(),
                                    ["translateMix"] = reader.ReadFloat(),
                                };
                                if (frameCount > 0) ReadCurve(o);
                                frames.Add(o);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Invalid path timeline type: {type}");
                    }
                }
            }

            return pathTimelines.Count > 0 ? pathTimelines : null;
        }

        private JsonObject? ReadDeformTimelines()
        {
            JsonArray slots = root["slots"].AsArray();
            JsonArray skins = root["skins"].AsArray();
            JsonObject deformTimelines = [];

            for (int skinCount = reader.ReadVarInt(); skinCount > 0; skinCount--)
            {
                JsonObject skinValue = [];
                deformTimelines[(string)skins[reader.ReadVarInt()]["name"]] = skinValue;
                for (int slotCount = reader.ReadVarInt(); slotCount > 0; slotCount--)
                {
                    JsonObject slotValue = [];
                    skinValue[(string)slots[reader.ReadVarInt()]["name"]] = slotValue;
                    for (int attachmentCount = reader.ReadVarInt(); attachmentCount > 0; attachmentCount--)
                    {
                        JsonArray frames = [];
                        slotValue[reader.ReadStringRef()] = frames;
                        var frameCount = reader.ReadVarInt();
                        while (frameCount-- > 0)
                        {
                            var o = new JsonObject()
                            {
                                ["time"] = reader.ReadFloat(),
                            };
                            var end = reader.ReadVarInt();
                            if (end > 0)
                            {
                                var start = reader.ReadVarInt();
                                o["offset"] = start;
                                o["vertices"] = ReadFloatArray(end);
                            }
                            if (frameCount > 0) ReadCurve(o);
                            frames.Add(o);
                        }
                    }
                }
            }
            return deformTimelines.Count > 0 ? deformTimelines : null;
        }

        private JsonArray? ReadDrawOrderTimelines()
        {
            JsonArray slots = root["slots"].AsArray();
            JsonArray drawOrderTimelines = [];

            for (int drawOrderCount = reader.ReadVarInt(); drawOrderCount > 0; drawOrderCount--)
            {
                JsonObject data = new()
                {
                    ["time"] = reader.ReadFloat()
                };
                JsonArray offsets = [];
                data["offsets"] = offsets;
                for (int offsetCount = reader.ReadVarInt(); offsetCount > 0; offsetCount--)
                {
                    offsets.Add(new JsonObject()
                    {
                        ["slot"] = (string)slots[reader.ReadVarInt()]["name"],
                        ["offset"] = reader.ReadVarInt(),
                    });
                }
                drawOrderTimelines.Add(data);
            }

            return drawOrderTimelines.Count > 0 ? drawOrderTimelines : null;
        }

        private JsonArray? ReadEventTimelines()
        {
            JsonArray eventTimelines = [];
            for (int eventCount = reader.ReadVarInt(); eventCount > 0; eventCount--)
            {
                JsonObject data = [];
                data["time"] = reader.ReadFloat();
                JsonObject eventData = idx2event[reader.ReadVarInt()].AsObject();
                data["name"] = (string)eventData["name"];
                data["int"] = reader.ReadVarInt();
                data["float"] = reader.ReadFloat();
                data["string"] = reader.ReadBoolean() ? reader.ReadString() : (string)eventData["string"];
                if (eventData.ContainsKey("audio"))
                {
                    data["volume"] = (string)eventData["volume"];
                    data["balance"] = (string)eventData["balance"];
                }
                eventTimelines.Add(data);
            }

            return eventTimelines.Count > 0 ? eventTimelines : null;
        }

        private JsonArray ReadNames(JsonArray array)
        {
            JsonArray names = [];
            for (int n = reader.ReadVarInt(); n > 0; n--)
                names.Add((string)array[reader.ReadVarInt()]["name"]);
            return names;
        }

        private JsonArray ReadFloatArray(int n)
        {
            JsonArray array = [];
            while (n-- > 0)
                array.Add(reader.ReadFloat());
            return array;
        }

        private JsonArray ReadShortArray()
        {
            JsonArray array = [];
            for (int n = reader.ReadVarInt(); n > 0; n--)
                array.Add((reader.ReadByte() << 8) | reader.ReadByte());
            return array;
        }

        private JsonArray ReadVertices(int vertexCount)
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

        private void ReadCurve(JsonObject obj)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case SkeletonBinary.CURVE_LINEAR:
                    obj["curve"] = 1 / 3f;
                    obj["c2"] = 1 / 3f;
                    obj["c3"] = 2 / 3f;
                    obj["c4"] = 2 / 3f;
                    break;
                case SkeletonBinary.CURVE_STEPPED:
                    obj["curve"] = "stepped";
                    break;
                case SkeletonBinary.CURVE_BEZIER:
                    obj["curve"] = reader.ReadFloat();
                    obj["c2"] = reader.ReadFloat();
                    obj["c3"] = reader.ReadFloat();
                    obj["c4"] = reader.ReadFloat();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid curve type: {type}"); ;
            }
        }

        private BinaryWriter writer;
        private readonly Dictionary<string, int> bone2idx = [];
        private readonly Dictionary<string, int> slot2idx = [];
        private readonly Dictionary<string, int> ik2idx = [];
        private readonly Dictionary<string, int> transform2idx = [];
        private readonly Dictionary<string, int> path2idx = [];
        private readonly Dictionary<string, int> event2idx = [];

        public override void WriteBinary(JsonObject root, string binPath, bool nonessential = false)
        {
            this.nonessential = nonessential;
            this.root = root;

            using var outputBody = new MemoryStream(); // 先把主体写入内存缓冲区
            writer = new(outputBody);

            WriteBones();
            WriteSlots();
            WriteIK();
            WriteTransform();
            WritePath();
            WriteSkins();
            WriteEvents();
            WriteAnimations();
            
            //using var output = File.Create(binPath); // 将数据写入文件
            //writer = new(output);

            WriteSkeleton();
            WriteStrings();
            //output.Write(outputBody.GetBuffer());

            writer = null;
            this.root = null;
        }

        private void WriteSkeleton()
        {
            JsonObject skeleton = root["skeleton"].AsObject();
            writer.WriteString((string)skeleton["hash"]);
            writer.WriteString((string)skeleton["spine"]);
            if (skeleton.TryGetPropertyValue("x", out var x)) writer.WriteFloat((float)x); else writer.WriteFloat(0);
            if (skeleton.TryGetPropertyValue("y", out var y)) writer.WriteFloat((float)y); else writer.WriteFloat(0);
            if (skeleton.TryGetPropertyValue("width", out var width)) writer.WriteFloat((float)width); else writer.WriteFloat(0);
            if (skeleton.TryGetPropertyValue("height", out var height)) writer.WriteFloat((float)height); else writer.WriteFloat(0);
            writer.WriteBoolean(nonessential);
            if (nonessential)
            {
                if (skeleton.TryGetPropertyValue("fps", out var fps)) writer.WriteFloat((float)fps); else writer.WriteFloat(30);
                if (skeleton.TryGetPropertyValue("images", out var images)) writer.WriteString((string)images); else writer.WriteString(null);
                if (skeleton.TryGetPropertyValue("audio", out var audio)) writer.WriteString((string)audio); else writer.WriteString(null);
            }
        }

        private void WriteStrings()
        {
            writer.WriteVarInt(writer.StringTable.Count);
            foreach (var s in writer.StringTable)
                writer.WriteString(s);
        }

        private void WriteBones()
        {
            if (!root.ContainsKey("bones"))
            {
                writer.WriteVarInt(0);
                return;
            }
            JsonArray bones = root["bones"].AsArray();
            writer.WriteVarInt(bones.Count);
            for (int i = 0, n = bones.Count; i < n; i++)
            {
                JsonObject data = bones[i].AsObject();
                var name = (string)data["name"];
                writer.WriteString(name);
                if (i > 0) writer.WriteVarInt(bone2idx[(string)data["parent"]]);
                if (data.TryGetPropertyValue("rotation", out var rotation)) writer.WriteFloat((float)rotation); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("x", out var x)) writer.WriteFloat((float)x); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("y", out var y)) writer.WriteFloat((float)y); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("scaleX", out var scaleX)) writer.WriteFloat((float)scaleX); else writer.WriteFloat(1);
                if (data.TryGetPropertyValue("scaleY", out var scaleY)) writer.WriteFloat((float)scaleY); else writer.WriteFloat(1);
                if (data.TryGetPropertyValue("shearX", out var shearX)) writer.WriteFloat((float)shearX); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("shearY", out var shearY)) writer.WriteFloat((float)shearY); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("length", out var length)) writer.WriteFloat((float)length); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("transform", out var transform)) writer.WriteVarInt((int)Enum.Parse<TransformMode>((string)transform, true)); else writer.WriteVarInt((int)TransformMode.Normal);
                if (data.TryGetPropertyValue("skin", out var skin)) writer.WriteBoolean((bool)skin); else writer.WriteBoolean(false);
                if (nonessential) writer.WriteInt(0);
                bone2idx[name] = i;
            }
        }

        private void WriteSlots()
        {
            if (!root.ContainsKey("slots"))
            {
                writer.WriteVarInt(0);
                return;
            }
            JsonArray slots = root["slots"].AsArray();
            writer.WriteVarInt(slots.Count);
            for (int i = 0, n = slots.Count; i < n; i++)
            {
                JsonObject data = slots[i].AsObject();
                var name = (string)data["name"];
                writer.WriteString(name);
                writer.WriteVarInt(bone2idx[(string)data["bone"]]);
                if (data.TryGetPropertyValue("color", out var color)) writer.WriteInt(int.Parse((string)color, NumberStyles.HexNumber)); else writer.WriteInt(0);
                if (data.TryGetPropertyValue("dark", out var dark)) writer.WriteInt(int.Parse((string)dark, NumberStyles.HexNumber)); else writer.WriteInt(-1);
                if (data.TryGetPropertyValue("attachment", out var attachment)) writer.WriteStringRef((string)attachment); else writer.WriteStringRef(null);
                if (data.TryGetPropertyValue("blend", out var blend)) writer.WriteVarInt((int)Enum.Parse<BlendMode>((string)blend, true)); else writer.WriteVarInt((int)BlendMode.Normal);
                slot2idx[name] = i;
            }
        }

        private void WriteIK()
        {
            if (!root.ContainsKey("ik"))
            {
                writer.WriteVarInt(0);
                return;
            }
            JsonArray ik = root["ik"].AsArray();
            writer.WriteVarInt(ik.Count);
            for (int i = 0, n = ik.Count; i < n; i++)
            {
                JsonObject data = ik[i].AsObject();
                var name = (string)data["name"];
                writer.WriteString(name);
                if (data.TryGetPropertyValue("order", out var order)) writer.WriteVarInt((int)order); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("skin", out var skin)) writer.WriteBoolean((bool)skin); else writer.WriteBoolean(false);
                if (data.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                writer.WriteVarInt(bone2idx[(string)data["target"]]);
                if (data.TryGetPropertyValue("mix", out var mix)) writer.WriteFloat((float)mix); else writer.WriteFloat(1);
                if (data.TryGetPropertyValue("softness", out var softness)) writer.WriteFloat((float)softness); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("bendPositive", out var bendPositive)) writer.WriteSByte((sbyte)((bool)bendPositive ? 1 : -1)); else writer.WriteSByte(1);
                if (data.TryGetPropertyValue("compress", out var compress)) writer.WriteBoolean((bool)compress); else writer.WriteBoolean(false);
                if (data.TryGetPropertyValue("stretch", out var stretch)) writer.WriteBoolean((bool)stretch); else writer.WriteBoolean(false);
                if (data.TryGetPropertyValue("uniform", out var uniform)) writer.WriteBoolean((bool)uniform); else writer.WriteBoolean(false);
                ik2idx[name] = i;
            }
        }

        private void WriteTransform()
        {
            if (!root.ContainsKey("transform"))
            {
                writer.WriteVarInt(0);
                return;
            }
            JsonArray transform = root["transform"].AsArray();
            writer.WriteVarInt(transform.Count);
            for (int i = 0, n = transform.Count; i < n; i++)
            {
                JsonObject data = transform[i].AsObject();
                var name = (string)data["name"];
                writer.WriteString(name);
                if (data.TryGetPropertyValue("order", out var order)) writer.WriteVarInt((int)order); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("skin", out var skin)) writer.WriteBoolean((bool)skin); else writer.WriteBoolean(false);
                if (data.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                writer.WriteVarInt(bone2idx[(string)data["target"]]);
                if (data.TryGetPropertyValue("local", out var local)) writer.WriteBoolean((bool)local); else writer.WriteBoolean(false);
                if (data.TryGetPropertyValue("relative", out var relative)) writer.WriteBoolean((bool)relative); else writer.WriteBoolean(false);
                if (data.TryGetPropertyValue("rotation", out var rotation)) writer.WriteFloat((float)rotation); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("x", out var x)) writer.WriteFloat((float)x); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("y", out var y)) writer.WriteFloat((float)y); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("scaleX", out var scaleX)) writer.WriteFloat((float)scaleX); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("scaleY", out var scaleY)) writer.WriteFloat((float)scaleY); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("shearY", out var shearY)) writer.WriteFloat((float)shearY); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("rotateMix", out var rotateMix)) writer.WriteFloat((float)rotateMix); else writer.WriteFloat(1);
                if (data.TryGetPropertyValue("translateMix", out var translateMix)) writer.WriteFloat((float)translateMix); else writer.WriteFloat(1);
                if (data.TryGetPropertyValue("scaleMix", out var scaleMix)) writer.WriteFloat((float)scaleMix); else writer.WriteFloat(1);
                if (data.TryGetPropertyValue("shearMix", out var shearMix)) writer.WriteFloat((float)shearMix); else writer.WriteFloat(1);
                transform2idx[name] = i;
            }
        }

        private void WritePath()
        {
            if (!root.ContainsKey("path"))
            {
                writer.WriteVarInt(0);
                return;
            }
            JsonArray path = root["path"].AsArray();
            writer.WriteVarInt(path.Count);
            for (int i = 0, n = path.Count; i < n; i++)
            {
                JsonObject data = path[i].AsObject();
                var name = (string)data["name"];
                writer.WriteString(name);
                if (data.TryGetPropertyValue("order", out var order)) writer.WriteVarInt((int)order); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("skin", out var skin)) writer.WriteBoolean((bool)skin); else writer.WriteBoolean(false);
                if (data.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                writer.WriteVarInt(bone2idx[(string)data["target"]]);
                if (data.TryGetPropertyValue("positionMode", out var positionMode)) writer.WriteVarInt((int)Enum.Parse<PositionMode>((string)positionMode, true)); else writer.WriteVarInt((int)PositionMode.Percent);
                if (data.TryGetPropertyValue("spacingMode", out var spacingMode)) writer.WriteVarInt((int)Enum.Parse<SpacingMode>((string)spacingMode, true)); else writer.WriteVarInt((int)SpacingMode.Length);
                if (data.TryGetPropertyValue("rotateMode", out var rotateMode)) writer.WriteVarInt((int)Enum.Parse<RotateMode>((string)rotateMode, true)); else writer.WriteVarInt((int)RotateMode.Tangent);
                if (data.TryGetPropertyValue("rotation", out var rotation)) writer.WriteFloat((float)rotation); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("position", out var position)) writer.WriteFloat((float)position); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("spacing", out var spacing)) writer.WriteFloat((float)spacing); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("rotateMix", out var rotateMix)) writer.WriteFloat((float)rotateMix); else writer.WriteFloat(1);
                if (data.TryGetPropertyValue("translateMix", out var translateMix)) writer.WriteFloat((float)translateMix); else writer.WriteFloat(1);
                path2idx[name] = i;
            }
        }

        private void WriteSkins()
        {
            //JsonArray skins = [];

            //// default skin
            //if (ReadSkin(true) is JsonObject data)
            //    skins.Add(data);

            //// other skins
            //for (int n = reader.ReadVarInt(); n > 0; n--)
            //    skins.Add(ReadSkin());

            //root["skins"] = skins;

            if (!root.ContainsKey("skins"))
            {
                writer.WriteVarInt(0);
                return;
            }
            JsonArray skins = root["skins"].AsArray();
            writer.WriteVarInt(skins.Count);
            for (int i = 0, n = skins.Count; i < n; i++)
            {
                throw new NotImplementedException();
            }
        }


        private void WriteEvents()
        {
            if (!root.ContainsKey("events"))
            {
                writer.WriteVarInt(0);
                return;
            }
            JsonObject events = root["events"].AsObject();
            writer.WriteVarInt(events.Count);
            int i = 0;
            foreach (var (name, _data) in events)
            {
                JsonObject data = _data.AsObject();
                writer.WriteStringRef(name);
                if (data.TryGetPropertyValue("int", out var @int)) writer.WriteVarInt((int)@int); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("float", out var @float)) writer.WriteFloat((float)@float); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("string", out var @string)) writer.WriteString((string)@string); else writer.WriteString("");
                if (data.TryGetPropertyValue("audio", out var _audio))
                {
                    var audio = (string)_audio;
                    writer.WriteString(audio);
                    if (audio is not null)
                    {
                        if (data.TryGetPropertyValue("volume", out var volume)) writer.WriteFloat((float)volume); else writer.WriteFloat(1);
                        if (data.TryGetPropertyValue("balance", out var balance)) writer.WriteFloat((float)balance); else writer.WriteFloat(0);
                    }
                }
                event2idx[name] = i++;
            }
        }

        private void WriteAnimations()
        {
            if (!root.ContainsKey("animations"))
            {
                writer.WriteVarInt(0);
                return;
            }
            JsonArray animations = root["animations"].AsArray();
            writer.WriteVarInt(animations.Count);
            for (int i = 0, n = animations.Count; i < n; i++)
            {
                throw new NotImplementedException();
            }
        }

        private void WriteNames(Dictionary<string, int> name2idx, JsonArray names)
        {
            writer.WriteVarInt(names.Count);
            foreach (var name in names)
                writer.WriteVarInt(name2idx[(string)name]);
        }

        public override JsonObject ToVersion(JsonObject root, Version version)
        {
            root = version switch
            {
                Version.V38 => root.DeepClone().AsObject(),
                _ => throw new NotImplementedException(),
            };
            return root;
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
