using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace ServerlessSlackBot;

public class MainOperationFunction : IHttpFunction
{
    /// <summary>
    /// SlackからのリクエストをCloudTasksから呼び出されて改めて処理するメインの関数
    /// </summary>
    /// <param name="context">The HTTP context, containing the request and the response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync(HttpContext context)
    {
        // JSONのリクエストデータを読み出す
        string body = await Utilities.ReadStreamAsync(context.Request.Body);
        // NewtonsoftでJSONのパース
        var jsonObj = JObject.Parse(body);
        Utilities.LogTrace("receive", new {jsonObj});

        // ダミーとして続きの長い処理
        await Task.Delay(TimeSpan.FromSeconds(10)); // 10sec

        Utilities.LogTrace("Delay End");

        // 一応セーフティー付けておく
        // 万が一自分にメンションしていても無視する
        var user = jsonObj["event"]["user"].ToString();
        var selfUserId = jsonObj["authorizations"][0]["user_id"].ToString();
        if (user == selfUserId)
        {
            Utilities.LogWarning("self app_mention");
            return;
        }

        // app_mentionイベントのリクエストから読み出す
        var resText = "";
        var threadTs = "";
        var channel = "";
        try
        {
            // スレッドの場合はthread_tsを、スレッドでない場合はtsを返信用に取得する
            threadTs = jsonObj["event"]["thread_ts"] != null ?
                jsonObj["event"]["thread_ts"].ToString() : jsonObj["event"]["ts"].ToString();

            var type = jsonObj["event"]["type"].ToString();
            var text = jsonObj["event"]["text"].ToString();
            channel = jsonObj["event"]["channel"].ToString();
            resText = $"[type: {type}] {text.Replace(selfUserId, user)}";
        }
        catch (Exception e)
        {
            // Tasks のリトライを避けるため、HTTPのエラーコードを返させないように処理する
            Utilities.LogInformation($"Exception: {e.Message}", new{e});
            resText = $"想定外のEventが届いています。Exception: {e.Message}";
        }
        await PostSlack(resText, threadTs, channel);
    }
    
    private async Task PostSlack(string text, string threadTs, string channel)
    {
        const string apiUrl = "https://slack.com/api/chat.postMessage";
        // Slackにスレッドで返答
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + Settings.BotUserOAuthAccessToken);
        var response = new
        {
            channel,
            text,
            thread_ts = threadTs
        };
        await httpClient.PostAsJsonAsync(apiUrl, response);
    }
}
