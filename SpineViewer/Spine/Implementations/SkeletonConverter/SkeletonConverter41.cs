using System.Text.Json;
using System.Text.Json.Nodes;
using System.Globalization;
using SpineRuntime41;
using System.Net.Mail;
using System.Windows.Forms.VisualStyles;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Drawing.Drawing2D;
using Accessibility;
using SpineViewer.Spine.SpineView;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Configuration;



namespace SpineViewer.Spine.Implementations.SkeletonConverter
{
    [SpineImplementation(SpineVersion.V41)]
    public class SkeletonConverter41 : Spine.SkeletonConverter
    {

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
            [SpacingMode.Proportional] = "proportional"
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
            [AttachmentType.Boundingbox] = "boundingbox",
            [AttachmentType.Mesh] = "mesh",
            [AttachmentType.Linkedmesh] = "linkedmesh",
            [AttachmentType.Path] = "path",
            [AttachmentType.Point] = "point",
            [AttachmentType.Clipping] = "clipping",
            [AttachmentType.Sequence] = "sequence",
        };
        private static readonly Dictionary<SequenceMode, string> SequenceModeJsonValue = new()
        {
            [SequenceMode.Hold] = "hold",
            [SequenceMode.Once] = "once",
            [SequenceMode.Loop] = "loop",
            [SequenceMode.Pingpong] = "pingpong",
            [SequenceMode.OnceReverse] = "onceReverse",
            [SequenceMode.LoopReverse] = "loopReverse",
            [SequenceMode.PingpongReverse] = "pingpongReverse"

        };

        private static readonly TransformMode[] TransformModeToValue = new TransformMode[]
        {
            TransformMode.Normal,
            TransformMode.OnlyTranslation,
            TransformMode.NoRotationOrReflection,
            TransformMode.NoScale,
            TransformMode.NoScaleOrReflection
        };
        private static readonly Dictionary<string, int> TransformModeToInt = new()
        {
            ["normal"] = 0,
            ["onlytranslation"] = 1,
            ["norotationorreflection"] = 2,
            ["noscale"] = 3,
            ["noscaleorreflection"] = 4
        };

        private BinaryReader reader = null;
        private JsonObject root = null;
        private bool nonessential = false;

        private readonly List<JsonObject> idx2event = [];
        private readonly List<string> skinNameList = [];
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
            //ReadPhysics();
            ReadSkins();
            //ReadLinkedMeshs();
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
            //long hash = reader.ReadLong();
            skeleton["hash"] = Convert.ToBase64String(Convert.FromHexString(reader.ReadLong().ToString("x16"))).TrimEnd('=');

            skeleton["spine"] = reader.ReadString();
            skeleton["x"] = reader.ReadFloat();
            skeleton["y"] = reader.ReadFloat();
            skeleton["width"] = reader.ReadFloat();
            skeleton["height"] = reader.ReadFloat();
            //skeleton["referenceScale"] = reader.ReadFloat();// * 1.0f;//乘scale
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

                data["transform"] = TransformModeToValue[reader.ReadVarInt()].ToString();
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
                data["blend"] = BlendModeJsonValue[(BlendMode)reader.ReadVarInt()];
                //if (nonessential)
                //{
                //    reader.ReadBoolean();
                //}
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
                data["softness"] = reader.ReadFloat();// * scale
                data["bendPositive"] = reader.ReadSByte() == 1;
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
                data["shearX"] = reader.ReadFloat();
                data["mixRotate"] = reader.ReadFloat();
                data["mixX"] = reader.ReadFloat();
                data["mixY"] = reader.ReadFloat();
                data["mixScaleX"] = reader.ReadFloat();
                data["mixScaleY"] = reader.ReadFloat();
                data["mixShearY"] = reader.ReadFloat();


