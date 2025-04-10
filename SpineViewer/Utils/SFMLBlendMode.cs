using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Utils
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
        //public static readonly SFML.Graphics.BlendMode Normal = new(
        //    SFML.Graphics.BlendMode.Factor.SrcAlpha,
        //    SFML.Graphics.BlendMode.Factor.OneMinusSrcAlpha,
        //    SFML.Graphics.BlendMode.Equation.Add,
        //    SFML.Graphics.BlendMode.Factor.One,
        //    SFML.Graphics.BlendMode.Factor.OneMinusSrcAlpha,
        //    SFML.Graphics.BlendMode.Equation.Add
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
        //public static readonly SFML.Graphics.BlendMode Additive = new(
        //    SFML.Graphics.BlendMode.Factor.SrcAlpha,
        //    SFML.Graphics.BlendMode.Factor.One,
        //    SFML.Graphics.BlendMode.Equation.Add,
        //    SFML.Graphics.BlendMode.Factor.One,
        //    SFML.Graphics.BlendMode.Factor.One,
        //    SFML.Graphics.BlendMode.Equation.Add
        //);

        /// <summary>
        /// Normal Blend with PremultipliedAlpha
        /// <code>
        /// [res.c * res.a] = [src.c * src.a] * 1 + [dst.c * dst.a] * (1 - src.a)
        ///          res.a  =          src.a  * 1 +          dst.a  * (1 - src.a)
        /// </code>
        /// </summary>
        public static readonly SFML.Graphics.BlendMode NormalPma = new(
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Factor.OneMinusSrcAlpha,
            SFML.Graphics.BlendMode.Equation.Add,
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Factor.OneMinusSrcAlpha,
            SFML.Graphics.BlendMode.Equation.Add
        );

        /// <summary>
        /// Additive Blend with PremultipliedAlpha
        /// <code>
        /// [res.c * res.a] = [src.c * src.a] * 1 + [dst.c * dst.a] * 1
        ///          res.a  =          src.a  * 1 +          dst.a  * 1
        /// </code>
        /// </summary>
        public static readonly SFML.Graphics.BlendMode AdditivePma = new(
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Equation.Add,
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Equation.Add
        );

        /// <summary>
        /// Multiply Blend with PremultipliedAlpha
        /// <code>
        /// res.c = src.c * dst.c + dst.c * (1 - src.a)
        /// res.a = src.a *     1 + dst.a * (1 - src.a)
        /// </code>
        /// </summary>
        public static readonly SFML.Graphics.BlendMode MultiplyPma = new(
            SFML.Graphics.BlendMode.Factor.DstColor,
            SFML.Graphics.BlendMode.Factor.OneMinusSrcAlpha,
            SFML.Graphics.BlendMode.Equation.Add,
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Factor.OneMinusSrcAlpha,
            SFML.Graphics.BlendMode.Equation.Add
        );

        /// <summary>
        /// Screen Blend with PremultipliedAlpha Only
        /// <code>
        /// res.c = src.c * 1 + dst.c * (1 - src.c) = 1 - [(1 - src.c)(1 - dst.c)]
        /// res.a = src.a * 1 + dst.a * (1 - src.a)
        /// </code>
        /// </summary>
        public static readonly SFML.Graphics.BlendMode ScreenPma = new(
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Factor.OneMinusSrcColor,
            SFML.Graphics.BlendMode.Equation.Add,
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Factor.OneMinusSrcAlpha,
            SFML.Graphics.BlendMode.Equation.Add
        );

        /// <summary>
        /// 仅源像素混合模式
        /// </summary>
        public static readonly SFML.Graphics.BlendMode SourceOnly = new(
            SFML.Graphics.BlendMode.Factor.One, 
            SFML.Graphics.BlendMode.Factor.Zero
        );
    }
}
