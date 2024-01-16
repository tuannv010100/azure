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
using PlayFab.ClientModels;
using System.Collections.Generic;
using PlayFab.Samples;
using System.Linq;

namespace Company.Function
{
    public static class UpdateStatisticBot
    {
        [FunctionName("Leaderboard")]
        [Obsolete]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            PlayFabSettings.staticSettings.TitleId = Environment.GetEnvironmentVariable("PLAYFAB_TITLE_ID");
            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY", EnvironmentVariableTarget.Process);
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;

            string playerId = args["playerId"].ToString();
            string playerRivalId = args["playerRival"].ToString();
            string statisticName = args["statisticName"].ToString();
            bool isWin = (bool)args["isWin"];
            var requestLeaderboardRival = new PlayFab.ServerModels.GetLeaderboardAroundUserRequest(){
                PlayFabId = playerRivalId,
                StatisticName = statisticName,
                MaxResultsCount = 1
            };
            var resultLeaderboardRival = await PlayFabServerAPI.GetLeaderboardAroundUserAsync(requestLeaderboardRival);
            int statisticRial = resultLeaderboardRival.Result.Leaderboard[0].StatValue;
            int positionRial = resultLeaderboardRival.Result.Leaderboard[0].Position;

            var requestLeaderboardPlayer = new PlayFab.ServerModels.GetLeaderboardAroundUserRequest(){
                PlayFabId = playerId,
                StatisticName = statisticName,
                MaxResultsCount = 1
            };
            var resultLeaderboardPlayer = await PlayFabServerAPI.GetLeaderboardAroundUserAsync(requestLeaderboardPlayer);
            int staticPlayer = resultLeaderboardPlayer.Result.Leaderboard[0].StatValue;
            int posotionPlayer = resultLeaderboardPlayer.Result.Leaderboard[0].Position;
            
            int newStatisticPlayer = NewStatistic(staticPlayer, statisticRial, isWin, posotionPlayer);
            int newStatisticRival = NewStatistic(statisticRial, staticPlayer, !isWin, positionRial);
            UpdateStatisticPlayer(newStatisticPlayer, playerId, statisticName);
            UpdateStatisticPlayer(newStatisticRival, playerRivalId, statisticName);
            return new OkObjectResult(playerId);
        }
        public static int K_factor(int position){
            int k = 0;
            if (position >= 20000) k = 25;
            if ((position >= 10000) && (position < 20000)) k = 23;
            if ((position >= 5000) && (position < 10000)) k = 21;
            if ((position >= 4000) && (position < 5000)) k = 19;
            if ((position >= 3000) && (position < 4000)) k = 18;
            if ((position >= 2000) && (position < 3000)) k = 17;
            if ((position >= 1000) && (position < 2000)) k = 16;
            if ((position >= 500) && (position < 1000)) k = 15;
            if ((position >= 100) && (position < 500)) k = 14;
            if ((position >= 50) && (position < 100)) k = 13;
            if ((position >= 10) && (position < 50)) k = 12;
            if (position < 10) k = 10;
            return k;
        }
        public static int NewStatistic(int statisticPlayer, int statisticRial, bool isWin, int position){
            var expectedWinner = 1 / (1 + Math.Pow(10, (statisticRial - statisticPlayer) / 400));
            var expectedLoser = 1 / (1 + Math.Pow(10, (statisticPlayer - statisticRial) / 400));
            int playerStatistic;
            if (isWin) {
                playerStatistic = (int)Math.Floor(statisticPlayer + K_factor(position) * (1 - expectedWinner));
            } else {
                playerStatistic = (int)((statisticPlayer + K_factor(position) * (0 - expectedLoser)) > 0 ?
                    Math.Floor(statisticPlayer + K_factor(position) * (0 - expectedLoser)) : 0);
            }
            return playerStatistic;
        }
        public static void UpdateStatisticPlayer(int statistic, string playerId, string statisticName){
            var statisticUpdate = new List<PlayFab.ServerModels.StatisticUpdate>();
            var data = new PlayFab.ServerModels.StatisticUpdate(){
                Value = statistic,
                StatisticName = statisticName
            };
            statisticUpdate.Add(data);
            var request = new PlayFab.ServerModels.UpdatePlayerStatisticsRequest(){
                PlayFabId = playerId,
                Statistics = statisticUpdate
            };
            PlayFabServerAPI.UpdatePlayerStatisticsAsync(request);
        }
    }
}
