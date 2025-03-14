using Newtonsoft.Json;
using OSDownloader.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSDownloader.Models
{
    internal class APIHandler
    {

        public static async Task<APIContentsRootObject> getAPIResponseObjectAsync()
        {
            if (Settings.Default.UseFakeAPILocation)
            {
                //return new Task<APIContentsRootObject>(() =>
                return await Task.Run(() =>
                {
                    Thread.Sleep(2000);
                    string apiContentsString = File.ReadAllText(Settings.Default.FakeAPIFilePath);
                    APIContentsRootObject apiContentsObj = JsonConvert.DeserializeObject<APIContentsRootObject>(apiContentsString);
                    return apiContentsObj;
                });
            }
            else
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    // Send a GET request to the API URL asynchronously
                    HttpResponseMessage response = await httpClient.GetAsync(Settings.Default.APIURL);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string asynchronously
                    string apiContentsString = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON string to the APIContentsRootObject
                    return JsonConvert.DeserializeObject<APIContentsRootObject>(apiContentsString);
                }
            }
        }
    }
}
