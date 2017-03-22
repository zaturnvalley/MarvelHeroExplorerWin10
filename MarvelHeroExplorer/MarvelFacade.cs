using MarvelHeroExplorer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace MarvelHeroExplorer
{
    public class MarvelFacade
    {
        private const string PrivateKey = "f4e0e571e93efa1dbdfe700b6fea05370b91478f";
        private const string PublicKey = "0df5b92f3c787763281834f9a8049a71";
        private const int MaxCharacters = 1500;
        public async Task<CharacterDataWrapper> GetCharacterList()
        {
            // Assemble the URL
            Random random = new Random();
            var offset = random.Next(1500);

            // Get the MD5 Hash
            var timeStamp = DateTime.Now.Ticks.ToString();
            var hash = CreateHash(timeStamp);

            string url = String.Format("https://gateway.marvel.com:443/v1/public/characters?limit=10&offset={0}&apikey={1}apikey={1}&ts={2}&hash={3}", offset, PublicKey, timeStamp);

            // Call out to Marvel
            HttpClient http = new HttpClient();
            var response = await http.GetAsync(url);
            var jsonMessage = await response.Content.ReadAsStringAsync();

            // Response -> string / json -> deserialize
            var serializer = new DataContractJsonSerializer(typeof(CharacterDataWrapper));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonMessage));

            var result = (CharacterDataWrapper)serializer.ReadObject(ms);
            return result;
        }

        private static string CreateHash(string timeStamp)
        {
            var toBeHashed = timeStamp + PrivateKey + PublicKey;
            var hashedMessage = ComputeMD5(toBeHashed);
            return hashedMessage;
        }
        private static string ComputeMD5(string str)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);
            return res;
        }
    }
}
