# Copilot 開発ガイドライン

## プロジェクト概要

- Blazor Server (.NET 8)
- Azure Table Storage

## 重要ルール

### 業務ID（BusinessId）

新規機能追加時は **必ず BusinessId を定数で設定** してください。

```csharp
public class XxxService
{
    private const string BusinessId = "xxx-feature";  // ケバブケース
    
    public async Task<Xxx> CreateAsync(Xxx entity)
    {
        entity.PartitionKey = BusinessId;
        entity.BusinessId = BusinessId;
        // ...
    }
}
```

### DateTime

UTC必須：`DateTime.UtcNow` または `new DateTime(..., DateTimeKind.Utc)`

### 設定ファイル

接続文字列は `appsettings.Development.json` に設定（Git除外済み）
