using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab;
using System.Collections.Generic;
using PlayFab.Samples;

namespace Company.Function
{
    public static class UnlockContainerItem
    {
        [FunctionName("UnlockContainerItem")]
        [Obsolete]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UnlockContainer")] HttpRequest req, 
            ILogger log)
        {
            PlayFabSettings.TitleId = Environment.GetEnvironmentVariable("PLAYFAB_TITLE_ID");
            PlayFabSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY");
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            var itemInstance = JsonConvert.DeserializeObject<PlayFab.ServerModels.ItemInstance>(args["data"].ToString());
            string playerId = args["playerId"].ToString();
            string itemId = itemInstance.ItemId + "x10";
            string itemInstanceId = itemInstance.ItemInstanceId;
            ConsumeItemx10(playerId, itemInstanceId);
            var listItem = new List<string>
            {
                itemId
            };
            var request = new PlayFab.ServerModels.GrantItemsToUserRequest(){
                PlayFabId = playerId,
                ItemIds = listItem
            };
            await PlayFabServerAPI.GrantItemsToUserAsync(request);
            var requestData = new PlayFab.ServerModels.UnlockContainerItemRequest(){
                PlayFabId = playerId,
                ContainerItemId = itemId
            }; 
            var result = await PlayFabServerAPI.UnlockContainerItemAsync(requestData);
            return new OkObjectResult(result.Result.GrantedItems);
        }
        public static async void ConsumeItemx10(string playerId, string itemInstanceId){
            var request = new PlayFab.ServerModels.ConsumeItemRequest(){
                ConsumeCount = 10,
                ItemInstanceId = itemInstanceId,
                PlayFabId = playerId
            };
            await PlayFabServerAPI.ConsumeItemAsync(request);
        }
    }
    public class Data {
        private int Amount {get; set;}
        private string ItemId {get; set;}
        public Data(int amount, string itemId)
        {
            this.Amount = amount;
            this.ItemId = itemId;
        }
    }
}
