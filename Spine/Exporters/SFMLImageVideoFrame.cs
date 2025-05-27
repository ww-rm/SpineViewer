using FFMpegCore.Pipes;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spine.Exporters
{
    /// <summary>
    /// <see cref="SFML.Graphics.Image"/> 帧对象包装类, 将接管给定对象生命周期
    /// </summary>
    public class SFMLImageVideoFrame(Image image) : IVideoFrame, IDisposable
    {
        private readonly Image _image = image;

        /// <summary>
        /// 接管的 <see cref="SFML.Graphics.Image"/> 内部对象
        /// </summary>
        public Image Image => _image;

        public int Width => (int)_image.Size.X;
        public int Height => (int)_image.Size.Y;
        public string Format => "rgba";
        public void Serialize(Stream pipe) => pipe.Write(_image.Pixels);
        public async Task SerializeAsync(Stream pipe, CancellationToken token) => await pipe.WriteAsync(_image.Pixels, token);

        #region IDisposable 接口实现

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _image.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
