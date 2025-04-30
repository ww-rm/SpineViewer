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
    [SpineImplementation(SpineVersion.V38)]
    public class SkeletonConverter38 : Spine.SkeletonConverter
    {
        private static readonly Dictionary<TransformMode, string> TransformModeJsonValue = new()
        {
            [TransformMode.Normal] = "normal",
            [TransformMode.OnlyTranslation] = "onlyTranslation",
            [TransformMode.NoRotationOrReflection] = "noRotationOrReflection",
            [TransformMode.NoScale] = "noScale",
            [TransformMode.NoScaleOrReflection] = "noScaleOrReflection",
        };

        private static readonly Dictionary<BlendMode, string> BlendModeJsonValue = new()
        {
            [BlendMode.Normal] = "normal",
            [BlendMode.Additive] = "additive",
            [BlendMode.Multiply] = "multiply",
            [BlendMode.Screen] = "screen",
        };

        private static readonly Dictionary<PositionMode, string> PositionModeJsonValue = new()
        {
            [PositionMode.Fixed] = "fixed",
            [PositionMode.Percent] = "percent",
        };

        private static readonly Dictionary<SpacingMode, string> SpacingModeJsonValue = new()
        {
            [SpacingMode.Length] = "length",
            [SpacingMode.Fixed] = "fixed",
            [SpacingMode.Percent] = "percent",
        };

        private static readonly Dictionary<RotateMode, string> RotateModeJsonValue = new()
        {
            [RotateMode.Tangent] = "tangent",
            [RotateMode.Chain] = "chain",
            [RotateMode.ChainScale] = "chainScale",
        };

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

            // 清理临时属性
            foreach (var (_, data) in root["events"].AsObject()) data.AsObject().Remove("__name__");

            return root;
        }

        private void ReadSkeleton()
        {
            JsonObject skeleton = [];
            skeleton["hash"] = reader.ReadString();
            var version = reader.ReadString();
            if (version == "3.8.75") version = "3.8.76"; // replace 3.8.75 to another version to avoid detection in official runtime
            skeleton["spine"] = version;
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
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
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
                data["transform"] = TransformModeJsonValue[SkeletonBinary.TransformModeValues[reader.ReadVarInt()]];
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
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["bone"] = (string)bones[reader.ReadVarInt()]["name"];
                data["color"] = reader.ReadInt().ToString("x8"); // 0xrrggbbaa -> rrggbbaa
                int dark = reader.ReadInt();
                if (dark != -1) data["dark"] = dark.ToString("x6"); // 0x00rrggbb -> rrggbb
                data["attachment"] = reader.ReadStringRef();
                data["blend"] = BlendModeJsonValue[(BlendMode)reader.ReadVarInt()];
                slots.Add(data);
            }
            root["slots"] = slots;
        }

        private void ReadIK()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray ik = [];
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
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
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
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
            JsonArray slots = root["slots"].AsArray();
            JsonArray path = [];
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                data["bones"] = ReadNames(bones);
                data["target"] = (string)slots[reader.ReadVarInt()]["name"];
                data["positionMode"] = PositionModeJsonValue[(PositionMode)reader.ReadVarInt()];
                data["spacingMode"] = SpacingModeJsonValue[(SpacingMode)reader.ReadVarInt()];
                data["rotateMode"] = RotateModeJsonValue[(RotateMode)reader.ReadVarInt()];
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
                skin["transform"] = ReadNames(root["transform"].AsArray());
                skin["path"] = ReadNames(root["path"].AsArray());
                slotCount = reader.ReadVarInt();
            }

            JsonArray slots = root["slots"].AsArray();
            JsonObject skinAttachments = [];
            for (int i = 0; i < slotCount; i++)
            {
                JsonObject slotAttachments = [];
                skinAttachments[(string)slots[reader.ReadVarInt()]["name"]] = slotAttachments;
                for (int ii = 0, attachmentCount = reader.ReadVarInt(); ii < attachmentCount; ii++)
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

            var name = reader.ReadStringRef() ?? keyName;
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
                    attachment["uvs"] = ReadFloatArray(vertexCount << 1); // vertexCount = uvs.Length >> 1
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
                    // 补充 Json 中的必需 key
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
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
            {
                JsonObject data = [];
                var name = reader.ReadStringRef();
                events[name] = data;
                data["__name__"] = name; // 数据里是不应该有这个字段的, 但是为了 ReadEventTimelines 里能够提供 name 字段, 临时增加了一下
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
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
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

            for (int i = 0, slotCount = reader.ReadVarInt(); i < slotCount; i++)
            {
                JsonObject timeline = [];
                slotTimelines[(string)slots[reader.ReadVarInt()]["name"]] = timeline;
                for (int ii = 0, timelineCount = reader.ReadVarInt(); ii < timelineCount; ii++)
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

            for (int i = 0, boneCount = reader.ReadVarInt(); i < boneCount; i++)
            {
                JsonObject timeline = [];
                boneTimelines[(string)bones[reader.ReadVarInt()]["name"]] = timeline;
                for (int ii = 0, timelineCount = reader.ReadVarInt(); ii < timelineCount; ii++)
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

            for (int i = 0, ikCount = reader.ReadVarInt(); i < ikCount; i++)
            {
                JsonArray frames = [];
                ikTimelines[(string)ik[reader.ReadVarInt()]["name"]] = frames;
                for (int frameIdx = 0, frameCount = reader.ReadVarInt(); frameIdx < frameCount; frameIdx ++)
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

            for (int i = 0, transformCount = reader.ReadVarInt(); i < transformCount; i++)
            {
                JsonArray frames = [];
                transformTimelines[(string)transform[reader.ReadVarInt()]["name"]] = frames;
                for (int frameIdx = 0, frameCount = reader.ReadVarInt(); frameIdx < frameCount; frameIdx++)
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

            for (int i = 0, pathCount = reader.ReadVarInt(); i < pathCount; i++)
            {
                JsonObject timeline = [];
                pathTimelines[(string)path[reader.ReadVarInt()]["name"]] = timeline;
                for (int ii = 0, timelineCount = reader.ReadVarInt(); ii < timelineCount; ii++)
                {
                    JsonArray frames = [];
                    var type = reader.ReadSByte();
                    var frameCount = reader.ReadVarInt();
                    switch (type)
                    {
                        case SkeletonBinary.PATH_POSITION:
                            timeline["position"] = frames;
                            while (frameCount-- > 0)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["position"] = reader.ReadFloat(),
                                };
                                if (frameCount > 0) ReadCurve(o);
                                frames.Add(o);
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

            for (int i = 0, skinCount = reader.ReadVarInt(); i < skinCount; i++)
            {
                JsonObject skinValue = [];
                deformTimelines[(string)skins[reader.ReadVarInt()]["name"]] = skinValue;
                for (int ii = 0, slotCount = reader.ReadVarInt(); ii < slotCount; ii++)
                {
                    JsonObject slotValue = [];
                    skinValue[(string)slots[reader.ReadVarInt()]["name"]] = slotValue;
                    for (int iii = 0, attachmentCount = reader.ReadVarInt(); iii < attachmentCount; iii++)
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
                                o["offset"] = reader.ReadVarInt();
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

            for (int i = 0, drawOrderCount = reader.ReadVarInt(); i < drawOrderCount; i++)
            {
                JsonObject data = new()
                {
                    ["time"] = reader.ReadFloat()
                };
                JsonArray offsets = [];
                data["offsets"] = offsets;
                for (int ii = 0, offsetCount = reader.ReadVarInt(); ii < offsetCount; ii++)
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
            for (int i = 0, eventCount = reader.ReadVarInt(); i < eventCount; i++)
            {
                JsonObject data = [];
                data["time"] = reader.ReadFloat();
                JsonObject eventData = idx2event[reader.ReadVarInt()].AsObject();
                data["name"] = (string)eventData["__name__"];
                data["int"] = reader.ReadVarInt();
                data["float"] = reader.ReadFloat();
                if (reader.ReadBoolean()) data["string"] = reader.ReadString();
                if (eventData.ContainsKey("audio"))
                {
                    data["volume"] = reader.ReadFloat();
                    data["balance"] = reader.ReadFloat();
                }
                eventTimelines.Add(data);
            }

            return eventTimelines.Count > 0 ? eventTimelines : null;
        }

        private JsonArray ReadNames(JsonArray array)
        {
            JsonArray names = [];
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
                names.Add((string)array[reader.ReadVarInt()]["name"]);
            return names;
        }

        private JsonArray ReadFloatArray(int n)
        {
            JsonArray array = [];
            for (int i = 0; i < n; i++)
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
                    throw new ArgumentOutOfRangeException($"Invalid curve type: {type}");
            }
        }

        private BinaryWriter writer;
        private readonly Dictionary<string, int> bone2idx = [];
        private readonly Dictionary<string, int> slot2idx = [];
        private readonly Dictionary<string, int> ik2idx = [];
        private readonly Dictionary<string, int> transform2idx = [];
        private readonly Dictionary<string, int> path2idx = [];
        private readonly Dictionary<string, int> skin2idx = [];
        private readonly Dictionary<string, int> event2idx = [];

        public override void WriteBinary(JsonObject root, string binPath, bool nonessential = false)
        {
            this.nonessential = nonessential;
            this.root = root;

            using var outputBody = new MemoryStream(); // 先把主体写入内存缓冲区
            BinaryWriter tmpWriter = writer = new (outputBody);

            WriteBones();
            WriteSlots();
            WriteIK();
            WriteTransform();
            WritePath();
            WriteSkins();
            WriteEvents();
            WriteAnimations();

            using var output = File.Create(binPath); // 将数据写入文件
            writer = new(output);

            // 把字符串表保留过来
            writer.StringTable.AddRange(tmpWriter.StringTable);

            WriteSkeleton();
            WriteStrings();
            outputBody.Seek(0, SeekOrigin.Begin);
            outputBody.CopyTo(output);

            writer = null;
            this.root = null;

            bone2idx.Clear();
            slot2idx.Clear();
            ik2idx.Clear();
            transform2idx.Clear();
            path2idx.Clear();
            skin2idx.Clear();
            event2idx.Clear();
        }

        private void WriteSkeleton()
        {
            JsonObject skeleton = root["skeleton"].AsObject();
            writer.WriteString((string)skeleton["hash"]);
            var version = (string)skeleton["spine"];
            if (version == "3.8.75") version = "3.8.76"; // replace 3.8.75 to another version to avoid detection in official runtime
            writer.WriteString(version);
            writer.WriteFloat((float)(skeleton["x"] ?? 0));
            writer.WriteFloat((float)(skeleton["y"] ?? 0));
            writer.WriteFloat((float)(skeleton["width"] ?? 0));
            writer.WriteFloat((float)(skeleton["height"] ?? 0));
            writer.WriteBoolean(nonessential);
            if (nonessential)
            {
                writer.WriteFloat((float)(skeleton["fps"] ?? 30));
                writer.WriteString((string)skeleton["images"]);
                writer.WriteString((string)skeleton["audio"]);
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
                writer.WriteFloat((float)(data["rotation"] ?? 0));
                writer.WriteFloat((float)(data["x"] ?? 0));
                writer.WriteFloat((float)(data["y"] ?? 0));
                writer.WriteFloat((float)(data["scaleX"] ?? 1));
                writer.WriteFloat((float)(data["scaleY"] ?? 1));
                writer.WriteFloat((float)(data["shearX"] ?? 0));
                writer.WriteFloat((float)(data["shearY"] ?? 0));
                writer.WriteFloat((float)(data["length"] ?? 0));
                writer.WriteVarInt(Array.IndexOf(SkeletonBinary.TransformModeValues, Enum.Parse<TransformMode>((string)(data["transform"] ?? "normal"), true)));
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
                writer.WriteInt(int.Parse((string)(data["color"] ?? "ffffffff"), NumberStyles.HexNumber));
                writer.WriteInt(int.Parse((string)(data["dark"] ?? "ffffff"), NumberStyles.HexNumber));
                writer.WriteStringRef((string)data["attachment"]);
                writer.WriteVarInt((int)Enum.Parse<BlendMode>((string)(data["blend"] ?? "normal"), true));
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
                writer.WriteVarInt((int)(data["order"] ?? 0));
                writer.WriteBoolean((bool)(data["skin"] ?? false));
                WriteNames(bone2idx, (JsonArray)(data["bones"] ?? new JsonArray()));
                writer.WriteVarInt(bone2idx[(string)data["target"]]);
                writer.WriteFloat((float)(data["mix"] ?? 1));
                writer.WriteFloat((float)(data["softness"] ?? 0));
                writer.WriteSByte((sbyte)((bool)(data["bendPositive"] ?? true) ? 1 : -1));
                writer.WriteBoolean((bool)(data["compress"] ?? false));
                writer.WriteBoolean((bool)(data["stretch"] ?? false));
                writer.WriteBoolean((bool)(data["uniform"] ?? false));
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
                writer.WriteVarInt((int)(data["order"] ?? 0));
                writer.WriteBoolean((bool)(data["skin"] ?? false));
                WriteNames(bone2idx, (JsonArray)(data["bones"] ?? new JsonArray()));
                writer.WriteVarInt(bone2idx[(string)data["target"]]);
                writer.WriteBoolean((bool)(data["local"] ?? false));
                writer.WriteBoolean((bool)(data["relative"] ?? false));
                writer.WriteFloat((float)(data["rotation"] ?? 0));
                writer.WriteFloat((float)(data["x"] ?? 0));
                writer.WriteFloat((float)(data["y"] ?? 0));
                writer.WriteFloat((float)(data["scaleX"] ?? 0));
                writer.WriteFloat((float)(data["scaleY"] ?? 0));
                writer.WriteFloat((float)(data["shearY"] ?? 0));
                writer.WriteFloat((float)(data["rotateMix"] ?? 1));
                writer.WriteFloat((float)(data["translateMix"] ?? 1));
                writer.WriteFloat((float)(data["scaleMix"] ?? 1));
                writer.WriteFloat((float)(data["shearMix"] ?? 1));
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
                writer.WriteVarInt((int)(data["order"] ?? 0));
                writer.WriteBoolean((bool)(data["skin"] ?? false));
                WriteNames(bone2idx, (JsonArray)(data["bones"] ?? new JsonArray()));
                writer.WriteVarInt(slot2idx[(string)data["target"]]);
                writer.WriteVarInt((int)Enum.Parse<PositionMode>((string)(data["positionMode"] ?? "percent"), true));
                writer.WriteVarInt((int)Enum.Parse<SpacingMode>((string)(data["spacingMode"] ?? "length"), true));
                writer.WriteVarInt((int)Enum.Parse<RotateMode>((string)(data["rotateMode"] ?? "tangent"), true));
                writer.WriteFloat((float)(data["rotation"] ?? 0));
                writer.WriteFloat((float)(data["position"] ?? 0));
                writer.WriteFloat((float)(data["spacing"] ?? 0));
                writer.WriteFloat((float)(data["rotateMix"] ?? 1));
                writer.WriteFloat((float)(data["translateMix"] ?? 1));
                path2idx[name] = i;
            }
        }

        private void WriteSkins()
        {
            if (!root.ContainsKey("skins"))
            {
                writer.WriteVarInt(0); // default 的 slotCount
                writer.WriteVarInt(0); // 其他皮肤数量
                return;
            }

            JsonArray skins = root["skins"].AsArray();
            bool hasDefault = false;
            foreach (JsonObject skin in skins)
            {
                if ((string)skin["name"] == "default")
                {
                    hasDefault = true;
                    WriteSkin(skin, true);
                    skin2idx["default"] = skin2idx.Count;
                    break;
                }
            }

            if (!hasDefault) writer.WriteVarInt(0);

            int skinCount = hasDefault ? skins.Count - 1 : skins.Count;
            if (skinCount <= 0)
            {
                writer.WriteVarInt(0);
                return;
            }

            writer.WriteVarInt(skinCount);
            foreach (JsonObject skin in skins)
            {
                var name = (string)skin["name"];
                if (name != "default")
                {
                    WriteSkin(skin);
                    skin2idx[name] = skin2idx.Count;
                }
            }
        }

        private void WriteSkin(JsonObject skin, bool isDefault = false)
        {
            JsonObject skinAttachments = null;
            if (isDefault)
            {
                // 这里固定有一个给 default 的 count 值, 算是占位符, 如果是 0 则表示没有 default 的 skin
                if (skin["attachments"] is JsonObject attachments) skinAttachments = attachments;
                writer.WriteVarInt(skinAttachments?.Count ?? 0);
            }
            else
            {
                writer.WriteStringRef((string)skin["name"]);
                WriteNames(bone2idx, (JsonArray)(skin["bones"] ?? new JsonArray()));
                WriteNames(bone2idx, (JsonArray)(skin["ik"] ?? new JsonArray()));
                WriteNames(bone2idx, (JsonArray)(skin["transform"] ?? new JsonArray()));
                WriteNames(bone2idx, (JsonArray)(skin["path"] ?? new JsonArray()));
                if (skin["attachments"] is JsonObject attachments) skinAttachments = attachments;
                writer.WriteVarInt(skinAttachments?.Count ?? 0);
            }

            if (skinAttachments is null)
                return;

            foreach (var (slotName, _slotAttachments) in skinAttachments)
            {
                JsonObject slotAttachments = _slotAttachments.AsObject();
                writer.WriteVarInt(slot2idx[slotName]);
                writer.WriteVarInt(slotAttachments.Count);
                foreach (var (attachmentKey, attachment) in slotAttachments)
                {
                    writer.WriteStringRef(attachmentKey);
                    WriteAttachment(attachment.AsObject(), attachmentKey);
                }
            }
        }

        private void WriteAttachment(JsonObject attachment, string keyName)
        {
            int vertexCount;

            string name = keyName;
            AttachmentType type = AttachmentType.Region;

            if (attachment["name"] is JsonValue _name) name = (string)_name;
            if (attachment["type"] is JsonValue _type) type = Enum.Parse<AttachmentType>((string)_type, true);
            writer.WriteStringRef(name);
            writer.WriteByte((byte)type);

            switch (type)
            {
                case AttachmentType.Region:
                    writer.WriteStringRef((string)attachment["path"]);
                    writer.WriteFloat((float)(attachment["rotation"] ?? 0));
                    writer.WriteFloat((float)(attachment["x"] ?? 0));
                    writer.WriteFloat((float)(attachment["y"] ?? 0));
                    writer.WriteFloat((float)(attachment["scaleX"] ?? 1));
                    writer.WriteFloat((float)(attachment["scaleY"] ?? 1));
                    writer.WriteFloat((float)(attachment["width"] ?? 32));
                    writer.WriteFloat((float)(attachment["height"] ?? 32));
                    writer.WriteInt(int.Parse((string)(attachment["color"] ?? "ffffffff"), NumberStyles.HexNumber));
                    break;
                case AttachmentType.Boundingbox:
                    vertexCount = (int)(attachment["vertexCount"] ?? 0);
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount);
                    if (nonessential) writer.WriteInt(0);
                    break;
                case AttachmentType.Mesh:
                    writer.WriteStringRef((string)attachment["path"]);
                    writer.WriteInt(int.Parse((string)(attachment["color"] ?? "ffffffff"), NumberStyles.HexNumber));
                    vertexCount = attachment["uvs"].AsArray().Count >> 1;
                    writer.WriteVarInt(vertexCount);
                    WriteFloatArray(attachment["uvs"].AsArray(), vertexCount << 1); // vertexCount = uvs.Length >> 1
                    WriteShortArray(attachment["triangles"].AsArray());
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount);
                    writer.WriteVarInt((int)(attachment["hull"] ?? 0));
                    if (nonessential)
                    {
                        WriteShortArray((JsonArray)(attachment["edges"] ?? new JsonArray()));
                        writer.WriteFloat((float)(attachment["width"] ?? 0));
                        writer.WriteFloat((float)(attachment["height"] ?? 0));
                    }
                    break;
                case AttachmentType.Linkedmesh:
                    writer.WriteStringRef((string)attachment["path"]);
                    writer.WriteInt(int.Parse((string)(attachment["color"] ?? "ffffffff"), NumberStyles.HexNumber));
                    writer.WriteStringRef((string)attachment["skin"]);
                    writer.WriteStringRef((string)attachment["parent"]);
                    writer.WriteBoolean((bool)(attachment["deform"] ?? true));
                    if (nonessential)
                    {
                        writer.WriteFloat((float)(attachment["width"] ?? 0));
                        writer.WriteFloat((float)(attachment["height"] ?? 0));
                    }
                    break;
                case AttachmentType.Path:
                    writer.WriteBoolean((bool)(attachment["closed"] ?? false));
                    writer.WriteBoolean((bool)(attachment["constantSpeed"] ?? true));
                    vertexCount = (int)(attachment["vertexCount"] ?? 0);
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount);
                    WriteFloatArray(attachment["lengths"].AsArray(), vertexCount / 3);
                    if (nonessential) writer.WriteInt(0);
                    break;
                case AttachmentType.Point:
                    writer.WriteFloat((float)(attachment["rotation"] ?? 0));
                    writer.WriteFloat((float)(attachment["x"] ?? 0));
                    writer.WriteFloat((float)(attachment["y"] ?? 0));
                    if (nonessential) writer.WriteInt(0);
                    break;
                case AttachmentType.Clipping:
                    writer.WriteVarInt(slot2idx[(string)attachment["end"]]);
                    vertexCount = (int)(attachment["vertexCount"] ?? 0);
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount);
                    if (nonessential) writer.WriteInt(0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid attachment type: {type}");
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
                writer.WriteVarInt((int)(data["int"] ?? 0));
                writer.WriteFloat((float)(data["float"] ?? 0));
                writer.WriteString((string)(data["string"] ?? ""));
                if (data["audio"] is JsonValue _audio)
                {
                    var audio = (string)_audio;
                    writer.WriteString(audio);
                    if (audio is not null)
                    {
                        writer.WriteFloat((float)(data["volume"] ?? 1));
                        writer.WriteFloat((float)(data["balance"] ?? 0));
                    }
                }
                else
                {
                    writer.WriteString(null);
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

            JsonObject animations = root["animations"].AsObject();
            writer.WriteVarInt(animations.Count);
            foreach (var (name, _data) in animations)
            {
                JsonObject data = _data.AsObject();
                writer.WriteString(name);
                WriteSlotTimelines((JsonObject)(data["slots"] ?? new JsonObject()));
                WriteBoneTimelines((JsonObject)(data["bones"] ?? new JsonObject()));
                WriteIKTimelines((JsonObject)(data["ik"] ?? new JsonObject()));
                WriteTransformTimelines((JsonObject)(data["transform"] ?? new JsonObject()));
                WritePathTimelines((JsonObject)(data["path"] ?? new JsonObject()));
                WriteDeformTimelines((JsonObject)(data["deform"] ?? new JsonObject()));
                WriteDrawOrderTimelines((JsonArray)(data["drawOrder"] ?? data["draworder"] ?? new JsonArray()));
                WriteEventTimelines((JsonArray)(data["events"] ?? new JsonArray()));
            }
        }

        private void WriteSlotTimelines(JsonObject slotTimelines)
        {
            writer.WriteVarInt(slotTimelines.Count);
            foreach (var (name, _timeline) in slotTimelines)
            {
                JsonObject timeline = _timeline.AsObject();
                writer.WriteVarInt(slot2idx[name]);
                writer.WriteVarInt(timeline.Count);
                foreach (var (type, _frames) in timeline)
                {
                    JsonArray frames = _frames.AsArray();
                    if (type == "attachment")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_ATTACHMENT);
                        writer.WriteVarInt(frames.Count);
                        foreach (JsonObject o in frames)
                        {
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            writer.WriteStringRef((string)o["name"]);
                        }
                    }
                    else if (type == "color")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_COLOR);
                        writer.WriteVarInt(frames.Count);
                        for (int i = 0, n = frames.Count; i < n; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            writer.WriteInt(int.Parse((string)o["color"], NumberStyles.HexNumber));
                            if (i < n - 1) WriteCurve(o);
                        }
                    }
                    else if (type == "twoColor")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_TWO_COLOR);
                        writer.WriteVarInt(frames.Count);
                        for (int i = 0, n = frames.Count; i < n; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            writer.WriteInt(int.Parse((string)o["light"], NumberStyles.HexNumber));
                            writer.WriteInt(int.Parse((string)o["dark"], NumberStyles.HexNumber));
                            if (i < n - 1) WriteCurve(o);
                        }
                    }
                }
            }
        }

        private void WriteBoneTimelines(JsonObject boneTimelines)
        {
            writer.WriteVarInt(boneTimelines.Count);
            foreach (var (name, _timeline) in boneTimelines)
            {
                JsonObject timeline = _timeline.AsObject();
                writer.WriteVarInt(bone2idx[name]);
                writer.WriteVarInt(timeline.Count);
                foreach (var (type, _frames) in timeline)
                {
                    JsonArray frames = _frames.AsArray();
                    if (type == "rotate")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_ROTATE);
                        writer.WriteVarInt(frames.Count);
                        for (int i = 0, n = frames.Count; i < n; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            writer.WriteFloat((float)(o["angle"] ?? 0));
                            if (i < n - 1) WriteCurve(o);
                        }
                    }
                    else if (type == "translate")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_TRANSLATE);
                        writer.WriteVarInt(frames.Count);
                        for (int i = 0, n = frames.Count; i < n; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            writer.WriteFloat((float)(o["x"] ?? 0));
                            writer.WriteFloat((float)(o["y"] ?? 0));
                            if (i < n - 1) WriteCurve(o);
                        }
                    }
                    else if (type == "scale")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SCALE);
                        writer.WriteVarInt(frames.Count);
                        for (int i = 0, n = frames.Count; i < n; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            writer.WriteFloat((float)(o["x"] ?? 1));
                            writer.WriteFloat((float)(o["y"] ?? 1));
                            if (i < n - 1) WriteCurve(o);
                        }
                    }
                    else if (type == "shear")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SHEAR);
                        writer.WriteVarInt(frames.Count);
                        for (int i = 0, n = frames.Count; i < n; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            writer.WriteFloat((float)(o["x"] ?? 0));
                            writer.WriteFloat((float)(o["y"] ?? 0));
                            if (i < n - 1) WriteCurve(o);
                        }
                    }
                }
            }
        }

        private void WriteIKTimelines(JsonObject ikTimelines)
        {
            writer.WriteVarInt(ikTimelines.Count);
            foreach (var (name, _frames) in ikTimelines)
            {
                JsonArray frames = _frames.AsArray();
                writer.WriteVarInt(ik2idx[name]);
                writer.WriteVarInt(frames.Count);
                for (int i = 0, n = frames.Count; i < n; i++)
                {
                    JsonObject o = frames[i].AsObject();
                    writer.WriteFloat((float)(o["time"] ?? 0));
                    writer.WriteFloat((float)(o["mix"] ?? 1));
                    writer.WriteFloat((float)(o["softness"] ?? 0));
                    writer.WriteSByte((sbyte)((bool)(o["bendPositive"] ?? true) ? 1 : -1));
                    writer.WriteBoolean((bool)(o["compress"] ?? false));
                    writer.WriteBoolean((bool)(o["stretch"] ?? false));
                    if (i < n - 1) WriteCurve(o);
                }
            }
        }

        private void WriteTransformTimelines(JsonObject transformTimelines)
        {
            writer.WriteVarInt(transformTimelines.Count);
            foreach (var (name, _frames) in transformTimelines)
            {
                JsonArray frames = _frames.AsArray();
                writer.WriteVarInt(transform2idx[name]);
                writer.WriteVarInt(frames.Count);
                for (int i = 0, n = frames.Count; i < n; i++)
                {
                    JsonObject o = frames[i].AsObject();
                    writer.WriteFloat((float)(o["time"] ?? 0));
                    writer.WriteFloat((float)(o["rotateMix"] ?? 1));
                    writer.WriteFloat((float)(o["translateMix"] ?? 1));
                    writer.WriteFloat((float)(o["scaleMix"] ?? 1));
                    writer.WriteFloat((float)(o["shearMix"] ?? 1));
                    if (i < n - 1) WriteCurve(o);
                }
            }
        }

        private void WritePathTimelines(JsonObject pathTimelines)
        {
            writer.WriteVarInt(pathTimelines.Count);
            foreach (var (name, _timeline) in pathTimelines)
            {
                JsonObject timeline = _timeline.AsObject();
                writer.WriteVarInt(path2idx[name]);
                writer.WriteVarInt(timeline.Count);
                foreach (var (type, _frame) in timeline)
                {
                    JsonArray frames = _frame.AsArray();
                    if (type == "position")
                    {
                        writer.WriteByte(SkeletonBinary.PATH_POSITION);
                        writer.WriteVarInt(frames.Count);
                        for (int i = 0, n = frames.Count; i < n; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            writer.WriteFloat((float)(o["position"] ?? 0));
                            if (i < n - 1) WriteCurve(o);
                        }
                    }
                    else if (type == "spacing")
                    {
                        writer.WriteByte(SkeletonBinary.PATH_SPACING);
                        writer.WriteVarInt(frames.Count);
                        for (int i = 0, n = frames.Count; i < n; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            writer.WriteFloat((float)(o["spacing"] ?? 0));
                            if (i < n - 1) WriteCurve(o);
                        }
                    }
                    else if (type == "mix")
                    {
                        writer.WriteByte(SkeletonBinary.PATH_MIX);
                        writer.WriteVarInt(frames.Count);
                        for (int i = 0, n = frames.Count; i < n; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            writer.WriteFloat((float)(o["rotateMix"] ?? 1));
                            writer.WriteFloat((float)(o["translateMix"] ?? 1));
                            if (i < n - 1) WriteCurve(o);
                        }
                    }
                }
            }
        }

        private void WriteDeformTimelines(JsonObject deformTimelines)
        {
            writer.WriteVarInt(deformTimelines.Count);
            foreach (var (skinName, _skinValue) in deformTimelines)
            {
                JsonObject skinValue = _skinValue.AsObject();
                writer.WriteVarInt(skin2idx[skinName]);
                writer.WriteVarInt(skinValue.Count);
                foreach (var (slotName, _slotValue) in skinValue)
                {
                    JsonObject slotValue = _slotValue.AsObject();
                    writer.WriteVarInt(slot2idx[slotName]);
                    writer.WriteVarInt(slotValue.Count);
                    foreach (var (attachmentName, _frames) in slotValue)
                    {
                        JsonArray frames = _frames.AsArray();
                        writer.WriteStringRef(attachmentName);
                        writer.WriteVarInt(frames.Count);
                        for (int i = 0, n = frames.Count; i < n; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            writer.WriteFloat((float)(o["time"] ?? 0));
                            if (o["vertices"] is JsonArray vertices)
                            {
                                writer.WriteVarInt(vertices.Count);
                                if (vertices.Count > 0)
                                {
                                    writer.WriteVarInt((int)(o["offset"] ?? 0));
                                    WriteFloatArray(vertices, vertices.Count);
                                }
                            }
                            else
                            {
                                writer.WriteVarInt(0);
                            }
                            if (i < n - 1) WriteCurve(o);
                        }
                    }
                }
            }
        }

        private void WriteDrawOrderTimelines(JsonArray drawOrderTimelines)
        {
            writer.WriteVarInt(drawOrderTimelines.Count);
            foreach (JsonObject data in drawOrderTimelines)
            {
                writer.WriteFloat((float)(data["time"] ?? 0));
                if (data["offsets"] is JsonArray offsets)
                {
                    writer.WriteVarInt(offsets.Count);
                    foreach (JsonObject o in offsets)
                    {
                        writer.WriteVarInt(slot2idx[(string)o["slot"]]);
                        writer.WriteVarInt((int)o["offset"]);
                    }
                }
                else
                {
                    writer.WriteVarInt(0);
                }
            }
        }

        private void WriteEventTimelines(JsonArray eventTimelines)
        {
            JsonObject events = root["events"].AsObject();

            writer.WriteVarInt(eventTimelines.Count);
            foreach(JsonObject data in eventTimelines)
            {
                JsonObject eventData = events[(string)data["name"]].AsObject();
                writer.WriteFloat((float)(data["time"] ?? 0));
                writer.WriteVarInt(event2idx[(string)data["name"]]);
                writer.WriteVarInt((int)(data["int"] ?? eventData["int"] ?? 0));
                writer.WriteFloat((float)(data["float"] ?? eventData["float"] ?? 0));
                if (data["string"] is JsonValue @string)
                {
                    writer.WriteBoolean(true);
                    writer.WriteString((string)@string);
                }
                else
                {
                    writer.WriteBoolean(false);
                }

                if (eventData.ContainsKey("audio"))
                {
                    writer.WriteFloat((float)(data["volume"] ?? eventData["volume"] ?? 1));
                    writer.WriteFloat((float)(data["balance"] ?? eventData["balance"] ?? 0));
                }
            }
        }

        private void WriteNames(Dictionary<string, int> name2idx, JsonArray names)
        {
            writer.WriteVarInt(names.Count);
            foreach (string name in names)
                writer.WriteVarInt(name2idx[name]);
        }

        public void WriteFloatArray(JsonArray array, int n)
        {
            for (int i = 0; i < n; i++)
                writer.WriteFloat((float)array[i]);
        }

        public void WriteShortArray(JsonArray array)
        {
            writer.WriteVarInt(array.Count);
            foreach (int i in array)
            {
                writer.WriteByte((byte)(i >> 8));
                writer.WriteByte((byte)i);
            }
        }

        private void WriteVertices(JsonArray vertices, int vertexCount)
        {
            bool hasWeight = vertices.Count != (vertexCount << 1);
            writer.WriteBoolean(hasWeight);
            if (!hasWeight)
            {
                WriteFloatArray(vertices, vertexCount << 1);
            }
            else
            {
                int idx = 0;
                for (int i = 0; i < vertexCount; i++)
                {
                    var bonesCount = (int)vertices[idx++];
                    writer.WriteVarInt(bonesCount);
                    for (int j = 0; j < bonesCount; j++)
                    {
                        writer.WriteVarInt((int)vertices[idx++]);
                        writer.WriteFloat((float)vertices[idx++]);
                        writer.WriteFloat((float)vertices[idx++]);
                        writer.WriteFloat((float)vertices[idx++]);
                    }
                }
            }
        }

        private void WriteCurve(JsonObject obj)
        {
            if (obj["curve"] is JsonValue curve)
            {
                if (curve.GetValueKind() == JsonValueKind.String)
                {
                    writer.WriteByte(SkeletonBinary.CURVE_STEPPED);
                }
                else
                {
                    writer.WriteByte(SkeletonBinary.CURVE_BEZIER);
                    writer.WriteFloat((float)curve);
                    writer.WriteFloat((float)(obj["c2"] ?? 0));
                    writer.WriteFloat((float)(obj["c3"] ?? 1));
                    writer.WriteFloat((float)(obj["c4"] ?? 1));
                }
            }
            else
            {
                writer.WriteByte(SkeletonBinary.CURVE_LINEAR);
            }
        }

        public override JsonObject ReadJson(string jsonPath)
        {
            // replace 3.8.75 to another version to avoid detection in official runtime
            var root = base.ReadJson(jsonPath);
            var skeleton = root["skeleton"].AsObject();
            var version = (string)skeleton["spine"];
            if (version == "3.8.75") skeleton["spine"] = "3.8.76";
            return root;
        }

        public override void WriteJson(JsonObject root, string jsonPath)
        {
            // replace 3.8.75 to another version to avoid detection in official runtime
            var skeleton = root["skeleton"].AsObject();
            var version = (string)skeleton["spine"];
            if (version == "3.8.75") skeleton["spine"] = "3.8.76";
            base.WriteJson(root, jsonPath);
        }

        public override JsonObject ToVersion(JsonObject root, SpineVersion version)
        {
            root = version switch
            {
                SpineVersion.V38 => root.DeepClone().AsObject(),
                _ => throw new NotImplementedException(),
            };
            return root;
        }

    }
}
