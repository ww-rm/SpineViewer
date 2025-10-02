using NLog;
using SkiaSharp;
using Spine.Interfaces.Attachments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Interfaces
{
    /// <summary>
    /// 命中测试等级枚举值
    /// </summary>
    public enum HitTestLevel { None, Bounds, Meshes, Pixels }

    public static class SpineExtension
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 命中检测精确度等级
        /// </summary>
        public static HitTestLevel HitTestLevel { get; set; }

        /// <summary>
        /// 命中测试时输出命中的插槽名称
        /// </summary>
        public static bool LogHitSlots { get; set; }

        /// <summary>
        /// 获取当前状态包围盒
        /// </summary>
        public static void GetBounds(this ISlot self, out float x, out float y, out float w, out float h)
        {
            float[] vertices = new float[8];
            int verticesLength = 0;
            var attachment = self.Attachment;
            switch (attachment)
            {
                case IRegionAttachment:
                case IMeshAttachment:
                    verticesLength = attachment.ComputeWorldVertices(self, ref vertices);
                    break;
                default:
                    break;
            }

            if (verticesLength > 0)
            {
                float minX = int.MaxValue;
                float minY = int.MaxValue;
                float maxX = int.MinValue;
                float maxY = int.MinValue;
                for (int ii = 0; ii + 1 < verticesLength; ii += 2)
                {
                    float vx = vertices[ii];
                    float vy = vertices[ii + 1];
                    minX = Math.Min(minX, vx);
                    minY = Math.Min(minY, vy);
                    maxX = Math.Max(maxX, vx);
                    maxY = Math.Max(maxY, vy);
                }
                x = minX;
                y = minY;
                w = maxX - minX;
                h = maxY - minY;
            }
            else
            {
                x = self.Bone.WorldX;
                y = self.Bone.WorldY;
                w = 0;
                h = 0;
            }
        }

        /// <summary>
        /// 获取当前状态包围盒
        /// </summary>
        public static void GetBounds(this ISkeleton self, out float x, out float y, out float w, out float h)
        {
            float minX = int.MaxValue;
            float minY = int.MaxValue;
            float maxX = int.MinValue;
            float maxY = int.MinValue;
            foreach (var slot in self.IterDrawOrder())
            {
                if (slot.A <= 0 || !slot.Bone.Active || slot.Disabled)
                    continue;

                float[] vertices = new float[8];
                int verticesLength = 0;
                var attachment = slot.Attachment;
                switch (attachment)
                {
                    case IRegionAttachment:
                    case IMeshAttachment:
                        verticesLength = attachment.ComputeWorldVertices(slot, ref vertices);
                        break;
                    default:
                        break;
                }

                for (int ii = 0; ii + 1 < verticesLength; ii += 2)
                {
                    float vx = vertices[ii];
                    float vy = vertices[ii + 1];
                    minX = Math.Min(minX, vx);
                    minY = Math.Min(minY, vy);
                    maxX = Math.Max(maxX, vx);
                    maxY = Math.Max(maxY, vy);
                }
            }
            x = minX;
            y = minY;
            w = maxX - minX;
            h = maxY - minY;
        }

        /// <summary>
        /// 命中测试, 当插槽全透明或者处于禁用或者骨骼处于未激活则无法命中
        /// </summary>
        /// <param name="precise">是否精确命中检测, 否则仅使用包围盒进行命中检测</param>
        /// <param name="cache">调用方管理的缓存表</param>
        public static bool HitTest(this ISlot self, float x, float y, Dictionary<SFML.Graphics.Texture, SFML.Graphics.Image> cache = null)
        {
            if (self.A <= 0 || !self.Bone.Active || self.Disabled)
                return false;

            if (HitTestLevel == HitTestLevel.None || HitTestLevel == HitTestLevel.Bounds)
            {
                self.GetBounds(out var bx, out var by, out var bw, out var bh);
                return x >= bx && x <= (bx + bw) && y >= by && y <= (by + bh);
            }
            else if (HitTestLevel == HitTestLevel.Meshes || HitTestLevel == HitTestLevel.Pixels)
            {
                float[] vertices = new float[8];
                int[] triangles;
                float[] uvs;
                SFML.Graphics.Texture tex;

                switch (self.Attachment)
                {
                    case IRegionAttachment regionAttachment:
                        _ = regionAttachment.ComputeWorldVertices(self, ref vertices);
                        triangles = regionAttachment.Triangles;
                        uvs = regionAttachment.UVs;
                        tex = regionAttachment.RendererObject;
                        break;
                    case IMeshAttachment meshAttachment:
                        _ = meshAttachment.ComputeWorldVertices(self, ref vertices);
                        triangles = meshAttachment.Triangles;
                        uvs = meshAttachment.UVs;
                        tex = meshAttachment.RendererObject;
                        break;
                    default:
                        return false;
                }

                var trianglesLength = triangles.Length;
                for (int i = 0; i + 2 < trianglesLength; i += 3)
                {
                    var idx0 = triangles[i] << 1;
                    var idx1 = triangles[i + 1] << 1;
                    var idx2 = triangles[i + 2] << 1;

                    float x0 = vertices[idx0] - x, y0 = vertices[idx0 + 1] - y;
                    float x1 = vertices[idx1] - x, y1 = vertices[idx1 + 1] - y;
                    float x2 = vertices[idx2] - x, y2 = vertices[idx2 + 1] - y;

                    float c0 = Cross(x0, y0, x1, y1);
                    float c1 = Cross(x1, y1, x2, y2);
                    float c2 = Cross(x2, y2, x0, y0);

                    // 判断是否全部同号 (或为 0, 点在边上)
                    if ((c0 >= 0 && c1 >= 0 && c2 >= 0) || (c0 <= 0 && c1 <= 0 && c2 <= 0))
                    {
                        if (HitTestLevel == HitTestLevel.Meshes)
                            return true;

                        float u0 = uvs[idx0], v0 = uvs[idx0 + 1];
                        float u1 = uvs[idx1], v1 = uvs[idx1 + 1];
                        float u2 = uvs[idx2], v2 = uvs[idx2 + 1];
                        float inv = 1 / (c0 + c1 + c2);
                        float w0 = c1 * inv;
                        float w1 = c2 * inv;
                        float w2 = c0 * inv;
                        float u = u0 * w0 + u1 * w1 + u2 * w2;
                        float v = v0 * w0 + v1 * w1 + v2 * w2;

                        SFML.Graphics.Image img = null;
                        if (cache is not null)
                        {
                            if (!cache.TryGetValue(tex, out img))
                            {
                                img = cache[tex] = tex.CopyToImage();
                            }
                        }
                        else
                        {
                            img = tex.CopyToImage();
                        }

                        var texSize = img.Size;
                        var pixel = img.GetPixel((uint)(u * texSize.X), (uint)(v * texSize.Y));
                        bool hit = pixel.A > 0;

                        // 无缓存需要立即释放资源
                        if (cache is null)
                        {
                            img.Dispose();
                        }

                        return hit;
                    }
                }
                return false;
            }
            else
            {
                throw new NotImplementedException(HitTestLevel.ToString());
            }
        }

        /// <summary>
        /// 逐插槽的命中测试, 命中后会提前返回结果中止计算
        /// </summary>
        public static bool HitTest(this ISkeleton self, float x, float y)
        {
            if (HitTestLevel == HitTestLevel.None)
            {
                self.GetBounds(out var bx, out var by, out var bw, out var bh);
                return x >= bx && x <= (bx + bw) && y >= by && y <= (by + bh);
            }

            var cache = new Dictionary<SFML.Graphics.Texture, SFML.Graphics.Image>();
            bool hit = false;
            string hitSlotName = "";
            foreach (var st in self.IterDrawOrder().Reverse())
            {
                if (st.HitTest(x, y, cache))
                {
                    hit = true;
                    hitSlotName = st.Name;
                    break;
                }
            }
            foreach (var img in cache.Values) img.Dispose();

            if (hit && LogHitSlots)
            {
                _logger.Debug("Hit ({0}): [{1}]", self.Name, hitSlotName);
            }
            return hit;
        }

        /// <summary>
        /// 向量叉积
        /// </summary>
        private static float Cross(float x0, float y0, float x1, float y1) => x0 * y1 - y0 * x1;
    }
}
