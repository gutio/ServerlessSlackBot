# ServerlessSlackBot

SlackのEvent API を使った Serverless な Slack Botのサンプル。

Event API でメッセージを受け取って、しばらく経過した後にSlackにメッセージを送信するサンプルです。

## ファイル概要
- Settings: 設定情報を保持するソースです
- ReceiveFunction: SlackのEvent APIを受け取ってCloud Tasks に登録し直す 受信用 Cloud Functions
- MainOperationFunction: Cloud Tasks から受け取ったEvent APIを処理する メイン処理用 Cloud Functions

## 起動方法
1. SlackでAppを作成し、レスポンス用のchat:write の権限を付与して Bot User OAuth Tokenを取得して、SettingsのBotUserOAuthAccessTokenに設定します。 
2. MainOperationFunction をCloud Functionにデプロイし、URLをSettingsのMainOperationFunctionURLに設定します。
   > `gcloud functions deploy MAIN_NAME --entry-point ServerlessSlackBot.MainOperationFunction --gen2 --runtime dotnet6 --trigger-http --allow-unauthenticated`
3. Cloud Tasks を有効にして、Queueを作成したのちに、SettingsのProjectId、Location、Queueに設定します。
4. ReceiveFunction をCloud Functionにデプロイします。
   > `gcloud functions deploy RECEIVE_NAME --entry-point ServerlessSlackBot.ReceiveFunction --gen2 --runtime dotnet6 --trigger-http --allow-unauthenticated`
5. Slack AppのEvent SubscriptionsのRequest URLにReceiveFunctionのURLを設定します。
6. Subscribe to bot eventsのapp_mentionを設定します。
7. Slack上でBotにメンションを飛ばすと、しばらく経過した後にBotからメッセージが返ってきます。
