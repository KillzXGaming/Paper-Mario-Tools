using BfresLibrary;
using Newtonsoft.Json;
using System.Text;

namespace LightConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (Directory.Exists(arg))
                    CreateResLight(arg);
                else if(arg.EndsWith("light.bfres.zst"))
                    Dump(new ResFile(Zstd.Decompress(arg)));
                else if (arg.EndsWith("light.bfres"))
                    Dump(new ResFile(arg));
                else if (arg.EndsWith("probe.header.json"))
                    CreateProbeHeader(arg);
                else if (arg.EndsWith("probe.header.zst"))
                    Dump(new ProbeHeader(Zstd.Decompress(arg)), arg);
                else if (arg.EndsWith("probe.header"))
                    Dump(new ProbeHeader(File.OpenRead(arg)), arg);
                else if (arg.EndsWith(".data"))
                    Dump(new RenderParams(File.OpenRead(arg)), arg);
                else if (arg.EndsWith("render_params.json"))
                    CreateRenderParams(arg);
            }
        }

        static void Dump(ResFile resFile)
        {
            //dump as folder
            string folder = resFile.Name;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (var anim in resFile.SceneAnims.Values)
                anim.Export(Path.Combine(folder, $"{anim.Name}.json"), resFile);
        }

        static void Dump(ProbeHeader header, string arg)
        {
            string name = Path.GetFileNameWithoutExtension(arg);
            header.Export($"{name}.json");
        }

        static void Dump(RenderParams renderParams, string arg)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new Vector3Converter());
            settings.Converters.Add(new Vector4Converter());
            settings.Converters.Add(new VectorConverter());

            string name = Path.GetFileNameWithoutExtension(arg);
            File.WriteAllText($"{name}.json", JsonConvert.SerializeObject(renderParams, Formatting.Indented, settings));
        }

        static void CreateRenderParams(string arg)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new Vector3Converter());
            settings.Converters.Add(new Vector4Converter());
            settings.Converters.Add(new VectorConverter());

            var renderParams = JsonConvert.DeserializeObject<RenderParams>(File.ReadAllText(arg), settings);
            renderParams.Save($"{arg.Replace(".json", ".data")}");
        }

        static void CreateProbeHeader(string arg)
        {
            var header = ProbeHeader.Create(arg);
            string name = Path.GetFileNameWithoutExtension(arg);

            var mem = new MemoryStream();
            header.Save(mem);
            File.WriteAllBytes($"{name}.zst", Zstd.Compress(mem.ToArray()));
        }

        static void CreateResLight(string folder)
        {
            string name = new DirectoryInfo(folder).Name;

            ResFile resFile = new ResFile();
            resFile.ChangePlatform(true, 8, 0, 9, 1, 0, new BfresLibrary.PlatformConverters.ConverterHandle());
            resFile.Alignment = 3;
            resFile.Name = name;

            foreach (var file in  Directory.GetFiles(folder))
            {
                SceneAnim anim = new SceneAnim();
                anim.Import(file, resFile);
                anim.Name = Path.GetFileNameWithoutExtension(file);
                resFile.SceneAnims.Add(anim.Name, anim);
            }

            var mem = new MemoryStream();
            resFile.Save(mem);
            File.WriteAllBytes($"{name}.bfres.zst", Zstd.Compress(mem.ToArray()));
        }
    }
}
