using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    /// <summary>
    /// SFML 混合模式
    /// </summary>
    public static class BlendModeSFML
    {
        /// <summary>
        /// Alpha Blend
        /// <code>
        /// res.c = src.c * src.a + dst.c * (1 - src.a)
        /// res.a = src.a * 1     + dst.a * (1 - src.a)
        /// </code>
        /// </summary>
        public static SFML.Graphics.BlendMode Normal = SFML.Graphics.BlendMode.Alpha;

        /// <summary>
        /// Additive Blend
        /// <code>
        /// res.c = src.c * src.a + dst.c * 1
        /// res.a = src.a * 1     + dst.a * 1
        /// </code>
        /// </summary>
        public static SFML.Graphics.BlendMode Additive = SFML.Graphics.BlendMode.Add;

        /// <summary>
        /// Multiply Blend (PremultipliedAlpha Only)
        /// <code>
        /// res.c = src.c * dst.c + dst.c * (1 - src.a)
        /// res.a = src.a * 1     + dst.a * (1 - src.a)
        /// </code>
        /// </summary>
        public static SFML.Graphics.BlendMode Multiply = new(
            SFML.Graphics.BlendMode.Factor.DstColor,
            SFML.Graphics.BlendMode.Factor.OneMinusSrcAlpha,
            SFML.Graphics.BlendMode.Equation.Add,
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Factor.OneMinusSrcAlpha,
            SFML.Graphics.BlendMode.Equation.Add
        );

        /// <summary>
        /// Screen Blend (PremultipliedAlpha Only)
        /// <code>
        /// res.c = src.c * 1 + dst.c * (1 - src.c) = 1 - [(1 - src.c)(1 - dst.c)]
        /// res.a = src.a * 1 + dst.a * (1 - src.a)
        /// </code>
        /// </summary>
        public static SFML.Graphics.BlendMode Screen = new(
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Factor.OneMinusSrcColor,
            SFML.Graphics.BlendMode.Equation.Add,
            SFML.Graphics.BlendMode.Factor.One,
            SFML.Graphics.BlendMode.Factor.OneMinusSrcAlpha,
            SFML.Graphics.BlendMode.Equation.Add
        );
    }
}
