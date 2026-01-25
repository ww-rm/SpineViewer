using PsdWriter;
using SFML.System;
using Spine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Exporters
{
    public class PsdExporter : BaseExporter
    {
        public PsdExporter(uint width = 100, uint height = 100) : base(width, height) { }
        public PsdExporter(Vector2u resolution) : base(resolution) { }

        public sealed override void Export(string output, params SpineObject[] spines) => Export(output, default, spines);

        /// <summary>
        /// 导出给定的模型, 从前往后对应从上往下的渲染顺序
        /// </summary>
        /// <param name="output">Psd 文件输出路径</param>
        /// <param name="ct">取消令牌</param>
        /// <param name="spines">要导出的模型, 从前往后对应从上往下的渲染顺序</param>
        public void Export(string output, CancellationToken ct, params SpineObject[] spines)
        {
            var resolution = _renderTexture.Size;
            var psdWriter = new PsdWriter.PsdWriter(resolution.X, resolution.Y);
            _renderTexture.SetActive(true);

            var layerCount = spines.Select(sp => sp.IterDrawCount).Sum();
            var layerIdx = 0;

            _progressReporter?.Invoke(layerCount, 0, $"[0/{layerCount}] {output}");
            foreach (var sp in spines.Reverse())
            {
                if (ct.IsCancellationRequested)
                {
                    _logger.Info("Export cancelled");
                    break;
                }

                psdWriter.BeginGroup(sp.Name);
                foreach (var (slot, image) in sp.IterDraw(_renderTexture))
                {
                    if (ct.IsCancellationRequested)
                    {
                        image.Dispose();
                        break;
                    }
                    _progressReporter?.Invoke(layerCount, layerIdx + 1, $"[{layerIdx + 1}/{layerCount}] {output} <{sp.Name}/{slot.Name}>");
                    psdWriter.AddRgbaLayer(image.Pixels, slot.Name, true, ConvertBlendMode(slot.Blend));
                    image.Dispose();
                    layerIdx++;
                }
                psdWriter.EndGroup();
            }

            _renderTexture.SetActive(false);
            psdWriter.WriteTo(output);
        }

        private string ConvertBlendMode(SFML.Graphics.BlendMode blendMode)
        {
            if (blendMode == SFMLBlendMode.NormalPma)
                return PsdWriter.Sections.Layers.BlendModes.Normal;
            else if (blendMode == SFMLBlendMode.AdditivePma)
                return PsdWriter.Sections.Layers.BlendModes.LinearDodge;
            else if (blendMode == SFMLBlendMode.MultiplyPma)
                return PsdWriter.Sections.Layers.BlendModes.Multiply;
            else if (blendMode == SFMLBlendMode.ScreenPma)
                return PsdWriter.Sections.Layers.BlendModes.Screen;
            else
                throw new NotImplementedException(blendMode.ToString());
        }
    }
}
