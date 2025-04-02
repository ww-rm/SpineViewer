using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineRuntime36;

namespace SpineViewer.Spine.Implementations.Spine
{
    [SpineImplementation(SpineVersion.V36)]
    internal class Spine36 : SpineViewer.Spine.Spine
    {
        private static readonly Animation EmptyAnimation = new(EMPTY_ANIMATION, [], 0);

        private class TextureLoader : SpineRuntime36.TextureLoader
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
        private static TextureLoader textureLoader = new();

        private Atlas atlas;
        private SkeletonBinary? skeletonBinary;
        private SkeletonJson? skeletonJson;
        private SkeletonData skeletonData;
        private AnimationStateData animationStateData;

        private Skeleton skeleton;
        private AnimationState animationState;

        private SkeletonClipping clipping = new();

        public Spine36(string skelPath, string atlasPath) : base(skelPath, atlasPath)
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

            skeleton = new Skeleton(skeletonData);
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
                var sk = skin;

                var val = Math.Max(value, SCALE_MIN);
                if (skeletonBinary is not null)
                {
                    skeletonBinary.Scale = val;
                    skeletonData = skeletonBinary.ReadSkeletonData(SkelPath);
                }
                else if (skeletonJson is not null)
                {
                    skeletonJson.Scale = val;
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
                for (int i = 0; i < animations.Length; i++) setAnimation(i, animations[i]);
                skin = sk;
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

        protected override string skin
        {
            get => skeleton.Skin?.Name ?? "default";
            set
            {
                if (!skinNames.Contains(value)) return;
                skeleton.SetSkin(value);
                skeleton.SetSlotsToSetupPose();
            }
        }

        protected override int[] getTrackIndices() => animationState.Tracks.Select((_, i) => i).Where(i => animationState.Tracks.Items[i] is not null).ToArray();

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

        protected override RectangleF bounds
        {
            get
            {
                float[] _ = [];
                skeleton.GetBounds(out var x, out var y, out var w, out var h, ref _);
                return new RectangleF(x, y, w, h);
            }
        }

        protected override void update(float delta)
        {
            animationState.Update(delta);
            animationState.Apply(skeleton);
            skeleton.Update(delta);
            skeleton.UpdateWorldTransform();
        }

        private SFML.Graphics.BlendMode GetSFMLBlendMode(BlendMode spineBlendMode)
        {
            return spineBlendMode switch
            {
                BlendMode.Normal => BlendModeSFML.Normal,
                BlendMode.Additive => BlendModeSFML.Additive,
                BlendMode.Multiply => BlendModeSFML.Multiply,
                BlendMode.Screen => BlendModeSFML.Screen,
                _ => throw new NotImplementedException($"{spineBlendMode}"),
            };
        }

        protected override void draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states)
        {
            vertexArray.Clear();
            states.Texture = null;

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

                    regionAttachment.ComputeWorldVertices(slot.Bone, worldVertices, 0);
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

                    if (meshAttachment.WorldVerticesLength > worldVertices.Length)
                        worldVertices = worldVerticesBuffer = new float[meshAttachment.WorldVerticesLength * 2];
                    meshAttachment.ComputeWorldVertices(slot, worldVertices);
                    worldVerticesCount = meshAttachment.WorldVerticesLength / 2;
                    worldTriangleIndices = meshAttachment.Triangles;
                    worldTriangleIndicesLength = meshAttachment.Triangles.Length;
                    uvs = meshAttachment.UVs;
                    tintR *= meshAttachment.R;
                    tintG *= meshAttachment.G;
                    tintB *= meshAttachment.B;
                    tintA *= meshAttachment.A;
                }
                else if (attachment is ClippingAttachment clippingAttachment)
                {
                    clipping.ClipStart(slot, clippingAttachment);
                    continue;
                }
                else
                {
                    clipping.ClipEnd(slot);
                    continue;
                }

                SFML.Graphics.BlendMode blendMode = GetSFMLBlendMode(slot.Data.BlendMode);

                states.Texture ??= texture;
                if (states.BlendMode != blendMode || states.Texture != texture)
                {
                    if (vertexArray.VertexCount > 0)
                    {
                        if (usePremultipliedAlpha && (states.BlendMode == BlendModeSFML.Normal || states.BlendMode == BlendModeSFML.Additive))
                            states.Shader = Shader.FragmentShader;
                        else
                            states.Shader = null;

                        // 调试纹理
                        if (!isDebug || debugTexture)
                            target.Draw(vertexArray, states);

                        vertexArray.Clear();
                    }
                    states.BlendMode = blendMode;
                    states.Texture = texture;
                }

                if (clipping.IsClipping)
                {
                    // 这里必须单独记录 Count, 和 Items 的 Length 是不一致的
                    clipping.ClipTriangles(worldVertices, worldVerticesCount * 2, worldTriangleIndices, worldTriangleIndicesLength, uvs);
                    worldVertices = clipping.ClippedVertices.Items;
                    worldVerticesCount = clipping.ClippedVertices.Count / 2;
                    worldTriangleIndices = clipping.ClippedTriangles.Items;
                    worldTriangleIndicesLength = clipping.ClippedTriangles.Count;
                    uvs = clipping.ClippedUVs.Items;
                }

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
                    vertexArray.Append(vertex);
                }

                clipping.ClipEnd(slot);
            }
            clipping.ClipEnd();

            if (usePremultipliedAlpha && (states.BlendMode == BlendModeSFML.Normal || states.BlendMode == BlendModeSFML.Additive))
                states.Shader = Shader.FragmentShader;
            else
                states.Shader = null;

            // 调试纹理
            if (!isDebug || debugTexture)
                target.Draw(vertexArray, states);

            // 包围盒
            if (isDebug && isSelected && debugBounds)
            {
                var b = bounds;
                boundsVertices[0] = boundsVertices[4] = new(new(b.Left, b.Top), BoundsColor);
                boundsVertices[1] = new(new(b.Right, b.Top), BoundsColor);
                boundsVertices[2] = new(new(b.Right, b.Bottom), BoundsColor);
                boundsVertices[3] = new(new(b.Left, b.Bottom), BoundsColor);
                target.Draw(boundsVertices);
            }
        }
    }
}