                transform.Add(data);
            }
            root["transform"] = transform;
        }

        private void ReadPath()
        {

            JsonArray bones = root["bones"].AsArray();
            JsonArray slots = root["slots"].AsArray();
            JsonArray path = [];
            for (int n = reader.ReadVarInt(); n > 0; n--)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                data["bones"] = ReadNames(bones);
                data["target"] = (string)slots[reader.ReadVarInt()]["name"];

                data["positionMode"] = PositionModeJsonValue[((PositionMode)reader.ReadVarInt())];
                data["spacingMode"] = SpacingModeJsonValue[((SpacingMode)reader.ReadVarInt())];
                data["rotateMode"] = RotateModeJsonValue[((RotateMode)reader.ReadVarInt())];
                data["rotation"] = reader.ReadFloat();
                data["position"] = reader.ReadFloat();// * scale
                data["spacing"] = reader.ReadFloat();//* scale
                data["mixRotate"] = reader.ReadFloat();
                data["mixX"] = reader.ReadFloat();
                data["mixY"] = reader.ReadFloat();

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
                skinNameList.Add("default");
                slotCount = reader.ReadVarInt();
                if (slotCount <= 0) return null;
            }
            else
            {
                //skin["name"] = reader.ReadString();
                skin["name"] = reader.ReadStringRef();
                skinNameList.Add((string)skin["name"]);
                skin["bone"] = ReadNames(root["bones"].AsArray());
                skin["ik"] = ReadNames(root["ik"].AsArray());
                skin["transform"] = ReadNames(root["transform"].AsArray());
                skin["path"] = ReadNames(root["path"].AsArray());
                //skin["physics"] = ReadNames(root["physics"].AsArray());
                slotCount = reader.ReadVarInt();
            }

            JsonArray slots = root["slots"].AsArray();
            JsonObject skinAttachments = [];
            while (slotCount-- > 0)
            {
                JsonObject slotAttachments = [];
                //int tmp = ;
                skinAttachments[(string)slots[reader.ReadVarInt()]["name"]] = slotAttachments;
                //skinAttachments[(string)slots[tmp]["name"]] = slotAttachments;
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
            //JsonArray skins = root["skins"].AsArray();
            JsonObject attachment = [];
            int vertexCount;
            string path;

            string name = reader.ReadStringRef();
            if (name == null) name = keyName;
            int type = reader.ReadByte();

            attachment["name"] = name;
            attachment["type"] = AttachmentTypeJsonValue[((AttachmentType)(type))];
            switch ((AttachmentType)(type))
            {
                case AttachmentType.Region:
                    path = reader.ReadStringRef();
                    if (path == null) path = name;
                    attachment["path"] = path;
                    attachment["rotation"] = reader.ReadFloat();

                    attachment["x"] = reader.ReadFloat();
                    attachment["y"] = reader.ReadFloat();
                    attachment["scaleX"] = reader.ReadFloat();
                    attachment["scaleY"] = reader.ReadFloat();
                    attachment["width"] = reader.ReadFloat();
                    attachment["height"] = reader.ReadFloat();
                    attachment["color"] = reader.ReadInt().ToString("x8");
                    attachment["sequence"] = ReadSequence();

                    break;
                case AttachmentType.Boundingbox:
                    vertexCount = reader.ReadVarInt();
                    attachment["vertexCount"] = vertexCount;
                    attachment["vertices"] = ReadVertices(vertexCount);
                    if (nonessential) reader.ReadInt();
                    break;
                case AttachmentType.Mesh:
                    path = reader.ReadStringRef();
                    if (path == null) path = name;
                    attachment["path"] = path;
                    attachment["color"] = reader.ReadInt().ToString("x8");
                    vertexCount = reader.ReadVarInt();
                    attachment["uvs"] = ReadFloatArray(vertexCount << 1); // vertexCount = uvs.Length >> 1
                    attachment["triangles"] = ReadShortArray();
                    attachment["vertices"] = ReadVertices(vertexCount);
                    attachment["hull"] = reader.ReadVarInt();
                    attachment["sequence"] = ReadSequence();
                    if (nonessential)
                    {
                        attachment["edge"] = ReadShortArray();
                        attachment["width"] = reader.ReadFloat();
                        attachment["height"] = reader.ReadFloat();
                    }

                    break;
                case AttachmentType.Linkedmesh:
                    path = reader.ReadStringRef();
                    if (path == null) path = name;
                    attachment["path"] = path;
                    attachment["color"] = reader.ReadInt().ToString("x8");
                    attachment["skin"] = reader.ReadStringRef();
                    attachment["parent"] = reader.ReadStringRef();
                    attachment["timelines"] = reader.ReadBoolean();
                    attachment["sequence"] = ReadSequence();
                    if (nonessential)
                    {
                        attachment["width"] = reader.ReadFloat();//*scale
                        attachment["height"] = reader.ReadFloat();//*scale
                    }

                    break;
                case AttachmentType.Path:
                    attachment["closed"] = reader.ReadBoolean();
                    attachment["constantSpeed"] = reader.ReadBoolean();
                    vertexCount = reader.ReadVarInt();
                    attachment["vertexCount"] = vertexCount;
                    attachment["vertices"] = ReadVertices(vertexCount);
                    attachment["lengths"] = ReadFloatArray(vertexCount / 3);
                    if (nonessential)
                    {
                        reader.ReadInt();
                    }

                    break;
                case AttachmentType.Point:
                    attachment["rotation"] = reader.ReadFloat();
                    attachment["x"] = reader.ReadFloat();
                    attachment["y"] = reader.ReadFloat();
                    if (nonessential) reader.ReadInt(); //int color = nonessential ? input.ReadInt() : 0;

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

        private JsonObject ReadSequence()
        {
            //Sequence sequence = new Sequence(count);
            if (!reader.ReadBoolean()) return null;
            return new JsonObject()
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
            for (int n = reader.ReadVarInt(); n > 0; n--)
            {
                JsonObject data = [];
                //var name = reader.ReadString();
                var name = reader.ReadStringRef();
                events[name] = data;
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
            int count = reader.ReadVarInt();

            while (count-- > 0)
            {
                JsonObject data = [];
                animations[reader.ReadString()] = data;
                reader.ReadVarInt();//用来数组预先分配空间
                if (ReadSlotTimelines() is JsonObject slots) data["slots"] = slots;
                if (ReadBoneTimelines() is JsonObject bones) data["bones"] = bones;
                if (ReadIKTimelines() is JsonObject ik) data["ik"] = ik;
                if (ReadTransformTimelines() is JsonObject transform) data["transform"] = transform;
                if (ReadPathTimelines() is JsonObject path) data["path"] = path;
                if (ReadAttachmentTinelines() is JsonObject attachment) data["attachment"] = attachment;

                if (ReadDrawOrderTimelines() is JsonArray draworder) data["draworder"] = draworder;
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
                int slotindex = reader.ReadVarInt();
                slotTimelines[(string)slots[slotindex]["name"]] = timeline;
                for (int nn = reader.ReadVarInt(); nn > 0; nn--)
                {
                    int timelineType = reader.ReadUByte();
                    int frameCount = reader.ReadVarInt();
                    float time;
                    int r, g, b, a, r2, g2, b2;

                    //int bezire;
                    JsonArray jsArray = [];
                    //JsonObject temp;
                    switch (timelineType)
                    {
                        case SkeletonBinary.SLOT_ATTACHMENT:
                            timeline["attachment"] = jsArray;
                            while (frameCount-- > 0)
                            {
                                jsArray.Add(new JsonObject()
                                {
                                    ["time"] = reader.ReadFloat(),
                                    ["name"] = reader.ReadStringRef(),
                                });
                            }
                            break;
                        case SkeletonBinary.SLOT_RGBA:
                            timeline["rgba"] = jsArray;
                            reader.ReadVarInt();//贝塞尔曲线的数量。
                            //我还是觉得frameCount一定不为0，否则就是文件格式错误。故未作更改
                            //原因如下
                            //从事实上来说，至少在3.8版本中，如果一个动画没有关键帧，则该动画不会被导出，也就不会出现frameCount为0的情况
                            //从理论上来说，如果有一个动画，只有骨骼的移动，相当于这个动画只在transformTimeline上有关键帧
                            //在其他timeline上没有关键帧，这样在读取的时候，在进行其他timeline的读取时，读到0而不进入循环体
                            //直接返回空，只有在transformTimeline时才进入循环体读取数据。
                            //那么，如果一个动画能够进入某个timeline的循环体，说明该动画在该timeline上有关键帧，但frameCount=0
                            //说明在该timeline上的关键帧的数量是0。这是矛盾的。
                            time = reader.ReadFloat();
                            r = reader.Read();
                            g = reader.Read();
                            b = reader.Read();
                            a = reader.Read();
                            for (int frame = 0; frame < frameCount; frame++)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = time,
                                    ["color"] = $"{r:x2}{g:x2}{b:x2}{a:x2}"
                                };
                                jsArray.Add(o);
                                if (frame == frameCount - 1) break;
                                time = reader.ReadFloat();
                                r = reader.Read();
                                g = reader.Read();
                                b = reader.Read();
                                a = reader.Read();
                                ReadCurve(o, 4);
                            }

                            break;
                        case SkeletonBinary.SLOT_RGB:
                            timeline["rgb"] = jsArray;
                            reader.ReadVarInt();
                            time = reader.ReadFloat();
                            r = reader.Read();
                            g = reader.Read();
                            b = reader.Read();

                            for (int frame = 0; frame < frameCount; frame++)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = time,
                                    ["color"] = $"{r:x2}{g:x2}{b:x2}"
                                };
                                jsArray.Add(o);
                                if (frame == frameCount - 1) break;
                                time = reader.ReadFloat();
                                r = reader.Read();
                                g = reader.Read();
                                b = reader.Read();
                                ReadCurve(o, 3);
                            }

                            break;
                        //ok
                        case SkeletonBinary.SLOT_RGBA2:
                            timeline["rgba2"] = jsArray;
                            reader.ReadVarInt();
                            time = reader.ReadFloat();
                            r = reader.Read();
                            g = reader.Read();
                            b = reader.Read();
                            a = reader.Read();
                            r2 = reader.Read();
                            g2 = reader.Read();
                            b2 = reader.Read();

                            for (int frame = 0; frame < frameCount; frame++)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = time,
                                    ["light"] = $"{r:x2}{g:x2}{b:x2}{a:x2}",
                                    ["dark"] = $"{r2:x2}{g2:x2}{b2:x2}",
                                };
                                jsArray.Add(o);
                                if (frame == frameCount - 1) break;
                                time = reader.ReadFloat();
                                r = reader.Read();
                                g = reader.Read();
                                b = reader.Read();
                                a = reader.Read();
                                r2 = reader.Read();
                                g2 = reader.Read();
                                b2 = reader.Read();
                                ReadCurve(o, 7);
                            }

                            break;
                        case SkeletonBinary.SLOT_RGB2:
                            timeline["rgb2"] = jsArray;
                            reader.ReadVarInt();
                            time = reader.ReadFloat();
                            r = reader.Read();
                            g = reader.Read();
                            b = reader.Read();
                            r2 = reader.Read();
                            b2 = reader.Read();
                            g2 = reader.Read();
                            for (int frame = 0; frame < frameCount; frame++)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = time,
                                    ["light"] = $"{r:x2}{g:x2}{b:x2}",
                                    ["dark"] = $"{r2:x2}{g2:x2}{b2:x2}",
                                };
                                jsArray.Add(o);
                                if (frame == frameCount - 1) break;
                                time = reader.ReadFloat();
                                r = reader.Read();
                                g = reader.Read();
                                b = reader.Read();
                                r2 = reader.Read();
                                b2 = reader.Read();
                                g2 = reader.Read();
                                ReadCurve(o, 6);
                            }

                            break;
                        case SkeletonBinary.SLOT_ALPHA:
                            timeline["alpha"] = jsArray;
                            reader.ReadVarInt();
                            time = reader.ReadFloat();
                            var aa = reader.Read() / 255f;
                            for (int frame = 0; frame < frameCount; frame++)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = time,
                                    ["value"] = aa,
                                };
                                jsArray.Add(o);
                                if (frame == frameCount - 1) break;
                                time = reader.ReadFloat();
                                aa = reader.Read() / 255f;
                                ReadCurve(o, 1);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Invalid slot timeline type: {timelineType}");
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
                    var type = reader.ReadUByte();
                    var frameCount = reader.ReadVarInt();

                    reader.ReadVarInt();//贝塞尔曲线数量
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
            float time, mix, softness;

            for (int ikCount = reader.ReadVarInt(); ikCount > 0; ikCount--)
            {
                JsonArray frames = [];
                ikTimelines[(string)ik[reader.ReadVarInt()]["name"]] = frames;
                int frameCount = reader.ReadVarInt();
                reader.ReadVarInt();

                time = reader.ReadFloat();
                mix = reader.ReadFloat();
                softness = reader.ReadFloat();

                for (int frame = 0; frame < frameCount; frame++)
                {
                    var o = new JsonObject()
                    {
                        ["time"] = time,
                        ["mix"] = mix,
                        ["softness"] = softness,//scale
                        ["bendPositive"] = reader.ReadSByte() == 1,
                        ["compress"] = reader.ReadBoolean(),
                        ["stretch"] = reader.ReadBoolean(),
                    };
                    frames.Add(o);
                    if (frame == frameCount - 1) break;

                    time = reader.ReadFloat();
                    mix = reader.ReadFloat();
                    softness = reader.ReadFloat();
                    ReadCurve(o, 2);
                }
            }

            return ikTimelines.Count > 0 ? ikTimelines : null;
        }

        private JsonObject? ReadTransformTimelines()
        {
            JsonArray transform = root["transform"].AsArray();
            JsonObject transformTimelines = [];
            float time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY;

            for (int transformCount = reader.ReadVarInt(); transformCount > 0; transformCount--)
            {
                JsonArray frames = [];
                transformTimelines[(string)transform[reader.ReadVarInt()]["name"]] = frames;
                int frameCount = reader.ReadVarInt();
                reader.ReadVarInt();
                time = reader.ReadFloat();
                mixRotate = reader.ReadFloat();
                mixX = reader.ReadFloat();
                mixY = reader.ReadFloat();
                mixScaleX = reader.ReadFloat();
                mixScaleY = reader.ReadFloat();
                mixShearY = reader.ReadFloat();
                for (int frame = 0; frame < frameCount; frame++)
                {
                    var o = new JsonObject()
                    {
                        ["time"] = time,
                        ["mixRotate"] = mixRotate,
                        ["mixX"] = mixX,
                        ["mixY"] = mixY,
                        ["mixScaleX"] = mixScaleX,
                        ["mixScaleY"] = mixScaleY,
                        ["mixShearY"] = mixShearY,
                    };
                    frames.Add(o);
                    if (frame == frameCount - 1) break;
                    time = reader.ReadFloat();
                    mixRotate = reader.ReadFloat();
                    mixX = reader.ReadFloat();
                    mixY = reader.ReadFloat();
                    mixScaleX = reader.ReadFloat();
                    mixScaleY = reader.ReadFloat();
                    mixShearY = reader.ReadFloat();
                    //if (frameCount > 1) ReadCurve(o);
                    ReadCurve(o, 6);
                }
            }

            return transformTimelines.Count > 0 ? transformTimelines : null;
        }

        private JsonObject? ReadPathTimelines()
        {
            JsonArray path = root["path"].AsArray();
            JsonObject pathTimelines = [];
            float time, value, value1, value2;

            for (int pathCount = reader.ReadVarInt(); pathCount > 0; pathCount--)
            {
                JsonObject timeline = [];
                pathTimelines[(string)(path[reader.ReadVarInt()]["name"])] = timeline;
                for (int timelineCount = reader.ReadVarInt(); timelineCount > 0; timelineCount--)
                {
                    JsonArray frames = [];
                    var type = reader.ReadUByte();
                    var frameCount = reader.ReadVarInt();
                    reader.ReadVarInt();//bezireCount
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
                            time = reader.ReadFloat();
                            value = reader.ReadFloat();
                            value1 = reader.ReadFloat();
                            value2 = reader.ReadFloat();
                            for (int frame = 0; frame < frameCount; ++frame)
                            {
                                var o = new JsonObject()
                                {
                                    ["time"] = time,
                                    ["mixRotate"] = value,
                                    ["mixX"] = value1,
                                    ["mixY"] = value2,
                                };
                                frames.Add(o);
                                if (frame == frameCount - 1) break;
                                time = reader.ReadFloat();
                                value = reader.ReadFloat();
                                value1 = reader.ReadFloat();
                                value2 = reader.ReadFloat();
                                ReadCurve(o, 3);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Invalid path timeline type: {type}");
                    }
                }
            }

            return pathTimelines.Count > 0 ? pathTimelines : null;
        }

        private JsonObject? ReadAttachmentTinelines()
        {
            JsonArray skin = root["skins"].AsArray();
            JsonArray slot = root["slots"].AsArray();
            //JsonArray attachment = root["attachments"].AsArray();

            JsonObject attachmenttimeline = [];

            for (int skinCount = reader.ReadVarInt(); skinCount > 0; skinCount--)
            {
                JsonObject slotlist = [];
                var skinname = (string)((skin[reader.ReadVarInt()]["name"]));

                attachmenttimeline[skinname] = slotlist;
                for (int slotCount = reader.ReadVarInt(); slotCount > 0; slotCount--)
                {
                    JsonObject attachmentlist = new JsonObject();
                    var slotname = (string)((slot[reader.ReadVarInt()]["name"]));
                    slotlist[slotname] = attachmentlist;

                    for (int attachmentCount = reader.ReadVarInt(); attachmentCount > 0; attachmentCount--)
                    {
                        JsonObject fucklist = new JsonObject();
                        JsonArray timeline = [];
                        var attachmentname = reader.ReadStringRef();
                        attachmentlist[attachmentname] = fucklist;
                        //JsonArray attachmentdata = skin[skinname][slotname][attachmentname].AsArray();
                        var type = reader.ReadUByte();
                        var frameCount = reader.ReadVarInt();
                        float time;
                        switch (type)
                        {
                            case SkeletonBinary.ATTACHMENT_DEFORM:
                                reader.ReadVarInt();
                                fucklist["deform"] = timeline;

                                time = reader.ReadFloat();
                                for (int frame = 0; frame < frameCount; frame++)
                                {
                                    var end = reader.ReadVarInt();
                                    var vertex = new JsonArray();
                                    var o = new JsonObject()
                                    {
                                        ["time"] = time,
                                    };
                                    if (end != 0)
                                    {
                                        var start = reader.ReadVarInt();
                                        o["offset"] = start;
                                        end += start;
                                        for (; start < end; start++)
                                        {
                                            vertex.Add(reader.ReadFloat());
                                        }
                                    }
                                    if (vertex.Count > 0) o["vertices"] = vertex;
                                    timeline.Add(o);
                                    if (frame == frameCount - 1) break;
                                    time = reader.ReadFloat();
                                    ReadCurve(o, 1);
                                }
                                break;
                            case SkeletonBinary.ATTACHMENT_SEQUENCE:
                                fucklist["sequence"] = timeline;
                                while (frameCount-- > 0)
                                {
                                    var o = new JsonObject()
                                    {
                                        ["time"] = reader.ReadFloat(),
                                    };
                                    var modeAndIndex = reader.ReadInt();
                                    o["mode"] = SequenceModeJsonValue[((SequenceMode)(modeAndIndex & 0xf))];
                                    o["index"] = modeAndIndex >> 4;
                                    o["delay"] = reader.ReadFloat();
                                    timeline.Add(o);
                                }

                                break;
                        }
                    }
                }
            }
            return attachmenttimeline.Count > 0 ? attachmenttimeline : null;
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
            JsonObject events = root["events"].AsObject();
            JsonArray eventTimelines = [];

            List<String> eventNames = new List<String>();
            List<JsonObject> eventData = new List<JsonObject>();
            foreach (var item in events)
            {
                eventNames.Add(item.Key);
                eventData.Add(item.Value.AsObject());
            }
            for (int eventCount = reader.ReadVarInt(); eventCount > 0; eventCount--)
            {
                JsonObject data = [];
                data["time"] = reader.ReadFloat();//
                int index = reader.ReadVarInt();
                data["name"] = eventNames[index];
                data["int"] = reader.ReadVarInt();//
                data["float"] = reader.ReadFloat();//
                data["string"] = reader.ReadBoolean() ? reader.ReadString() : (string)eventData[index]["string"];
                if (eventData[index].ContainsKey("audio"))
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
                return ReadFloatArray(vertexCount << 1);//scale

            for (int i = 0; i < vertexCount; i++)
            {
                int bonesCount = reader.ReadVarInt();
                vertices.Add(bonesCount);
                for (int j = 0; j < bonesCount; j++)
                {
                    vertices.Add(reader.ReadVarInt());
                    vertices.Add(reader.ReadFloat());//* scale
                    vertices.Add(reader.ReadFloat());//* scale
                    vertices.Add(reader.ReadFloat());
                }
            }
            return vertices;
        }

        private void ReadCurve(JsonObject obj, int num)
        {
            JsonArray curve = [];
            byte type = reader.ReadUByte();
            //reader.ReadByte()
            switch (type)
            {
                case SkeletonBinary.CURVE_LINEAR:
                    break;
                case SkeletonBinary.CURVE_STEPPED:
                    obj["curve"] = "stepped";
                    break;
                case SkeletonBinary.CURVE_BEZIER:
                    for (int i = 0; i < num * 4; i++)
                    {
                        curve.Add(reader.ReadFloat());
                    }
                    obj["curve"] = curve;
                    break;
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
            //WritePhysics();
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
            writer.WriteLong(long.Parse(Convert.ToHexString(Convert.FromBase64String(skeleton["hash"] + "=")), NumberStyles.HexNumber));
            writer.WriteString((string)skeleton["spine"]);
            writer.WriteFloat((float)(skeleton["x"] ?? 0f));
            writer.WriteFloat((float)(skeleton["y"] ?? 0f));
            writer.WriteFloat((float)(skeleton["width"] ?? 0f));
            writer.WriteFloat((float)(skeleton["height"] ?? 0f));
            //if (skeleton.TryGetPropertyValue("referenceScale", out var reference)) writer.WriteFloat((float)reference); else writer.WriteFloat(100);
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
                if (data.TryGetPropertyValue("transform", out var transform)) writer.WriteVarInt(TransformModeToInt[((string)data["transform"]).ToLower()]); else writer.WriteVarInt(TransformModeToInt["normal"]);
                writer.WriteBoolean((bool)(data["skin"] ?? false));
                if (nonessential)
                {
                    writer.WriteInt(-1);
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
                writer.WriteBoolean((bool)(data["skin"] ?? false));
                if (data.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                writer.WriteVarInt(bone2idx[(string)data["target"]]);

                writer.WriteFloat((float)(data["mix"] ?? 1f));
                writer.WriteFloat((float)(data["softness"] ?? 0f));
                if (data.TryGetPropertyValue("bendPositive", out var bendPositive))
                {
                    writer.WriteSByte((sbyte)(((bool)bendPositive ? 1 : -1)));
                }
                else
                {
                    writer.WriteSByte(1);
                }
                //writer.WriteSByte((sbyte)(((bool)data["bendPositive"] ? 1 : -1) ?? 1));//默认为true
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
                if (data.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                writer.WriteVarInt(bone2idx[(string)data["target"]]);

                writer.WriteBoolean((bool)(data["local"] ?? false));
                writer.WriteBoolean((bool)(data["relative"] ?? false));
                writer.WriteFloat((float)(data["rotation"] ?? 0f));
                writer.WriteFloat((float)(data["x"] ?? 0f));
                writer.WriteFloat((float)(data["y"] ?? 0f));
                writer.WriteFloat((float)(data["scaleX"] ?? 0f));
                writer.WriteFloat((float)(data["scaleY"] ?? 0f));
                writer.WriteFloat((float)(data["shearY"] ?? 0f));
                writer.WriteFloat((float)(data["mixRotate"] ?? 1f));
                writer.WriteFloat((float)(data["mixX"] ?? 1f));
                writer.WriteFloat((float)(data["mixY"] ?? data["mixX"] ?? 1f));
                writer.WriteFloat((float)(data["mixScaleX"] ?? 1));
                writer.WriteFloat((float)(data["mixScaleY"] ?? data["mixScaleX"] ?? 1f));
                writer.WriteFloat((float)(data["mixShearY"] ?? 1f));

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
                writer.WriteVarInt((int)(data["order"] ?? 0f));
                writer.WriteBoolean((bool)(data["skin"] ?? false));
                if (data.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                writer.WriteVarInt(slot2idx[(string)data["target"]]);

                if (data.TryGetPropertyValue("positionMode", out var positionMode)) writer.WriteVarInt((int)(PositionMode)Enum.Parse(typeof(PositionMode), (string)positionMode, true)); else writer.WriteVarInt((int)PositionMode.Percent);
                if (data.TryGetPropertyValue("spacingMode", out var spacingMode)) writer.WriteVarInt((int)(SpacingMode)Enum.Parse(typeof(SpacingMode), (string)spacingMode, true)); else writer.WriteVarInt((int)SpacingMode.Length);
                if (data.TryGetPropertyValue("rotateMode", out var rotateMode)) writer.WriteVarInt((int)(RotateMode)Enum.Parse(typeof(RotateMode), (string)rotateMode, true)); else writer.WriteVarInt((int)RotateMode.Tangent);
                writer.WriteFloat((float)(data["rotation"] ?? 0f));

                writer.WriteFloat((float)(data["position"] ?? 0f));
                writer.WriteFloat((float)(data["spacing"] ?? 0f));
                writer.WriteFloat((float)(data["mixRotate"] ?? 1f));
                writer.WriteFloat((float)(data["mixX"] ?? 1f));
                writer.WriteFloat((float)(data["mixY"] ?? data["mixX"] ?? 1f));
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
            foreach (JsonObject sk in skins)
            {
                if ((string)sk["name"] == "default")
                {
                    skin2idx["default"] = 0;
                    break;
                }
            }
            foreach (JsonObject sk in skins)
            {
                if ((string)sk["name"] != "default")
                {
                    skin2idx["default"] = skin2idx.Count;
                }
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
                writer.WriteStringRef((string)skin["name"]);
                //if (nonessential) writer.WriteInt(0);
                if (skin.TryGetPropertyValue("bones", out var bones)) WriteNames(bone2idx, bones.AsArray()); else writer.WriteVarInt(0);
                if (skin.TryGetPropertyValue("ik", out var ik)) WriteNames(ik2idx, ik.AsArray()); else writer.WriteVarInt(0);
                if (skin.TryGetPropertyValue("transform", out var transform)) WriteNames(transform2idx, transform.AsArray()); else writer.WriteVarInt(0);
                if (skin.TryGetPropertyValue("path", out var path)) WriteNames(path2idx, path.AsArray()); else writer.WriteVarInt(0);
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

            if (attachment.TryGetPropertyValue("name", out var name) && (string)name != keyName) writer.WriteStringRef((string)name); else writer.WriteStringRef(null);
            AttachmentType type = AttachmentType.Region;
            if (attachment.TryGetPropertyValue("type", out var type1)) type = (AttachmentType)Enum.Parse(typeof(AttachmentType), (string)type1, true);
            writer.WriteByte((byte)type);
            switch (type)
            {
                case AttachmentType.Region:
                    if (attachment.TryGetPropertyValue("path", out var path)) writer.WriteStringRef((string)path); else writer.WriteStringRef(null);
                    writer.WriteFloat((float)(attachment["rotation"] ?? 0f));

                    writer.WriteFloat((float)(attachment["x"] ?? 0f));
                    writer.WriteFloat((float)(attachment["y"] ?? 0f));
                    writer.WriteFloat((float)(attachment["scaleX"] ?? 1f));
                    writer.WriteFloat((float)(attachment["scaleY"] ?? 1f));
                    writer.WriteFloat((float)(attachment["width"] ?? 32f));
                    writer.WriteFloat((float)(attachment["height"] ?? 32f));
                    if (attachment.TryGetPropertyValue("color", out var color)) writer.WriteInt(int.Parse((string)color, NumberStyles.HexNumber)); else writer.WriteInt(-1);
                    if (attachment.TryGetPropertyValue("sequence", out var sequence) && sequence != null) WriteSequence(sequence.AsObject()); else writer.WriteBoolean(false);
                    break;
                case AttachmentType.Boundingbox:
                    if (attachment.TryGetPropertyValue("vertexCount", out var _vertexCount1)) vertexCount = (int)_vertexCount1; else vertexCount = 0;
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount);
                    if (nonessential) writer.WriteInt(0);
                    break;
                case AttachmentType.Mesh:
                    if (attachment.TryGetPropertyValue("path", out var path1)) writer.WriteStringRef((string)path1); else writer.WriteStringRef(null);
                    if (attachment.TryGetPropertyValue("color", out var color1)) writer.WriteInt(int.Parse((string)color1, NumberStyles.HexNumber)); else writer.WriteInt(-1);
                    vertexCount = attachment["uvs"].AsArray().Count >> 1;
                    writer.WriteVarInt(vertexCount);
                    WriteFloatArray(attachment["uvs"].AsArray());
                    WriteShortArray(attachment["triangles"].AsArray());
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount);
                    writer.WriteVarInt((int)(attachment["hull"] ?? 0f));
                    if (attachment.TryGetPropertyValue("sequence", out var sequence2) && sequence2 != null) WriteSequence(sequence2.AsObject()); else writer.WriteBoolean(false);
                    if (nonessential)
                    {
                        if (attachment.TryGetPropertyValue("edges", out var edges)) WriteShortArray(edges.AsArray()); else writer.WriteVarInt(0);
                        writer.WriteFloat((float)(attachment["width"] ?? 0f));
                        writer.WriteFloat((float)(attachment["height"] ?? 0f));
                    }

                    break;
                case AttachmentType.Linkedmesh:
                    if (attachment.TryGetPropertyValue("path", out var path2)) writer.WriteStringRef((string)path2); else writer.WriteStringRef(null);
                    if (attachment.TryGetPropertyValue("color", out var color2)) writer.WriteInt(int.Parse((string)color2, NumberStyles.HexNumber)); else writer.WriteInt(-1);
                    if (attachment.TryGetPropertyValue("skin", out var skin)) writer.WriteStringRef((string)skin); else writer.WriteStringRef(null);
                    if (attachment.TryGetPropertyValue("parent", out var parent)) writer.WriteStringRef((string)parent); else writer.WriteStringRef(null);
                    writer.WriteBoolean((bool)(attachment["timelines"] ?? true));
                    if (attachment.TryGetPropertyValue("sequence", out var sequence1) && sequence1 != null) WriteSequence(sequence1.AsObject()); else writer.WriteBoolean(false);
                    if (nonessential)
                    {
                        writer.WriteFloat((float)(attachment["width"] ?? 0f));
                        writer.WriteFloat((float)(attachment["height"] ?? 0f));
                    }

                    break;
                case AttachmentType.Path:
                    writer.WriteBoolean((bool)(attachment["closed"] ?? false));
                    writer.WriteBoolean((bool)(attachment["constantSpeed"] ?? true));
                    if (attachment.TryGetPropertyValue("vertexCount", out var _vertexCount3)) vertexCount = (int)_vertexCount3; else vertexCount = 0;
                    writer.WriteVarInt(vertexCount);
                    WriteVertices(attachment["vertices"].AsArray(), vertexCount);
                    WriteFloatArray(attachment["lengths"].AsArray());
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
                    if (attachment.TryGetPropertyValue("vertexCount", out var _vertexCount4)) vertexCount = (int)_vertexCount4; else vertexCount = 0;
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
                writer.WriteFloat((float)(data["float"] ?? 0f));
                writer.WriteString((string)(data["string"] ?? ""));
                if (data.TryGetPropertyValue("audio", out var _audio))
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
                writer.WriteVarInt(data.Count);
                if (data.TryGetPropertyValue("slots", out var slots)) WriteSlotTimelines(slots.AsObject()); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("bones", out var bones)) WriteBoneTimelines(bones.AsObject()); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("ik", out var ik)) WriteIKTimelines(ik.AsObject()); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("transform", out var transform)) WriteTransformTimelines(transform.AsObject()); else writer.WriteVarInt(0);
                if (data.TryGetPropertyValue("path", out var path)) WritePathTimelines(path.AsObject()); else writer.WriteVarInt(0);


                //if (data.TryGetPropertyValue("physics", out var physics)) WritePhysicsTimelines(physics.AsObject()); else writer.WriteVarInt(0);
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
            JsonObject frame;
            writer.WriteVarInt(slotTimelines.Count);
            foreach (var (name, _timeline) in slotTimelines)
            {
                JsonObject timeline = _timeline.AsObject();
                writer.WriteVarInt(slot2idx[name]);
                writer.WriteVarInt(timeline.Count);
                foreach (var (type, _frames) in timeline)
                {
                    var typeName = type.ToLower();
                    JsonArray frames = _frames.AsArray();
                    if (typeName == "attachment")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_ATTACHMENT);
                        writer.WriteVarInt(frames.Count);
                        foreach (JsonObject o in frames)
                        {
                            writer.WriteFloat((float)(o["time"] ?? 0f));
                            //writer.WriteStringRef((string)o["name"]);
                            if (o.TryGetPropertyValue("name", out var name1)) writer.WriteStringRef((string)name1); else writer.WriteStringRef(null);
                        }
                    }
                    else if (typeName == "rgba")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_RGBA);
                        writer.WriteVarInt(frames.Count);
                        //writer.WriteVarInt(4);
                        writer.WriteVarInt(GetBezierCount(frames) * 4);
                        frame = frames[0].AsObject();
                        writer.WriteFloat((float)(frame["time"] ?? 0f));
                        writer.WriteInt(int.Parse((string)frame["color"], NumberStyles.HexNumber));
                        for (int i = 1; i < frames.Count; i++)
                        {
                            frame = frames[i].AsObject();
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                            writer.WriteInt(int.Parse((string)frame["color"], NumberStyles.HexNumber));
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (typeName == "rgb")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_RGB);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 3);

                        frame = frames[0].AsObject();
                        var color = Convert.FromHexString((string)frame["color"]);
                        writer.WriteFloat((float)(frame["time"] ?? 0f));
                        writer.Write(color[0]); writer.Write(color[1]); writer.Write(color[2]);
                        for (int i = 1; i < frames.Count; i++)
                        {
                            frame = frames[i].AsObject();
                            color = Convert.FromHexString((string)frame["color"]);
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                            writer.Write(color[0]); writer.Write(color[1]); writer.Write(color[2]);
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (typeName == "rgba2")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_RGBA2);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 7);
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
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (typeName == "rgb2")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_RGB2);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 6);

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
                            WriteCurve(frames[i - 1].AsObject());
                        }
                    }
                    else if (typeName == "alpha")
                    {
                        writer.WriteByte(SkeletonBinary.SLOT_ALPHA);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));

                        frame = frames[0].AsObject();
                        writer.WriteFloat((float)(frame["time"] ?? 0f));
                        writer.Write((byte)((float)(frame["value"] ?? 1f) * 255));
                        for (int i = 1; i < frames.Count; i++)
                        {
                            frame = frames[i].AsObject();
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                            writer.Write((byte)((float)(frame["value"] ?? 1f) * 255));
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
                    var typeLower = type.ToLower();

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
            JsonObject frame;

            writer.WriteVarInt(ikTimelines.Count);
            foreach (var (name, _frames) in ikTimelines)
            {
                JsonArray frames = _frames.AsArray();
                writer.WriteVarInt(ik2idx[name]);
                writer.WriteVarInt(frames.Count);
                writer.WriteVarInt(GetBezierCount(frames) * 2);

                frame = frames[0].AsObject();
                writer.WriteFloat((float)(frame["time"] ?? 0f));
                writer.WriteFloat((float)(frame["mix"] ?? 1f));
                writer.WriteFloat((float)(frame["softness"] ?? 0f));
                if (frame.TryGetPropertyValue("bendPositive", out var bend)) writer.WriteSByte((sbyte)((bool)bend ? 1 : -1)); else writer.WriteSByte(1);
                writer.WriteBoolean((bool)(frame["compress"] ?? false));
                writer.WriteBoolean((bool)(frame["stretch"] ?? false));

                for (int i = 1; i < frames.Count; i++)
                {
                    frame = frames[i - 1].AsObject();
                    writer.WriteFloat((float)(frame["time"] ?? 0f));
                    writer.WriteFloat((float)(frame["mix"] ?? 1f));
                    writer.WriteFloat((float)(frame["softness"] ?? 0f));

                    WriteCurve(frames[i - 1].AsObject());

                    if (frame.TryGetPropertyValue("bendPositive", out var bend1)) writer.WriteSByte((sbyte)((bool)bend1 ? 1 : -1)); else writer.WriteSByte(1);
                    writer.WriteBoolean((bool)(frame["compress"] ?? false));
                    writer.WriteBoolean((bool)(frame["stretch"] ?? false));
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

                for (int i = 0; i < frames.Count; i++)
                {
                    JsonObject frame = frames[i].AsObject();
                    writer.WriteFloat((float)(frame["time"] ?? 0f));
                    writer.WriteFloat((float)(frame["mixRotate"] ?? 1f));
                    writer.WriteFloat((float)(frame["mixX"] ?? 1f));
                    writer.WriteFloat((float)(frame["mixY"] ?? frame["mixX"] ?? 1f));
                    writer.WriteFloat((float)(frame["mixScaleX"] ?? 1f));
                    writer.WriteFloat((float)(frame["mixScaleY"] ?? frame["mixScaleX"] ?? 1f));
                    writer.WriteFloat((float)(frame["mixShearY"] ?? 1f));
                    if (i == 0) continue;
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
                        WriteCurveFrames(frames, frames.Count, "value", 0);
                    }
                    else if (type == "spacing")
                    {
                        writer.WriteByte(SkeletonBinary.PATH_SPACING);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames));
                        WriteCurveFrames(frames, frames.Count, "value", 0);
                    }
                    else if (type == "mix")
                    {
                        writer.WriteByte(SkeletonBinary.PATH_MIX);
                        writer.WriteVarInt(frames.Count);
                        writer.WriteVarInt(GetBezierCount(frames) * 3);

                        JsonObject frame;
                        for (int i = 0; i < frames.Count; i++)
                        {
                            frame = frames[0].AsObject();
                            writer.WriteFloat((float)(frame["time"] ?? 0f));
                            writer.WriteFloat((float)(frame["mixRotate"] ?? 1f));
                            writer.WriteFloat((float)(frame["mixX"] ?? 1f));
                            writer.WriteFloat((float)(frame["mixY"] ?? frame["mixX"] ?? 1f));
                            if (i == 0) continue;
                            WriteCurve(frames[i - 1].AsObject());
                        }

                    }
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
                        JsonArray frames = [];
                        JsonObject o;
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
                                    writer.WriteFloat((float)(o["time"] ?? 0f));
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
                                        writer.WriteFloat((float)(o["time"] ?? 0f));
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
                                        writer.WriteFloat((float)(o["time"] ?? 0f));
                                        if (o.TryGetPropertyValue("mode", out var mode1)) mode = (int)Enum.Parse<SequenceMode>((string)mode1, true);
                                        if (o.TryGetPropertyValue("index", out var index1)) index = (int)index1;
                                        writer.WriteInt(((index << 4) | (mode & 0xF)));
                                        if (o.TryGetPropertyValue("delay", out var delay))
                                        {
                                            writer.WriteFloat((float)delay);
                                            lastDelay = (float)delay;
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
                writer.WriteFloat((float)(data["time"] ?? 0));
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
                writer.WriteFloat((float)(data["time"] ?? 0f));
                writer.WriteVarInt(event2idx[(string)data["name"]]);
                writer.WriteVarInt((int)(data["int"] ?? eventData["int"] ?? 0));
                writer.WriteFloat((float)(data["float"] ?? eventData["float"] ?? 0f));

                if (data.TryGetPropertyValue("string", out var @string))
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
        public void WriteFloatArray(JsonArray array)
        {
            foreach (var i in array)
            {
                writer.WriteFloat((float)i);
            }
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
        private void WriteSequence(JsonObject sequence)
        {
            //writer.WriteBoolean(null == sequence);
            if (sequence == null)
            {
                writer.WriteBoolean(false);
                return;
            }
            writer.WriteBoolean(true);
            writer.WriteVarInt((int)(sequence["count"] ?? 0));
            writer.WriteVarInt((int)(sequence["start"] ?? 1));
            writer.WriteVarInt((int)(sequence["digits"] ?? 0));
            writer.WriteVarInt((int)(sequence["setup"] ?? 0));

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

        private JsonObject ToV38(JsonObject root, bool keep)
        {
            JsonObject data = root.DeepClone().AsObject();

            //skeleton
            data["skeleton"]["spine"] = "3.8.75";
            data["reserved"] = new JsonObject()
            {
                ["spine"] = "4.1.23",
            };

            //transform
            if (data.TryGetPropertyValue("transform", out var transform))
            {
                foreach (var (name, _data) in transform.AsObject())
                {
                    JsonObject data1 = _data.AsObject();
                    JsonObject reservedItem = new JsonObject();
                    if (data1.TryGetPropertyValue("mixRotate", out var mixRotate))
                    {
                        data["rotateMix"] = (float)mixRotate;
                        data.Remove("mixRotate");
                        if (keep) reservedItem["mixRotate"] = (float)mixRotate;
                    }
                    if (data1.TryGetPropertyValue("mixX", out var mixX))
                    {
                        data["translateMix"] = (float)mixX;
                        data.Remove("mixX");
                        if (keep) reservedItem["mixX"] = (float)mixX;
                    }
                    if (data1.TryGetPropertyValue("mixY", out var mixY))
                    {
                        data.Remove("mixY");
                        if (keep) reservedItem["mixY"] = (float)mixY;
                    }
                    if (data1.TryGetPropertyValue("mixScaleX", out var mixScaleX))
                    {
                        data["scaleMix"] = (float)mixScaleX;
                        data.Remove("mixScaleX");
                        if (keep) reservedItem["mixScaleX"] = (float)mixScaleX;
                    }
                    if (data1.TryGetPropertyValue("mixScaleY", out var mixScaleY))
                    {
                        data.Remove("mixScaleY");
                        if (keep) reservedItem["mixScaleY"] = (float)mixScaleY;
                    }
                    if (data1.TryGetPropertyValue("mixShearY", out var mixShearY))
                    {
                        data["shearMix"] = (float)mixShearY;
                        data.Remove("mixShearY");
                        if (keep) reservedItem["mixShearY"] = (float)mixShearY;
                    }
                    if (reservedItem.Count > 0) data1["reserved"] = reservedItem;
                }
            }

            //path
            if (data.TryGetPropertyValue("path", out var path))
            {
                JsonObject reservedPath = [];
                data["path_reserved"] = reservedPath;
                foreach (var (name, _data) in path.AsObject())
                {
                    JsonObject data1 = _data.AsObject();
                    if (data1.TryGetPropertyValue("spacingMode", out var spacing) && ((string)spacing).ToLower() == "proportional")
                    {
                        path.AsObject().Remove(name);
                        reservedPath[name] = data1;//先加入，到时候再决定是否保留
                    }
                }

            }

            //skin
            if (data.TryGetPropertyValue("skins", out var skins))
            {
                JsonObject reservedSkin = [];
                data["skins41"] = reservedSkin;
                foreach (var (name, _data) in skins.AsObject())
                {
                    JsonObject reservedSkinItem = [];
                    reservedSkin[name] = reservedSkinItem;
                    JsonObject data1 = _data.AsObject();
                    if (data1.TryGetPropertyValue("path", out var path1))
                    {
                        if (data["path_reserved"].AsObject().ContainsKey((string)(path1.AsObject()["path"])))
                        {
                            data1.Remove("path");
                            if (keep) data1["reserved"] = new JsonObject()
                            {
                                ["path"] = (string)path1,
                            };
                        }
                    }

                }
            }

            //animation

            if (data.TryGetPropertyValue("animations", out var animations))
            {
                foreach (var (name, _animation) in animations.AsObject())
                {
                    JsonObject animation = _animation.AsObject();
                    if (animation.TryGetPropertyValue("slots", out var slots))
                    {
                        foreach (var (slotName, _slot) in slots.AsObject())
                        {
                            JsonObject slotData = _slot.AsObject();
                            JsonObject reserved = [];
                            foreach (var (timelineName, _timelines) in slotData.AsObject())
                            {
                                var timelines = _timelines.AsArray();
                                if (timelineName == "attachment") continue;
                                else if (timelineName == "aplha")
                                {
                                    slotData.Remove(timelineName);
                                    if (keep) reserved[timelineName] = timelines;
                                }
                                //一般来说，颜色的timeline是互斥的。
                                else if (timelineName == "rgba")
                                {
                                    slotData.Remove(timelineName);
                                    slotData["color"] = timelines;
                                    reserved["colorType"] = timelineName;
                                    
                                }
                                else if (timelineName == "rgb")
                                {
                                    slotData.Remove(timelineName);
                                    reserved["colorType"] = timelineName;
                                    slotData["color"] = timelines;
                                    foreach ( JsonObject timeline in timelines)
                                    {
                                        if (timeline.TryGetPropertyValue("color", out var color))
                                        {
                                            timeline["color"] = (string)color + "ff";
                                        }
                                        //emm,3.8版本的曲线只有一条,故在还原的时候应该指定是使用原来的还是全部采用新的。
                                        if (timeline.TryGetPropertyValue("curve", out var curve) && curve.GetValueKind() == JsonValueKind.Array)
                                        {
                                            curve = curve.AsArray();
                                            timeline["reserved"] = new JsonObject()
                                            {
                                                ["curve"] = curve
                                            };                                            
                                            timeline["curve"] = curve[0];
                                            timeline["c2"] = curve[1];
                                            timeline["c3"] = curve[2];
                                            timeline["c4"] = curve[3];
                                        }
                                    }
                                }
                                else if (timelineName == "rgba2")
                                {
                                    slotData.Remove(timelineName);
                                    slotData["twoColor"] = timelines;
                                    reserved["colorType"] = timelineName;
                                }
                                
                                
                            }
                            if (reserved.Count > 0) slotData["reserved"] = reserved;
                        }
                    }
                }
            }






            return data;
        }

        public override JsonObject ToVersion(JsonObject root, SpineVersion version)
        {
            //我的想法是，格式转换先统一转换到一个中心版本，然后再转换到目标版本
            //这样虽然时间是两倍，但除了中心版本以外的其他版本的版本转换代码只要写一份
            //并且虽说是两倍时间，但实际上不同版本之间的大部分内容都是一样的
            //故两倍时间和一倍时间在一般人的时间感知上基本没区别。
            //对于那些不同版本之间新增或删除的内容，当转到中心版本的时候保留
            //从中心版本转到别的版本时再给用户一个选项，由用户决定是否保留
            //因为一些原因，用户不得不使用3.8版本来打开和编辑文件。
            //当他们处理完，还要转回去，故应该保留那些内容。不过，应该警告他们，不要随意删减和重命名，否则有可能出错
            root = version switch
            {
                SpineVersion.V41 => root.DeepClone().AsObject(),
                _ => throw new NotImplementedException(),
            };
            return root;
        }



    }
}
