using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Global
{
    [Serializable]
    public class Config
    {
        public int port { get; set; }
    }

    public static class ConfigLoader
    {
        private const string ConfigName = "config.txt";

        public static Config LoadOrCreateConfig()
        {
            var path = Path.Combine(Application.dataPath, ConfigName);

            if (!File.Exists(path))
            {
                return CreateDefaultConfig(path);
            }

            var json = File.ReadAllText(path);
            var config = JsonConvert.DeserializeObject<Config>(json);

            return config;
        }

        private static Config CreateDefaultConfig(string path)
        {
            var config = new Config
            {
                port = 30237
            };

            var json = JsonConvert.SerializeObject(config);
            File.WriteAllText(path, json);

            return config;
        }
    }
}