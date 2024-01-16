using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using PlayFab;

namespace Company.Function
{
    public static class StatisticBot
    {
        [FunctionName("StatisticBot")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var data = new PlayFab.ClientModels.StatisticUpdate(){
                StatisticName = "TopPlayers",
                Value = 20
            };
            var request = new PlayFab.ClientModels.UpdatePlayerStatisticsRequest{
                Statistics = new List<PlayFab.ClientModels.StatisticUpdate>{
                    data
                }
            };
            var result = await PlayFabClientAPI.UpdatePlayerStatisticsAsync(request);
            return new OkObjectResult(result.Result.ToString());
        }
    }
}
