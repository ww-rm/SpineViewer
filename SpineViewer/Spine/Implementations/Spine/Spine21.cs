using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineRuntime21;

namespace SpineViewer.Spine.Implementations.Spine
{
    [SpineImplementation(Version.V21)]
    internal class Spine21 : SpineViewer.Spine.Spine
    {
        private static readonly Animation EmptyAnimation = new(EMPTY_ANIMATION, [], 0);

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
                page.width = (int)texture.Size.X;
                page.height = (int)texture.Size.Y;
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

        // 2.1.x 不支持剪裁
        //private SkeletonClipping clipping = new(); 

        public Spine21(string skelPath, string? atlasPath = null) : base(skelPath, atlasPath)
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

            animationStateData = new AnimationStateData(skeletonData);
            skeleton = new Skeleton(skeletonData);
            animationState = new AnimationState(animationStateData);

            foreach (var anime in skeletonData.Animations)
                animationNames.Add(anime.Name);
            CurrentAnimation = DefaultAnimationName;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            atlas.Dispose();
        }

        public override string FileVersion { get => skeletonData.Version; }

        public override float Scale
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
                var position = Position;
                var flipX = FlipX;
                var flipY = FlipY;
                var savedTrack0 = animationState.GetCurrent(0);

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
                Position = position;
                FlipX = flipX;
                FlipY = flipY;

                // 恢复原本 Track0 上所有动画
                if (savedTrack0 is not null)
                {
                    var entry = animationState.SetAnimation(0, savedTrack0.Animation.Name, true);
                    entry.Time = savedTrack0.Time;
                    // 2.1.x 没有提供 Next 访问器，故放弃还原后续动画，问题不大，因为预览画面目前不需要连续播放不同动画，只需要循环同一个动画
                    //var savedEntry = savedTrack0.Next;
                    //while (savedEntry is not null)
                    //{
                    //    entry = animationState.AddAnimation(0, savedEntry.Animation.Name, true, 0);
                    //    entry.Time = savedEntry.TrackTime;
                    //    savedEntry = savedEntry.Next;
                    //}
                }
            }
        }

        public override PointF Position 
        { 
            get => new(skeleton.X, skeleton.Y); 
            set 
            { 
                skeleton.X = value.X; 
                skeleton.Y = value.Y; 
            } 
        }

        public override bool FlipX
        {
            get => skeleton.FlipX;
            set => skeleton.FlipX = value;
        }

        public override bool FlipY
        {
            get => skeleton.FlipY;
            set => skeleton.FlipY = value;
        }

        public override string CurrentAnimation
        {
            get => animationState.GetCurrent(0)?.Animation.Name ?? EMPTY_ANIMATION;
            set
            {
                if (value == EMPTY_ANIMATION)
                    animationState.SetAnimation(0, EmptyAnimation, false);
                else if (animationNames.Contains(value))
                    animationState.SetAnimation(0, value, true);
                Update(0);
            }
        }

        public override RectangleF Bounds
        {
            get
            {
                float[] temp = new float[8];
                var drawOrderItems = skeleton.DrawOrder;
                float minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
                for (int i = 0, n = skeleton.DrawOrder.Count; i < n; i++)
                {
                    Slot slot = drawOrderItems[i];
                    int verticesLength = 0;
                    float[] vertices = null;
                    Attachment attachment = slot.Attachment;
                    var regionAttachment = attachment as RegionAttachment;
                    if (regionAttachment != null)
                    {
                        verticesLength = 8;
                        vertices = temp;
                        if (vertices.Length < 8) vertices = temp = new float[8];
                        regionAttachment.ComputeWorldVertices(slot.Bone, temp);
                    }
                    else
                    {
                        var meshAttachment = attachment as MeshAttachment;
                        if (meshAttachment != null)
                        {
                            MeshAttachment mesh = meshAttachment;
                            verticesLength = mesh.Vertices.Length;
                            vertices = temp;
                            if (vertices.Length < verticesLength) vertices = temp = new float[verticesLength];
                            mesh.ComputeWorldVertices(slot, temp);
                        }
                    }

                    if (vertices != null)
                    {
                        for (int ii = 0; ii < verticesLength; ii += 2)
                        {
                            float vx = vertices[ii], vy = vertices[ii + 1];
                            minX = Math.Min(minX, vx);
                            minY = Math.Min(minY, vy);
                            maxX = Math.Max(maxX, vx);
                            maxY = Math.Max(maxY, vy);
                        }
                    }
                }
                return new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }
        }

        public override float GetAnimationDuration(string name) { return skeletonData.FindAnimation(name)?.Duration ?? 0f; }

        public override void Update(float delta)
        {
            skeleton.Update(delta);
            animationState.Update(delta);
            animationState.Apply(skeleton);
            skeleton.UpdateWorldTransform();
        }

        //private SFML.Graphics.BlendMode GetSFMLBlendMode(BlendMode spineBlendMode)
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

        public override void Draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states)
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
                SFML.Graphics.BlendMode blendMode = slot.Data.AdditiveBlending ? BlendModeSFML.Additive : BlendModeSFML.Normal;

                states.Texture ??= texture;
                if (states.BlendMode != blendMode || states.Texture != texture)
                {
                    if (vertexArray.VertexCount > 0)
                    {
                        if (UsePremultipliedAlpha && (states.BlendMode == BlendModeSFML.Normal || states.BlendMode == BlendModeSFML.Additive))
                            states.Shader = FragmentShader;
                        else
                            states.Shader = null;
                        target.Draw(vertexArray, states);
                        vertexArray.Clear();
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
                    vertexArray.Append(vertex);
                }

                //clipping.ClipEnd(slot);
            }

            if (UsePremultipliedAlpha && (states.BlendMode == BlendModeSFML.Normal || states.BlendMode == BlendModeSFML.Additive))
                states.Shader = FragmentShader;
            else
                states.Shader = null;
            target.Draw(vertexArray, states);
            //clipping.ClipEnd();
        }
    }
}
