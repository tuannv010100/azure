using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PlayFab;
using PlayFab.ServerModels;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.IO;
namespace Company.Function
{
    public static class HttpTrigger1
    {
        [FunctionName("HttpTrigger1")]
        [Obsolete]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "getPlayerCarInfo")] HttpRequest req,
            ILogger log)
        {
            PlayFabSettings.TitleId = Environment.GetEnvironmentVariable("PLAYFAB_TITLE_ID");
            PlayFabSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var requestModel = JsonConvert.DeserializeObject<RequestModel>(requestBody);

            if (requestModel == null || string.IsNullOrWhiteSpace(requestModel.PlayerId))
            {
                return new BadRequestObjectResult("Invalid input.");
            }
            var data = new Dictionary<string, string>
            {
                {"UserData", requestModel.Data}
            };
            var request = new UpdateUserDataRequest
            {
                PlayFabId = requestModel.PlayerId,
                Data = data
            };

            var response = await PlayFabServerAPI.UpdateUserDataAsync(request);
            if (response.Error != null)
            {
                log.LogError(response.Error.GenerateErrorReport());
                return new StatusCodeResult(500);
            }

            if (response.Result.ToString().Length > 0)
            {

                return new OkObjectResult(response.Result);
            }

            return new OkObjectResult("userInfo not found.");
        }
    }
        public class RequestModel
        {
            public string PlayerId { get; set; }
            public string Data {get;set;}
        }
}
