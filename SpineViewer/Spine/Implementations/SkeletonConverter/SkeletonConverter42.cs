using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Globalization;
using SpineRuntime42;


namespace SpineViewer.Spine.Implementations.SkeletonConverter
{
    [SpineImplementation(SpineVersion.V42)]
    public class SkeletonConverter42 : Spine.SkeletonConverter
    {
        private static readonly Dictionary<Inherit, string> InheritJsonValue = new()
        {
            [Inherit.Normal] = "normal",
            [Inherit.OnlyTranslation] = "onlyTranslation",
            [Inherit.NoRotationOrReflection] = "noRotationOrReflection",
            [Inherit.NoScale] = "noScale",
            [Inherit.NoScaleOrReflection] = "noScaleOrReflection",
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

        private static readonly Dictionary<AttachmentType, string> AttachmentTypeJsonValue = new()
        {
            [AttachmentType.Region] = "region",
            [AttachmentType.Boundingbox] = "bounding",
            [AttachmentType.Mesh] = "mesh",
            [AttachmentType.Linkedmesh] = "linkedMesh",
            [AttachmentType.Path] = "path",
            [AttachmentType.Point] = "point",
            [AttachmentType.Clipping] = "clipping",
            [AttachmentType.Sequence] = "sequence",
        };

        private static readonly Dictionary<SequenceMode, string> SequenceModeJsonValue = new()
        {
            [SequenceMode.Loop] = "loop",
            [SequenceMode.Once] = "once",
            [SequenceMode.LoopReverse] = "loopReverse",
            [SequenceMode.OnceReverse] = "onceReverse",
            [SequenceMode.Hold] = "hold",
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
            ReadPhysics();
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
            skeleton["hash"] = Convert.ToBase64String(Convert.FromHexString(reader.ReadLong().ToString("x16"))).TrimEnd('=');
            skeleton["spine"] = reader.ReadString();
            skeleton["x"] = reader.ReadFloat();
            skeleton["y"] = reader.ReadFloat();
            skeleton["width"] = reader.ReadFloat();
            skeleton["height"] = reader.ReadFloat();
            skeleton["referenceScale"] = reader.ReadFloat();
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
                data["inherit"] = InheritJsonValue[InheritEnum.Values[reader.ReadVarInt()]];
                data["skin"] = reader.ReadBoolean();
                if (nonessential)
                {
                    reader.ReadInt();
                    reader.ReadString();
                    reader.ReadBoolean();
                }
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
                if (nonessential) reader.ReadBoolean();
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
                data["bones"] = ReadNames(bones);
                data["target"] = (string)bones[reader.ReadVarInt()]["name"];

                int flags = reader.Read();
                if ((flags & 1) != 0) data["skin"] = true;
                if ((flags & 2) == 0) data["bendPositive"] = false;
                if ((flags & 4) != 0) data["compress"] = true;
                if ((flags & 8) != 0) data["stretch"] = true;
                if ((flags & 16) != 0) data["uniform"] = true;
                if ((flags & 32) == 0) data["mix"] = 0; else if ((flags & 64) != 0) data["mix"] = reader.ReadFloat();
                if ((flags & 128) != 0) data["softness"] = reader.ReadFloat();
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
                data["bones"] = ReadNames(bones);
                data["target"] = (string)bones[reader.ReadVarInt()]["name"];

                int flags = reader.Read();
                if ((flags & 1) != 0) data["skin"] = true;
                if ((flags & 2) != 0) data["local"] = true;
                if ((flags & 4) != 0) data["relative"] = true;
                if ((flags & 8) != 0) data["rotation"] = reader.ReadFloat();
                if ((flags & 16) != 0) data["x"] = reader.ReadFloat();
                if ((flags & 32) != 0) data["y"] = reader.ReadFloat();
                if ((flags & 64) != 0) data["scaleX"] = reader.ReadFloat();
                if ((flags & 128) != 0) data["scaleY"] = reader.ReadFloat();

                flags = reader.Read();
                if ((flags & 1) != 0) data["shearY"] = reader.ReadFloat();
                if ((flags & 2) != 0) data["mixRotate"] = reader.ReadFloat();
                if ((flags & 4) != 0) data["mixX"] = reader.ReadFloat();
                if ((flags & 8) != 0) data["mixY"] = reader.ReadFloat();
                if ((flags & 16) != 0) data["mixScaleX"] = reader.ReadFloat();
                if ((flags & 32) != 0) data["mixScaleY"] = reader.ReadFloat();
                if ((flags & 64) != 0) data["mixShearY"] = reader.ReadFloat();

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

                int flags = reader.Read();
                data["positionMode"] = PositionModeJsonValue[(PositionMode)(flags & 1)];
                data["spacingMode"] = SpacingModeJsonValue[(SpacingMode)((flags >> 1) & 3)];
                data["rotateMode"] = RotateModeJsonValue[(RotateMode)((flags >> 3) & 3)];
                if ((flags & 128) != 0) data["rotation"] = reader.ReadFloat();
                data["position"] = reader.ReadFloat();
                data["spacing"] = reader.ReadFloat();
                data["mixRotate"] = reader.ReadFloat();
                data["mixX"] = reader.ReadFloat();
                data["mixY"] = reader.ReadFloat();

                path.Add(data);
            }
            root["path"] = path;
        }

        private void ReadPhysics()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray physics = [];
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["bone"] = (string)bones[reader.ReadVarInt()]["name"];

                int flags = reader.Read();
                if ((flags & 1) != 0) data["skin"] = true;
                if ((flags & 2) != 0) data["x"] = reader.ReadFloat();
                if ((flags & 4) != 0) data["y"] = reader.ReadFloat();
                if ((flags & 8) != 0) data["rotate"] = reader.ReadFloat();
                if ((flags & 16) != 0) data["scaleX"] = reader.ReadFloat();
                if ((flags & 32) != 0) data["shearX"] = reader.ReadFloat();
                if ((flags & 64) != 0) data["limit"] = reader.ReadFloat();
                data["fps"] = reader.ReadUByte();
                data["inertia"] = reader.ReadFloat();
                data["strength"] = reader.ReadFloat();
                data["damping"] = reader.ReadFloat();
                if ((flags & 128) != 0) data["mass"] = reader.ReadFloat();
                data["wind"] = reader.ReadFloat();
                data["gravity"] = reader.ReadFloat();

                flags = reader.Read();
                if ((flags & 1) != 0) data["inertiaGlobal"] = true;
                if ((flags & 2) != 0) data["strengthGlobal"] = true;
                if ((flags & 4) != 0) data["dampingGlobal"] = true;
                if ((flags & 8) != 0) data["massGlobal"] = true;
                if ((flags & 16) != 0) data["windGlobal"] = true;
                if ((flags & 32) != 0) data["gravityGlobal"] = true;
                if ((flags & 64) != 0) data["mixGlobal"] = true;
                if ((flags & 128) != 0) data["mix"] = reader.ReadFloat();

