using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2_Live
{
    static class FileManager
    {
        private static readonly string AppFolderName = "GW2 Live";

        private static readonly string AppFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolderName);

        public static void SaveToFile<T>(T obj, string folder, string file)
        {
            string folderPath = Path.Combine(AppFolderPath, folder);
            string filePath = Path.Combine(folderPath, file);

            Directory.CreateDirectory(folderPath);
            using (var writer = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Converters.Add(new NoFormatArrayConverter());
                serializer.Serialize(writer, obj);
            }
        }

        public static async Task<T> ReadFromFile<T>(string folder, string file)
        {
            string filePath = Path.Combine(AppFolderPath, folder, file);

            using (var reader = new StreamReader(filePath))
            {
                string json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        private class NoFormatArrayConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType.IsArray;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteRawValue(JsonConvert.SerializeObject(value, Formatting.None));
            }
        }
    }
}
