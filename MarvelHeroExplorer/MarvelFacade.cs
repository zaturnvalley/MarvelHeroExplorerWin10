using MarvelHeroExplorer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private const string ImageNotAvailablePath = "http://i.annihil.us/u/prod/marvel/i/mg/b/40/image_not_available";
        public static async Task PopulateMarvelCharactersAsync(ObservableCollection<Character> marvelCharacters)
        {
            var characterDataWrapper = await GetCharacterDataWrapperAsync();

            var characters = characterDataWrapper.data.results;
            foreach (var character in characters)
            {
                // Filter characters that missing thumbnail images
                if (character.thumbnail != null
                    && character.thumbnail.path != ""
                    && character.thumbnail.path != ImageNotAvailablePath)
                {
                    character.thumbnail.small = String.Format("{0}/standard_small.{1}", 
                        character.thumbnail.path,
                        character.thumbnail.extension);

                    character.thumbnail.large = string.Format("{0}/portrait_xlarge.{1}",
                        character.thumbnail.path,
                        character.thumbnail.extension);

                    marvelCharacters.Add(character);
                }
            }
        }
        private static async Task<CharacterDataWrapper> GetCharacterDataWrapperAsync()
        {
            // Assemble the URL
            Random random = new Random();
            var offset = random.Next(MaxCharacters);

            // Get the MD5 Hash
            var timeStamp = DateTime.Now.Ticks.ToString();
            var hash = CreateHash(timeStamp);

            string url = String.Format("https://gateway.marvel.com:443/v1/public/characters?limit=10&offset={0}&apikey={1}&ts={2}&hash={3}", offset, PublicKey, timeStamp, hash);

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