                physics.Add(data);
            }
            root["physics"] = physics;
        }

        private void ReadSkins()
        {
            JsonArray skins = [];
            root["skins"] = skins;

            // default skin
            if (ReadSkin(true) is JsonObject data)
                skins.Add(data);

            // other skins
            for (int n = reader.ReadVarInt(); n > 0; n--)
                skins.Add(ReadSkin());
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
                skin["name"] = reader.ReadString();
                if (nonessential) reader.ReadInt();
                skin["bone"] = ReadNames(root["bones"].AsArray());
                skin["ik"] = ReadNames(root["ik"].AsArray());
                skin["transform"] = ReadNames(root["transform"].AsArray());
                skin["path"] = ReadNames(root["path"].AsArray());
                skin["physics"] = ReadNames(root["physics"].AsArray());
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
            JsonArray skins = root["skins"].AsArray();
            JsonObject attachment = [];
            int vertexCount;

            int flags = reader.ReadUByte();

            var name = (flags & 8) != 0 ? reader.ReadStringRef() : keyName;
            var type = (AttachmentType)(flags & 0x7);

            attachment["name"] = name;
            attachment["type"] = AttachmentTypeJsonValue[type];

            switch (type)
            {
                case AttachmentType.Region:
                    if ((flags & 16) != 0) attachment["path"] = reader.ReadStringRef();
                    if ((flags & 32) != 0) attachment["color"] = reader.ReadInt().ToString("x8");
                    if ((flags & 64) != 0) attachment["sequence"] = ReadSequence();
                    if ((flags & 128) != 0) attachment["rotation"] = reader.ReadFloat();
                    attachment["x"] = reader.ReadFloat();
                    attachment["y"] = reader.ReadFloat();
                    attachment["scaleX"] = reader.ReadFloat();
                    attachment["scaleY"] = reader.ReadFloat();
                    attachment["width"] = reader.ReadFloat();
                    attachment["height"] = reader.ReadFloat();
                    break;
                case AttachmentType.Boundingbox:
                    vertexCount = reader.ReadVarInt();
                    attachment["vertexCount"] = vertexCount;
                    attachment["vertices"] = ReadVertices(vertexCount, (flags & 16) != 0);
                    if (nonessential) reader.ReadInt();
                    break;
                case AttachmentType.Mesh:
                    if ((flags & 16) != 0) attachment["path"] = reader.ReadStringRef();
                    if ((flags & 32) != 0) attachment["color"] = reader.ReadInt().ToString("x8");
                    if ((flags & 64) != 0) attachment["sequence"] = ReadSequence();
                    var hulllength = reader.ReadVarInt();
                    attachment["hull"] = hulllength;
                    vertexCount = reader.ReadVarInt();
                    attachment["vertices"] = ReadVertices(vertexCount, (flags & 128) != 0);
                    attachment["uvs"] = ReadFloatArray(vertexCount << 1); // vertexCount = uvs.Length >> 1
                    attachment["triangles"] = ReadShortArray(((vertexCount << 1) - hulllength - 2) * 3);
                    if (nonessential)
                    {
                        attachment["edges"] = ReadShortArray(reader.ReadVarInt());
                        attachment["width"] = reader.ReadFloat();
                        attachment["height"] = reader.ReadFloat();
                    }
                    break;
                case AttachmentType.Linkedmesh:
                    if ((flags & 16) != 0) attachment["path"] = reader.ReadStringRef();
                    if ((flags & 32) != 0) attachment["color"] = reader.ReadInt().ToString("x8");
                    if ((flags & 64) != 0) attachment["sequence"] = ReadSequence();
                    if ((flags & 128) == 0) attachment["timelines"] = false;
                    attachment["skin"] = (string)skins[reader.ReadVarInt()]["name"];
                    attachment["parent"] = reader.ReadStringRef();
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
                    if ((flags & 16) != 0) attachment["closed"] = true;
                    if ((flags & 32) == 0) attachment["constantSpeed"] = false;
                    vertexCount = reader.ReadVarInt();
                    attachment["vertexCount"] = vertexCount;
                    attachment["vertices"] = ReadVertices(vertexCount, (flags & 64) != 0);
                    attachment["lengths"] = ReadFloatArray(vertexCount * 2 / 6);
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
                    attachment["vertices"] = ReadVertices(vertexCount, (flags & 16) != 0);
                    if (nonessential) reader.ReadInt();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid attachment type: {flags & 0x7}");
            }
            return attachment;
        }

        private JsonObject ReadSequence()
        {
            return new()
            {
                ["count"] = reader.ReadVarInt(),
                ["start"] = reader.ReadVarInt(),
                ["digits"] = reader.ReadVarInt(),
                ["setup"] = reader.ReadVarInt(),
            };
        }

        private void ReadEvents()
        {
            idx2event.Clear();
            JsonObject events = [];
            for (int i = 0, n = reader.ReadVarInt(); i < n; i++)
            {
                JsonObject data = [];
                var name = reader.ReadString();
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
                reader.ReadVarInt(); // timelines 数组预分配大小
                if (ReadSlotTimelines() is JsonObject slots) data["slots"] = slots;
                if (ReadBoneTimelines() is JsonObject bones) data["bones"] = bones;
                if (ReadIKTimelines() is JsonObject ik) data["ik"] = ik;
                if (ReadTransformTimelines() is JsonObject transform) data["transform"] = transform;
                if (ReadPathTimelines() is JsonObject path) data["path"] = path;
                if (ReadPhysicsTimelines() is JsonObject physics) data["physics"] = physics;
                if (ReadAttachmentTimelines() is JsonObject attachment) data["attachment"] = attachment;
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
                    JsonObject frame;
                    var type = reader.ReadUByte();
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
                        case SkeletonBinary.SLOT_RGBA:
                            timeline["rgba"] = frames;
                            reader.ReadVarInt(); // bezierCount

                            // XXX: 此处还原运行时逻辑, 无条件读取第一帧
                            // 但是据分析可以明显发现读取漏洞, 没有考虑 frameCount == 0 的情况
                            // 意味着若存在该类型时间序列则 frameCount 一定不为 0, 否则不应出现该类型, 后同
                            frame = new JsonObject()
                            {
                                ["time"] = reader.ReadFloat(),
                                ["color"] = reader.ReadInt().ToString("x8"),
                            };
                            frames.Add(frame);
                            for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["color"] = reader.ReadInt().ToString("x8"),
                                };
                                ReadCurve(frame, 4);
                                frame = o;
                                frames.Add(frame);
                            }
                            break;
                        case SkeletonBinary.SLOT_RGB:
                            timeline["rgb"] = frames;
                            reader.ReadVarInt(); // bezierCount
                            frame = new JsonObject()
                            {
                                ["time"] = reader.ReadFloat(),
                                ["color"] = $"{reader.Read():x2}{reader.Read():x2}{reader.Read():x2}",
                            };
                            frames.Add(frame);
                            for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["color"] = $"{reader.Read():x2}{reader.Read():x2}{reader.Read():x2}",
                                };
                                ReadCurve(frame, 3);
                                frame = o;
                                frames.Add(frame);
                            }
                            break;
                        case SkeletonBinary.SLOT_RGBA2:
                            timeline["rgba2"] = frames;
                            reader.ReadVarInt(); // bezierCount
                            frame = new JsonObject()
                            {
                                ["time"] = reader.ReadFloat(),
                                ["light"] = reader.ReadInt().ToString("x8"),
                                ["dark"] = $"{reader.Read():x2}{reader.Read():x2}{reader.Read():x2}",
                            };
                            frames.Add(frame);
                            for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["light"] = reader.ReadInt().ToString("x8"),
                                    ["dark"] = $"{reader.Read():x2}{reader.Read():x2}{reader.Read():x2}",
                                };
                                ReadCurve(frame, 7);
                                frame = o;
                                frames.Add(frame);
                            }
                            break;
                        case SkeletonBinary.SLOT_RGB2:
                            timeline["rgb2"] = frames;
                            reader.ReadVarInt(); // bezierCount
                            frame = new JsonObject()
                            {
                                ["time"] = reader.ReadFloat(),
                                ["light"] = $"{reader.Read():x2}{reader.Read():x2}{reader.Read():x2}",
                                ["dark"] = $"{reader.Read():x2}{reader.Read():x2}{reader.Read():x2}",
                            };
                            frames.Add(frame);
                            for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["light"] = $"{reader.Read():x2}{reader.Read():x2}{reader.Read():x2}",
                                    ["dark"] = $"{reader.Read():x2}{reader.Read():x2}{reader.Read():x2}",
                                };
                                ReadCurve(frame, 6);
                                frame = o;
                                frames.Add(frame);
                            }
                            break;
                        case SkeletonBinary.SLOT_ALPHA:
                            timeline["alpha"] = frames;
                            reader.ReadVarInt(); // bezierCount
                            frame = new JsonObject()
                            {
                                ["time"] = reader.ReadFloat(),
                                ["value"] = reader.Read() / 255f,
                            };
                            frames.Add(frame);
                            for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["value"] = reader.Read() / 255f,
                                };
                                ReadCurve(frame, 1);
                                frame = o;
                                frames.Add(frame);
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
                    var type = reader.ReadUByte();
                    var frameCount = reader.ReadVarInt();

                    if (type == SkeletonBinary.BONE_INHERIT)
                    {
                        timeline["inherit"] = frames;
                        while (frameCount-- > 0)
                        {
                            frames.Add(new JsonObject()
                            {
                                ["time"] = reader.ReadFloat(),
                                ["inherit"] = InheritJsonValue[(Inherit)reader.ReadUByte()]
                            });
                        }
                        continue;
                    }

                    reader.ReadVarInt(); // bezierCount
                    switch (type)
                    {
                        case SkeletonBinary.BONE_ROTATE:
                            timeline["rotate"] = frames;
                            ReadCurveFrames(frames, frameCount, "value");
                            break;
                        case SkeletonBinary.BONE_TRANSLATE:
                            timeline["translate"] = frames;
                            ReadCurveFrames(frames, frameCount, "x", "y");
                            break;
                        case SkeletonBinary.BONE_TRANSLATEX:
                            timeline["translatex"] = frames;
                            ReadCurveFrames(frames, frameCount, "value");
                            break;
                        case SkeletonBinary.BONE_TRANSLATEY:
                            timeline["translatey"] = frames;
                            ReadCurveFrames(frames, frameCount, "value");
                            break;
                        case SkeletonBinary.BONE_SCALE:
                            timeline["scale"] = frames;
                            ReadCurveFrames(frames, frameCount, "x", "y");
                            break;
                        case SkeletonBinary.BONE_SCALEX:
                            timeline["scalex"] = frames;
                            ReadCurveFrames(frames, frameCount, "value");
                            break;
                        case SkeletonBinary.BONE_SCALEY:
                            timeline["scaley"] = frames;
                            ReadCurveFrames(frames, frameCount, "value");
                            break;
                        case SkeletonBinary.BONE_SHEAR:
                            timeline["shear"] = frames;
                            ReadCurveFrames(frames, frameCount, "x", "y");
                            break;
                        case SkeletonBinary.BONE_SHEARX:
                            timeline["shearx"] = frames;
                            ReadCurveFrames(frames, frameCount, "value");
                            break;
                        case SkeletonBinary.BONE_SHEARY:
                            timeline["sheary"] = frames;
                            ReadCurveFrames(frames, frameCount, "value");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Invalid bone timeline type: {type}");
                    }
                }
            }

            return boneTimelines.Count > 0 ? boneTimelines : null;
        }

        private void ReadCurveFrames(JsonArray frames, int frameCount, string name1)
        {
            var frame = new JsonObject()
            {
                ["time"] = reader.ReadFloat(),
                [name1] = reader.ReadFloat(),
            };
            frames.Add(frame);
            for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
            {
                var o = new JsonObject()
                {
                    ["time"] = reader.ReadFloat(),
                    [name1] = reader.ReadFloat(),
                };
                ReadCurve(frame, 1);
                frame = o;
                frames.Add(frame);
            }
        }

        private void ReadCurveFrames(JsonArray frames, int frameCount, string name1, string name2)
        {
            var frame = new JsonObject()
            {
                ["time"] = reader.ReadFloat(),
                [name1] = reader.ReadFloat(),
                [name2] = reader.ReadFloat(),
            };
            frames.Add(frame);
            for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
            {
                var o = new JsonObject()
                {
                    ["time"] = reader.ReadFloat(),
                    [name1] = reader.ReadFloat(),
                    [name2] = reader.ReadFloat(),
                };
                ReadCurve(frame, 2);
                frame = o;
                frames.Add(frame);
            }
        }

        private JsonObject? ReadIKTimelines()
        {
            JsonArray ik = root["ik"].AsArray();
            JsonObject ikTimelines = [];

            for (int i = 0, ikCount = reader.ReadVarInt(); i < ikCount; i++)
            {
                JsonArray frames = [];
                ikTimelines[(string)ik[reader.ReadVarInt()]["name"]] = frames;
                var frameCount = reader.ReadVarInt();
                reader.ReadVarInt(); // bezierCount

                int flags = reader.Read();
                JsonObject frame = new()
                {
                    ["time"] = reader.ReadFloat(),
                    ["mix"] = (flags & 1) != 0 ? ((flags & 2) != 0 ? reader.ReadFloat() : 1) : 0,
                    ["softness"] = (flags & 4) != 0 ? reader.ReadFloat() : 0,
                    ["bendPositive"] = (flags & 8) != 0,
                    ["compress"] = (flags & 16) != 0,
                    ["stretch"] = (flags & 32) != 0,
                };
                frames.Add(frame);
                for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
                {
                    flags = reader.Read();
                    var o = new JsonObject()
                    {
                        ["time"] = reader.ReadFloat(),
                        ["mix"] = (flags & 1) != 0 ? ((flags & 2) != 0 ? reader.ReadFloat() : 1) : 0,
                        ["softness"] = (flags & 4) != 0 ? reader.ReadFloat() : 0,
                        ["bendPositive"] = (flags & 8) != 0,
                        ["compress"] = (flags & 16) != 0,
                        ["stretch"] = (flags & 32) != 0,
                    };
                    if ((flags & 64) != 0) frame["curve"] = "stepped";
                    else if ((flags & 128) != 0) frame["curve"] = ReadFloatArray(8);
                    frame = o;
                    frames.Add(frame);
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
                int frameCount = reader.ReadVarInt();
                reader.ReadVarInt();
                var frame = new JsonObject()
                {
                    ["time"] = reader.ReadFloat(),
                    ["mixRotate"] = reader.ReadFloat(),
                    ["mixX"] = reader.ReadFloat(),
                    ["mixY"] = reader.ReadFloat(),
                    ["mixScaleX"] = reader.ReadFloat(),
                    ["mixScaleY"] = reader.ReadFloat(),
                    ["mixShearY"] = reader.ReadFloat(),
                };
                frames.Add(frame);
                for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
                {
                    var o = new JsonObject()
                    {
                        ["time"] = reader.ReadFloat(),
                        ["mixRotate"] = reader.ReadFloat(),
                        ["mixX"] = reader.ReadFloat(),
                        ["mixY"] = reader.ReadFloat(),
                        ["mixScaleX"] = reader.ReadFloat(),
                        ["mixScaleY"] = reader.ReadFloat(),
                        ["mixShearY"] = reader.ReadFloat(),
                    };
                    ReadCurve(frame, 6);
                    frame = o;
                    frames.Add(frame);
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
                pathTimelines[(string)(path[reader.ReadVarInt()]["name"])] = timeline;
                for (int ii = 0, timelineCount = reader.ReadVarInt(); ii < timelineCount; ii++)
                {
                    JsonArray frames = [];
                    JsonObject frame;
                    var type = reader.ReadUByte();
                    var frameCount = reader.ReadVarInt();
                    reader.ReadVarInt();
                    switch (type)
                    {
                        case SkeletonBinary.PATH_POSITION:
                            timeline["position"] = frames;
                            ReadCurveFrames(frames, frameCount, "value");
                            break;
                        case SkeletonBinary.PATH_SPACING:
                            timeline["spacing"] = frames;
                            ReadCurveFrames(frames, frameCount, "value");
                            break;
                        case SkeletonBinary.PATH_MIX:
                            timeline["mix"] = frames;
                            frame = new JsonObject()
                            {
                                ["time"] = reader.ReadFloat(),
                                ["mixRotate"] = reader.ReadFloat(),
                                ["mixX"] = reader.ReadFloat(),
                                ["mixY"] = reader.ReadFloat(),
                            };
                            frames.Add(frame);
                            for (int frameIdx = 1; frameIdx < frameCount; ++frameIdx)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["mixRotate"] = reader.ReadFloat(),
                                    ["mixX"] = reader.ReadFloat(),
                                    ["mixY"] = reader.ReadFloat(),
                                };
                                ReadCurve(frame, 3);
                                frame = o;
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

        private JsonObject? ReadPhysicsTimelines()
        {
            JsonArray physics = root["physics"].AsArray();
            JsonObject physicsTimelines = [];
            for (int i = 0, physicsCount = reader.ReadVarInt(); i < physicsCount; i++)
            {
                JsonObject timeline = [];

                physicsTimelines[(string)physics[reader.ReadVarInt() - 1]["name"]] = timeline;
                for (int ii = 0, timelineCount = reader.ReadVarInt(); ii < timelineCount; ii++)
                {
                    JsonArray frames = [];
                    var type = reader.ReadUByte();
                    var frameCount = reader.ReadVarInt();
                    if (type == SkeletonBinary.PHYSICS_RESET)
                    {
                        timeline["reset"] = frames;
                        while (frameCount-- > 0)
                        {
                            frames.Add(new JsonObject()
                            {
                                ["time"] = reader.ReadFloat()
                            });
                        }
                        continue;
                    }

                    reader.ReadVarInt();
                    switch (type)
                    {
                        case SkeletonBinary.PHYSICS_INERTIA:
                            timeline["inertia"] = frames;
                            break;
                        case SkeletonBinary.PHYSICS_STRENGTH:
                            timeline["strength"] = frames;
                            break;
                        case SkeletonBinary.PHYSICS_DAMPING:
                            timeline["damping"] = frames;
                            break;
                        case SkeletonBinary.PHYSICS_MASS:
                            timeline["mass"] = frames;
                            break;
                        case SkeletonBinary.PHYSICS_WIND:
                            timeline["wind"] = frames;
                            break;
                        case SkeletonBinary.PHYSICS_GRAVITY:
                            timeline["gravity"] = frames;
                            break;
                        case SkeletonBinary.PHYSICS_MIX:
                            timeline["mix"] = frames;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Invalid physics timeline type: {type}");
                    }
                    ReadCurveFrames(frames, frameCount, "value");
                }
            }

            return physicsTimelines.Count > 0 ? physicsTimelines : null;
        }

        private JsonObject? ReadAttachmentTimelines()
        {
            JsonArray skin = root["skins"].AsArray();
            JsonArray slot = root["slots"].AsArray();
            JsonObject attachmentTimelines = [];

            for (int i = 0, skinCount = reader.ReadVarInt(); i < skinCount; i++)
            {
                JsonObject skinValue = [];
                attachmentTimelines[(string)skin[reader.ReadVarInt()]["name"]] = skinValue;
                for (int ii = 0, slotCount = reader.ReadVarInt(); ii < slotCount; ii++)
                {
                    JsonObject slotValue = [];
                    skinValue[(string)slot[reader.ReadVarInt()]["name"]] = slotValue;
                    for (int iii = 0, attachmentCount = reader.ReadVarInt(); iii < attachmentCount; iii++)
                    {
                        JsonObject timeline = [];
                        slotValue[reader.ReadStringRef()] = timeline;

                        JsonArray frames = [];
                        JsonObject frame;
                        int end;

                        var type = reader.ReadUByte();
                        var frameCount = reader.ReadVarInt();
                        switch (type)
                        {
                            case SkeletonBinary.ATTACHMENT_DEFORM:
                                reader.ReadVarInt();
                                timeline["deform"] = frames;

                                frame = new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                };
                                end = reader.ReadVarInt();
                                if (end > 0)
                                {
                                    frame["offset"] = reader.ReadVarInt();
                                    frame["vertices"] = ReadFloatArray(end);
                                }
                                frames.Add(frame);
                                for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
                                {
                                    var o = new JsonObject()
                                    {
                                        ["time"] = reader.ReadFloat(),
                                    };
                                    end = reader.ReadVarInt();
                                    if (end > 0)
                                    {
                                        frame["offset"] = reader.ReadVarInt();
                                        frame["vertices"] = ReadFloatArray(end);
                                    }
                                    ReadCurve(frame, 1);
                                    frame = o;
                                    frames.Add(frame);
                                }
                                break;
                            case SkeletonBinary.ATTACHMENT_SEQUENCE:
                                timeline["sequence"] = frames;
                                while (frameCount-- > 0)
                                {
                                    var o = new JsonObject()
                                    {
                                        ["time"] = reader.ReadFloat(),
                                    };
                                    var modeAndIndex = reader.ReadInt();
                                    o["mode"] = SequenceModeJsonValue[(SequenceMode)(modeAndIndex & 0xf)];
                                    o["index"] = modeAndIndex >> 4;
                                    o["delay"] = reader.ReadFloat();
                                    frames.Add(o);
                                }
                                break;
                        }
                    }
                }
            }
            return attachmentTimelines.Count > 0 ? attachmentTimelines : null;
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
                if (reader.ReadString() is string @string) data["string"] = @string;
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

        private JsonArray ReadShortArray(int count)
        {
            JsonArray array = [];
            for (int i = 0; i < count; i++)
                array.Add(reader.ReadVarInt());
            return array;
        }

        private JsonArray ReadVertices(int vertexCount, bool weighted)
        {
            JsonArray vertices = [];

            if (!weighted)
                return ReadFloatArray(vertexCount << 1);

            for (int i = 0; i < vertexCount; i++)
            {
                int bonesCount = reader.ReadVarInt();
                vertices.Add(bonesCount);
                for (int ii = 0; ii < bonesCount; ii++)
                {
                    vertices.Add(reader.ReadVarInt());
                    vertices.Add(reader.ReadFloat());
                    vertices.Add(reader.ReadFloat());
                    vertices.Add(reader.ReadFloat());
                }
            }
            return vertices;
        }

        private void ReadCurve(JsonObject obj, int count)
        {
            var type = reader.ReadUByte();
            switch (type)
            {
                case SkeletonBinary.CURVE_LINEAR:
                    break;
                case SkeletonBinary.CURVE_STEPPED:
                    obj["curve"] = "stepped";
                    break;
                case SkeletonBinary.CURVE_BEZIER:
                    obj["curve"] = ReadFloatArray(count * 4);
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
        private readonly Dictionary<string, int> physics2idx = [];
        private readonly Dictionary<string, int> skin2idx = [];
        private readonly Dictionary<string, int> event2idx = [];

        public override void WriteBinary(JsonObject root, string binPath, bool nonessential = false)
        {
            this.nonessential = nonessential;
            this.root = root;

            using var outputBody = new MemoryStream(); // 先把主体写入内存缓冲区
            BinaryWriter tmpWriter = writer = new(outputBody);

            WriteBones();
            WriteSlots();
            WriteIK();
            WriteTransform();
            WritePath();
            WritePhysics();
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
            if (skeleton["hash"].GetValueKind() == JsonValueKind.String)
            {
                writer.WriteLong(114514);//我不知道那个hash该怎么转，随便写一个数字也不影响使用
            }
            else
            {
                writer.WriteLong((long.Parse((string)skeleton["hash"])));
            }
            var version = (string)skeleton["spine"];
            //if (version == "3.8.75") version = "3.8.76"; // replace 3.8.75 to another version to avoid detection in official runtime
            writer.WriteString(version);
            if (skeleton.TryGetPropertyValue("x", out var x)) writer.WriteFloat((float)x); else writer.WriteFloat(0);
            if (skeleton.TryGetPropertyValue("y", out var y)) writer.WriteFloat((float)y); else writer.WriteFloat(0);
            if (skeleton.TryGetPropertyValue("width", out var width)) writer.WriteFloat((float)width); else writer.WriteFloat(0);
            if (skeleton.TryGetPropertyValue("height", out var height)) writer.WriteFloat((float)height); else writer.WriteFloat(0);
            if (skeleton.TryGetPropertyValue("referenceScale", out var reference)) writer.WriteFloat((float)reference); else writer.WriteFloat(0f);
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
                if (data.TryGetPropertyValue("inherit", out var inherit)) writer.WriteVarInt((int)Enum.Parse<Inherit>((string)inherit, true)); else writer.WriteVarInt((int)Inherit.Normal);
                if (data.TryGetPropertyValue("skin", out var skin)) writer.WriteBoolean((bool)skin); else writer.WriteBoolean(false);
                if (nonessential)
                {
                    writer.WriteInt(0);
                    writer.WriteString("");
                    writer.WriteBoolean(false);
                }
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
                if (data.TryGetPropertyValue("color", out var color)) writer.WriteInt(int.Parse((string)color, NumberStyles.HexNumber)); else writer.WriteInt(-1); // 默认值是全 255
                if (data.TryGetPropertyValue("dark", out var dark)) writer.WriteInt(int.Parse((string)dark, NumberStyles.HexNumber)); else writer.WriteInt(-1);
                if (data.TryGetPropertyValue("attachment", out var attachment)) writer.WriteStringRef((string)attachment); else writer.WriteStringRef(null);
                if (data.TryGetPropertyValue("blend", out var blend)) writer.WriteVarInt((int)Enum.Parse<BlendMode>((string)blend, true)); else writer.WriteVarInt((int)BlendMode.Normal);
                if (nonessential)
                {
                    writer.WriteBoolean(false);
                }
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
            int flag = 0;
            for (int i = 0, n = ik.Count; i < n; i++)
            {
                JsonObject data = ik[i].AsObject();
                var name = (string)data["name"];
                writer.WriteString(name);
                if (data.TryGetPropertyValue("order", out var order)) writer.WriteVarInt((int)order); else writer.WriteVarInt(0);
                //if (data.TryGetPropertyValue("skin", out var skin)) writer.WriteBoolean((bool)skin); else writer.WriteBoolean(false);
                if (data.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                writer.WriteVarInt(bone2idx[(string)data["target"]]);

                flag = 0;
                if (data.TryGetPropertyValue("skin", out var skin) && (bool)skin)
                    flag |= 1;
                if (!data.TryGetPropertyValue("bendPositive", out var bendPositive) || (bool)bendPositive)
                    flag |= 2;
                if (data.TryGetPropertyValue("compress", out var compress) && (bool)compress)
                    flag |= 4;
                if (data.TryGetPropertyValue("stretch", out var stretch) && (bool)stretch)
                    flag |= 8;
                if (data.TryGetPropertyValue("uniform", out var uniform) && (bool)uniform)
                    flag |= 16;
                if (data.TryGetPropertyValue("mix", out var mix))
                {
                    flag |= 32;
                    if ((float)mix != 1)
                        flag |= 64;
                }
                if (data.TryGetPropertyValue("softness", out var softness))
                    flag |= 128;
                writer.WriteByte((byte)flag);
                if ((flag & 64) != 0) writer.WriteFloat((float)data["mix"]);
                if ((flag & 128) != 0) writer.WriteFloat((float)data["softness"]);
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
            int flag = 0;
            JsonArray transform = root["transform"].AsArray();
            writer.WriteVarInt(transform.Count);
            for (int i = 0, n = transform.Count; i < n; i++)
            {
                JsonObject data = transform[i].AsObject();
                var name = (string)data["name"];
                writer.WriteString(name);
                if (data.TryGetPropertyValue("order", out var order)) writer.WriteVarInt((int)order); else writer.WriteVarInt(0);
                //if (data.TryGetPropertyValue("skin", out var skin)) writer.WriteBoolean((bool)skin); else writer.WriteBoolean(false);
                if (data.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                writer.WriteVarInt(bone2idx[(string)data["target"]]);
                flag = 0;
                if (data.TryGetPropertyValue("skin", out var skin) && (bool)skin)
                    flag |= 1;
                if (data.TryGetPropertyValue("local", out var local) && (bool)local)
                    flag |= 2;
                if (data.TryGetPropertyValue("relative", out var relative) && (bool)relative)
                    flag |= 4;
                if (data.ContainsKey("rotation"))
                    flag |= 8;
                if (data.ContainsKey("x"))
                    flag |= 16;
                if (data.ContainsKey("y"))
                    flag |= 32;
                if (data.ContainsKey("scaleX"))
                    flag |= 64;
                if (data.ContainsKey("scaleY"))
                    flag |= 128;
                writer.WriteByte((byte)flag);
                if ((flag & 8) != 0) writer.WriteFloat((float)data["rotation"]);
                if ((flag & 16) != 0) writer.WriteFloat((float)data["x"]); // * scale 如果有的话
                if ((flag & 32) != 0) writer.WriteFloat((float)data["y"]);
                if ((flag & 64) != 0) writer.WriteFloat((float)data["scaleX"]);
                if ((flag & 128) != 0) writer.WriteFloat((float)data["scaleY"]);
                //if ((flag & 8) != 0)
                //{
                //    if (data.TryGetPropertyValue("rotation", out var rotation))
                //        writer.WriteFloat((float)rotation);
                //    else
                //        writer.WriteFloat(0);
                //}
                //if ((flag & 16) != 0)
                //{
                //    if (data.TryGetPropertyValue("x", out var x))
                //        writer.WriteFloat((float)x);
                //    else
                //        writer.WriteFloat(0);
                //}
                //if ((flag & 32) != 0)
                //{
                //    if (data.TryGetPropertyValue("y", out var y))
                //        writer.WriteFloat((float)y);
                //    else
                //        writer.WriteFloat(0);
                //}
                //if ((flag & 64) != 0)
                //{
                //    if (data.TryGetPropertyValue("scaleX", out var scaleX))
                //        writer.WriteFloat((float)scaleX);
                //    else
                //        writer.WriteFloat(0);
                //}
                //if ((flag & 128) != 0)
                //{
                //    if (data.TryGetPropertyValue("scaleY", out var scaleY))
                //        writer.WriteFloat((float)scaleY);
                //    else
                //        writer.WriteFloat(0);
                //}
                flag = 0;
                if (data.ContainsKey("shearY"))
                    flag |= 1;
                if (data.ContainsKey("mixRotate"))
                    flag |= 2;
                if (data.ContainsKey("mixX"))
                    flag |= 4;
                if (data.ContainsKey("mixY"))
                    flag |= 8;
                if (data.ContainsKey("mixScaleX"))
                    flag |= 16;
                if (data.ContainsKey("mixScaleY"))
                    flag |= 32;
                if (data.ContainsKey("mixShearY"))
                    flag |= 64;
                writer.WriteByte((byte)flag);
                if ((flag & 1) != 0) writer.WriteFloat((float)data["shearY"]);
                if ((flag & 2) != 0) writer.WriteFloat((float)data["mixRotate"]);
                if ((flag & 4) != 0) writer.WriteFloat((float)data["mixX"]);
                if ((flag & 8) != 0) writer.WriteFloat((float)data["mixY"]);
                if ((flag & 16) != 0) writer.WriteFloat((float)data["mixScaleX"]);
                if ((flag & 32) != 0) writer.WriteFloat((float)data["mixScaleY"]);
                if ((flag & 64) != 0) writer.WriteFloat((float)data["mixShearY"]);
                //if ((flag & 1) != 0)
                //{
                //    if (data.TryGetPropertyValue("shearY", out var shearY))
                //        writer.WriteFloat((float)shearY);
                //    else
                //        writer.WriteFloat(0);
                //}
                //if ((flag & 2) != 0)
                //{
                //    if (data.TryGetPropertyValue("mixRotate", out var mixRotate))
                //        writer.WriteFloat((float)mixRotate);
                //    else
                //        writer.WriteFloat(1);
                //}
                //if ((flag & 4) != 0)
                //{
                //    if (data.TryGetPropertyValue("mixX", out var mixX))
                //        writer.WriteFloat((float)mixX);
                //    else
                //        writer.WriteFloat(1);
                //}
                //if ((flag & 8) != 0)
                //{
                //    if (data.TryGetPropertyValue("mixY", out var mixY))
                //        writer.WriteFloat((float)mixY);
                //    else
                //        if (data.TryGetPropertyValue("mixX", out var mixX)) writer.WriteFloat((float)mixX); else writer.WriteFloat(1);
                //}
                //if ((flag & 16) != 0)
                //{
                //    if (data.TryGetPropertyValue("mixScaleX", out var mixScaleX))
                //        writer.WriteFloat((float)mixScaleX);
                //    else
                //        writer.WriteFloat(1);
                //}
                //if ((flag & 32) != 0)
                //{
                //    if (data.TryGetPropertyValue("mixScaleY", out var mixScaleY))
                //        writer.WriteFloat((float)mixScaleY);
                //    else
                //        if (data.TryGetPropertyValue("mixScaleX", out var mixScaleX)) writer.WriteFloat((float)mixScaleX); else writer.WriteFloat(1);
                //}
                //if ((flag & 64) != 0)
                //{
                //    if (data.TryGetPropertyValue("mixShearY", out var mixShearY))
                //        writer.WriteFloat((float)mixShearY);
                //    else
                //        writer.WriteFloat(1);
                //}
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
            int flag = 0;
            for (int i = 0, n = path.Count; i < n; i++)
            {
                JsonObject data = path[i].AsObject();
                var name = (string)data["name"];
                writer.WriteString(name);
                if (data.TryGetPropertyValue("order", out var order)) writer.WriteVarInt((int)order); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("skin", out var skin)) writer.WriteBoolean((bool)skin); else writer.WriteBoolean(false);
                if (data.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                writer.WriteVarInt(slot2idx[(string)data["target"]]);

                flag = 0;
                if (data.TryGetPropertyValue("positionMode", out var posModeStr))
                {
                    var posMode = (int)Enum.Parse<PositionMode>((string)posModeStr, true);
                    flag |= posMode & 1;
                }
                if (data.TryGetPropertyValue("spacingMode", out var spacingModeStr))
                {
                    var spacingMode = (int)Enum.Parse<SpacingMode>((string)spacingModeStr, true);
                    flag |= (spacingMode & 0b11) << 1;
                }
                if (data.TryGetPropertyValue("rotateMode", out var rotateModeStr))
                {
                    var rotateMode = (int)Enum.Parse<RotateMode>((string)rotateModeStr, true);
                    flag |= (rotateMode & 0b11) << 3;
                }
                if (data.TryGetPropertyValue("rotation", out var rotationVal))
                    flag |= 128;
                writer.WriteByte((byte)flag);
                if ((flag & 128) != 0)
                    writer.WriteFloat((float)data["rotation"]);
                if (data.TryGetPropertyValue("position", out var position)) writer.WriteFloat((float)position); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("spacing", out var spacing)) writer.WriteFloat((float)spacing); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("mixRotate", out var rotateMix)) writer.WriteFloat((float)rotateMix); else writer.WriteFloat(1);
                if (data.TryGetPropertyValue("mixX", out var translatex)) writer.WriteFloat((float)translatex); else writer.WriteFloat(1);
                if (data.TryGetPropertyValue("mixY", out var translatey))
                {
                    writer.WriteFloat((float)translatey);
                }
                else
                {
                    if (data.TryGetPropertyValue("mixX", out var translatex1)) writer.WriteFloat((float)translatex1); else writer.WriteFloat(1);
                }
                //mixScaleX,mixScaleY,mixShearY在从二进制里读取时未出现，但在从json加载中有体现
                path2idx[name] = i;
            }
        }

        private void WritePhysics()
        {
            if (!root.ContainsKey("physics"))
            {
                writer.WriteVarInt(0);
                return;
            }
            JsonArray physics = root["physics"].AsArray();
            writer.WriteVarInt(physics.Count);
            int flag = 0;
            for (int i = 0; i < physics.Count; i++)
            {
                JsonObject data = physics[i].AsObject();
                var name = (string)data["name"];
                writer.WriteString(name);
                if (data.TryGetPropertyValue("order", out var order)) writer.WriteVarInt((int)order); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("bone", out var bone)) writer.WriteVarInt(bone2idx[(string)bone]); else writer.WriteVarInt(0);
                // (data.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                //writer.WriteVarInt(bone2idx[(string)data["target"]]);

                flag = 0;
                //忽略等于默认值的情况
                if (data.TryGetPropertyValue("skin", out var _skin) && (bool)_skin) flag |= 1;
                if (data.TryGetPropertyValue("x", out var x)) flag |= 2;
                if (data.TryGetPropertyValue("y", out var y)) flag |= 4;
                if (data.TryGetPropertyValue("rotate", out var rot)) flag |= 8;
                if (data.TryGetPropertyValue("scalex", out var scaleX)) flag |= 16;
                if (data.TryGetPropertyValue("shearx", out var shearX)) flag |= 32;
                if (data.TryGetPropertyValue("limit", out var limit)) flag |= 64;
                if (data.TryGetPropertyValue("mass", out var mass)) flag |= 128;

                writer.Write(flag);
                if ((flag & 2) != 0) writer.WriteFloat((float)x);
                if ((flag & 4) != 0) writer.WriteFloat((float)y);
                if ((flag & 8) != 0) writer.WriteFloat((float)rot);
                if ((flag & 16) != 0) writer.WriteFloat((float)scaleX);
                if ((flag & 32) != 0) writer.WriteFloat((float)shearX);
                if ((flag & 64) != 0) writer.WriteFloat((float)limit);

                //writer.WriteByte((byte)data["fps"]);
                //writer.WriteFloat((float)data["inertia"]);
                //writer.WriteFloat((float)data["strength"]);
                //writer.WriteFloat((float)data["damping"]);
                if (data.TryGetPropertyValue("fps", out var fps)) writer.WriteByte((byte)fps); else writer.WriteByte(60);//无需取倒，因为在SkeltonBinary和SkeletonJson里面读取的时候都取倒了。
                if (data.TryGetPropertyValue("inertia", out var inertia)) writer.WriteFloat((float)inertia); else writer.WriteFloat(1);
                if (data.TryGetPropertyValue("strength", out var strength)) writer.WriteFloat((float)strength); else writer.WriteFloat(100);
                if (data.TryGetPropertyValue("damping", out var damping)) writer.WriteFloat((float)damping); else writer.WriteFloat(1);
                if ((flag & 128) != 0)
                {
                    if (data.TryGetPropertyValue("mass", out var ma)) writer.WriteFloat(1f / (float)ma); else writer.WriteFloat(1);
                }
                //writer.WriteFloat((float)data["wind"]);
                //writer.WriteFloat((float)data["gravity"]);
                if (data.TryGetPropertyValue("wind", out var wind)) writer.WriteFloat((float)wind); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("gravity", out var gravity)) writer.WriteFloat((float)gravity); else writer.WriteFloat(0);

                flag = 0;
                if (data.TryGetPropertyValue("inertiaGlobal", out var ig) && (bool)ig) flag |= 1;
                if (data.TryGetPropertyValue("strengthGlobal", out var sg) && (bool)sg) flag |= 2;
                if (data.TryGetPropertyValue("dampingGlobal", out var dg) && (bool)dg) flag |= 4;
                if (data.TryGetPropertyValue("massGlobal", out var mg) && (bool)mg) flag |= 8;
                if (data.TryGetPropertyValue("windGlobal", out var wg) && (bool)wg) flag |= 16;
                if (data.TryGetPropertyValue("gravityGlobal", out var gg) && (bool)gg) flag |= 32;
                if (data.TryGetPropertyValue("mixGlobal", out var mgVal) && (bool)mgVal) flag |= 64;
                if (data.TryGetPropertyValue("mix", out var mixVal)) flag |= 128;
                writer.Write(flag);

                if ((flag & 128) != 0)
                    writer.WriteFloat((float)mixVal);
                physics2idx[name] = i;
            }
        }

        private void WriteSkins()
        {

            if (!root.ContainsKey("skins"))
            {
                writer.WriteVarInt(0); // default 的 slotCount
                writer.WriteVarInt(0); // 其他皮肤数量
                //skin2idx["default"] = skin2idx.Count;
                return;
            }

            JsonArray skins = root["skins"].AsArray();
            //在官方的example中，出现了前面的skinAttachmentname
            foreach (var sk in skins)
            {
                skin2idx[(string)sk["name"]] = skin2idx.Count;
            }

            bool hasDefault = false;
            foreach (JsonObject skin in skins)
            {
                if ((string)skin["name"] == "default")
                {
                    hasDefault = true;
                    //skin2idx["default"] = skin2idx.Count;
                    WriteSkin(skin, true);

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
                    //skin2idx[name] = skin2idx.Count;
                    WriteSkin(skin);

                }
            }
        }

        private void WriteSkin(JsonObject skin, bool isDefault = false)
        {
            JsonObject skinAttachments = null;
            if (isDefault)
            {
                // 这里固定有一个给 default 的 count 值, 算是占位符, 如果是 0 则表示没有 default 的 skin
                if (skin.TryGetPropertyValue("attachments", out var attachments)) skinAttachments = attachments.AsObject();
                writer.WriteVarInt(skinAttachments?.Count ?? 0);
            }
            else
            {
                writer.WriteString((string)skin["name"]);
                if (nonessential) writer.WriteInt(0);
                if (skin.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                if (skin.TryGetPropertyValue("ik", out var ik)) WriteNames(ik2idx, ik.AsArray()); else writer.WriteVarInt(0);
                if (skin.TryGetPropertyValue("transform", out var transform)) WriteNames(transform2idx, transform.AsArray()); else writer.WriteVarInt(0);
                if (skin.TryGetPropertyValue("path", out var path)) WriteNames(path2idx, path.AsArray()); else writer.WriteVarInt(0);
                //if (skin.TryGetPropertyValue("attachments", out var attachments)) skinAttachments = attachments.AsObject();
                if (skin.TryGetPropertyValue("physics", out var physics)) WriteNames(physics2idx, physics.AsArray()); else writer.WriteVarInt(0);
                if (skin.TryGetPropertyValue("attachments", out var attachments)) skinAttachments = attachments.AsObject();
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
            JsonArray vertices;

            string name = keyName;
            AttachmentType type = AttachmentType.Region;
            bool hasName = false;

            //<---计算flag开始
            if (attachment.TryGetPropertyValue("name", out var _name))
            {
                name = (string)_name;
                hasName = name != keyName;
            }
            if (attachment.TryGetPropertyValue("type", out var _type))
                type = Enum.Parse<AttachmentType>((string)_type, true);
            byte flag = (byte)type;
            if (hasName)
                flag |= 8;
            switch (type)
            {
                case AttachmentType.Region:
                    if (attachment.TryGetPropertyValue("path", out var _))
                        flag |= 16;
                    if (attachment.TryGetPropertyValue("color", out var _color) && int.Parse((string)_color, NumberStyles.HexNumber) != -1)
                        flag |= 32;
                    if (attachment.TryGetPropertyValue("sequence", out var _))
                        flag |= 64;
                    if (attachment.TryGetPropertyValue("rotation", out var rotation) && (float)rotation != 0)
                        flag |= 128;
                    break;

                case AttachmentType.Boundingbox:
                    vertices = attachment["vertices"].AsArray();
                    if (attachment.TryGetPropertyValue("vertexCount", out var _vertexCount) && vertices.Count != (int)_vertexCount << 1) flag |= 16;
                    break;

                case AttachmentType.Mesh:
                    if (attachment.TryGetPropertyValue("path", out _))
                        flag |= 16;
                    if (attachment.TryGetPropertyValue("color", out var _color1) && int.Parse((string)_color1, NumberStyles.HexNumber) != -1)
                        flag |= 32;
                    if (attachment.TryGetPropertyValue("sequence", out _))
                        flag |= 64;
                    vertices = attachment["vertices"].AsArray();
                    if (attachment.TryGetPropertyValue("uvs", out var _uvs) && vertices.Count != _uvs.AsArray().Count)
                        flag |= 128;
                    //if (vertices.Count > 0 && vertices[0] is JsonValue v1 && v1.TryGetValue(out int _))
                    //    flag |= 128;
                    break;

                case AttachmentType.Linkedmesh:
                    if (attachment.TryGetPropertyValue("path", out _))
                        flag |= 16;
                    if (attachment.TryGetPropertyValue("color", out var _color2) && int.Parse((string)_color2, NumberStyles.HexNumber) != -1)
                        flag |= 32;
                    if (attachment.TryGetPropertyValue("sequence", out var _))
                        flag |= 64;
                    if (attachment.TryGetPropertyValue("timeline", out var timeline) && (bool)timeline)
                        flag |= 128;
                    break;

                case AttachmentType.Path:
                    if (attachment.TryGetPropertyValue("closed", out var _closed) && (bool)_closed)
                        flag |= 16;
                    if (attachment.TryGetPropertyValue("constantSpeed", out var _speed) && (bool)_speed)
                        flag |= 32;
                    else flag |= 32;//默认为true
                    vertices = attachment["vertices"].AsArray();
                    if (attachment.TryGetPropertyValue("vertexCount", out var _vertexCount1) && vertices.Count != (int)_vertexCount1 << 1)
                        flag |= 64;
                    //if (vertices.Count > 0 && vertices[0] is JsonValue v2 && v2.TryGetValue(out int _))
                    //    flag |= 64;
                    break;

                case AttachmentType.Clipping:
                    vertices = attachment["vertices"].AsArray();
                    if (attachment.TryGetPropertyValue("vertexCount", out var _vertexCount2) && vertices.Count != (int)_vertexCount2 << 1)
                        flag |= 16;
                    //if (vertices.Count > 0 && vertices[0] is JsonValue v3 && v3.TryGetValue(out int _))
                    //    flag |= 16;
                    break;

            }
            //--->计算flag结束
            writer.WriteByte(flag);
            if ((flag & 8) != 0)
                writer.WriteStringRef(name);


            switch (type)
            {
                case AttachmentType.Region:
                    if ((flag & 16) != 0)
                    {
                        if (attachment.TryGetPropertyValue("path", out var path)) writer.WriteStringRef((string)path); else writer.WriteStringRef(null);
                    }
                    if ((flag & 32) != 0)
                    {
                        if (attachment.TryGetPropertyValue("color", out var color)) writer.WriteInt(int.Parse((string)color, NumberStyles.HexNumber)); else writer.WriteInt(-1);
                    }
                    if ((flag & 64) != 0)
                    {
                        //emm,虽然如果没有sequence的话flag那里就不会|=64,都不会执行这里.
                        if (attachment.TryGetPropertyValue("sequence", out var sequence)) WriteSequence(sequence.AsArray());
                        else
                        {
                            writer.WriteVarInt(0);
                            writer.WriteVarInt(1);
                            writer.WriteVarInt(0);
                            writer.WriteVarInt(0);
                        }
                    }
                    if ((flag & 128) != 0)
                    {
                        if (attachment.TryGetPropertyValue("rotation", out var rotation1)) writer.WriteFloat((float)rotation1); else writer.WriteFloat(0);
                    }
                    if (attachment.TryGetPropertyValue("x", out var x1)) writer.WriteFloat((float)x1); else writer.WriteFloat(0);
                    if (attachment.TryGetPropertyValue("y", out var y1)) writer.WriteFloat((float)y1); else writer.WriteFloat(0);
                    if (attachment.TryGetPropertyValue("scaleX", out var scaleX)) writer.WriteFloat((float)scaleX); else writer.WriteFloat(1);
                    if (attachment.TryGetPropertyValue("scaleY", out var scaleY)) writer.WriteFloat((float)scaleY); else writer.WriteFloat(1);
                    if (attachment.TryGetPropertyValue("width", out var width)) writer.WriteFloat((float)width); else writer.WriteFloat(32);
                    if (attachment.TryGetPropertyValue("height", out var height)) writer.WriteFloat((float)height); else writer.WriteFloat(32);
                    break;
                case AttachmentType.Boundingbox:
                    if (attachment.TryGetPropertyValue("vertexCount", out var _vertexCount1)) vertexCount = (int)_vertexCount1; else vertexCount = 0;
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount, (flag & 16) != 0);
                    if (nonessential) writer.WriteInt(0);
                    break;
                case AttachmentType.Mesh:
                    if ((flag & 16) != 0)
                    {
                        if (attachment.TryGetPropertyValue("path", out var path)) writer.WriteStringRef((string)path); else writer.WriteStringRef(null);
                    }
                    if ((flag & 32) != 0)
                    {
                        if (attachment.TryGetPropertyValue("color", out var color1)) writer.WriteInt(int.Parse((string)color1, NumberStyles.HexNumber)); else writer.WriteInt(-1);
                    }
                    //if ((flag & 16) != 0 && attachment.TryGetPropertyValue("path", out var path2)) writer.WriteStringRef((string)path2); else writer.WriteStringRef(null);
                    //if ((flag & 32) != 0 && attachment.TryGetPropertyValue("color", out var color2)) writer.WriteInt(int.Parse((string)color2, NumberStyles.HexNumber));
                    if ((flag & 64) != 0)
                    {
                        if (attachment.TryGetPropertyValue("sequence", out var sequence)) WriteSequence(sequence.AsArray());
                        else
                        {
                            writer.WriteVarInt(0);
                            writer.WriteVarInt(1);
                            writer.WriteVarInt(0);
                            writer.WriteVarInt(0);
                        }
                    }
                    if (attachment.TryGetPropertyValue("hull", out var hull)) writer.WriteVarInt((int)hull); else writer.WriteVarInt(0);
                    vertexCount = attachment["uvs"].AsArray().Count >> 1;
                    if (attachment.TryGetPropertyValue("vertices", out var vertices1))
                    {
                        writer.WriteVarInt(vertexCount);
                        WriteVertices(vertices1.AsArray(), vertexCount, (flag & 128) != 0);
                    }
                    else
                        writer.WriteVarInt(0);
                    //WriteFloatArray(attachment["uvs"].AsArray(), vertexCount << 1);
                    WriteFloatArray(attachment["uvs"].AsArray());
                    WriteShortArray(attachment["triangles"].AsArray());
                    if (nonessential)
                    {
                        if (attachment.TryGetPropertyValue("edges", out var edges))
                        {
                            writer.WriteVarInt(edges.AsArray().Count);
                            WriteShortArray(edges.AsArray());
                        }
                        if (attachment.TryGetPropertyValue("width", out var _width)) writer.WriteFloat((float)_width); else writer.WriteFloat(0);
                        if (attachment.TryGetPropertyValue("height", out var _height)) writer.WriteFloat((float)_height); else writer.WriteFloat(0);
                    }
                    break;
                case AttachmentType.Linkedmesh:
                    if ((flag & 16) != 0)
                    {
                        if (attachment.TryGetPropertyValue("path", out var path)) writer.WriteStringRef((string)path); else writer.WriteStringRef(null);
                    }
                    if ((flag & 32) != 0)
                    {
                        if (attachment.TryGetPropertyValue("color", out var color2)) writer.WriteInt(int.Parse((string)color2, NumberStyles.HexNumber)); else writer.WriteInt(-1);
                    }
                    if ((flag & 64) != 0)
                    {
                        if (attachment.TryGetPropertyValue("sequence", out var sequence)) WriteSequence(sequence.AsArray());
                        else
                        {
                            writer.WriteVarInt(0);
                            writer.WriteVarInt(1);
                            writer.WriteVarInt(0);
                            writer.WriteVarInt(0);
                        }
                    }
                    if (attachment.TryGetPropertyValue("skin", out var skin)) writer.WriteVarInt(skin2idx[(string)skin]); else writer.WriteVarInt(0);
                    if (attachment.TryGetPropertyValue("parent", out var parent)) writer.WriteStringRef((string)parent); else writer.WriteStringRef(null);
                    if (nonessential)
                    {
                        if (attachment.TryGetPropertyValue("width", out var width1)) writer.WriteFloat((float)width1); else writer.WriteFloat(0);
                        if (attachment.TryGetPropertyValue("height", out var height1)) writer.WriteFloat((float)height1); else writer.WriteFloat(0);

                    }
                    break;
                case AttachmentType.Path:
                    var vertexCount1 = -1;
                    if (attachment.TryGetPropertyValue("vertexCount", out var _tmp)) vertexCount1 = (int)_tmp;
                    else vertexCount1 = 0;
                    writer.WriteVarInt(vertexCount1);
                    if (attachment.TryGetPropertyValue("vertices", out var vertices2))
                    {
                        WriteVertices(vertices2.AsArray(), vertexCount1, (flag & 64) != 0);
                    }
                    WriteFloatArray(attachment["lengths"].AsArray());
                    if (nonessential) writer.WriteInt(-1);
                    break;
                case AttachmentType.Point:
                    if (attachment.TryGetPropertyValue("rotation", out var rotation3)) writer.WriteFloat((float)rotation3); else writer.WriteFloat(0);
                    if (attachment.TryGetPropertyValue("x", out var x2)) writer.WriteFloat((float)x2); else writer.WriteFloat(0);
                    if (attachment.TryGetPropertyValue("y", out var y2)) writer.WriteFloat((float)y2); else writer.WriteFloat(0);
                    if (nonessential) writer.WriteInt(0);
                    break;
                case AttachmentType.Clipping:
                    writer.WriteVarInt(slot2idx[(string)attachment["end"]]);
                    if (attachment.TryGetPropertyValue("vertexCount", out var _vertexCount4)) vertexCount = (int)_vertexCount4; else vertexCount = 0;
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount, (flag & 16) != 0);
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
                writer.WriteString(name);
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
                writer.WriteVarInt(data.Count);
                if (data.TryGetPropertyValue("slots", out var slots)) WriteSlotTimelines(slots.AsObject()); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("bones", out var bones)) WriteBoneTimelines(bones.AsObject()); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("ik", out var ik)) WriteIKTimelines(ik.AsObject()); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("transform", out var transform)) WriteTransformTimelines(transform.AsObject()); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("path", out var path)) WritePathTimelines(path.AsObject()); else writer.WriteVarInt(0);


                if (data.TryGetPropertyValue("physics", out var physics)) WritePhysicsTimelines(physics.AsObject()); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("attachment", out var attachment)) WriteAttachmentTimelines(attachment.AsObject()); else writer.WriteVarInt(0);
                //if (data.TryGetPropertyValue("deform", out var deform)) WriteDeformTimelines(deform.AsObject()); else writer.WriteVarInt(0);
                //?
                if (data.TryGetPropertyValue("drawOrder", out var drawOrder)) WriteDrawOrderTimelines(drawOrder.AsArray()); else writer.WriteVarInt(0);
                //else
                //    if (data.TryGetPropertyValue("draworder", out var draworder)) WriteDrawOrderTimelines(draworder.AsArray()); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("events", out var events)) WriteEventTimelines(events.AsArray()); else writer.WriteVarInt(0);
            }
        }

        private void WriteSlotTimelines(JsonObject slotTimelines)
        {
            JsonObject tmp;
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
                            if (o.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                            //writer.WriteStringRef((string)o["name"]);
                            if (o.TryGetPropertyValue("name", out var name1)) writer.WriteStringRef((string)name1); else writer.WriteStringRef(null);
                        }
                    }
                    else if (type == "rgba")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_RGBA);
                        writer.WriteVarInt(frames.Count);
                        //writer.WriteVarInt(4);
                        writer.WriteVarInt(GetBezierCount(frames) * 4);
                        tmp = frames[0].AsObject();
                        if (tmp.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                        if (tmp.TryGetPropertyValue("color", out var color))
                        {
                            writer.WriteByte(byte.Parse(((string)color).Substring(0, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)color).Substring(2, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)color).Substring(4, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)color).Substring(6, 2), NumberStyles.HexNumber));
                        }
                        for (int i = 1; i < frames.Count; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            if (o.TryGetPropertyValue("time", out var time1)) writer.WriteFloat((float)time1); else writer.WriteFloat(0);
                            if (o.TryGetPropertyValue("color", out var color1))
                            {
                                writer.WriteByte(byte.Parse(((string)color1).Substring(0, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)color1).Substring(2, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)color1).Substring(4, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)color1).Substring(6, 2), NumberStyles.HexNumber));
                            }
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (type == "rgb")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_RGB);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 3);
                        tmp = frames[0].AsObject();
                        if (tmp.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                        if (tmp.TryGetPropertyValue("color", out var color))
                        {
                            writer.WriteByte(byte.Parse(((string)color).Substring(0, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)color).Substring(2, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)color).Substring(4, 2), NumberStyles.HexNumber));
                        }
                        for (int i = 1; i < frames.Count; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            if (o.TryGetPropertyValue("time", out var time1)) writer.WriteFloat((float)time1); else writer.WriteFloat(0);
                            if (o.TryGetPropertyValue("color", out var color1))
                            {
                                writer.WriteByte(byte.Parse(((string)color1).Substring(0, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)color1).Substring(2, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)color1).Substring(4, 2), NumberStyles.HexNumber));
                            }
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (type == "rgba2")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_RGBA2);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 7);
                        tmp = frames[0].AsObject();
                        if (tmp.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                        if (tmp.TryGetPropertyValue("light", out var light))
                        {
                            writer.WriteByte(byte.Parse(((string)light).Substring(0, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)light).Substring(2, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)light).Substring(4, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)light).Substring(6, 2), NumberStyles.HexNumber));
                        }
                        if (tmp.TryGetPropertyValue("dark", out var dark))
                        {
                            writer.WriteByte(byte.Parse(((string)dark).Substring(0, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)dark).Substring(2, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)dark).Substring(4, 2), NumberStyles.HexNumber));
                        }
                        for (int i = 1; i < frames.Count; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            if (o.TryGetPropertyValue("time", out var time1)) writer.WriteFloat((float)time1); else writer.WriteFloat(0);
                            if (tmp.TryGetPropertyValue("light", out var light1))
                            {
                                writer.WriteByte(byte.Parse(((string)light1).Substring(0, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)light1).Substring(2, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)light1).Substring(4, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)light1).Substring(6, 2), NumberStyles.HexNumber));
                            }
                            if (tmp.TryGetPropertyValue("dark", out var dark1))
                            {
                                writer.WriteByte(byte.Parse(((string)dark1).Substring(0, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)dark1).Substring(2, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)dark1).Substring(4, 2), NumberStyles.HexNumber));
                            }
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (type == "rgb2")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_RGB2);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 6);
                        tmp = frames[0].AsObject();
                        if (tmp.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                        if (tmp.TryGetPropertyValue("light", out var light))
                        {
                            writer.WriteByte(byte.Parse(((string)light).Substring(0, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)light).Substring(2, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)light).Substring(4, 2), NumberStyles.HexNumber));
                        }
                        if (tmp.TryGetPropertyValue("dark", out var dark))
                        {
                            writer.WriteByte(byte.Parse(((string)dark).Substring(0, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)dark).Substring(2, 2), NumberStyles.HexNumber));
                            writer.WriteByte(byte.Parse(((string)dark).Substring(4, 2), NumberStyles.HexNumber));
                        }
                        for (int i = 1; i < frames.Count; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            if (o.TryGetPropertyValue("time", out var time1)) writer.WriteFloat((float)time1); else writer.WriteFloat(0);
                            if (tmp.TryGetPropertyValue("light", out var light1))
                            {
                                writer.WriteByte(byte.Parse(((string)light1).Substring(0, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)light1).Substring(2, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)light1).Substring(4, 2), NumberStyles.HexNumber));
                            }
                            if (tmp.TryGetPropertyValue("dark", out var dark1))
                            {
                                writer.WriteByte(byte.Parse(((string)dark1).Substring(0, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)dark1).Substring(2, 2), NumberStyles.HexNumber));
                                writer.WriteByte(byte.Parse(((string)dark1).Substring(4, 2), NumberStyles.HexNumber));
                            }
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (type == "alpha")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_ALPHA);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));
                        tmp = frames[0].AsObject();
                        if (tmp.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                        if (tmp.TryGetPropertyValue("value", out var value)) writer.WriteFloat((float)value); else writer.WriteFloat(1);
                        for (int i = 1; i < frames.Count; i++)
                        {
                            JsonObject o = frames[i].AsObject();
                            if (o.TryGetPropertyValue("time", out var time1)) writer.WriteFloat((float)time1); else writer.WriteFloat(0);
                            if (o.TryGetPropertyValue("value", out var value1)) writer.WriteFloat((float)value1); else writer.WriteFloat(1);
                            WriteCurve(frames[i - 1].AsObject());
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
                    if (type == "inherit")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_INHERIT);
                        writer.WriteVarInt(frames.Count);
                        foreach (JsonObject frame in frames)
                        {
                            if (frame.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0f);
                            if (frame.TryGetPropertyValue("inherit", out var inherit)) writer.WriteByte((byte)Enum.Parse<Inherit>((string)inherit, true)); else writer.WriteByte((byte)Inherit.Normal);
                        }
                        continue;
                    }
                    //writer.WriteVarInt(2);//bezierCount
                    //if (type == "translate" || type == "scale" || type == "shear") writer.WriteVarInt(3); else writer.WriteVarInt(2);
                    if (type == "rotate")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_ROTATE);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));
                        WriteTimeline2(frames, 0);
                    }
                    else if (type == "translate")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_TRANSLATE);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 2);
                        JsonObject o = frames[0].AsObject();
                        if (o.TryGetPropertyValue("time", out var _time)) writer.WriteFloat((float)_time); else writer.WriteFloat(0);
                        if (o.TryGetPropertyValue("x", out var _x)) writer.WriteFloat((float)_x); else writer.WriteFloat(0);
                        if (o.TryGetPropertyValue("y", out var _y)) writer.WriteFloat((float)_y); else writer.WriteFloat(0);
                        for (int i = 1; i < frames.Count; i++)
                        {
                            o = frames[i].AsObject();
                            if (o.TryGetPropertyValue("time", out var _time1)) writer.WriteFloat((float)_time1); else writer.WriteFloat(0);
                            if (o.TryGetPropertyValue("x", out var _x1)) writer.WriteFloat((float)_x1); else writer.WriteFloat(0);
                            if (o.TryGetPropertyValue("y", out var _y1)) writer.WriteFloat((float)_y1); else writer.WriteFloat(0);
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (type == "translatex")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_TRANSLATEX);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));
                        WriteTimeline2(frames, 0);
                    }
                    else if (type == "translatey")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_TRANSLATEY);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));
                        WriteTimeline2(frames, 0);
                    }

                    else if (type == "scale")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SCALE);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 2);
                        JsonObject o = frames[0].AsObject();
                        if (o.TryGetPropertyValue("time", out var _time)) writer.WriteFloat((float)_time); else writer.WriteFloat(0);
                        if (o.TryGetPropertyValue("x", out var _x)) writer.WriteFloat((float)_x); else writer.WriteFloat(1);
                        if (o.TryGetPropertyValue("y", out var _y)) writer.WriteFloat((float)_y); else writer.WriteFloat(1);
                        for (int i = 1; i < frames.Count; i++)
                        {
                            o = frames[i].AsObject();
                            if (o.TryGetPropertyValue("time", out var _time1)) writer.WriteFloat((float)_time1); else writer.WriteFloat(0);
                            if (o.TryGetPropertyValue("x", out var _x1)) writer.WriteFloat((float)_x1); else writer.WriteFloat(1);
                            if (o.TryGetPropertyValue("y", out var _y1)) writer.WriteFloat((float)_y1); else writer.WriteFloat(1);
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (type == "scalex")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SCALEX);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));
                        WriteTimeline2(frames, 1);
                    }
                    else if (type == "scaley")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SCALEY);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));
                        WriteTimeline2(frames, 1);
                    }
                    else if (type == "shear")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SHEAR);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 2);
                        JsonObject o = frames[0].AsObject();
                        if (o.TryGetPropertyValue("time", out var _time)) writer.WriteFloat((float)_time); else writer.WriteFloat(0);
                        if (o.TryGetPropertyValue("x", out var _x)) writer.WriteFloat((float)_x); else writer.WriteFloat(0);
                        if (o.TryGetPropertyValue("y", out var _y)) writer.WriteFloat((float)_y); else writer.WriteFloat(0);
                        for (int i = 1; i < frames.Count; i++)
                        {
                            o = frames[i].AsObject();
                            if (o.TryGetPropertyValue("time", out var _time1)) writer.WriteFloat((float)_time1); else writer.WriteFloat(0);
                            if (o.TryGetPropertyValue("x", out var _x1)) writer.WriteFloat((float)_x1); else writer.WriteFloat(0);
                            if (o.TryGetPropertyValue("y", out var _y1)) writer.WriteFloat((float)_y1); else writer.WriteFloat(0);
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (type == "shearx")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SHEARX);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));
                        WriteTimeline2(frames, 0);
                    }
                    else if (type == "sheary")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SHEARY);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));
                        WriteTimeline2(frames, 0);
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
                writer.WriteVarInt(GetBezierCount(frames) * 2);

                JsonObject o = frames[0].AsObject();

                int flag = 0;
                if (o.TryGetPropertyValue("mix", out var mix))
                {
                    if (Math.Abs((float)mix) > 1e-5)//mix != 0
                    {
                        flag |= 1;
                        if (Math.Abs((float)mix - 1f) > 1e-5)//mix != 1
                            flag |= 2;
                    }
                }
                else flag |= 1;//默认是1
                if (o.TryGetPropertyValue("softness", out var softness))
                {
                    if (Math.Abs((float)softness) > 1e-5)
                        flag |= 4;
                }
                if (o.TryGetPropertyValue("bendPositive", out var bendPositive) && (bool)bendPositive) flag |= 8;
                else flag |= 8;//默认是True
                if (o.TryGetPropertyValue("compress", out var compress) && (bool)compress) flag |= 16;
                if (o.TryGetPropertyValue("stretch", out var stretch) && (bool)stretch) flag |= 32;

                writer.Write(flag);
                if (o.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                if ((flag & 1) != 0 && (flag & 2) != 0)
                {
                    if (o.TryGetPropertyValue("mix", out var mix1)) writer.WriteFloat((float)mix1); else writer.WriteFloat(1);
                }
                if ((flag & 4) != 0)
                {
                    if (o.TryGetPropertyValue("softness", out var softness1)) writer.WriteFloat((float)softness1); else writer.WriteFloat(0);
                }


                for (int i = 1; i < frames.Count; i++)
                {
                    o = frames[i].AsObject();
                    flag = 0;
                    if (o.TryGetPropertyValue("mix", out var mix1))
                    {
                        if (Math.Abs((float)mix1) > 1e-5)
                        {
                            flag |= 1;
                            if (Math.Abs((float)mix1 - 1f) > 1e-5)
                                flag |= 2;
                        }
                    }
                    else flag |= 1;
                    if (o.TryGetPropertyValue("softness", out var softness1))
                    {
                        if (Math.Abs((float)softness1) > 1e-5)
                            flag |= 4;
                    }
                    if (o.TryGetPropertyValue("bendPositive", out var bendPositive1) && (bool)bendPositive1) flag |= 8;
                    else flag |= 8;//默认是True
                    if (o.TryGetPropertyValue("compress", out var compress1) && (bool)compress1) flag |= 16;
                    if (o.TryGetPropertyValue("stretch", out var stretch1) && (bool)stretch1) flag |= 32;
                    //当前元素的curve由后一个元素的flag控制
                    JsonObject o2 = frames[i - 1].AsObject();
                    if (o2.TryGetPropertyValue("curve", out var curve))
                    {
                        if (curve.GetValueKind() == JsonValueKind.String) flag |= 64;
                        else if (curve.GetValueKind() == JsonValueKind.Array) flag |= 128;
                    }
                    writer.Write(flag);
                    if (o.TryGetPropertyValue("time", out var time1)) writer.WriteFloat((float)time1); else writer.WriteFloat(0);
                    if ((flag & 1) != 0 && (flag & 2) != 0)
                    {
                        if (o.TryGetPropertyValue("mix", out var mix2)) writer.WriteFloat((float)mix2); else writer.WriteFloat(1);
                    }
                    if ((flag & 4) != 0)
                    {
                        if (o.TryGetPropertyValue("softness", out var softness2)) writer.WriteFloat((float)softness2); else writer.WriteFloat(0);
                    }
                    WriteCurveWithoutType(frames[i - 1].AsObject());
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
                writer.WriteVarInt(GetBezierCount(frames) * 6);

                JsonObject o = frames[0].AsObject();
                if (o.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                if (o.TryGetPropertyValue("mixRotate", out var mixRotate)) writer.WriteFloat((float)mixRotate); else writer.WriteFloat(1);
                if (o.TryGetPropertyValue("mixX", out var mixX)) writer.WriteFloat((float)mixX); else writer.WriteFloat(1);
                if (o.TryGetPropertyValue("mixY", out var mixY)) writer.WriteFloat((float)mixY);
                else
                {
                    if (o.TryGetPropertyValue("mixX", out var mixX1)) writer.WriteFloat((float)mixX1); else writer.WriteFloat(1);
                }
                if (o.TryGetPropertyValue("mixScaleX", out var mixScaleX)) writer.WriteFloat((float)mixScaleX); else writer.WriteFloat(1);
                if (o.TryGetPropertyValue("mixScaleY", out var mixScaleY)) writer.WriteFloat((float)mixScaleY);
                else
                {
                    if (o.TryGetPropertyValue("mixScaleX", out var mixScaleX1)) writer.WriteFloat((float)mixScaleX1); else writer.WriteFloat(1);
                }
                if (o.TryGetPropertyValue("mixShearY", out var mixShearY)) writer.WriteFloat((float)mixShearY); else writer.WriteFloat(1);
                for (int i = 1; i < frames.Count; i++)
                {
                    o = frames[i].AsObject();
                    if (o.TryGetPropertyValue("time", out var time1)) writer.WriteFloat((float)time1); else writer.WriteFloat(0);
                    if (o.TryGetPropertyValue("mixRotate", out var mixRotate1)) writer.WriteFloat((float)mixRotate1); else writer.WriteFloat(1);
                    if (o.TryGetPropertyValue("mixX", out var mixX1)) writer.WriteFloat((float)mixX1); else writer.WriteFloat(1);
                    if (o.TryGetPropertyValue("mixY", out var mixY2)) writer.WriteFloat((float)mixY2);
                    else
                    {
                        if (o.TryGetPropertyValue("mixX", out var mixX3)) writer.WriteFloat((float)mixX3); else writer.WriteFloat(1);
                    }
                    if (o.TryGetPropertyValue("mixScaleX", out var mixScaleX1)) writer.WriteFloat((float)mixScaleX1); else writer.WriteFloat(1);
                    if (o.TryGetPropertyValue("mixScaleY", out var mixScaleY1)) writer.WriteFloat((float)mixScaleY1);
                    else
                    {
                        if (o.TryGetPropertyValue("mixScaleX", out var mixScaleX2)) writer.WriteFloat((float)mixScaleX2); else writer.WriteFloat(1);
                    }
                    if (o.TryGetPropertyValue("mixShearY", out var mixShearY1)) writer.WriteFloat((float)mixShearY1); else writer.WriteFloat(1);
                    WriteCurve(frames[i - 1].AsObject());
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
                        writer.WriteVarInt(GetBezierCount(frames));
                        WriteTimeline2(frames, 0);
                    }
                    else if (type == "spacing")
                    {
                        writer.WriteByte(SkeletonBinary.PATH_SPACING);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));
                        WriteTimeline2(frames, 0);
                    }
                    else if (type == "mix")
                    {
                        writer.WriteByte(SkeletonBinary.PATH_MIX);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 3);

                        JsonObject o = frames[0].AsObject();
                        if (o.TryGetPropertyValue("time", out var _time)) writer.WriteFloat((float)_time); else writer.WriteFloat(0);
                        if (o.TryGetPropertyValue("mixRotate", out var _mixRotate)) writer.WriteFloat((float)_mixRotate); else writer.WriteFloat(1);
                        if (o.TryGetPropertyValue("mixX", out var _mixX)) writer.WriteFloat((float)_mixX); else writer.WriteFloat(1);
                        if (o.TryGetPropertyValue("mixY", out var _mixY)) writer.WriteFloat((float)_mixY);
                        else
                        {
                            if (o.TryGetPropertyValue("mixX", out var mixX1)) writer.WriteFloat((float)mixX1); else writer.WriteFloat(1);
                        }
                        for (int i = 1; i < frames.Count; i++)
                        {
                            o = frames[i].AsObject();
                            if (o.TryGetPropertyValue("time", out var _time1)) writer.WriteFloat((float)_time1); else writer.WriteFloat(0);
                            if (o.TryGetPropertyValue("mixRotate", out var _mixRotate1)) writer.WriteFloat((float)_mixRotate1); else writer.WriteFloat(1);
                            if (o.TryGetPropertyValue("mixX", out var _mixX1)) writer.WriteFloat((float)_mixX1); else writer.WriteFloat(1);
                            if (o.TryGetPropertyValue("mixY", out var _mixY1)) writer.WriteFloat((float)_mixY1);
                            else
                            {
                                if (o.TryGetPropertyValue("mixX", out var mixX1)) writer.WriteFloat((float)mixX1); else writer.WriteFloat(1);
                            }
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                }
            }
        }

        private void WritePhysicsTimelines(JsonObject physicsTimelines)
        {
            writer.WriteVarInt(physicsTimelines.Count);
            foreach (var (name, _timeline) in physicsTimelines)
            {
                JsonObject timeline = _timeline.AsObject();
                if (!string.IsNullOrEmpty(name)) writer.WriteVarInt(physics2idx[name] + 1);
                else writer.WriteVarInt(-1);//此处官方给的example中有一个样例的name是空的，在celestial-circus-pro.json的第3266行.

                writer.WriteVarInt(timeline.Count);
                foreach (var (type, _frames) in timeline)
                {
                    JsonArray frames = _frames.AsArray();
                    if (type == "reset")
                    {
                        writer.WriteByte(SkeletonBinary.PHYSICS_RESET);
                        writer.WriteVarInt(frames.Count);
                        foreach (JsonObject frame in frames)
                        {
                            if (frame.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                        }
                        continue;
                    }
                    switch (type)
                    {
                        case "inertia":
                            writer.WriteByte(SkeletonBinary.PHYSICS_INERTIA);
                            break;
                        case "strength":
                            writer.WriteByte(SkeletonBinary.PHYSICS_STRENGTH);
                            break;
                        case "damping":
                            writer.WriteByte(SkeletonBinary.PHYSICS_DAMPING);
                            break;
                        case "mass":
                            writer.WriteByte(SkeletonBinary.PHYSICS_MASS);
                            break;
                        case "wind":
                            writer.WriteByte(SkeletonBinary.PHYSICS_WIND);
                            break;
                        case "gravity":
                            writer.WriteByte(SkeletonBinary.PHYSICS_GRAVITY);
                            break;
                        case "mix":
                            writer.WriteByte(SkeletonBinary.PHYSICS_MIX);
                            break;
                    }
                    writer.WriteVarInt(frames.Count);
                    writer.WriteVarInt(GetBezierCount(frames));
                    WriteTimeline2(frames, 0);
                }
            }
        }

        private void WriteAttachmentTimelines(JsonObject attachmentTimelines)
        {
            writer.WriteVarInt(attachmentTimelines.Count);//skinCount
            foreach (var (skinName, _skin) in attachmentTimelines)
            {
                JsonObject skin = _skin.AsObject();
                writer.WriteVarInt(skin2idx[skinName]);
                writer.WriteVarInt(skin.Count);//slotCount
                foreach (var (slotName, _slot) in skin)
                {
                    JsonObject slot = _slot.AsObject();
                    writer.WriteVarInt(slot2idx[slotName]);
                    writer.WriteVarInt(slot.Count);//attachmentCount
                    foreach (var (attachmentName, _attachment) in slot)
                    {
                        //JsonArray frames = _attachment.AsArray();
                        writer.WriteStringRef(attachmentName);
                        JsonObject o = [];
                        JsonArray frames = [];
                        foreach (var (key, value) in _attachment.AsObject())
                        {
                            switch ((string)(key))
                            {
                                case "deform":
                                    frames = value.AsArray();
                                    writer.WriteByte(SkeletonBinary.ATTACHMENT_DEFORM);
                                    writer.WriteVarInt(frames.Count);
                                    writer.WriteVarInt(GetBezierCount(frames));
                                    o = frames[0].AsObject();
                                    if (o.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                                    if (o.TryGetPropertyValue("vertices", out var _vertices))
                                    {
                                        JsonArray vertices = _vertices.AsArray();
                                        if (vertices.Count > 0)
                                        {
                                            int offset = 0;
                                            if (o.TryGetPropertyValue("offset", out var offset1)) offset = (int)offset1;
                                            writer.WriteVarInt(vertices.Count);
                                            writer.WriteVarInt(offset);
                                            foreach (var vertex in vertices)
                                            {
                                                writer.WriteFloat((float)vertex);
                                            }
                                        }
                                        else writer.WriteVarInt(0);
                                    }
                                    else writer.WriteVarInt(0);
                                    for (int i = 1; i < frames.Count; i++)
                                    {
                                        o = frames[i].AsObject();
                                        if (o.TryGetPropertyValue("time", out var time1)) writer.WriteFloat((float)time1); else writer.WriteFloat(0);
                                        WriteCurve(frames[i - 1].AsObject());
                                        if (o.TryGetPropertyValue("vertices", out var _vertices1))
                                        {
                                            JsonArray vertices = _vertices1.AsArray();
                                            if (vertices.Count > 0)
                                            {
                                                int offset = 0;
                                                if (o.TryGetPropertyValue("offset", out var offset1)) offset = (int)offset1;
                                                writer.WriteVarInt(vertices.Count);
                                                writer.WriteVarInt(offset);
                                                foreach (var vertex in vertices)
                                                {
                                                    writer.WriteFloat((float)vertex);
                                                }
                                            }
                                            else writer.WriteVarInt(0);
                                        }
                                        else writer.WriteVarInt(0);
                                        //WriteCurve(frames[i - 1].AsObject());
                                    }
                                    break;

                                case "sequence":
                                    frames = value.AsArray();
                                    writer.WriteByte(SkeletonBinary.ATTACHMENT_SEQUENCE);
                                    writer.WriteVarInt(frames.Count);
                                    float lastDelay = 0;
                                    foreach (var frame in frames)
                                    {
                                        int mode = (int)SequenceMode.Hold;
                                        int index = 0;
                                        o = frame.AsObject();
                                        if (o.TryGetPropertyValue("time", out var time1)) writer.WriteFloat((float)time1); else writer.WriteFloat(0);
                                        if (o.TryGetPropertyValue("mode", out var mode1)) mode = (int)Enum.Parse<SequenceMode>((string)mode1, true);
                                        if (o.TryGetPropertyValue("index", out var index1)) index = (int)index1;
                                        writer.WriteInt(((index << 4) | (mode & 0xF)));
                                        if (o.TryGetPropertyValue("delay", out var delay))
                                        {
                                            lastDelay = (float)delay;
                                            writer.WriteFloat((float)delay);
                                        }
                                        else writer.WriteFloat(lastDelay);
                                    }

                                    break;
                            }
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
                if (data.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("offsets", out var _offsets))
                {
                    JsonArray offsets = _offsets.AsArray();
                    writer.WriteVarInt(offsets.Count);
                    foreach (JsonObject o in offsets)
                    {
                        writer.WriteVarInt(slot2idx[(string)o["slot"]]);
                        writer.WriteVarInt((int)o["offset"]);
                    }
                }
                else writer.WriteVarInt(0);
            }
        }

        private void WriteEventTimelines(JsonArray eventTimelines)
        {
            JsonObject events = root["events"].AsObject();

            writer.WriteVarInt(eventTimelines.Count);
            foreach (JsonObject data in eventTimelines)
            {
                JsonObject eventData = events[(string)data["name"]].AsObject();
                if (data.TryGetPropertyValue("time", out var time)) writer.WriteFloat((float)time); else writer.WriteFloat(0);
                writer.WriteVarInt(event2idx[(string)data["name"]]);
                if (data.TryGetPropertyValue("int", out var @int)) writer.WriteVarInt((int)@int);
                else
                    if (eventData.TryGetPropertyValue("int", out var @int2)) writer.WriteVarInt((int)@int2); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("float", out var @float)) writer.WriteFloat((float)@float);
                else
                    if (eventData.TryGetPropertyValue("float", out var @float2)) writer.WriteFloat((float)@float2); else writer.WriteFloat(0);
                if (data.TryGetPropertyValue("string", out var @string))
                {
                    writer.WriteString((string)@string);
                }
                else
                {
                    writer.WriteString((string)eventData["string"]);
                }

                if (eventData.ContainsKey("audio"))
                {
                    if (data.TryGetPropertyValue("volume", out var volume)) writer.WriteFloat((float)volume);
                    else
                        if (eventData.TryGetPropertyValue("volume", out var volume2)) writer.WriteFloat((float)volume2); else writer.WriteFloat(1);
                    if (data.TryGetPropertyValue("balance", out var balance)) writer.WriteFloat((float)balance);
                    else
                        if (eventData.TryGetPropertyValue("balance", out var balance2)) writer.WriteFloat((float)balance2); else writer.WriteFloat(0);
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
        public void WriteFloatArray(JsonArray array)
        {
            foreach (var i in array)
            {
                writer.WriteFloat((float)i);
            }
        }

        public void WriteShortArray(JsonArray array)
        {
            foreach (var i in array)
            {
                writer.WriteVarInt((int)i);
            }
        }

        private void WriteVertices(JsonArray vertices, int vertexCount, bool flag)
        {

            bool hasWeight = vertices.Count != (vertexCount << 1);
            //writer.WriteBoolean(hasWeight);
            if (!flag)
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

        private void WriteTimeline2(JsonArray frames, float defaultValue)
        {
            JsonObject o = frames[0].AsObject();
            if (o.TryGetPropertyValue("time", out var _time)) writer.WriteFloat((float)_time); else writer.WriteFloat(0);
            if (o.TryGetPropertyValue("value", out var _value)) writer.WriteFloat((float)_value); else writer.WriteFloat(defaultValue);
            for (int i = 1; i < frames.Count; i++)
            {
                o = frames[i].AsObject();
                if (o.TryGetPropertyValue("time", out var _time1)) writer.WriteFloat((float)_time1); else writer.WriteFloat(0);
                if (o.TryGetPropertyValue("value", out var _value1)) writer.WriteFloat((float)_value1); else writer.WriteFloat(defaultValue);
                WriteCurve(frames[i - 1].AsObject());
            }
        }
        private void WriteCurve(JsonObject obj)
        {
            if (obj.TryGetPropertyValue("curve", out var curve))
            {
                if (curve.GetValueKind() == JsonValueKind.String)
                {
                    writer.WriteByte(SkeletonBinary.CURVE_STEPPED);
                }
                else
                {
                    writer.WriteByte(SkeletonBinary.CURVE_BEZIER);
                    foreach (var c in curve.AsArray())
                    {
                        writer.WriteFloat((float)c);
                    }
                }
            }
            else
            {
                writer.WriteByte(SkeletonBinary.CURVE_LINEAR);
            }
        }
        private void WriteCurveWithoutType(JsonObject obj)
        {
            if (obj.TryGetPropertyValue("curve", out var curve) && curve.GetValueKind() == JsonValueKind.Array)
            {
                foreach (var c in curve.AsArray())
                {
                    writer.WriteFloat((float)c);
                }
            }
        }

        private int GetBezierCount(JsonArray frames)
        {
            int bezierCount = 0;
            foreach (JsonObject frame in frames)
            {
                if (frame.TryGetPropertyValue("curve", out var curveValue) && curveValue is JsonArray)
                    bezierCount++;
            }
            return bezierCount;
        }
        private void WriteSequence(JsonArray sequence)
        {
            if (sequence.Contains("count")) writer.WriteVarInt((int)sequence["count"]); else writer.WriteVarInt(0);
            if (sequence.Contains("start")) writer.WriteVarInt((int)sequence["start"]); else writer.WriteVarInt(0);
            if (sequence.Contains("digits")) writer.WriteVarInt((int)sequence["digits"]); else writer.WriteVarInt(0);
            if (sequence.Contains("setup")) writer.WriteVarInt((int)sequence["setup"]); else writer.WriteVarInt(0);
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
                SpineVersion.V42 => root.DeepClone().AsObject(),
                _ => throw new NotImplementedException(),
            };
            return root;
        }



    }
}
