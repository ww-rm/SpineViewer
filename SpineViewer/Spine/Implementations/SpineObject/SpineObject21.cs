using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineRuntime21;
using SpineViewer.Extensions;
using SpineViewer.Utils;

namespace SpineViewer.Spine.Implementations.SpineObject
{
    [SpineImplementation(SpineVersion.V21)]
    internal class SpineObject21 : Spine.SpineObject
    {
        //private static SFML.Graphics.BlendMode GetSFMLBlendMode(BlendMode spineBlendMode)
        //{
        //    return spineBlendMode switch
        //    {
        //        BlendMode.Normal => BlendMode.Normal,
        //        BlendMode.Additive => BlendMode.Additive,
        //        BlendMode.Multiply => BlendMode.Multiply,
        //        BlendMode.Screen => BlendMode.Screen,
        //        _ => throw new NotImplementedException($"{spineBlendMode}"),
        //    };
        //}

        private static readonly Animation EmptyAnimation = new(EMPTY_ANIMATION, [], 0);

        private readonly Atlas atlas;
        private readonly SkeletonBinary? skeletonBinary;
        private readonly SkeletonJson? skeletonJson;
        private SkeletonData skeletonData;
        private AnimationStateData animationStateData;

        private Skeleton skeleton;
        private AnimationState animationState;

        // 2.1.x 不支持剪裁
        //private SkeletonClipping clipping = new(); 

        /// <summary>
        /// 所有插槽在所有皮肤中可用的附件集合
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, Attachment>> slotAttachments = [];

        public SpineObject21(string skelPath, string atlasPath) : base(skelPath, atlasPath)
        {
            try { atlas = new Atlas(AtlasPath, textureLoader); }
            catch (Exception ex) { throw new InvalidDataException($"Failed to load atlas '{atlasPath}'", ex); }
            try
            {
                // 先尝试二进制文件
                skeletonJson = null;
                skeletonBinary = new SkeletonBinary(atlas);
                skeletonData = skeletonBinary.ReadSkeletonData(SkelPath);
            }
            catch
            {
                try
                {
                    // 再尝试 Json 文件
                    skeletonBinary = null;
                    skeletonJson = new SkeletonJson(atlas);
                    skeletonData = skeletonJson.ReadSkeletonData(SkelPath);
                }
                catch
                {
                    // 都不行就报错
                    atlas.Dispose();
                    throw new InvalidDataException($"Unknown skeleton file format {SkelPath}");
                }
            }

            foreach (var sk in skeletonData.Skins)
            {
                foreach (var (k, att) in sk.Attachments)
                {
                    var slotName = skeletonData.Slots[k.Key].Name;
                    if (!slotAttachments.TryGetValue(slotName, out var attachments))
                        slotAttachments[slotName] = attachments = new() { [EMPTY_ATTACHMENT] = null };
                    attachments[att.Name] = att;
                }
            }
            SlotAttachmentNames = slotAttachments.ToFrozenDictionary(item => item.Key, item => item.Value.Keys.ToImmutableArray());
            SkinNames = skeletonData.Skins.Select(v => v.Name).ToImmutableArray();
            AnimationNames = [EMPTY_ANIMATION, .. skeletonData.Animations.Select(v => v.Name)];

            skeleton = new Skeleton(skeletonData) { Skin = new(Guid.NewGuid().ToString()) }; // 挂载一个空皮肤当作容器
            animationStateData = new AnimationStateData(skeletonData);
            animationState = new AnimationState(animationStateData);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            atlas?.Dispose();
        }

        public override string FileVersion { get => skeletonData.Version; }

        protected override float scale
        {
            get => Math.Abs(skeleton.ScaleX);
            set
            {
                skeleton.ScaleX = Math.Sign(skeleton.ScaleX) * value;
                skeleton.ScaleY = Math.Sign(skeleton.ScaleY) * value;
            }
        }

        protected override PointF position
        {
            get => new(skeleton.X, skeleton.Y);
            set
            {
                skeleton.X = value.X;
                skeleton.Y = value.Y;
            }
        }

