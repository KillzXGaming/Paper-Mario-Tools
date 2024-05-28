using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.Text;
using Newtonsoft.Json;

namespace LightConverter
{
    public class ProbeHeader
    {
        public uint[] AxisParameters;

        public float[] ProbePosition = new float[3];
        public float[] BoxScale = new float[3]; //unsure
        public float[] Unknown = new float[4];

        public float ProbeParam1;
        public float ProbeParam2;

        public float[] Color = new float[3];

        public float[] Unknown2 = new float[3];

        public string Type = "";

        public uint Unknown0xA0;
        public float Unknown0xA4;
        public float Unknown0xA8;
        public uint Unknown0xAC;

        public ProbeHeader() { }

        public ProbeHeader(Stream stream)
        {
            using (var reader = new BinaryDataReader(stream))
            {
                reader.ReadUInt32(); //100
                uint num_axis_textures = reader.ReadUInt32(); //4
                AxisParameters = reader.ReadUInt32s((int)num_axis_textures);
                ProbePosition = reader.ReadSingles(3);
                BoxScale = reader.ReadSingles(3);
                Unknown = reader.ReadSingles(4); //0
                ProbeParam1 = reader.ReadSingle();
                ProbeParam2 = reader.ReadSingle();
                Color = reader.ReadSingles(3);
                Unknown2 = reader.ReadSingles(3); //0

                Type = Encoding.UTF8.GetString(reader.ReadBytes(64)).Replace("\0", "");
                Unknown0xA0 = reader.ReadUInt32();
                Unknown0xA4 = reader.ReadSingle();
                Unknown0xA8 = reader.ReadSingle();
                Unknown0xAC = reader.ReadUInt32();
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new BinaryDataWriter(stream))
            {
                writer.Write(100);
                writer.Write(AxisParameters.Length); //4
                writer.Write(AxisParameters);
                writer.Write(ProbePosition);
                writer.Write(BoxScale);
                writer.Write(Unknown);
                writer.Write(ProbeParam1);
                writer.Write(ProbeParam2);
                writer.Write(Color);
                writer.Write(Unknown2);
                writer.Write(Encoding.UTF8.GetBytes(Type));
                writer.Write(new byte[64 - Type.Length]); //fixed string, fill rest of length
                writer.Write(Unknown0xA0);
                writer.Write(Unknown0xA4);
                writer.Write(Unknown0xA8);
                writer.Write(Unknown0xAC);
            }
        }

        public void Export(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static ProbeHeader Create(string path)
        {
            return JsonConvert.DeserializeObject<ProbeHeader>(File.ReadAllText(path));
        }
    }
}
