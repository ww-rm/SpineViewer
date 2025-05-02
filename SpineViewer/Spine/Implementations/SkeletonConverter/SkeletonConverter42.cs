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
                if ((flags & 128) != 0) data["mass"] = 1.0f / reader.ReadFloat(); //在binary中存储的是质量的倒数，在json中存储的是质量
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

            //处理linkedmesh的问题，后面有相关代码和注释
            foreach (JsonObject skin in skins)
            {
                foreach (var (_, slot) in skin["attachments"].AsObject())
                {
                    foreach (var (_, attachment) in slot.AsObject())
                    {
                        if ((string)attachment["type"] == "linkedMesh")
                        {
                            attachment["skin"] = (string)skins[(int)attachment["skin"]]["name"];
                        }
                    }
                }
            }
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

            var type = (AttachmentType)(flags & 0x7);

            if ((flags & 8) != 0) attachment["name"] = reader.ReadStringRef();
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
                    //attachment["skin"] = (string)skins[reader.ReadVarInt()]["name"];
                    //还是那个mix-and-match，4.2版本的mix-and-match，出现了读取错误,后面写的时候也一样，linkedmesh的skin为后面还未访问的skin
                    attachment["skin"] = reader.ReadVarInt();
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

                var index = reader.ReadVarInt();
                physicsTimelines[index > 0 ? (string)physics[index - 1]["name"] : ""] = timeline;
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
            JsonArray skins = root["skins"].AsArray();
            JsonArray slots = root["slots"].AsArray();
            JsonObject attachmentTimelines = [];

            for (int i = 0, skinCount = reader.ReadVarInt(); i < skinCount; i++)
            {
                JsonObject skinValue = [];
                attachmentTimelines[(string)skins[reader.ReadVarInt()]["name"]] = skinValue;
                for (int ii = 0, slotCount = reader.ReadVarInt(); ii < slotCount; ii++)
                {
                    JsonObject slotValue = [];
                    skinValue[(string)slots[reader.ReadVarInt()]["name"]] = slotValue;
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

        private JsonArray ReadShortArray(int n)
        {
            JsonArray array = [];
            for (int i = 0; i < n; i++)
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

        private void ReadCurve(JsonObject frame, int n)
        {
            var type = reader.ReadUByte();
            switch (type)
            {
                case SkeletonBinary.CURVE_LINEAR:
                    break;
                case SkeletonBinary.CURVE_STEPPED:
                    frame["curve"] = "stepped";
                    break;
                case SkeletonBinary.CURVE_BEZIER:
                    frame["curve"] = ReadFloatArray(n * 4);
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
            physics2idx.Clear();
            skin2idx.Clear();
            event2idx.Clear();
        }


        private void WriteSkeleton()
        {
            JsonObject skeleton = root["skeleton"].AsObject();
            writer.WriteLong(long.Parse(Convert.ToHexString(Convert.FromBase64String(skeleton["hash"] + "=")), NumberStyles.HexNumber));
            writer.WriteString((string)skeleton["spine"]);
            writer.WriteFloat((float)(skeleton["x"] ?? 0f));
            writer.WriteFloat((float)(skeleton["y"] ?? 0f));
            writer.WriteFloat((float)(skeleton["width"] ?? 0f));
            writer.WriteFloat((float)(skeleton["height"] ?? 0f));
            writer.WriteFloat((float)(skeleton["referenceScale"] ?? 100f));
            writer.WriteBoolean(nonessential);
            if (nonessential)
            {
                writer.WriteFloat((float)(skeleton["fps"] ?? 30f));
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
                writer.WriteFloat((float)(data["rotation"] ?? 0f));
                writer.WriteFloat((float)(data["x"] ?? 0f));
                writer.WriteFloat((float)(data["y"] ?? 0f));
                writer.WriteFloat((float)(data["scaleX"] ?? 1f));
                writer.WriteFloat((float)(data["scaleY"] ?? 1f));
                writer.WriteFloat((float)(data["shearX"] ?? 0f));
                writer.WriteFloat((float)(data["shearY"] ?? 0f));
                writer.WriteFloat((float)(data["length"] ?? 0f));
                writer.WriteVarInt((int)Enum.Parse<Inherit>((string)(data["inherit"] ?? "normal"), true));
                writer.WriteBoolean((bool)(data["skin"] ?? false));
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
                writer.WriteInt(int.Parse((string)(data["color"] ?? "ffffffff"), NumberStyles.HexNumber));
                writer.WriteInt(int.Parse((string)(data["dark"] ?? "ffffff"), NumberStyles.HexNumber));
                writer.WriteStringRef((string)data["attachment"]);
                writer.WriteVarInt((int)Enum.Parse<BlendMode>((string)(data["blend"] ?? "normal"), true));
                if (nonessential) writer.WriteBoolean(false);
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
                if (data["bones"] is JsonArray bones) WriteNames(bone2idx, bones); else writer.WriteVarInt(0);
                writer.WriteVarInt(bone2idx[(string)data["target"]]);

                int flags = 0;
                if (data["skin"] is JsonValue skin && (bool)skin) flags |= 1;
                if (data["bendPositive"] is not JsonValue bendPositive || (bool)bendPositive) flags |= 2;
                if (data["compress"] is JsonValue compress && (bool)compress) flags |= 4;
                if (data["stretch"] is JsonValue stretch && (bool)stretch) flags |= 8;
                if (data["uniform"] is JsonValue uniform && (bool)uniform) flags |= 16;
                if (data["mix"] is JsonValue) flags |= 32 + 64; else flags |= 32;
                if (data["softness"] is JsonValue) flags |= 128;
                writer.WriteByte((byte)flags);
                if ((flags & 64) != 0) writer.WriteFloat((float)data["mix"]);
                if ((flags & 128) != 0) writer.WriteFloat((float)data["softness"]);
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
                if (data["bones"] is JsonArray bones) WriteNames(bone2idx, bones); else writer.WriteVarInt(0);
                writer.WriteVarInt(bone2idx[(string)data["target"]]);

                int flags = 0;
                if (data["skin"] is JsonValue skin && (bool)skin) flags |= 1;
                if (data["local"] is JsonValue local && (bool)local) flags |= 2;
                if (data["relative"] is JsonValue relative && (bool)relative) flags |= 4;
                if (data["rotation"] is JsonValue) flags |= 8;
                if (data["x"] is JsonValue) flags |= 16;
                if (data["y"] is JsonValue) flags |= 32;
                if (data["scaleX"] is JsonValue) flags |= 64;
                if (data["scaleY"] is JsonValue) flags |= 128;
                writer.WriteByte((byte)flags);
                if ((flags & 8) != 0) writer.WriteFloat((float)data["rotation"]);
                if ((flags & 16) != 0) writer.WriteFloat((float)data["x"]);
                if ((flags & 32) != 0) writer.WriteFloat((float)data["y"]);
                if ((flags & 64) != 0) writer.WriteFloat((float)data["scaleX"]);
                if ((flags & 128) != 0) writer.WriteFloat((float)data["scaleY"]);

                flags = 0;
                if (data["shearY"] is JsonValue) flags |= 1;
                if (data["mixRotate"] is JsonValue) flags |= 2;
                if (data["mixX"] is JsonValue) flags |= 4;
                if (data["mixY"] is JsonValue) flags |= 8;
                if (data["mixScaleX"] is JsonValue) flags |= 16;
                if (data["mixScaleY"] is JsonValue) flags |= 32;
                if (data["mixShearY"] is JsonValue) flags |= 64;
                writer.WriteByte((byte)flags);
                if ((flags & 1) != 0) writer.WriteFloat((float)data["shearY"]);
                if ((flags & 2) != 0) writer.WriteFloat((float)data["mixRotate"]);
                if ((flags & 4) != 0) writer.WriteFloat((float)data["mixX"]);
                if ((flags & 8) != 0) writer.WriteFloat((float)data["mixY"]);
                if ((flags & 16) != 0) writer.WriteFloat((float)data["mixScaleX"]);
                if ((flags & 32) != 0) writer.WriteFloat((float)data["mixScaleY"]);
                if ((flags & 64) != 0) writer.WriteFloat((float)data["mixShearY"]);
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
                if (data["bones"] is JsonArray bones) WriteNames(bone2idx, bones); else writer.WriteVarInt(0);
                writer.WriteVarInt(slot2idx[(string)data["target"]]);

                int flags = 0;
                if (data["positionMode"] is JsonValue positionMode) flags |= ((int)Enum.Parse<PositionMode>((string)positionMode, true)) & 0b1;
                else flags |= (int)PositionMode.Percent & 0b1;
                if (data["spacingMode"] is JsonValue spacingMode) flags |= ((int)Enum.Parse<SpacingMode>((string)spacingMode, true) & 0b11) << 1;
                else flags |= ((int)SpacingMode.Length & 0b11) << 1;
                if (data["rotateMode"] is JsonValue rotateMode) flags |= ((int)Enum.Parse<RotateMode>((string)rotateMode, true) & 0b11) << 3;
                else flags |= ((int)RotateMode.Tangent & 0b11) << 3;
                if (data["rotation"] is JsonValue) flags |= 128;
                writer.WriteByte((byte)flags);
                if ((flags & 128) != 0) writer.WriteFloat((float)data["rotation"]);

                writer.WriteFloat((float)(data["position"] ?? 0f));
                writer.WriteFloat((float)(data["spacing"] ?? 0f));
                writer.WriteFloat((float)(data["mixRotate"] ?? 1f));
                writer.WriteFloat((float)(data["mixX"] ?? 1f));
                writer.WriteFloat((float)(data["mixY"] ?? data["mixX"] ?? 1f));
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
            for (int i = 0; i < physics.Count; i++)
            {
                JsonObject data = physics[i].AsObject();
                var name = (string)data["name"];
                writer.WriteString(name);
                writer.WriteVarInt((int)(data["order"] ?? 0));
                writer.WriteVarInt(bone2idx[(string)data["bone"]]);

                int flags = 0;
                if (data["skin"] is JsonValue skin && (bool)skin) flags |= 1;
                if (data["x"] is JsonValue) flags |= 2;
                if (data["y"] is JsonValue) flags |= 4;
                if (data["rotate"] is JsonValue) flags |= 8;
                if (data["scaleX"] is JsonValue) flags |= 16;
                if (data["shearX"] is JsonValue) flags |= 32;
                if (data["limit"] is JsonValue) flags |= 64;
                if (data["mass"] is JsonValue) flags |= 128;
                writer.WriteByte((byte)flags);
                if ((flags & 2) != 0) writer.WriteFloat((float)data["x"]);
                if ((flags & 4) != 0) writer.WriteFloat((float)data["y"]);
                if ((flags & 8) != 0) writer.WriteFloat((float)data["rotate"]);
                if ((flags & 16) != 0) writer.WriteFloat((float)data["scaleX"]);
                if ((flags & 32) != 0) writer.WriteFloat((float)data["shearX"]);
                if ((flags & 64) != 0) writer.WriteFloat((float)data["limit"]);
                writer.WriteByte((byte)(int)(data["fps"] ?? 60));
                writer.WriteFloat((float)(data["inertia"] ?? 1f));
                writer.WriteFloat((float)(data["strength"] ?? 100f));
                writer.WriteFloat((float)(data["damping"] ?? 1f));
                if ((flags & 128) != 0) writer.WriteFloat(1f / (float)data["mass"]);
                writer.WriteFloat((float)(data["wind"] ?? 0f));
                writer.WriteFloat((float)(data["gravity"] ?? 0f));

                flags = 0;
                if (data["inertiaGlobal"] is JsonValue inertiaGlobal && (bool)inertiaGlobal) flags |= 1;
                if (data["strengthGlobal"] is JsonValue strengthGlobal && (bool)strengthGlobal) flags |= 2;
                if (data["dampingGlobal"] is JsonValue dampingGlobal && (bool)dampingGlobal) flags |= 4;
                if (data["massGlobal"] is JsonValue massGlobal && (bool)massGlobal) flags |= 8;
                if (data["windGlobal"] is JsonValue windGlobal && (bool)windGlobal) flags |= 16;
                if (data["gravityGlobal"] is JsonValue gravityGlobal && (bool)gravityGlobal) flags |= 32;
                if (data["mixGlobal"] is JsonValue mixGlobal && (bool)mixGlobal) flags |= 64;
                if (data["mix"] is JsonValue) flags |= 128;
                writer.WriteByte((byte)flags);
                if ((flags & 128) != 0) writer.WriteFloat((float)data["mix"]);

                physics2idx[name] = i;
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
            //此处还是得先把数据全读了，反面例子还是那个mix-and-match，4.2版本的mix-and-match中，default用到了后面的skin,转换失败
            //mix-and-match的full-skins/boy这个skin，他的linkedmesh调用了后面的full-skins/boy
            JsonArray skins = root["skins"].AsArray();

            for (int i = 0; i < skins.Count; i++)
            {
                var name = (string)skins[i]["name"];
                if (name == "default" && i != 0)
                {
                    skin2idx[(string)skins[0]["name"]] = skin2idx.Count;
                    skin2idx["default"] = 0;
                    continue;
                }
                skin2idx[name] = skin2idx.Count;
            }

            bool hasDefault = false;
            foreach (JsonObject skin in skins)
            {
                if ((string)skin["name"] == "default")
                {
                    hasDefault = true;
                    WriteSkin(skin, true);
                    //skin2idx["default"] = skin2idx.Count;
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
                    //skin2idx[name] = skin2idx.Count;
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
                if (skin["bones"] is JsonArray bones) WriteNames(bone2idx, bones); else writer.WriteVarInt(0);
                if (skin["ik"] is JsonArray ik) WriteNames(ik2idx, ik); else writer.WriteVarInt(0);
                if (skin["transform"] is JsonArray transform) WriteNames(transform2idx, transform); else writer.WriteVarInt(0);
                if (skin["path"] is JsonArray path) WriteNames(path2idx, path); else writer.WriteVarInt(0);
                if (skin["physics"] is JsonArray physics) WriteNames(physics2idx, physics); else writer.WriteVarInt(0);
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

            int flags = 0;

            AttachmentType type = AttachmentType.Region;

            if (attachment["type"] is JsonValue _type) { type = Enum.Parse<AttachmentType>((string)_type, true); flags |= (int)type & 0x7; }
            if (attachment["name"] is JsonValue) flags |= 8;

            switch (type)
            {
                case AttachmentType.Region:
                    if (attachment["path"] is JsonValue) flags |= 16;
                    if (attachment["color"] is JsonValue) flags |= 32;
                    if (attachment["sequence"] is JsonObject) flags |= 64;
                    if (attachment["rotation"] is JsonValue) flags |= 128;
                    break;
                case AttachmentType.Boundingbox:
                    if (attachment["vertexCount"] is JsonValue _vc1 && attachment["vertices"].AsArray().Count != (int)_vc1 << 1) flags |= 16;
                    break;
                case AttachmentType.Mesh:
                    if (attachment["path"] is JsonValue) flags |= 16;
                    if (attachment["color"] is JsonValue) flags |= 32;
                    if (attachment["sequence"] is JsonObject) flags |= 64;
                    if (attachment["vertices"].AsArray().Count != attachment["uvs"].AsArray().Count) flags |= 128;
                    break;
                case AttachmentType.Linkedmesh:
                    if (attachment["path"] is JsonValue) flags |= 16;
                    if (attachment["color"] is JsonValue) flags |= 32;
                    if (attachment["sequence"] is JsonObject) flags |= 64;
                    if (attachment["timeline"] is not JsonValue timeline || (bool)timeline) flags |= 128;
                    break;
                case AttachmentType.Path:
                    if (attachment["closed"] is JsonValue closed && (bool)closed) flags |= 16;
                    if (attachment["constantSpeed"] is not JsonValue constantSpeed || (bool)constantSpeed) flags |= 32;
                    if (attachment["vertexCount"] is JsonValue _vc2 && attachment["vertices"].AsArray().Count != (int)_vc2 << 1) flags |= 64;
                    break;
                case AttachmentType.Clipping:
                    if (attachment["vertexCount"] is JsonValue _vc3 && attachment["vertices"].AsArray().Count != (int)_vc3 << 1) flags |= 16;
                    break;

            }

            writer.WriteUByte((byte)flags);
            if ((flags & 8) != 0) writer.WriteStringRef((string)attachment["name"]);
            switch (type)
            {
                case AttachmentType.Region:
                    if ((flags & 16) != 0) writer.WriteStringRef((string)attachment["path"]);
                    if ((flags & 32) != 0) writer.WriteInt(int.Parse((string)attachment["color"], NumberStyles.HexNumber));
                    if ((flags & 64) != 0) WriteSequence(attachment["sequence"].AsObject());
                    if ((flags & 128) != 0) writer.WriteFloat((float)attachment["rotation"]);
                    writer.WriteFloat((float)(attachment["x"] ?? 0f));
                    writer.WriteFloat((float)(attachment["y"] ?? 0f));
                    writer.WriteFloat((float)(attachment["scaleX"] ?? 1f));
                    writer.WriteFloat((float)(attachment["scaleY"] ?? 1f));
                    writer.WriteFloat((float)(attachment["width"] ?? 32f));
                    writer.WriteFloat((float)(attachment["height"] ?? 32f));
                    break;
                case AttachmentType.Boundingbox:
                    vertexCount = (int)(attachment["vertexCount"] ?? 0);
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount, (flags & 16) != 0);
                    if (nonessential) writer.WriteInt(0);
                    break;
                case AttachmentType.Mesh:
                    if ((flags & 16) != 0) writer.WriteStringRef((string)attachment["path"]);
                    if ((flags & 32) != 0) writer.WriteInt(int.Parse((string)attachment["color"], NumberStyles.HexNumber));
                    if ((flags & 64) != 0) WriteSequence(attachment["sequence"].AsObject());
                    writer.WriteVarInt((int)(attachment["hull"] ?? 0));
                    vertexCount = attachment["uvs"].AsArray().Count >> 1;
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount, (flags & 128) != 0);
                    WriteFloatArray(attachment["uvs"].AsArray(), vertexCount << 1);
                    WriteShortArray(attachment["triangles"].AsArray(), ((vertexCount << 1) - (int)(attachment["hull"] ?? 0) - 2) * 3);
                    if (nonessential)
                    {
                        if (attachment["edges"] is JsonArray edges)
                        {
                            writer.WriteVarInt(edges.Count);
                            WriteShortArray(edges, edges.Count);
                        }
                        writer.WriteFloat((float)(attachment["width"] ?? 0f));
                        writer.WriteFloat((float)(attachment["height"] ?? 0f));
                    }
                    break;
                case AttachmentType.Linkedmesh:
                    if ((flags & 16) != 0) writer.WriteStringRef((string)attachment["path"]);
                    if ((flags & 32) != 0) writer.WriteInt(int.Parse((string)attachment["color"], NumberStyles.HexNumber));
                    if ((flags & 64) != 0) WriteSequence(attachment["sequence"].AsObject());
                    if (attachment["skin"] is JsonValue skin) writer.WriteVarInt(skin2idx[(string)attachment["skin"]]); else writer.WriteVarInt(0); // XXX: 此处很抽象, json 里允许 skin 为 null, 但是二进制里一定是读取一个有效索引
                    if (attachment["parent"] is JsonValue parent) writer.WriteStringRef((string)parent); else writer.WriteStringRef(null);
                    if (nonessential)
                    {
                        writer.WriteFloat((float)(attachment["width"] ?? 0f));
                        writer.WriteFloat((float)(attachment["height"] ?? 0f));
                    }
                    break;
                case AttachmentType.Path:
                    vertexCount = (int)(attachment["vertexCount"] ?? 0);
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount, (flags & 64) != 0);
                    WriteFloatArray(attachment["lengths"].AsArray(), vertexCount * 2 / 6);
                    if (nonessential) writer.WriteInt(0);
                    break;
                case AttachmentType.Point:
                    writer.WriteFloat((float)(attachment["rotation"] ?? 0f));
                    writer.WriteFloat((float)(attachment["x"] ?? 0f));
                    writer.WriteFloat((float)(attachment["y"] ?? 0f));
                    if (nonessential) writer.WriteInt(0);
                    break;
                case AttachmentType.Clipping:
                    writer.WriteVarInt(slot2idx[(string)attachment["end"]]);
                    vertexCount = (int)(attachment["vertexCount"] ?? 0);
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount, (flags & 16) != 0);
                    if (nonessential) writer.WriteInt(0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid attachment type: {type}");
            }
        }

        private void WriteSequence(JsonObject sequence)
        {
            writer.WriteVarInt((int)sequence["count"]);
            writer.WriteVarInt((int)(sequence["start"] ?? 1));
            writer.WriteVarInt((int)(sequence["digits"] ?? 0));
            writer.WriteVarInt((int)(sequence["setup"] ?? 0));
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
                writer.WriteVarInt((int)(data["int"] ?? 0));
                writer.WriteFloat((float)(data["float"] ?? 0f));
                writer.WriteString((string)(data["string"] ?? ""));
                if (data["audio"] is JsonValue _audio)
                {
                    var audio = (string)_audio;
                    writer.WriteString(audio);
                    if (audio is not null)
                    {
                        writer.WriteFloat((float)(data["volume"] ?? 1f));
                        writer.WriteFloat((float)(data["balance"] ?? 0f));
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
                writer.WriteVarInt(1); // timelines 数组预分配大小
                if (data["slots"] is JsonObject slots) WriteSlotTimelines(slots); else writer.WriteVarInt(0);
                if (data["bones"] is JsonObject bones) WriteBoneTimelines(bones); else writer.WriteVarInt(0);
                if (data["ik"] is JsonObject ik) WriteIKTimelines(ik); else writer.WriteVarInt(0);
                if (data["transform"] is JsonObject transform) WriteTransformTimelines(transform); else writer.WriteVarInt(0);
                if (data["path"] is JsonObject path) WritePathTimelines(path); else writer.WriteVarInt(0);
                if (data["physics"] is JsonObject physics) WritePhysicsTimelines(physics); else writer.WriteVarInt(0);
                if (data["attachment"] is JsonObject attachment) WriteAttachmentTimelines(attachment); else writer.WriteVarInt(0);
                if (data["drawOrder"] is JsonArray drawOrder) WriteDrawOrderTimelines(drawOrder); else writer.WriteVarInt(0);
                if (data["events"] is JsonArray events) WriteEventTimelines(events); else writer.WriteVarInt(0);
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
                    JsonObject frame;
                    if (type == "attachment")
                    {
                        writer.WriteUByte(SkeletonBinary.SLOT_ATTACHMENT);
                        writer.WriteVarInt(frames.Count);
                        foreach (JsonObject o in frames)
                        {
                            writer.WriteFloat((float)(o["time"] ?? 0f));
                            writer.WriteStringRef((string)o["name"]);
                        }
                    }
                    else if (type == "rgba")
                    {
                        writer.WriteUByte(SkeletonBinary.SLOT_RGBA);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count() * 4);

                        frame = frames[0].AsObject();
                        writer.WriteFloat((float)(frame["time"] ?? 0f));
                        writer.WriteInt(int.Parse((string)frame["color"], NumberStyles.HexNumber));
                        for (int i = 1, n = frames.Count; i < n; i++)
                        {
                            frame = frames[i].AsObject();
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                            writer.WriteInt(int.Parse((string)frame["color"], NumberStyles.HexNumber));
                            WriteCurve(frames[i - 1].AsObject(), 4);
                        }
                    }
                    else if (type == "rgb")
                    {
                        writer.WriteUByte(SkeletonBinary.SLOT_RGB);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count() * 3);

                        frame = frames[0].AsObject();
                        var color = Convert.FromHexString((string)frame["color"]);
                        writer.WriteFloat((float)(frame["time"] ?? 0f));
                        writer.Write(color[0]); writer.Write(color[1]); writer.Write(color[2]);
                        for (int i = 1, n = frames.Count; i < n; i++)
                        {
                            frame = frames[i].AsObject();
                            color = Convert.FromHexString((string)frame["color"]);
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                            writer.Write(color[0]); writer.Write(color[1]); writer.Write(color[2]);
                            WriteCurve(frames[i - 1].AsObject(), 3);
                        }
                    }
                    else if (type == "rgba2")
                    {
                        writer.WriteUByte(SkeletonBinary.SLOT_RGBA2);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count() * 7);

                        frame = frames[0].AsObject();
                        var dark = Convert.FromHexString((string)frame["dark"]);
                        writer.WriteFloat((float)(frame["time"] ?? 0f));
                        writer.WriteInt(int.Parse((string)frame["light"], NumberStyles.HexNumber));
                        writer.Write(dark[0]); writer.Write(dark[1]); writer.Write(dark[2]);
                        for (int i = 1; i < frames.Count; i++)
                        {
                            frame = frames[i].AsObject();
                            dark = Convert.FromHexString((string)frame["dark"]);
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                            writer.WriteInt(int.Parse((string)frame["light"], NumberStyles.HexNumber));
                            writer.Write(dark[0]); writer.Write(dark[1]); writer.Write(dark[2]);
                            WriteCurve(frames[i - 1].AsObject(), 7);
                        }
                    }
                    else if (type == "rgb2")
                    {
                        writer.WriteUByte(SkeletonBinary.SLOT_RGB2);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count() * 6);

                        frame = frames[0].AsObject();
                        var light = Convert.FromHexString((string)frame["light"]);
                        var dark = Convert.FromHexString((string)frame["dark"]);
                        writer.WriteFloat((float)(frame["time"] ?? 0f));
                        writer.Write(light[0]); writer.Write(light[1]); writer.Write(light[2]);
                        writer.Write(dark[0]); writer.Write(dark[1]); writer.Write(dark[2]);
                        for (int i = 1; i < frames.Count; i++)
                        {
                            frame = frames[i].AsObject();
                            light = Convert.FromHexString((string)frame["light"]);
                            dark = Convert.FromHexString((string)frame["dark"]);
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                            writer.Write(light[0]); writer.Write(light[1]); writer.Write(light[2]);
                            writer.Write(dark[0]); writer.Write(dark[1]); writer.Write(dark[2]);
                            WriteCurve(frames[i - 1].AsObject(), 6);
                        }
                    }
                    else if (type == "alpha")
                    {
                        writer.WriteUByte(SkeletonBinary.SLOT_ALPHA);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                        frame = frames[0].AsObject();
                        writer.WriteFloat((float)(frame["time"] ?? 0f));
                        writer.Write((byte)((float)(frame["value"] ?? 1f) * 255));
                        for (int i = 1; i < frames.Count; i++)
                        {
                            frame = frames[i].AsObject();
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                            writer.Write((byte)((float)(frame["value"] ?? 1f) * 255));
                            WriteCurve(frames[i - 1].AsObject(), 1);
                        }
                    }
                }
            }
        }

        private void WriteCurve(JsonObject frame, int n)
        {
            if (frame["curve"] is JsonNode curve)
            {
                if (curve.GetValueKind() == JsonValueKind.String)
                {
                    writer.WriteByte(SkeletonBinary.CURVE_STEPPED);
                }
                else
                {
                    writer.WriteByte(SkeletonBinary.CURVE_BEZIER);
                    WriteFloatArray(curve.AsArray(), n * 4);
                }
            }
            else
            {
                writer.WriteByte(SkeletonBinary.CURVE_LINEAR);
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
                        foreach (JsonObject o in frames)
                        {
                            writer.WriteFloat((float)(o["time"] ?? 0f));
                            writer.WriteByte((byte)Enum.Parse<Inherit>((string)(o["inherit"] ?? "normal"), true));
                        }
                        continue;
                    }

                    if (type == "rotate")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_ROTATE);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                        WriteCurveFrames(frames, frames.Count, "value", 0);
                    }
                    else if (type == "translate")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_TRANSLATE);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count() * 2);
                        WriteCurveFrames(frames, frames.Count, "x", 0, "y", 0);
                    }
                    else if (type == "translatex")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_TRANSLATEX);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                        WriteCurveFrames(frames, frames.Count, "value", 0);
                    }
                    else if (type == "translatey")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_TRANSLATEY);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                        WriteCurveFrames(frames, frames.Count, "value", 0);
                    }

                    else if (type == "scale")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SCALE);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count() * 2);
                        WriteCurveFrames(frames, frames.Count, "x", 1, "y", 1);
                    }
                    else if (type == "scalex")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SCALEX);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                        WriteCurveFrames(frames, frames.Count, "value", 1);
                    }
                    else if (type == "scaley")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SCALEY);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                        WriteCurveFrames(frames, frames.Count, "value", 1);
                    }
                    else if (type == "shear")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SHEAR);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count() * 2);
                        WriteCurveFrames(frames, frames.Count, "x", 0, "y", 0);
                    }
                    else if (type == "shearx")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SHEARX);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                        WriteCurveFrames(frames, frames.Count, "value", 0);
                    }
                    else if (type == "sheary")
                    {
                        writer.WriteByte(SkeletonBinary.BONE_SHEARY);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                        WriteCurveFrames(frames, frames.Count, "value", 0);
                    }
                }
            }
        }

        private void WriteCurveFrames(JsonArray frames, int frameCount, string name1, float def1)
        {
            JsonObject frame = frames[0].AsObject();
            writer.WriteFloat((float)(frame["time"] ?? 0f));
            writer.WriteFloat((float)(frame[name1] ?? def1));
            for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
            {
                frame = frames[frameIdx].AsObject();
                writer.WriteFloat((float)(frame["time"] ?? 0f));
                writer.WriteFloat((float)(frame[name1] ?? def1));
                WriteCurve(frames[frameIdx - 1].AsObject(), 1);
            }
        }

        private void WriteCurveFrames(JsonArray frames, int frameCount, string name1, float def1, string name2, float def2)
        {
            JsonObject frame = frames[0].AsObject();
            writer.WriteFloat((float)(frame["time"] ?? 0f));
            writer.WriteFloat((float)(frame[name1] ?? def1));
            writer.WriteFloat((float)(frame[name2] ?? def2));
            for (int frameIdx = 1; frameIdx < frameCount; frameIdx++)
            {
                frame = frames[frameIdx].AsObject();
                writer.WriteFloat((float)(frame["time"] ?? 0f));
                writer.WriteFloat((float)(frame[name1] ?? def1));
                writer.WriteFloat((float)(frame[name2] ?? def2));
                WriteCurve(frames[frameIdx - 1].AsObject(), 2);
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
                writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count() * 2);

                int flags = 0;
                JsonObject frame = frames[0].AsObject();
                if (frame["mix"] is JsonValue) flags |= 1 + 2;
                if (frame["softness"] is JsonValue) flags |= 4;
                if ((bool)(frame["bendPositive"] ?? true)) flags |= 8;
                if ((bool)(frame["compress"] ?? true)) flags |= 16;
                if ((bool)(frame["stretch"] ?? false)) flags |= 32;

                writer.Write((byte)flags);
                writer.WriteFloat((float)(frame["time"] ?? 0f));
                if ((flags & (1 + 2)) != 0) writer.WriteFloat((float)frame["mix"]);
                if ((flags & 4) != 0) writer.WriteFloat((float)frame["softness"]);

                for (int i = 1; i < frames.Count; i++)
                {
                    flags = 0;
                    frame = frames[i].AsObject();
                    if (frame["mix"] is JsonValue) flags |= 1 + 2;
                    if (frame["softness"] is JsonValue) flags |= 4;
                    if ((bool)(frame["bendPositive"] ?? true)) flags |= 8;
                    if ((bool)(frame["compress"] ?? true)) flags |= 16;
                    if ((bool)(frame["stretch"] ?? false)) flags |= 32;

                    if (frames[i - 1]["curve"] is JsonNode curve)
                    {
                        if (curve.GetValueKind() == JsonValueKind.String) flags |= 64;
                        else if (curve.GetValueKind() == JsonValueKind.Array) flags |= 128;
                    }

                    writer.Write((byte)flags);
                    writer.WriteFloat((float)(frame["time"] ?? 0f));
                    if ((flags & (1 + 2)) != 0) writer.WriteFloat((float)frame["mix"]);
                    if ((flags & 4) != 0) writer.WriteFloat((float)frame["softness"]);
                    if ((flags & 128) != 0) WriteFloatArray(frames[i - 1]["curve"].AsArray(), 8);
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
                writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count() * 6);

                JsonObject frame = frames[0].AsObject();
                writer.WriteFloat((float)(frame["time"] ?? 0f));
                writer.WriteFloat((float)(frame["mixRotate"] ?? 1f));
                writer.WriteFloat((float)(frame["mixX"] ?? 1f));
                writer.WriteFloat((float)(frame["mixY"] ?? frame["mixX"] ?? 1f));
                writer.WriteFloat((float)(frame["mixScaleX"] ?? 1f));
                writer.WriteFloat((float)(frame["mixScaleY"] ?? frame["mixScaleX"] ?? 1f));
                writer.WriteFloat((float)(frame["mixShearY"] ?? 1f));
                for (int i = 1; i < frames.Count; i++)
                {
                    frame = frames[i].AsObject();
                    writer.WriteFloat((float)(frame["time"] ?? 0f));
                    writer.WriteFloat((float)(frame["mixRotate"] ?? 1f));
                    writer.WriteFloat((float)(frame["mixX"] ?? 1f));
                    writer.WriteFloat((float)(frame["mixY"] ?? frame["mixX"] ?? 1f));
                    writer.WriteFloat((float)(frame["mixScaleX"] ?? 1f));
                    writer.WriteFloat((float)(frame["mixScaleY"] ?? frame["mixScaleX"] ?? 1f));
                    writer.WriteFloat((float)(frame["mixShearY"] ?? 1f));
                    WriteCurve(frames[i - 1].AsObject(), 6);
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
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                        WriteCurveFrames(frames, frames.Count, "value", 0);
                    }
                    else if (type == "spacing")
                    {
                        writer.WriteByte(SkeletonBinary.PATH_SPACING);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                        WriteCurveFrames(frames, frames.Count, "value", 0);
                    }
                    else if (type == "mix")
                    {
                        writer.WriteByte(SkeletonBinary.PATH_MIX);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count() * 3);

                        JsonObject frame = frames[0].AsObject();
                        writer.WriteFloat((float)(frame["time"] ?? 0f));
                        writer.WriteFloat((float)(frame["mixRotate"] ?? 1f));
                        writer.WriteFloat((float)(frame["mixX"] ?? 1f));
                        writer.WriteFloat((float)(frame["mixY"] ?? frame["mixX"] ?? 1f));
                        for (int i = 1; i < frames.Count; i++)
                        {
                            frame = frames[i].AsObject();
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                            writer.WriteFloat((float)(frame["mixRotate"] ?? 1f));
                            writer.WriteFloat((float)(frame["mixX"] ?? 1f));
                            writer.WriteFloat((float)(frame["mixY"] ?? frame["mixX"] ?? 1f));
                            WriteCurve(frames[i - 1].AsObject(), 3);
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
                if (!string.IsNullOrEmpty(name)) writer.WriteVarInt(physics2idx[name] + 1); else writer.WriteVarInt(0); // -1 表示应用到全部

                writer.WriteVarInt(timeline.Count);
                foreach (var (type, _frames) in timeline)
                {
                    JsonArray frames = _frames.AsArray();
                    if (type == "reset")
                    {
                        writer.WriteUByte(SkeletonBinary.PHYSICS_RESET);
                        writer.WriteVarInt(frames.Count);
                        foreach (JsonObject frame in frames)
                        {
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                        }
                        continue;
                    }
                    switch (type)
                    {
                        case "inertia":
                            writer.WriteUByte(SkeletonBinary.PHYSICS_INERTIA);
                            break;
                        case "strength":
                            writer.WriteUByte(SkeletonBinary.PHYSICS_STRENGTH);
                            break;
                        case "damping":
                            writer.WriteUByte(SkeletonBinary.PHYSICS_DAMPING);
                            break;
                        case "mass":
                            writer.WriteUByte(SkeletonBinary.PHYSICS_MASS);
                            break;
                        case "wind":
                            writer.WriteUByte(SkeletonBinary.PHYSICS_WIND);
                            break;
                        case "gravity":
                            writer.WriteUByte(SkeletonBinary.PHYSICS_GRAVITY);
                            break;
                        case "mix":
                            writer.WriteUByte(SkeletonBinary.PHYSICS_MIX);
                            break;
                    }
                    writer.WriteVarInt(frames.Count);
                    writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());
                    WriteCurveFrames(frames, frames.Count, "value", 0);
                }
            }
        }

        private void WriteAttachmentTimelines(JsonObject attachmentTimelines)
        {
            writer.WriteVarInt(attachmentTimelines.Count); // skinCount
            foreach (var (skinName, _skinValue) in attachmentTimelines)
            {
                JsonObject skinValue = _skinValue.AsObject();
                writer.WriteVarInt(skin2idx[skinName]);
                writer.WriteVarInt(skinValue.Count); // slotCount
                foreach (var (slotName, _slotValue) in skinValue)
                {
                    JsonObject slotValue = _slotValue.AsObject();
                    writer.WriteVarInt(slot2idx[slotName]);
                    writer.WriteVarInt(slotValue.Count); // attachmentCount
                    foreach (var (attachmentName, timeline) in slotValue)
                    {
                        writer.WriteStringRef(attachmentName);
                        foreach (var (type, _frames) in timeline.AsObject())
                        {
                            JsonArray frames = _frames.AsArray();
                            if (type == "deform")
                            {
                                JsonObject frame;
                                writer.WriteUByte(SkeletonBinary.ATTACHMENT_DEFORM);
                                writer.WriteVarInt(frames.Count);
                                writer.WriteVarInt(frames.Where(it => it["curve"] is JsonArray).Count());

                                frame = frames[0].AsObject();
                                writer.WriteFloat((float)(frame["time"] ?? 0f));
                                if (frame["vertices"] is JsonArray v0)
                                {
                                    var end = v0.Count;
                                    writer.WriteVarInt(end);
                                    if (end > 0)
                                    {
                                        writer.WriteVarInt((int)(frame["offset"] ?? 0));
                                        WriteFloatArray(v0, end);
                                    }
                                }
                                for (int i = 1; i < frames.Count; i++)
                                {
                                    frame = frames[i].AsObject();
                                    writer.WriteFloat((float)(frame["time"] ?? 0f));
                                    if (frame["vertices"] is JsonArray vertices)
                                    {
                                        var end = vertices.Count;
                                        writer.WriteVarInt(end);
                                        if (end > 0)
                                        {
                                            writer.WriteVarInt((int)(frame["offset"] ?? 0));
                                            WriteFloatArray(vertices, end);
                                        }
                                    }
                                    WriteCurve(frames[i - 1].AsObject(), 1);
                                }
                            }
                            else if (type == "sequence")
                            {
                                writer.WriteByte(SkeletonBinary.ATTACHMENT_SEQUENCE);
                                writer.WriteVarInt(frames.Count);

                                float lastDelay = 0;
                                int modeAndIndex = 0;
                                foreach (JsonObject frame in frames)
                                {
                                    modeAndIndex = (int)Enum.Parse<SequenceMode>((string)(frame["mode"] ?? "hold"), true) & 0xf;
                                    modeAndIndex |= (int)(frame["index"] ?? 0) << 4;
                                    if (frame["delay"] is JsonValue delay) lastDelay = (float)delay;
                                    writer.WriteFloat((float)(frame["time"] ?? 0f));
                                    writer.WriteInt(modeAndIndex);
                                    writer.WriteFloat(lastDelay);
                                }
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
                writer.WriteFloat((float)(data["time"] ?? 0f));
                if (data["offsets"] is JsonArray offsets)
                {
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
                writer.WriteFloat((float)(data["time"] ?? 0f));
                writer.WriteVarInt(event2idx[(string)data["name"]]);
                writer.WriteVarInt((int)(data["int"] ?? eventData["int"] ?? 0));
                writer.WriteFloat((float)(data["float"] ?? eventData["float"] ?? 0f));
                writer.WriteString((string)(data["string"] ?? eventData["string"]));
                if (eventData.ContainsKey("audio"))
                {
                    writer.WriteFloat((float)(data["volume"] ?? eventData["volume"] ?? 1f));
                    writer.WriteFloat((float)(data["balance"] ?? eventData["balance"] ?? 0f));
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

        public void WriteShortArray(JsonArray array, int n)
        {
            for (int i = 0; i < n; i++)
                writer.WriteVarInt((int)array[i]);
        }

        private void WriteVertices(JsonArray vertices, int vertexCount, bool weighted)
        {
            if (!weighted)
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