        protected override bool flipX
        {
            get => skeleton.ScaleX < 0;
            set
            {
                if (skeleton.ScaleX > 0 && value || skeleton.ScaleX < 0 && !value)
                    skeleton.ScaleX *= -1;
            }
        }

        protected override bool flipY
        {
            get => skeleton.ScaleY < 0;
            set
            {
                if (skeleton.ScaleY > 0 && value || skeleton.ScaleY < 0 && !value)
                    skeleton.ScaleY *= -1;
            }
        }

        protected override string getSlotAttachment(string slot) => skeleton.FindSlot(slot)?.Attachment?.Name ?? EMPTY_ATTACHMENT;

        protected override void setSlotAttachment(string slot, string name)
        {
            if (slotAttachments.TryGetValue(slot, out var attachments)
                && attachments.TryGetValue(name, out var att)
                && skeleton.FindSlot(slot) is Slot s)
                s.Attachment = att;
        }

        protected override void addSkin(string name)
        {
            // default 不需要加载
            if (name != "default" && skeletonData.FindSkin(name) is Skin sk)
            {
                // XXX: 3.7 及以下不支持 AddSkin
                foreach (var (k, v) in sk.Attachments)
                    skeleton.Skin.AddAttachment(k.Key, k.Value, v);
            }
            skeleton.SetSlotsToSetupPose();
            skeleton.UpdateCache();
        }

        protected override void clearSkins()
        {
            skeleton.Skin.Attachments.Clear();
            skeleton.SetSlotsToSetupPose();
            skeleton.UpdateCache();
        }

        protected override int[] getTrackIndices() => animationState.Tracks.Select((_, i) => i).Where(i => animationState.Tracks[i] is not null).ToArray();

        protected override string getAnimation(int track) => animationState.GetCurrent(track)?.Animation.Name ?? EMPTY_ANIMATION;

        protected override void setAnimation(int track, string name)
        {
            if (name == EMPTY_ANIMATION)
                animationState.SetAnimation(track, EmptyAnimation, true);
            else if (AnimationNames.Contains(name))
                animationState.SetAnimation(track, name, true);
        }

        protected override void clearTrack(int i) => animationState.ClearTrack(i);

        public override float GetAnimationDuration(string name) { return skeletonData.FindAnimation(name)?.Duration ?? 0f; }

        protected override RectangleF getCurrentBounds()
        {
            skeleton.GetBounds(out var x, out var y, out var w, out var h);
            return new RectangleF(x, y, w, h);
        }

        protected override RectangleF getBounds()
        {
            // 初始化临时对象
            var maxDuration = 0f;
            var tmpSkeleton = new Skeleton(skeletonData) { Skin = new(Guid.NewGuid().ToString()) };
            var tmpAnimationState = new AnimationState(animationStateData);
            tmpSkeleton.ScaleX = skeleton.ScaleX;
            tmpSkeleton.ScaleY = skeleton.ScaleY;
            tmpSkeleton.X = skeleton.X;
            tmpSkeleton.Y = skeleton.Y;
            foreach (var (name, _) in skinLoadStatus.Where(e => e.Value))
            {
                foreach (var (k, v) in skeletonData.FindSkin(name).Attachments)
                    tmpSkeleton.Skin.AddAttachment(k.Key, k.Value, v);
            }
            foreach (var tr in animationState.Tracks.Select((_, i) => i).Where(i => animationState.Tracks[i] is not null))
            {
                var ani = animationState.GetCurrent(tr).Animation;
                tmpAnimationState.SetAnimation(tr, ani, true);
                if (ani.Duration > maxDuration) maxDuration = ani.Duration;
            }
            tmpSkeleton.SetSlotsToSetupPose();
            tmpAnimationState.Update(0);
            tmpAnimationState.Apply(tmpSkeleton);
            tmpSkeleton.Update(0);
            tmpSkeleton.UpdateWorldTransform();

            // 按 10 帧每秒计算边框
            var bounds = getCurrentBounds();
            float[] _ = [];
            for (float tick = 0, delta = 0.1f; tick < maxDuration; tick += delta)
            {
                tmpSkeleton.GetBounds(out var x, out var y, out var w, out var h);
                bounds = bounds.Union(new(x, y, w, h));
                tmpAnimationState.Update(delta);
                tmpAnimationState.Apply(tmpSkeleton);
                tmpSkeleton.Update(delta);
                tmpSkeleton.UpdateWorldTransform();
            }

            return bounds;
        }

