using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace Spine.Utils
{
    public static class SFMLShader
    {
        /// <summary>
        /// 用于非预乘纹理的 fragment shader, 乘上了插值后的透明度用于实现透明度变化(插值预乘), 并且输出预乘后的像素值
        /// </summary>
        private const string FRAGMENT_VertexAlpha = 
            "uniform sampler2D t;" +
            "void main() { vec4 p = texture(t, gl_TexCoord[0].xy);" +
            "p.rgb *= p.a * gl_Color.a;" +
            "gl_FragColor = gl_Color * p; }"
        ;

        /// <summary>
        /// 用于预乘纹理的 fragment shader, 乘上了插值后的透明度用于实现透明度变化(插值预乘)
        /// </summary>
        private const string FRAGMENT_VertexAlphaPma = 
            "uniform sampler2D t;" +
            "void main() { vec4 p = texture(t, gl_TexCoord[0].xy);" +
            "p.rgb *= gl_Color.a;" +
            "gl_FragColor = gl_Color * p; }"
        ;

        /// <summary>
        /// 预乘转非预乘 fragment shader
        /// </summary>
        private const string FRAGMENT_InvPma = 
            "uniform sampler2D t;" +
            "void main() { vec4 p = texture(t, gl_TexCoord[0].xy);" +
            "if (p.a > 0) p.rgb /= max(max(max(p.r, p.g), p.b), p.a);" +
            "gl_FragColor = p; }"
        ;

        /// <summary>
        /// 考虑了顶点透明度变化的着色器, 输入是非预乘纹理像素, 输出是预乘像素
        /// </summary>
        public static Shader? VertexAlpha { get; }

        /// <summary>
        /// 考虑了顶点透明度变化的着色器, 输入和输出均是预乘像素值
        /// </summary>
        public static Shader? VertexAlphaPma { get; }

        /// <summary>
        /// 反预乘着色器, 用于得到正确透明度的非预乘画面
        /// </summary>
        public static Shader? InversePma { get; }

        /// <summary>
        /// 加载 Shader, 可能会存在异常导致着色器加载失败, 会在日志中输出信息
        /// </summary>
        static SFMLShader()
        {
            try
            {
                VertexAlpha = Shader.FromString(null, null, FRAGMENT_VertexAlpha);
                VertexAlphaPma = Shader.FromString(null, null, FRAGMENT_VertexAlphaPma);
                InversePma = Shader.FromString(null, null, FRAGMENT_InvPma);
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Trace(ex.ToString());
                logger.Error("Failed to init shaders, {0}", ex.Message);
            }
        }
    }
}
