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
using PlayFab.Samples;
using System.Collections.Generic;
using Azure.Gzip;
namespace Company.Function
{
    public static class MailboxData
    {
        [FunctionName("MailboxData")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // GzipDecompress gzip = new();
            PlayFabSettings.staticSettings.TitleId = Environment.GetEnvironmentVariable("PLAYFAB_TITLE_ID");
            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY", EnvironmentVariableTarget.Process);
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            string data = args["data"].ToString();
            string playerId = args["playerId"].ToString();
            var newMail = JsonConvert.DeserializeObject<Mailbox>(data);

            var request = new PlayFab.ServerModels.GetUserDataRequest(){
                Keys = new List<string> { "MailboxData" },
                PlayFabId = playerId
            };
            var result = await PlayFabServerAPI.GetUserDataAsync(request);
            var oldMailbox = new List<Mailbox>();
            Dictionary<string, PlayFab.ServerModels.UserDataRecord> userData = result.Result.Data;
            if(userData.Count != 0)
            {
                foreach (var kvp in userData)
                {
                    var key = kvp.Key;
                    var value = kvp.Value;
                    if (value.Value.Length != 0)
                    {
                        if (key == "MailboxData")
                        {
                            JsonSerializerSettings setting = new()
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            };
                            oldMailbox = JsonConvert.DeserializeObject<List<Mailbox>>(value.Value.ToString(), setting);
                        }
                    }
                }
            }
            oldMailbox.Add(newMail);
            var dataRequestUpdateMailBox = new Dictionary<string, string>
            {
                { "MailboxData", JsonConvert.SerializeObject(oldMailbox) }
            };
            var requestUpdateMailbox = new PlayFab.ServerModels.UpdateUserDataRequest(){
                PlayFabId = playerId,
                Data = dataRequestUpdateMailBox
            };
            var resultUpdateMail = await PlayFabServerAPI.UpdateUserDataAsync(requestUpdateMailbox); 

            return new OkObjectResult(JsonConvert.SerializeObject(data));
        }
    }
    public class Mailbox{
        public string Sender;
        public string Receiver;
        public string Topic;
        public string Content;
        public List<Attachment> Attachments;
        public string DateSent;
    }
    public class Attachment {
        public string Id;
        public int Count;
    }
}