        protected override void update(float delta)
        {
            animationState.Update(delta);
            animationState.Apply(skeleton);
            skeleton.Update(delta);
            skeleton.UpdateWorldTransform();
        }

        protected override void draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states)
        {
            triangleVertices.Clear();
            states.Texture = null;
            states.Shader = SFMLShader.GetSpineShader(usePma);

            // 要用 DrawOrder 而不是 Slots
            foreach (var slot in skeleton.DrawOrder)
            {
                var attachment = slot.Attachment;

                SFML.Graphics.Texture texture;

                float[] worldVertices = worldVerticesBuffer;    // 顶点世界坐标, 连续的 [x0, y0, x1, y1, ...] 坐标值
                int worldVerticesCount;                         // 等于顶点数组的长度除以 2
                int[] worldTriangleIndices;                     // 三角形索引, 从顶点坐标数组取的时候要乘以 2, 最大值是 worldVerticesCount - 1
                int worldTriangleIndicesLength;                 // 三角形索引数组长度
                float[] uvs;                                    // 纹理坐标
                float tintR = skeleton.R * slot.R;
                float tintG = skeleton.G * slot.G;
                float tintB = skeleton.B * slot.B;
                float tintA = skeleton.A * slot.A;

                if (attachment is RegionAttachment regionAttachment)
                {
                    texture = (SFML.Graphics.Texture)((AtlasRegion)regionAttachment.RendererObject).page.rendererObject;

                    regionAttachment.ComputeWorldVertices(slot.Bone, worldVertices);
                    worldVerticesCount = 4;
                    worldTriangleIndices = [0, 1, 2, 2, 3, 0];
                    worldTriangleIndicesLength = 6;
                    uvs = regionAttachment.UVs;
                    tintR *= regionAttachment.R;
                    tintG *= regionAttachment.G;
                    tintB *= regionAttachment.B;
                    tintA *= regionAttachment.A;
                }
                else if (attachment is MeshAttachment meshAttachment)
                {
                    texture = (SFML.Graphics.Texture)((AtlasRegion)meshAttachment.RendererObject).page.rendererObject;

                    if (meshAttachment.Vertices.Length > worldVertices.Length)
                        worldVertices = worldVerticesBuffer = new float[meshAttachment.Vertices.Length * 2];
                    meshAttachment.ComputeWorldVertices(slot, worldVertices);
                    worldVerticesCount = meshAttachment.Vertices.Length / 2;
                    worldTriangleIndices = meshAttachment.Triangles;
                    worldTriangleIndicesLength = meshAttachment.Triangles.Length;
                    uvs = meshAttachment.UVs;
                    tintR *= meshAttachment.R;
                    tintG *= meshAttachment.G;
                    tintB *= meshAttachment.B;
                    tintA *= meshAttachment.A;
                }
                // 2.1.x 不支持剪裁
                //else if (attachment is ClippingAttachment clippingAttachment)
                //{
                //    clipping.ClipStart(slot, clippingAttachment);
                //    continue;
                //}
                else
                {
                    //clipping.ClipEnd(slot);
                    continue;
                }

                // 似乎 2.1.x 也没有 BlendMode
                SFML.Graphics.BlendMode blendMode = slot.Data.AdditiveBlending ? SFMLBlendMode.AdditivePma : SFMLBlendMode.NormalPma;

                states.Texture ??= texture;
                if (states.BlendMode != blendMode || states.Texture != texture)
                {
                    if (triangleVertices.VertexCount > 0)
                    {
                        target.Draw(triangleVertices, states);
                        triangleVertices.Clear();
                    }
                    states.BlendMode = blendMode;
                    states.Texture = texture;
                }

                //if (clipping.IsClipping)
                //{
                //    // 这里必须单独记录 Count, 和 Items 的 Length 是不一致的
                //    clipping.ClipTriangles(worldVertices, worldVerticesCount * 2, worldTriangleIndices, worldTriangleIndicesLength, uvs);
                //    worldVertices = clipping.ClippedVertices.Items;
                //    worldVerticesCount = clipping.ClippedVertices.Count / 2;
                //    worldTriangleIndices = clipping.ClippedTriangles.Items;
                //    worldTriangleIndicesLength = clipping.ClippedTriangles.Count;
                //    uvs = clipping.ClippedUVs.Items;
                //}

                var textureSizeX = texture.Size.X;
                var textureSizeY = texture.Size.Y;

                SFML.Graphics.Vertex vertex = new();
                vertex.Color.R = (byte)(tintR * 255);
                vertex.Color.G = (byte)(tintG * 255);
                vertex.Color.B = (byte)(tintB * 255);
                vertex.Color.A = (byte)(tintA * 255);

                // 必须用 worldTriangleIndicesLength 不能直接 foreach
                for (int i = 0; i < worldTriangleIndicesLength; i++)
                {
                    var index = worldTriangleIndices[i] * 2;
                    vertex.Position.X = worldVertices[index];
                    vertex.Position.Y = worldVertices[index + 1];
                    vertex.TexCoords.X = uvs[index] * textureSizeX;
                    vertex.TexCoords.Y = uvs[index + 1] * textureSizeY;
                    triangleVertices.Append(vertex);
                }

                //clipping.ClipEnd(slot);
            }
            //clipping.ClipEnd();

            target.Draw(triangleVertices, states);
        }

