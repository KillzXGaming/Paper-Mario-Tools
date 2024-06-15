using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightConverter
{
    public class Hashing
    {
        private static Dictionary<uint, string> _hashes = new Dictionary<uint, string>();

        public static Dictionary<uint, string> Hashes
        {
            get
            {
                if (_hashes.Count == 0)
                    _hashes = GenerateHashList();
                return _hashes;
            }
        }

        static Dictionary<uint, string> GenerateHashList()
        {
            Dictionary<uint, string> list = new Dictionary<uint, string>();

            string file = "hash_strings.txt";
            if (!File.Exists(file))
                return list;

            var hashList = File.ReadAllText(file);
            foreach (string hashStr in hashList.Split('\n', '\r'))
            {
                uint hash = Crc32.Compute(hashStr);
                if (!list.ContainsKey(hash))
                    list.Add(hash, hashStr);
            }
            return list;
        }
    }
}
