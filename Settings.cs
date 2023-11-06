namespace ServerlessSlackBot;

public class Settings
{
    // GCP Project ID
    public static string ProjectId = "PROJECT_ID";

    // GCP Queue Location
    public static string Location = "LOCATION"; // e.g.) us-central1

    // GCP Queue Name
    public static string Queue = "QUEUE_NAME";

    // Main Operation Cloud Function URL
    public static string MainOperetionFunctionURL =
        "https://LOCATION-PROJECT_ID.cloudfunctions.net/OPERATION_FUNCTION_NAME";

    // Slack Bot Token
    public static string BotUserOAuthAccessToken = "xoxb-XXXXXXXXXXXX-XXXXXXXXXXXXXXXXXXXXXXXX";
    
    // Slack Signing Secret
    public static string SlackSigningSecret =
        "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
}
