using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    public static class Shader
    {
        /// <summary>
        /// 用于解决 PMA 和渐变动画问题的片段着色器
        /// </summary>
        private const string FRAGMENT_SHADER = (
            "uniform sampler2D t;" +
            "void main() { vec4 p = texture2D(t, gl_TexCoord[0].xy);" +
            "if (p.a > 0) p.rgb /= max(max(max(p.r, p.g), p.b), p.a);" +
            "gl_FragColor = gl_Color * p; }"
        );

        /// <summary>
        /// 针对预乘 Alpha 通道的片段着色器
        /// </summary>
        public static SFML.Graphics.Shader? FragmentShader { get; private set; }

        /// <summary>
        /// 加载 Shader, 可能会存在异常导致着色器加载失败
        /// </summary>
        /// <exception cref="SFML.LoadingFailedException"></exception>
        public static void Init()
        {
            FragmentShader = SFML.Graphics.Shader.FromString(null, null, FRAGMENT_SHADER);
        }
    }
}