        protected override void debugDraw(SFML.Graphics.RenderTarget target)
        {
            lineVertices.Clear();
            rectLineVertices.Clear();

            if (debugRegions)
            {
                SFML.Graphics.Vertex vt = new() { Color = AttachmentLineColor };
                foreach (var slot in skeleton.Slots)
                {
                    if (slot.Attachment is RegionAttachment regionAttachment)
                    {
                        regionAttachment.ComputeWorldVertices(slot.Bone, worldVerticesBuffer);

                        vt.Position.X = worldVerticesBuffer[0];
                        vt.Position.Y = worldVerticesBuffer[1];
                        lineVertices.Append(vt);

                        vt.Position.X = worldVerticesBuffer[2];
                        vt.Position.Y = worldVerticesBuffer[3];
                        lineVertices.Append(vt); lineVertices.Append(vt);

                        vt.Position.X = worldVerticesBuffer[4];
                        vt.Position.Y = worldVerticesBuffer[5];
                        lineVertices.Append(vt); lineVertices.Append(vt);

                        vt.Position.X = worldVerticesBuffer[6];
                        vt.Position.Y = worldVerticesBuffer[7];
                        lineVertices.Append(vt); lineVertices.Append(vt);

                        vt.Position.X = worldVerticesBuffer[0];
                        vt.Position.Y = worldVerticesBuffer[1];
                        lineVertices.Append(vt);
                    }
                }
            }

            if (debugMeshes)
            {
                SFML.Graphics.Vertex vt = new() { Color = MeshLineColor };
                foreach (var slot in skeleton.Slots)
                {
                    if (slot.Attachment is MeshAttachment meshAttachment)
                    {
                        if (meshAttachment.Vertices.Length > worldVerticesBuffer.Length)
                            worldVerticesBuffer = new float[meshAttachment.Vertices.Length * 2];

                        meshAttachment.ComputeWorldVertices(slot, worldVerticesBuffer);

                        var triangleIndices = meshAttachment.Triangles;
                        for (int i = 0; i < triangleIndices.Length; i += 3)
                        {
                            var idx0 = triangleIndices[i] * 2;
                            var idx1 = triangleIndices[i + 1] * 2;
                            var idx2 = triangleIndices[i + 2] * 2;

                            vt.Position.X = worldVerticesBuffer[idx0];
                            vt.Position.Y = worldVerticesBuffer[idx0 + 1];
                            lineVertices.Append(vt);

                            vt.Position.X = worldVerticesBuffer[idx1];
                            vt.Position.Y = worldVerticesBuffer[idx1 + 1];
                            lineVertices.Append(vt); lineVertices.Append(vt);

                            vt.Position.X = worldVerticesBuffer[idx2];
                            vt.Position.Y = worldVerticesBuffer[idx2 + 1];
                            lineVertices.Append(vt); lineVertices.Append(vt);

                            vt.Position.X = worldVerticesBuffer[idx0];
                            vt.Position.Y = worldVerticesBuffer[idx0 + 1];
                            lineVertices.Append(vt);
                        }
                    }
                }
            }

            if (debugMeshHulls)
            {
                SFML.Graphics.Vertex vt = new() { Color = AttachmentLineColor };
                foreach (var slot in skeleton.Slots)
                {
                    if (slot.Attachment is MeshAttachment meshAttachment)
                    {
                        if (meshAttachment.Vertices.Length > worldVerticesBuffer.Length)
                            worldVerticesBuffer = new float[meshAttachment.Vertices.Length * 2];

                        meshAttachment.ComputeWorldVertices(slot, worldVerticesBuffer);

                        var hullLength = (meshAttachment.HullLength >> 1) << 1;

                        if (debugMeshHulls && hullLength > 2)
                        {
                            vt.Position.X = worldVerticesBuffer[0];
                            vt.Position.Y = worldVerticesBuffer[1];
                            lineVertices.Append(vt);

                            for (int i = 2; i < hullLength; i += 2)
                            {
                                vt.Position.X = worldVerticesBuffer[i];
                                vt.Position.Y = worldVerticesBuffer[i + 1];
                                lineVertices.Append(vt);
                                lineVertices.Append(vt);
                            }

                            vt.Position.X = worldVerticesBuffer[0];
                            vt.Position.Y = worldVerticesBuffer[1];
                            lineVertices.Append(vt);
                        }
                    }
                }
            }

            if (debugBoundingBoxes)
            {
                throw new NotImplementedException();
            }

            if (debugPaths)
            {
                throw new NotImplementedException();
            }

            if (debugClippings) { } // 没有剪裁附件

            if (debugBounds)
            {
                var vt = new SFML.Graphics.Vertex() { Color = BoundsColor };
                var b = getCurrentBounds();

                vt.Position.X = b.Left;
                vt.Position.Y = b.Top;
                lineVertices.Append(vt);

                vt.Position.X = b.Right;
                vt.Position.Y = b.Top;
                lineVertices.Append(vt); lineVertices.Append(vt);

                vt.Position.X = b.Right;
                vt.Position.Y = b.Bottom;
                lineVertices.Append(vt); lineVertices.Append(vt);

                vt.Position.X = b.Left;
                vt.Position.Y = b.Bottom;
                lineVertices.Append(vt); lineVertices.Append(vt);

                vt.Position.X = b.Left;
                vt.Position.Y = b.Top;
                lineVertices.Append(vt);
            }

            // 骨骼线放最后画
            if (debugBones)
            {
                var width = scale;
                foreach (var bone in skeleton.Bones)
                {
                    var boneLength = bone.Data.Length;
                    var p1 = new SFML.System.Vector2f(bone.WorldX, bone.WorldY);
                    var p2 = new SFML.System.Vector2f(bone.WorldX + boneLength * bone.M00, bone.WorldY + boneLength * bone.M10);
                    AddRectLine(p1, p2, BoneLineColor, width);
                }
            }

            target.Draw(lineVertices);
            target.Draw(rectLineVertices);

            // 骨骼的点最后画, 层级处于骨骼线上面
            if (debugBones)
            {
                var radius = scale;
                foreach (var bone in skeleton.Bones)
                {
                    DrawCirclePoint(target, new(bone.WorldX, bone.WorldY), BonePointColor, radius);
                }
            }
        }
    }
}
