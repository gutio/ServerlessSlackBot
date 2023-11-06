using System;
using System.Security.Cryptography;
using System.Text;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Google.Cloud.Tasks.V2;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;

namespace ServerlessSlackBot;

public class ReceiveFunction : IHttpFunction
{
    /// <summary>
    /// Slackからのリクエストを受け取って、CloudTasksに登録するCloud Function
    /// </summary>
    /// <param name="context">The HTTP context, containing the request and the response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async System.Threading.Tasks.Task HandleAsync(HttpContext context)
    {
        // JSONのリクエストデータを読み出す
        string body = await Utilities.ReadStreamAsync(context.Request.Body);
        var timestamp = context.Request.Headers["X-Slack-Request-Timestamp"];
        var signature = context.Request.Headers["X-Slack-Signature"];
        // 署名の検証
        if (!VerifySignature(body, timestamp, signature))
        {
            context.Response.StatusCode = 401;
            return;
        }
        
        // NewtonsoftでJSONのパース
        JObject jsonObj = JObject.Parse(body);
        // リクエストタイプの取得
        string requestType = jsonObj["type"].ToString();

        // WebHook URL の検証
        // SEE: https://api.slack.com/events/url_verification
        if (requestType == "url_verification")
        {
            var challenge = jsonObj["challenge"].ToString();
            await context.Response.WriteAsync(challenge);
            return;
        }

        // CloudTasksに登録する
        CloudTasksClient client = await CloudTasksClient.CreateAsync();
        QueueName parent = new QueueName(Settings.ProjectId, Settings.Location, Settings.Queue);
        // タスク名はメッセージごとのユニークなIDを使う
        // 万が一レスポンスが遅れてSlackから再送されたとしても、
        // 同じタスク名の場合はCloudTasksが重複を排除してくれる
        // SEE: https://cloud.google.com/tasks/docs/reference/rest/v2/projects.locations.queues.tasks/create
        TaskName taskName = new TaskName(Settings.ProjectId, Settings.Location, Settings.Queue,
            jsonObj["event"]["client_msg_id"].ToString());

        var response = await client.CreateTaskAsync(new CreateTaskRequest
        {
            Parent = parent.ToString(),
            Task = new Task
            {
                Name = taskName.ToString(),
                // 型名がNamespace間でバッティングするので明示する
                HttpRequest = new Google.Cloud.Tasks.V2.HttpRequest
                {
                    HttpMethod = HttpMethod.Post,
                    Url = Settings.MainOperetionFunctionURL,
                    Body = ByteString.CopyFromUtf8(body) // そのまま渡す
                },
                ScheduleTime = Timestamp.FromDateTime(
                    DateTime.UtcNow.AddSeconds(0)) // 遅延なしに今すぐキック
            }
        });

        // ３秒制限があるので一旦急いで200を返す
        await context.Response.WriteAsync("OK");
    }
    
    private bool VerifySignature(string body, string timestamp, string signature)
    {
        // 署名の検証
        // SEE: https://api.slack.com/docs/verifying-requests-from-slack
        var sigBaseString = $"v0:{timestamp}:{body}";
        // この関数内部で直接設定情報にアクセスするのかは悩みどころ
        var mySignature = $"v0={CreateSignature(sigBaseString, Settings.SlackSigningSecret)}";
        return mySignature == signature;
    }
    
    private string CreateSignature(string inputStr, string secretKey)
    {
        HMACSHA256 hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var signature = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(inputStr));
        var base16Signature = Convert.ToHexString(signature).ToLower();
        return base16Signature;
    }
}
