using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.SpineWrappers
{
    /// <summary>
    /// 实现不同版本的 TextureLoader
    /// </summary>
    public class TextureLoader :
        SpineRuntime21.TextureLoader,
        SpineRuntime36.TextureLoader,
        SpineRuntime37.TextureLoader,
        SpineRuntime38.TextureLoader,
        SpineRuntime40.TextureLoader,
        SpineRuntime41.TextureLoader,
        SpineRuntime42.TextureLoader
    {
        /// <summary>
        /// 强制启用 Smooth
        /// </summary>
        public bool ForceSmooth { get; set; } = false;

        /// <summary>
        /// 强制启用 Repeated
        /// </summary>
        public bool ForceRepeated { get; set; } = false;

        /// <summary>
        /// 强制启用 Mipmap
        /// </summary>
        public bool ForceMipmap { get; set; } = false;

        public void Load(SpineRuntime21.AtlasPage page, string path)
        {
            var texture = new SFML.Graphics.Texture(path);
            if (page.magFilter == SpineRuntime21.TextureFilter.Linear)
            {
                texture.Smooth = true;
            }
            if (page.uWrap == SpineRuntime21.TextureWrap.Repeat && page.vWrap == SpineRuntime21.TextureWrap.Repeat)
            {
                texture.Repeated = true;
            }
            switch (page.minFilter)
            {
                case SpineRuntime21.TextureFilter.Linear:
                    texture.Smooth = true;
                    break;
                case SpineRuntime21.TextureFilter.MipMap:
                case SpineRuntime21.TextureFilter.MipMapNearestNearest:
                    texture.GenerateMipmap();
                    break;
                case SpineRuntime21.TextureFilter.MipMapLinearNearest:
                case SpineRuntime21.TextureFilter.MipMapNearestLinear:
                case SpineRuntime21.TextureFilter.MipMapLinearLinear:
                    texture.Smooth = true;
                    texture.GenerateMipmap();
                    break;
            }

            if (ForceSmooth) texture.Smooth = true;
            if (ForceRepeated) texture.Repeated = true;
            if (ForceMipmap) texture.GenerateMipmap();

            page.rendererObject = texture;
        }

        public void Load(SpineRuntime36.AtlasPage page, string path)
        {
            var texture = new SFML.Graphics.Texture(path);
            if (page.magFilter == SpineRuntime36.TextureFilter.Linear)
            {
                texture.Smooth = true;
            }
            if (page.uWrap == SpineRuntime36.TextureWrap.Repeat && page.vWrap == SpineRuntime36.TextureWrap.Repeat)
            {
                texture.Repeated = true;
            }
            switch (page.minFilter)
            {
                case SpineRuntime36.TextureFilter.Linear:
                    texture.Smooth = true;
                    break;
                case SpineRuntime36.TextureFilter.MipMap:
                case SpineRuntime36.TextureFilter.MipMapNearestNearest:
                    texture.GenerateMipmap();
                    break;
                case SpineRuntime36.TextureFilter.MipMapLinearNearest:
                case SpineRuntime36.TextureFilter.MipMapNearestLinear:
                case SpineRuntime36.TextureFilter.MipMapLinearLinear:
                    texture.Smooth = true;
                    texture.GenerateMipmap();
                    break;
            }

            if (ForceSmooth) texture.Smooth = true;
            if (ForceRepeated) texture.Repeated = true;
            if (ForceMipmap) texture.GenerateMipmap();

            page.rendererObject = texture;
        }

        public void Load(SpineRuntime37.AtlasPage page, string path)
        {
            var texture = new SFML.Graphics.Texture(path);
            if (page.magFilter == SpineRuntime37.TextureFilter.Linear)
            {
                texture.Smooth = true;
            }
            if (page.uWrap == SpineRuntime37.TextureWrap.Repeat && page.vWrap == SpineRuntime37.TextureWrap.Repeat)
            {
                texture.Repeated = true;
            }
            switch (page.minFilter)
            {
                case SpineRuntime37.TextureFilter.Linear:
                    texture.Smooth = true;
                    break;
                case SpineRuntime37.TextureFilter.MipMap:
                case SpineRuntime37.TextureFilter.MipMapNearestNearest:
                    texture.GenerateMipmap();
                    break;
                case SpineRuntime37.TextureFilter.MipMapLinearNearest:
                case SpineRuntime37.TextureFilter.MipMapNearestLinear:
                case SpineRuntime37.TextureFilter.MipMapLinearLinear:
                    texture.Smooth = true;
                    texture.GenerateMipmap();
                    break;
            }

            if (ForceSmooth) texture.Smooth = true;
            if (ForceRepeated) texture.Repeated = true;
            if (ForceMipmap) texture.GenerateMipmap();

            page.rendererObject = texture;
        }

        public void Load(SpineRuntime38.AtlasPage page, string path)
        {
            var texture = new SFML.Graphics.Texture(path);
            if (page.magFilter == SpineRuntime38.TextureFilter.Linear)
            {
                texture.Smooth = true;
            }
            if (page.uWrap == SpineRuntime38.TextureWrap.Repeat && page.vWrap == SpineRuntime38.TextureWrap.Repeat)
            {
                texture.Repeated = true;
            }
            switch (page.minFilter)
            {
                case SpineRuntime38.TextureFilter.Linear:
                    texture.Smooth = true;
                    break;
                case SpineRuntime38.TextureFilter.MipMap:
                case SpineRuntime38.TextureFilter.MipMapNearestNearest:
                    texture.GenerateMipmap();
                    break;
                case SpineRuntime38.TextureFilter.MipMapLinearNearest:
                case SpineRuntime38.TextureFilter.MipMapNearestLinear:
                case SpineRuntime38.TextureFilter.MipMapLinearLinear:
                    texture.Smooth = true;
                    texture.GenerateMipmap();
                    break;
            }

            if (ForceSmooth) texture.Smooth = true;
            if (ForceRepeated) texture.Repeated = true;
            if (ForceMipmap) texture.GenerateMipmap();

            page.rendererObject = texture;
        }

        public void Load(SpineRuntime40.AtlasPage page, string path)
        {
            var texture = new SFML.Graphics.Texture(path);
            if (page.magFilter == SpineRuntime40.TextureFilter.Linear)
            {
                texture.Smooth = true;
            }
            if (page.uWrap == SpineRuntime40.TextureWrap.Repeat && page.vWrap == SpineRuntime40.TextureWrap.Repeat)
            {
                texture.Repeated = true;
            }
            switch (page.minFilter)
            {
                case SpineRuntime40.TextureFilter.Linear:
                    texture.Smooth = true;
                    break;
                case SpineRuntime40.TextureFilter.MipMap:
                case SpineRuntime40.TextureFilter.MipMapNearestNearest:
                    texture.GenerateMipmap();
                    break;
                case SpineRuntime40.TextureFilter.MipMapLinearNearest:
                case SpineRuntime40.TextureFilter.MipMapNearestLinear:
                case SpineRuntime40.TextureFilter.MipMapLinearLinear:
                    texture.Smooth = true;
                    texture.GenerateMipmap();
                    break;
            }

            if (ForceSmooth) texture.Smooth = true;
            if (ForceRepeated) texture.Repeated = true;
            if (ForceMipmap) texture.GenerateMipmap();

            page.rendererObject = texture;
        }

        public void Load(SpineRuntime41.AtlasPage page, string path)
        {
            var texture = new SFML.Graphics.Texture(path);
            if (page.magFilter == SpineRuntime41.TextureFilter.Linear)
            {
                texture.Smooth = true;
            }
            if (page.uWrap == SpineRuntime41.TextureWrap.Repeat && page.vWrap == SpineRuntime41.TextureWrap.Repeat)
            {
                texture.Repeated = true;
            }
            switch (page.minFilter)
            {
                case SpineRuntime41.TextureFilter.Linear:
                    texture.Smooth = true;
                    break;
                case SpineRuntime41.TextureFilter.MipMap:
                case SpineRuntime41.TextureFilter.MipMapNearestNearest:
                    texture.GenerateMipmap();
                    break;
                case SpineRuntime41.TextureFilter.MipMapLinearNearest:
                case SpineRuntime41.TextureFilter.MipMapNearestLinear:
                case SpineRuntime41.TextureFilter.MipMapLinearLinear:
                    texture.Smooth = true;
                    texture.GenerateMipmap();
                    break;
            }

            if (ForceSmooth) texture.Smooth = true;
            if (ForceRepeated) texture.Repeated = true;
            if (ForceMipmap) texture.GenerateMipmap();

            page.rendererObject = texture;
        }

        public void Load(SpineRuntime42.AtlasPage page, string path)
        {
            var texture = new SFML.Graphics.Texture(path);
            if (page.magFilter == SpineRuntime42.TextureFilter.Linear)
            {
                texture.Smooth = true;
            }
            if (page.uWrap == SpineRuntime42.TextureWrap.Repeat && page.vWrap == SpineRuntime42.TextureWrap.Repeat)
            {
                texture.Repeated = true;
            }
            switch (page.minFilter)
            {
                case SpineRuntime42.TextureFilter.Linear:
                    texture.Smooth = true;
                    break;
                case SpineRuntime42.TextureFilter.MipMap:
                case SpineRuntime42.TextureFilter.MipMapNearestNearest:
                    texture.GenerateMipmap();
                    break;
                case SpineRuntime42.TextureFilter.MipMapLinearNearest:
                case SpineRuntime42.TextureFilter.MipMapNearestLinear:
                case SpineRuntime42.TextureFilter.MipMapLinearLinear:
                    texture.Smooth = true;
                    texture.GenerateMipmap();
                    break;
            }

            if (ForceSmooth) texture.Smooth = true;
            if (ForceRepeated) texture.Repeated = true;
            if (ForceMipmap) texture.GenerateMipmap();

            page.rendererObject = texture;

            // 似乎是不需要设置的, 因为存在某些 png 和 atlas 大小不同的情况, 一般是有一些缩放, 如果设置了反而渲染异常
            // page.width = (int)texture.Size.X;
            // page.height = (int)texture.Size.Y;
        }

        public void Unload(object texture)
        {
            ((SFML.Graphics.Texture)texture).Dispose();
        }
    }
}
