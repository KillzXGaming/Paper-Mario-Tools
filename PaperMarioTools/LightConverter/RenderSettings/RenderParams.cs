using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LightConverter
{
    public class RenderParams
    {
        public Section[] Sections = new Section[0];

        public RenderParams() { }

        public RenderParams(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                Load(fs);
            }
        }

        public RenderParams(Stream fs)
        {
            Load(fs);
        }


        private void Load(Stream fs)
        {
            //bom
            fs.ReadByte();
            fs.ReadByte();
            fs.ReadByte();

            uint num_sections = ReadUint(ReadLine(fs));

            Sections = new Section[num_sections];
            for (int i = 0; i < num_sections; i++)
            {
                string[] properties = ReadLine(fs).Split(":");

                Sections[i] = new Section();
                Sections[i].hash_name = new HashString(ReadUint(properties[0]));
                Sections[i].size = ReadUint(properties[1]);
            }

            for (int i = 0; i < num_sections; i++)
            {
                var pos = fs.Position;

                while (fs.Position < pos + Sections[i].size)
                {
                    string v = ReadLine(fs);
                    if (string.IsNullOrEmpty(v))
                        continue;

                    string[] properties = v.Split(":");
                    if (properties.Length != 2)
                        continue;

                    var property_name = new HashString(ReadUint(properties[0]));
                    var value = ReadValue(properties[1]);
                    Sections[i].properties.Add(property_name.String, value);
                }

                fs.Seek(pos + Sections[i].size, SeekOrigin.Begin);
            }
        }

        private string ReadLine(Stream stream)
        {
            List<byte> chars = new List<byte>();
            while (stream.Position < stream.Length)
            {
                int chara = stream.ReadByte();
                if (chara == 0xA)
                    break;

                chars.Add((byte)chara);
            }
            return Encoding.UTF8.GetString(chars.ToArray());
        }

        public void Save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fs);
        }

        public void Save(Stream fs)
        {
            using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
            {
                //section amount in hex
                writer.Write($"{Sections.Length.ToString("x8")}\n");

                Stream[] saved_sections = new Stream[Sections.Length];
                for (int i = 0; i < Sections.Length; i++)
                {
                    //write section data into memory to get the expected size
                    saved_sections[i] = WriteSection(Sections[i]);

                    string hash = Sections[i].hash_name.Hash.ToString("x8");
                    string size = saved_sections[i].Length.ToString("x8");
                    var psize = Sections[i].size.ToString("x8");

                    writer.Write($"{hash}:{size}\n");
                }

                for (int i = 0; i < saved_sections.Length; i++)
                    saved_sections[i].CopyTo(writer.BaseStream);
              
                foreach (var stream in saved_sections)
                    stream?.Dispose();  
            }
        }

        private MemoryStream WriteSection(Section section)
        {
            var mem = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(mem))
            {
                string hash = section.hash_name.Hash.ToString("x8");
                writer.Write($"{hash}\n");
                foreach (var prop in section.properties)
                {
                    string phash = ToHash(prop.Key);
                    string pvalue = WriteValue(prop.Value);
                    writer.Write($"{phash}:{pvalue}\n");
                }
            }
            return new MemoryStream(mem.ToArray());
        }

        static object ReadValue(string value)
        {
            //values are formatted in certain ways
            if (value.StartsWith("0x")) //hex value, typically as float
                return ReadSingle(value);
            else if (value.Contains(",")) //array floats
            {
                string[] values = value.Split(",");
                if (values.Length == 2)
                    return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
                else if (values.Length == 3)
                    return new Vector3(float.Parse(values[0]), float.Parse(values[1]),
                        float.Parse(values[2]));
                else if (values.Length == 4)
                    return new Vector4(float.Parse(values[0]), float.Parse(values[1]),
                                       float.Parse(values[2]), float.Parse(values[3]));
                else
                    throw new Exception();
            }
            else if (int.TryParse(value, out var value_int))
                return value_int;
            else
                return value.ToString();
        }

        static uint ReadUint(string hexString) => Convert.ToUInt32(hexString, 16);
        static float ReadSingle(string hexString)
        {
            byte[] byteArray = BitConverter.GetBytes(ReadUint(hexString));

            return BitConverter.ToSingle(byteArray, 0); 
        }

        static string ToHash(string hexString)
        {
            //hex decimal
            if (uint.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, null, out uint output))
                return output.ToString("x8");
            //else string to crc32
            return Crc32.Compute(hexString).ToString("x8");
        }

        static string WriteValue(object value)
        {
            if (value is Vector2)
            {
                Vector2 v = (Vector2)value;
                return $"{v.X.ToString("F6")},{v.Y.ToString("F6")}";
            }
            else if(value is Vector3)
            {
                Vector3 v = (Vector3)value;
                return $"{v.X.ToString("F6")},{v.Y.ToString("F6")},{v.Z.ToString("F6")}";
            }
            else if (value is Vector4)
            {
                Vector4 v = (Vector4)value;
                return $"{v.X.ToString("F6")},{v.Y.ToString("F6")},{v.Z.ToString("F6")},{v.W.ToString("F6")}";
            }
            else if (value is float)
                return ToHex(((float)value));
            else if (value is double)
                return ToHex((float)((double)value));
            else
                return value.ToString();
        }

        static string ToHex(float value)
        {
            if (value == 0) return "0x0";

            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            string hexString = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            string formattedHexString = "0x" + hexString.PadLeft(8, '0');
            return formattedHexString;
        }

        public class Section
        {
            public HashString hash_name;

            [JsonIgnore]
            public uint size;

            public Dictionary<string, object> properties = new Dictionary<string, object>();
        }

        public class HashString
        {
            public uint Hash;

            public string String;

            public HashString() { }

            public HashString(uint hash_name)
            {
                Hash = hash_name;
                if (Hashing.Hashes.ContainsKey(hash_name))
                    String = Hashing.Hashes[hash_name];
                else
                    String = hash_name.ToString("X");
            }

            public override string ToString()
            {
                return String.ToString();
            }
        }
    }
}
