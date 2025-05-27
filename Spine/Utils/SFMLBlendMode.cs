using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace Spine.Utils
{
    /// <summary>
    /// SFML 混合模式, 预乘模式下输入和输出的像素值都是预乘的
    /// </summary>
    public static class SFMLBlendMode
    {
        ///// <summary>
        ///// Normal Blend, 无预乘, 仅在 dst.a 是 1 时得到正确结果, 其余情况是有偏结果
        ///// <para>当 <c>src.c &lt; dst.c</c> 时, 结果偏大, 例如 src 是半透明纯黑, dst 是全透明纯白</para>
        ///// <para>当 <c>src.c &gt; dst.c</c> 时, 结果偏小, 例如 src 是半透明纯白, dst 是全透明纯黑</para>
        ///// <code>
        ///// res.c = src.c * src.a + dst.c * (1 - src.a)
        ///// res.a = src.a *     1 + dst.a * (1 - src.a)
        ///// </code>
        ///// </summary>
        //public static readonly BlendMode Normal = new(
        //    BlendMode.Factor.SrcAlpha,
        //    BlendMode.Factor.OneMinusSrcAlpha,
        //    BlendMode.Equation.Add,
        //    BlendMode.Factor.One,
        //    BlendMode.Factor.OneMinusSrcAlpha,
        //    BlendMode.Equation.Add
        //);

        ///// <summary>
        ///// Additive Blend, 无预乘, 仅在 dst.a 是 1 时得到正确结果, 其余情况是有偏结果
        ///// <para>当 <c>src.a + dst.a >= 1</c> 时, 结果偏大, 例如 src 是不透明纯黑, dst 是全透明纯白</para>
        ///// <para>当 <c>src.a + dst.a &lt; 1</c> 时, 结果偏差方式类似 <see cref="Normal"/>, 均可假设 dst 是全透明纯白进行判断</para>
        ///// <code>
        ///// res.c = src.c * src.a + dst.c * 1
        ///// res.a = src.a *     1 + dst.a * 1
        ///// </code>
        ///// </summary>
        //public static readonly BlendMode Additive = new(
        //    BlendMode.Factor.SrcAlpha,
        //    BlendMode.Factor.One,
        //    BlendMode.Equation.Add,
        //    BlendMode.Factor.One,
        //    BlendMode.Factor.One,
        //    BlendMode.Equation.Add
        //);

        /// <summary>
        /// Normal Blend with PremultipliedAlpha
        /// <code>
        /// [res.c * res.a] = [src.c * src.a] * 1 + [dst.c * dst.a] * (1 - src.a)
        ///          res.a  =          src.a  * 1 +          dst.a  * (1 - src.a)
        /// </code>
        /// </summary>
        public static readonly BlendMode NormalPma = new(
            BlendMode.Factor.One,
            BlendMode.Factor.OneMinusSrcAlpha,
            BlendMode.Equation.Add,
            BlendMode.Factor.One,
            BlendMode.Factor.OneMinusSrcAlpha,
            BlendMode.Equation.Add
        );

        /// <summary>
        /// Additive Blend with PremultipliedAlpha
        /// <code>
        /// [res.c * res.a] = [src.c * src.a] * 1 + [dst.c * dst.a] * 1
        ///          res.a  =          src.a  * 1 +          dst.a  * 1
        /// </code>
        /// </summary>
        public static readonly BlendMode AdditivePma = new(
            BlendMode.Factor.One,
            BlendMode.Factor.One,
            BlendMode.Equation.Add,
            BlendMode.Factor.One,
            BlendMode.Factor.One,
            BlendMode.Equation.Add
        );

        /// <summary>
        /// Multiply Blend with PremultipliedAlpha
        /// <code>
        /// res.c = src.c * dst.c + dst.c * (1 - src.a)
        /// res.a = src.a *     1 + dst.a * (1 - src.a)
        /// </code>
        /// </summary>
        public static readonly BlendMode MultiplyPma = new(
            BlendMode.Factor.DstColor,
            BlendMode.Factor.OneMinusSrcAlpha,
            BlendMode.Equation.Add,
            BlendMode.Factor.One,
            BlendMode.Factor.OneMinusSrcAlpha,
            BlendMode.Equation.Add
        );

        /// <summary>
        /// Screen Blend with PremultipliedAlpha Only
        /// <code>
        /// res.c = src.c * 1 + dst.c * (1 - src.c) = 1 - [(1 - src.c)(1 - dst.c)]
        /// res.a = src.a * 1 + dst.a * (1 - src.a)
        /// </code>
        /// </summary>
        public static readonly BlendMode ScreenPma = new(
            BlendMode.Factor.One,
            BlendMode.Factor.OneMinusSrcColor,
            BlendMode.Equation.Add,
            BlendMode.Factor.One,
            BlendMode.Factor.OneMinusSrcAlpha,
            BlendMode.Equation.Add
        );

        /// <summary>
        /// 仅源像素混合模式
        /// </summary>
        public static readonly BlendMode SourceOnly = new(
            BlendMode.Factor.One, 
            BlendMode.Factor.Zero
        );
    }
}
