using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineRuntime21;
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

        private class TextureLoader : SpineRuntime21.TextureLoader
        {
            public void Load(AtlasPage page, string path)
            {
                var texture = new SFML.Graphics.Texture(path);
                if (page.magFilter == TextureFilter.Linear)
                    texture.Smooth = true;
                if (page.uWrap == TextureWrap.Repeat && page.vWrap == TextureWrap.Repeat)
                    texture.Repeated = true;

                page.rendererObject = texture;
            }

            public void Unload(object texture)
            {
                ((SFML.Graphics.Texture)texture).Dispose();
            }
        }

        private readonly static TextureLoader textureLoader = new();
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

        public SpineObject21(string skelPath, string atlasPath) : base(skelPath, atlasPath)
        {
            atlas = new Atlas(AtlasPath, textureLoader);
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
                    throw new InvalidDataException($"Unknown skeleton file format {SkelPath}");
                }
            }

            foreach (var skin in skeletonData.Skins)
                skinNames.Add(skin.Name);

            foreach (var anime in skeletonData.Animations)
                animationNames.Add(anime.Name);

            skeleton = new Skeleton(skeletonData) { Skin = new(Guid.NewGuid().ToString()) }; // 挂载一个空皮肤当作容器
            animationStateData = new AnimationStateData(skeletonData);
            animationState = new AnimationState(animationStateData);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            atlas.Dispose();
        }

        public override string FileVersion { get => skeletonData.Version; }

        protected override float scale
        {
            get
            {
                if (skeletonBinary is not null)
                    return skeletonBinary.Scale;
                else if (skeletonJson is not null)
                    return skeletonJson.Scale;
                else
                    return 1f;
            }
            set
            {
                // 保存状态
                var pos = position;
                var fX = flipX;
                var fY = flipY;
                var animations = animationState.Tracks.Where(te => te is not null).Select(te => te.Animation.Name).ToArray();

                if (skeletonBinary is not null)
                {
                    skeletonBinary.Scale = value;
                    skeletonData = skeletonBinary.ReadSkeletonData(SkelPath);
                }
                else if (skeletonJson is not null)
                {
                    skeletonJson.Scale = value;
                    skeletonData = skeletonJson.ReadSkeletonData(SkelPath);
                }

                // reload skel-dependent data
                animationStateData = new AnimationStateData(skeletonData) { DefaultMix = animationStateData.DefaultMix };
                skeleton = new Skeleton(skeletonData);
                animationState = new AnimationState(animationStateData);

                // 恢复状态
                position = pos;
                flipX = fX;
                flipY = fY;
                foreach (var s in loadedSkins) addSkin(s);
                for (int i = 0; i < animations.Length; i++) setAnimation(i, animations[i]);
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
            get => skeleton.FlipX;
            set => skeleton.FlipX = value;
        }

        protected override bool flipY
        {
            get => skeleton.FlipY;
            set => skeleton.FlipY = value;
        }

        protected override void addSkin(string name)
        {
            if (skeletonData.FindSkin(name) is Skin sk)
            {
                // XXX: 3.7 及以下不支持 AddSkin
                foreach (var (k, v) in sk.Attachments)
                    skeleton.Skin.AddAttachment(k.Key, k.Value, v);
            }
            skeleton.SetSlotsToSetupPose();
        }

        protected override void clearSkin()
        {
            skeleton.Skin.Attachments.Clear();
            skeleton.SetSlotsToSetupPose();
        }

        protected override int[] getTrackIndices() => animationState.Tracks.Select((_, i) => i).Where(i => animationState.Tracks[i] is not null).ToArray();

        protected override string getAnimation(int track) => animationState.GetCurrent(track)?.Animation.Name ?? EMPTY_ANIMATION;

        protected override void setAnimation(int track, string name)
        {
            if (name == EMPTY_ANIMATION)
                animationState.SetAnimation(track, EmptyAnimation, false);
            else if (animationNames.Contains(name))
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
            tmpSkeleton.FlipX = skeleton.FlipX;
            tmpSkeleton.FlipY = skeleton.FlipY;
            tmpSkeleton.X = skeleton.X;
            tmpSkeleton.Y = skeleton.Y;
            foreach (var name in loadedSkins)
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

            // 切成 100 帧获取边界最大值
            var bounds = getCurrentBounds();
            for (float tick = 0, delta = maxDuration / 100; tick < maxDuration; tick += delta)
            {
                tmpSkeleton.GetBounds(out var x, out var y, out var w, out var h);
                if (x < bounds.X) bounds.X = x;
                if (y < bounds.Y) bounds.Y = y;
                if (w > bounds.Width) bounds.Width = w;
                if (h > bounds.Height) bounds.Height = h;
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
