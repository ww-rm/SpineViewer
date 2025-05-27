using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

namespace SpineViewer.Extensions
{
    public static class WpfExtension
    {
        /// <summary>
        /// 从本地 WebP 文件读取，并保留透明度，返回一个可以直接用于 WPF Image.Source 的 BitmapSource。
        /// </summary>
        public static BitmapSource LoadWebpWithAlpha(string path)
        {
            using var stream = File.OpenRead(path);
            using var skCodec = SKCodec.Create(stream);

            // 强制指定读取格式 PixelFormats.Bgra32
            var info = skCodec.Info.WithColorType(SKColorType.Bgra8888).WithAlphaType(SKAlphaType.Unpremul);

            var pixelCount = info.Width * info.Height;
            var pixelData = new byte[pixelCount * 4];

            var result = skCodec.GetPixels(info, out pixelData);
            if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
                throw new InvalidOperationException($"Failed to decode webp：{result}");

            var wb = new WriteableBitmap(info.Width, info.Height, 96, 96, PixelFormats.Bgra32, null);
            wb.WritePixels(new(0, 0, info.Width, info.Height), pixelData, info.Width * 4, 0);
            wb.Freeze();
            return wb;
        }

        //public static void SaveToFile(this BitmapSource bitmap, string path)
        //{
        //    var ext = Path.GetExtension(path)?.ToLower();
        //    BitmapEncoder encoder = ext switch
        //    {
        //        ".jpg" or ".jpeg" => new JpegBitmapEncoder(),
        //        ".bmp" => new BmpBitmapEncoder(),
        //        ".gif" => new GifBitmapEncoder(),
        //        ".tiff" => new TiffBitmapEncoder(),
        //        _ => new PngBitmapEncoder(),// 默认用 PNG
        //    };
        //    encoder.Frames.Add(BitmapFrame.Create(bitmap));

        //    // 确保目录存在
        //    var dir = Path.GetDirectoryName(path);
        //    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        //        Directory.CreateDirectory(dir);

        //    // 写出文件
        //    using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        //    encoder.Save(fs);
        //}

        ///// <summary>
        ///// 将 Bgra32/Pbgra32 格式存储为 webp 格式
        ///// </summary>
        //public static void SaveToWebp(this BitmapSource source, string path, int quality = 80)
        //{
        //    if (source.Format != PixelFormats.Bgra32 && source.Format != PixelFormats.Pbgra32)
        //        throw new NotSupportedException($"Not supported format: {source.Format}");

        //    int width = source.PixelWidth;
        //    int height = source.PixelHeight;
        //    int stride = width * 4;
        //    var pixelData = new byte[height * stride];
        //    source.CopyPixels(pixelData, stride, 0);

        //    var alphaType = source.Format == PixelFormats.Pbgra32 ? SKAlphaType.Premul : SKAlphaType.Unpremul;
        //    var info = new SKImageInfo(width, height, SKColorType.Bgra8888, alphaType);
        //    using var skBitmap = new SKBitmap(info);
        //    Marshal.Copy(pixelData, 0, skBitmap.GetPixels(), pixelData.Length);

        //    using var skImage = SKImage.FromBitmap(skBitmap);
        //    using var data = skImage.Encode(SKEncodedImageFormat.Webp, quality);
        //    // using var data = skImage.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 100));

        //    Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
        //    using var stream = File.OpenWrite(path);
        //    data.SaveTo(stream);
        //}
    }
}
