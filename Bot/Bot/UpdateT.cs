using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot
{
    public class UpdateT
    {
        public int createSet = 0;
        public string title = " by @Stickerim_Bot";
        public string newTitle = "";
        public string stickerSetNameGlobal = "";
        public string stickerSetTitleGlobal = "";
        public async Task AppendToFileAsync<T>(string filePath, List<T> items)
        {
            try
            {
                List<T> existingItems = await ReadFromFileAsync<T>(filePath);

                if (existingItems == null)
                {
                    existingItems = new List<T>();
                }

                existingItems.AddRange(items);
                await WriteToFileAsync(filePath, existingItems);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to append items to the file. Exception: {ex.Message}");
            }
        }

        public async Task<List<T>> ReadFromFileAsync<T>(string filePath)
        {
            try
            {
                using StreamReader file = System.IO.File.OpenText(filePath);
                string json = await file.ReadToEndAsync();
                return JsonConvert.DeserializeObject<List<T>>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read items from the file. Exception: {ex.Message}");
            }
        }

        public async Task WriteToFileAsync<T>(string filePath, List<T> items)
        {
            try
            {
                string json = JsonConvert.SerializeObject(items, Formatting.Indented);

                using StreamWriter file = System.IO.File.CreateText(filePath);
                await file.WriteAsync(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to write items to the file. Exception: {ex.Message}");
            }
        }

        public int CountElementsInJsonFile(string filePath)
        {
            try
            {
                string json = System.IO.File.ReadAllText(filePath);
                List<object> items = JsonConvert.DeserializeObject<List<object>>(json);
                return items.Count;
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException($"The specified file '{filePath}' was not found. Exception: {ex.Message}");
            }
            catch (JsonSerializationException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize JSON. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to count elements in JSON file. Exception: {ex.Message}");
            }
        }
        public async Task<JArray> FindItemsByUserIdFromFileAsync(string filePath, long userId)
        {
            try
            {
                List<StickerSets> stickerSets = await ReadStickerSetsFromFileAsync(filePath);
                JArray jsonArray = JArray.FromObject(stickerSets);
                var filteredItems = jsonArray
                    .Where(item => (long)item["userId"] == userId)
                    .ToList();
                return new JArray(filteredItems);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to find sticker sets by user ID. Exception: {ex.Message}");
            }
        }

        public async Task<List<StickerSets>> ReadStickerSetsFromFileAsync(string filePath)
        {
            try
            {
                return await ReadFromFileAsync<StickerSets>(filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read sticker sets from the file. Exception: {ex.Message}");
            }
        }
    }
}

