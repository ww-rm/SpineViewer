using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    public abstract class AtlasConverter : ImplementationResolver<AtlasConverter, SpineImplementationAttribute, SpineVersion>
    {
        public static AtlasConverter? Create(SpineVersion version)
        {
            if (HasImplementation(version))
                return New(version, Array.Empty<object>());
            return null;
        }

        public List<Dictionary<string,Object>> ReadAltas(string path)
        {
            List<Dictionary<string, Object>> images = [];
            Dictionary<string, Object> image = [];
            TextReader reader = new StreamReader(path);
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            while (ReadHead(reader, image))
            {
                string line = ReadPage(reader, image);
                ReadRegion(reader, line, image);
                images.Add(image);
                image = new Dictionary<string, Object>();
            }
            return images;
        }

        public abstract void ToFile(string path, List<Dictionary<string, Object>> data);

        //3.8，4.1，4.2版本的head和page,region部分的读取基本相同，故写成virtual方法
        protected virtual bool ReadHead(TextReader reader, Dictionary<string, Object> data)
        {
            string line = reader.ReadLine();
            string[] element = new string[1];
            while (line != null && line.Trim().Length == 0)
            {
                line = reader.ReadLine();
            }
            if (line == null) return false;
            data["head"] = line;
            return true;
        }

        protected virtual string ReadPage(TextReader reader, Dictionary<string, Object> data)
        {
            Dictionary<string, string> pageData = [];
            data["pages"] = pageData;
            string line = reader.ReadLine();
            var element = ReadElement(line);
            while (element != null)
            {
                pageData[element.Value.Key] = element.Value.Value;
                line = reader.ReadLine();
                element = ReadElement(line);
            }
            return line;
        }

        protected virtual void ReadRegion(TextReader reader, string line, Dictionary<string, Object> data)
        {
            List<Dictionary<string, string>> regions = [];
            data["regions"] = regions;
            while (line != null && line.Trim().Length != 0)
            {
                Dictionary<string, string> region = [];
                region["name"] = line.Trim();
                regions.Add(region);
                line = reader.ReadLine();
                var element = ReadElement(line);
                while (element != null)
                {
                    region[element.Value.Key] = element.Value.Value;
                    line = reader.ReadLine();
                    element = ReadElement(line);
                }

            }
        }


        protected virtual KeyValuePair<string,string>? ReadElement(string line)
        {
            if (line == null) return null;
            line = line.Trim();
            if (line.Length == 0) return null;
            int colonIndex = line.IndexOf(':');
            if (colonIndex < 0) return null;
            string name = line.Substring(0, colonIndex).Trim();
            //line = line.Substring(colonIndex + 1).Trim();
            //List<string> splited = [];
            //foreach (var i in line.Split(',', StringSplitOptions.RemoveEmptyEntries))
            //{
            //    splited.Add(i.Trim());
            //}            
            return new KeyValuePair<string,string>(name, line.Substring(colonIndex + 1).Trim());
        }

        protected virtual void WriteElement(TextWriter writer, KeyValuePair<string, string> element)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            writer.WriteLine($"{element.Key}: {element.Value}");
        }

        protected virtual void WriteElements(TextWriter writer, Dictionary<string, string> element)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            foreach (var i in element)
            {
                if (i.Key != "name")
                    WriteElement(writer, i);
            }
        }

    }
}
