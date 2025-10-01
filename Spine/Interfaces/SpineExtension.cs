using Spine.Interfaces.Attachments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Interfaces
{
    public static class SpineExtension
    {
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
        /// 命中测试, 当插槽全透明或者处于禁用或者骨骼处于未激活则无法命中
        /// </summary>
        public static bool HitTest(this ISlot self, float x, float y)
        {
            if (self.A <= 0 || !self.Bone.Active || self.Disabled)
                return false;

            self.GetBounds(out var bx, out var by, out var bw, out var bh);
            return x >= bx && x <= (bx + bw) && y >= by && y <= (by + bh);
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
        /// 逐插槽的命中测试, 不会计算处于禁用或者骨骼未激活的插槽, 比整体包围盒稍微精确一些
        /// </summary>
        public static bool HitTest(this ISkeleton self, float x, float y)
        {
            return self.IterDrawOrder().Any(st => st.HitTest(x, y));
        }
    }
}
