﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    public static class Shader
    {
        /// <summary>
        /// 用于非预乘纹理的 fragment shader, 乘上了插值后的透明度用于实现透明度变化(插值预乘), 并且输出预乘后的像素值
        /// </summary>
        private const string FRAGMENT_VertexAlpha = (
            "uniform sampler2D t;" +
            "void main() { vec4 p = texture(t, gl_TexCoord[0].xy);" +
            "p.rgb *= p.a * gl_Color.a;" +
            "gl_FragColor = gl_Color * p; }"
        );

        /// <summary>
        /// 用于预乘纹理的 fragment shader, 乘上了插值后的透明度用于实现透明度变化(插值预乘)
        /// </summary>
        private const string FRAGMENT_VertexAlphaPma = (
            "uniform sampler2D t;" +
            "void main() { vec4 p = texture(t, gl_TexCoord[0].xy);" +
            "p.rgb *= gl_Color.a;" +
            "gl_FragColor = gl_Color * p; }"
        );

        /// <summary>
        /// 预乘转非预乘 fragment shader
        /// </summary>
        private const string FRAGMENT_PmaInv = (
            "uniform sampler2D t;" +
            "void main() { vec4 p = texture(t, gl_TexCoord[0].xy);" +
            "p.rgb *= gl_Color.a;" +
            "gl_FragColor = gl_Color * p; }"
        );

        /// <summary>
        /// 考虑了顶点透明度变化的着色器, 输入是非预乘纹理像素, 输出是预乘像素
        /// </summary>
        private static SFML.Graphics.Shader? VertexAlpha = null;

        /// <summary>
        /// 考虑了顶点透明度变化的着色器, 输入和输出均是预乘像素值
        /// </summary>
        private static SFML.Graphics.Shader? VertexAlphaPma = null;

        /// <summary>
        /// 加载 Shader, 可能会存在异常导致着色器加载失败
        /// </summary>
        /// <exception cref="SFML.LoadingFailedException"></exception>
        public static void Init()
        {
            VertexAlpha = SFML.Graphics.Shader.FromString(null, null, FRAGMENT_VertexAlpha);
            VertexAlphaPma = SFML.Graphics.Shader.FromString(null, null, FRAGMENT_VertexAlphaPma);
        }

        /// <summary>
        /// 获取合适的着色器
        /// </summary>
        /// <param name="pma">纹理是否是预乘的</param>
        /// <param name="twoColor">是否是双色着色的(TODO)</param>
        public static SFML.Graphics.Shader? GetShader(bool pma, bool twoColor = false)
        {
            if (pma)
                return VertexAlphaPma;
            else
                return VertexAlpha;
        }
    }
}
