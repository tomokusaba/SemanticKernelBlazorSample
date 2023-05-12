# SemanticKernelBlazorSample

Semantic Kernelを使ってAIチャットをBlazorで実装してみた例です。
ストリーミング対応しています。
現在、Semantic Kernel 0.13の前提です。


appsettings.jsonを以下のように作成してください。

```
{
  "ServiceId": "gpt-4",
  "DeploymentName": "デプロイメント名",
  "BaseUrl": "エンドポイント",
  "Gtp4Key": "APIキー"

}
```
