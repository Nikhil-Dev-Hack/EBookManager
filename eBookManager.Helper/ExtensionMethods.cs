using System;
using eBookManager.Engine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace eBookManager.Helper
{
    public static class ExtensionMethods
    {
        public static int ToInt(this string value, int defaultInteger = 0)
        {
            try
            {
                if (int.TryParse(value, out int validInteger))
                {
                    //Out variables
                    return validInteger;

                }
                else
                {
                    return defaultInteger;
                }
            }
            catch
            {

                return defaultInteger;
            }
        }
        public async static Task WriteToDataStore(this List<StorageSpace> value, string storagePath, bool appendToExistingFile = false)
        {
            using (FileStream fs = File.Create(storagePath))
                await JsonSerializer.SerializeAsync(fs, value);
        }
        public static bool StorageSpaceExists(this List<StorageSpace> space, string nameValueToCheck, out int storageSpaceId)
        {
            bool exists = false;
            storageSpaceId = 0;
            if (space.Count() != 0)
            {
                int count = (from r in space where r.Name.Equals(nameValueToCheck) select r).Count();
                if (count > 0) exists = true;
                storageSpaceId = (from r in space select r.ID).Max() + 1;
                
            }
            return exists;
        }

        public async static Task<List<StorageSpace>> ReadFromDataStore(this List<StorageSpace> value, string storagePath)
        {
            if (!File.Exists(storagePath))
            {
                var newFile = File.Create(storagePath);
                newFile.Close();
            }
            using FileStream fs = File.OpenRead(storagePath);
            if (fs.Length == 0 ) return new List<StorageSpace>();

            var storageList = await JsonSerializer.DeserializeAsync<List<StorageSpace>>(fs);

            return storageList;
        }

    }
}
