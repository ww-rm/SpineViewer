using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine.Implementations.AtlasConvertor
{
    [SpineImplementation(SpineVersion.V38)]
    public class AtlasConverter38 : SpineViewer.Spine.AtlasConverter
    {
        public override void ToFile(string path, List<Dictionary<string, object>> images)
        {
            TextWriter writer = new StreamWriter(path);
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            foreach (var image in images)
            {
                string imgName = image.GetValueOrDefault("head") as string;
                writer.WriteLine(imgName);

                Dictionary<string, string> pages = image.GetValueOrDefault("pages") as Dictionary<string, string>;
                if (pages.TryGetValue("size", out var size))
                {
                    writer.WriteLine($"size: {size}");
                }
                if (pages.TryGetValue("format", out var format)) writer.WriteLine($"format: {pages["format"]}"); else writer.WriteLine("format: RGBA8888");
                if (pages.TryGetValue("filter", out var filter)) writer.WriteLine($"filter: {pages["filter"]}"); else writer.WriteLine("filter: Nearest,Nearest");
                if (pages.TryGetValue("repeat", out var repeat)) writer.WriteLine($"repeat: {pages["repeat"]}"); else writer.WriteLine("repeat: none");

                List<Dictionary<string, string>> regions = image.GetValueOrDefault("regions") as List<Dictionary<string, string>>;
                foreach (var i in regions)
                {
                    string sizeValue = "0, 0";
                    writer.WriteLine(i["name"]);
                    if (i.TryGetValue("rotate", out var rotate)) writer.WriteLine($"  rotate: {rotate}"); else writer.WriteLine("rotate: false");
                    //writer.WriteLine($"rotate: {i["rotate"] ?? "false"}");

                    if (i.TryGetValue("xy", out var xy)) writer.WriteLine($"  xy: {xy}");
                    else
                    {
                        if (i.TryGetValue("bounds", out var bounds))
                        {
                            int secondCommaIndex = bounds.IndexOf(',', bounds.IndexOf(',') + 1);
                            writer.WriteLine($"  xy: {bounds.Substring(0, secondCommaIndex)}");
                        }
                        else writer.WriteLine("  xy: 0,0");
                    }

                    if (i.TryGetValue("size", out var size1))
                    {
                        sizeValue = (string)size1;
                        writer.WriteLine($"  size: {size1}");
                    }
                    else
                    {
                        if (i.TryGetValue("bounds", out var bounds))
                        {
                            int secondCommaIndex = bounds.IndexOf(',', bounds.IndexOf(',') + 1);
                            sizeValue = bounds.Substring(secondCommaIndex + 1);
                            writer.WriteLine($"  size: {sizeValue}");
                            
                        }
                        else writer.WriteLine("  size: 32,32");//这个默认值是我自己加的，没看到默认值
                    }

                    if (i.TryGetValue("split", out var split)) writer.WriteLine($"  split: {split}");
                    if (i.TryGetValue("pad", out var pad)) writer.WriteLine($"  pad: {pad}");

                    if (i.TryGetValue("orig", out var orig)) writer.WriteLine($"  orig: {orig}");
                    else
                    {
                        if (i.TryGetValue("offsets", out var offsets))
                        {
                            int secondCommaIndex = offsets.IndexOf(',', offsets.IndexOf(',') + 1);
                            writer.WriteLine($"  orig: {offsets.Substring(secondCommaIndex + 1)}");
                            //writer.WriteLine($"orig: {offsets.Substring(0, secondCommaIndex)}");
                        }
                        else writer.WriteLine($"  orig: {sizeValue}");
                    }

                    if (i.TryGetValue("offset", out var offset)) writer.WriteLine($"  offset: {offset}");
                    else
                    {
                        if (i.TryGetValue("offsets", out var offsets))
                        {
                            int secondCommaIndex = offsets.IndexOf(',', offsets.IndexOf(',') + 1);
                            writer.WriteLine($"  offset: {offsets.Substring(0, secondCommaIndex)}");
                        }
                        else writer.WriteLine("  offset: 0,0");//这个默认值是我自己加的，没看到默认值
                    }

                    if (i.TryGetValue("index", out var index)) writer.WriteLine($"  index: {index}"); else writer.WriteLine("  index: -1");
                    //writer.WriteLine($"index: {i["index"] ?? "-1"}");

                    
                }
                writer.WriteLine();
                
            }
            writer.Close();
        }
    }
}
