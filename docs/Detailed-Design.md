# 詳細設計書 (Detailed Design Document)

## 1. モジュール詳細設計

### 1.1 プレゼンテーション層（Components/Pages）

#### 1.1.1 Home.razor（ダッシュボード）
**役割**: 経費申請の統計情報を表示するダッシュボード画面

**主要機能**:
- 総申請数の表示
- 申請中・承認済み・却下数の表示
- 今月・今年の合計金額表示
- 最近の経費申請リスト表示
- 部署別・カテゴリ別の集計表示

**依存サービス**:
- `ExpenseRequestService`: 経費データの取得

**主要メソッド**:
- `OnInitializedAsync()`: 初期化時にデータを読み込み

**状態管理**:
```csharp
- expenseRequests: List<ExpenseRequest>?  // 全経費データ
- isLoading: bool                          // ローディング状態
- totalCount: int                          // 総申請数
- pendingCount: int                        // 申請中の数
- approvedCount: int                       // 承認済みの数
- rejectedCount: int                       // 却下数
- currentMonthTotal: int                   // 今月の合計金額
- currentYearTotal: int                    // 今年の合計金額
```

#### 1.1.2 ExpenseList.razor（経費申請一覧）
**役割**: 全経費申請をテーブル形式で一覧表示

**主要機能**:
- 経費申請の一覧表示（申請日、社員名、部署、カテゴリ、金額、ステータス）
- 帳票表示へのリンク
- 新規作成ボタン

**依存サービス**:
- `ExpenseRequestService`: 経費データの取得

**主要メソッド**:
- `OnInitializedAsync()`: 一覧データを読み込み
- `GetStatusBadgeClass(string status)`: ステータスに応じたCSSクラスを返す

#### 1.1.3 ExpenseCreate.razor（経費申請作成）
**役割**: 新しい経費申請を作成するフォーム画面

**主要機能**:
- 経費情報入力フォーム
- バリデーション
- 作成成功時の一覧画面への遷移

**依存サービス**:
- `ExpenseRequestService`: 経費データの作成
- `EmployeeService`: 社員リストの取得

**バリデーションルール**:
- 経費発生日: 必須、未来日付は不可
- 社員名: 必須
- 部署: 必須
- カテゴリ: 必須、選択肢から選択
- 金額: 必須、0より大きい値
- 説明: 必須、最大500文字

**カテゴリ選択肢**:
```csharp
- "交通費"
- "宿泊費"
- "会議費"
- "接待費"
- "通信費"
- "消耗品費"
- "その他"
```

#### 1.1.4 ExpenseReport.razor（経費申請帳票）
**役割**: 経費申請の詳細を帳票形式で表示（印刷対応）

**主要機能**:
- 経費詳細情報の表示
- 印刷用レイアウト
- PDF出力対応（ブラウザ印刷機能を使用）

**依存サービス**:
- `ExpenseRequestService`: 経費データの取得

**ルートパラメータ**:
- `{id}`: ExpenseRequestのRowKey

#### 1.1.5 EmployeeList.razor（社員一覧）
**役割**: 社員情報を一覧表示

**主要機能**:
- 社員情報の一覧表示（社員ID、氏名、フリガナ、部署、役職、メールアドレス、入社日、ステータス）
- 新規登録ボタン

**依存サービス**:
- `EmployeeService`: 社員データの取得

**主要メソッド**:
- `OnInitializedAsync()`: 一覧データを読み込み
- `GetStatusBadgeClass(string status)`: ステータスに応じたCSSクラスを返す

#### 1.1.6 EmployeeCreate.razor（社員登録）
**役割**: 新しい社員情報を登録するフォーム画面

**主要機能**:
- 社員情報入力フォーム
- バリデーション
- 作成成功時の一覧画面への遷移

**依存サービス**:
- `EmployeeService`: 社員データの作成

**バリデーションルール**:
- 社員ID: 必須
- 氏名: 必須
- 部署: 必須
- メールアドレス: 必須、メールアドレス形式
- 入社日: 必須

### 1.2 ビジネスロジック層（Services）

#### 1.2.1 ExpenseRequestService
**役割**: 経費申請のCRUD操作を提供

**定数**:
```csharp
private const string BusinessId = "expense-request";
```

**コンストラクタ**:
```csharp
public ExpenseRequestService(string connectionString, string tableName)
```
- TableClientを初期化
- テーブルが存在しない場合は作成

**メソッド一覧**:

##### GetAllExpenseRequestsAsync()
```csharp
public async Task<List<ExpenseRequest>> GetAllExpenseRequestsAsync()
```
- **機能**: 全経費申請を取得
- **フィルタ**: `BusinessId == "expense-request"`
- **並び順**: 経費発生日の降順（新しい順）
- **戻り値**: `List<ExpenseRequest>`

##### GetExpenseRequestAsync(string partitionKey, string rowKey)
```csharp
public async Task<ExpenseRequest?> GetExpenseRequestAsync(string partitionKey, string rowKey)
```
- **機能**: 特定の経費申請を取得
- **戻り値**: `ExpenseRequest?` (存在しない場合はnull)
- **例外処理**: 404エラーの場合はnullを返す

##### CreateExpenseRequestAsync(ExpenseRequest expenseRequest)
```csharp
public async Task<ExpenseRequest> CreateExpenseRequestAsync(ExpenseRequest expenseRequest)
```
- **機能**: 新しい経費申請を作成
- **処理内容**:
  1. PartitionKeyに業務IDを設定
  2. BusinessIdに業務IDを設定
  3. RowKeyが空の場合、GUIDを生成
  4. Azure Table Storageに追加
- **戻り値**: 作成された`ExpenseRequest`

##### UpdateExpenseRequestAsync(ExpenseRequest expenseRequest)
```csharp
public async Task<ExpenseRequest> UpdateExpenseRequestAsync(ExpenseRequest expenseRequest)
```
- **機能**: 経費申請を更新
- **楽観的同時実行制御**: ETagを使用
- **戻り値**: 更新された`ExpenseRequest`

##### DeleteExpenseRequestAsync(string partitionKey, string rowKey)
```csharp
public async Task DeleteExpenseRequestAsync(string partitionKey, string rowKey)
```
- **機能**: 経費申請を削除
- **戻り値**: なし

#### 1.2.2 EmployeeService
**役割**: 社員情報のCRUD操作を提供

**定数**:
```csharp
private const string BusinessId = "employee-management";
```

**コンストラクタ**:
```csharp
public EmployeeService(string connectionString, string tableName)
```
- TableClientを初期化
- テーブルが存在しない場合は作成

**メソッド一覧**:

##### GetAllEmployeesAsync()
```csharp
public async Task<List<Employee>> GetAllEmployeesAsync()
```
- **機能**: 全社員を取得
- **フィルタ**: `BusinessId == "employee-management"`
- **並び順**: 社員IDの昇順
- **戻り値**: `List<Employee>`

##### GetEmployeeAsync(string partitionKey, string rowKey)
```csharp
public async Task<Employee?> GetEmployeeAsync(string partitionKey, string rowKey)
```
- **機能**: 特定の社員を取得
- **戻り値**: `Employee?` (存在しない場合はnull)
- **例外処理**: 404エラーの場合はnullを返す

##### CreateEmployeeAsync(Employee employee)
```csharp
public async Task<Employee> CreateEmployeeAsync(Employee employee)
```
- **機能**: 新しい社員を作成
- **処理内容**:
  1. PartitionKeyに業務IDを設定
  2. BusinessIdに業務IDを設定
  3. RowKeyが空の場合、GUIDを生成
  4. DateTimeをUTCに変換
  5. Azure Table Storageに追加
- **戻り値**: 作成された`Employee`

##### UpdateEmployeeAsync(Employee employee)
```csharp
public async Task<Employee> UpdateEmployeeAsync(Employee employee)
```
- **機能**: 社員情報を更新
- **処理内容**:
  1. DateTimeをUTCに変換
  2. ETagを使用して更新
- **楽観的同時実行制御**: ETagを使用
- **戻り値**: 更新された`Employee`

##### ConvertDateTimesToUtc(Employee employee)
```csharp
private void ConvertDateTimesToUtc(Employee employee)
```
- **機能**: DateTime型フィールドをUTCに変換（プライベートヘルパーメソッド）
- **対象フィールド**: HireDate, DateOfBirth

##### DeleteEmployeeAsync(string partitionKey, string rowKey)
```csharp
public async Task DeleteEmployeeAsync(string partitionKey, string rowKey)
```
- **機能**: 社員を削除
- **戻り値**: なし

### 1.3 データモデル層（Models）

#### 1.3.1 ExpenseRequest
**継承**: `ITableEntity` (Azure.Data.Tables)

**Azure Table Storage標準プロパティ**:
```csharp
public string PartitionKey { get; set; }      // "expense-request" (固定)
public string RowKey { get; set; }            // GUID
public DateTimeOffset? Timestamp { get; set; } // 自動設定（Azure側）
public ETag ETag { get; set; }                // 楽観的同時実行制御用
```

**業務プロパティ**:
```csharp
public string BusinessId { get; set; }        // "expense-request" (固定)
public DateTime ExpenseDate { get; set; }     // 経費発生日
public string EmployeeName { get; set; }      // 社員名
public string Department { get; set; }        // 部署
public string Category { get; set; }          // カテゴリ
public int Amount { get; set; }               // 金額
public string Description { get; set; }       // 説明
public string Status { get; set; }            // ステータス
```

**ステータス値**:
- `"申請中"`: 初期状態
- `"承認済"`: 承認された状態
- `"却下"`: 却下された状態

#### 1.3.2 Employee
**継承**: `ITableEntity` (Azure.Data.Tables)

**Azure Table Storage標準プロパティ**:
```csharp
public string PartitionKey { get; set; }      // "employee-management" (固定)
public string RowKey { get; set; }            // GUID
public DateTimeOffset? Timestamp { get; set; } // 自動設定（Azure側）
public ETag ETag { get; set; }                // 楽観的同時実行制御用
```

**業務プロパティ**:
```csharp
[Required(ErrorMessage = "社員IDは必須です")]
public string EmployeeId { get; set; }        // 社員ID

[Required(ErrorMessage = "氏名は必須です")]
public string FullName { get; set; }          // 氏名

public string Kana { get; set; }              // フリガナ

[Required(ErrorMessage = "部署は必須です")]
public string Department { get; set; }        // 部署

public string Position { get; set; }          // 役職

[EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
public string Email { get; set; }             // メールアドレス

public DateTime HireDate { get; set; }        // 入社日
public DateTime? DateOfBirth { get; set; }    // 生年月日
public string Status { get; set; }            // ステータス（デフォルト: "在職中"）
```

**ステータス値**:
- `"在職中"`: デフォルト値
- `"退職済み"`: 退職した社員
- `"休職中"`: 休職中の社員

## 2. データフロー

### 2.1 経費申請作成フロー

```
[ユーザー]
    ↓ (1) フォーム入力
[ExpenseCreate.razor]
    ↓ (2) CreateExpenseRequestAsync() 呼び出し
[ExpenseRequestService]
    ↓ (3) PartitionKey/RowKey/BusinessId設定
    ↓ (4) AddEntityAsync() 呼び出し
[Azure Table Storage]
    ↓ (5) データ保存完了
[ExpenseRequestService]
    ↓ (6) 作成されたエンティティを返す
[ExpenseCreate.razor]
    ↓ (7) NavigationManager.NavigateTo("/expense-requests")
[ExpenseList.razor]
```

### 2.2 経費申請一覧表示フロー

```
[ユーザー]
    ↓ (1) ページアクセス
[ExpenseList.razor]
    ↓ (2) OnInitializedAsync() 実行
    ↓ (3) GetAllExpenseRequestsAsync() 呼び出し
[ExpenseRequestService]
    ↓ (4) QueryAsync<ExpenseRequest>() 実行
    ↓     フィルタ: BusinessId == "expense-request"
[Azure Table Storage]
    ↓ (5) データ取得
[ExpenseRequestService]
    ↓ (6) ExpenseDate降順でソート
    ↓ (7) List<ExpenseRequest>を返す
[ExpenseList.razor]
    ↓ (8) 一覧をレンダリング
[ユーザー]
```

### 2.3 ダッシュボード統計表示フロー

```
[ユーザー]
    ↓ (1) ダッシュボードアクセス
[Home.razor]
    ↓ (2) OnInitializedAsync() 実行
    ↓ (3) GetAllExpenseRequestsAsync() 呼び出し
[ExpenseRequestService]
    ↓ (4) 全経費データ取得
[Home.razor]
    ↓ (5) ローカルで統計計算
    ↓     - 総数、ステータス別カウント
    ↓     - 今月・今年の合計金額
    ↓     - 部署別・カテゴリ別集計
    ↓ (6) 統計情報をレンダリング
[ユーザー]
```

## 3. エラーハンドリング

### 3.1 サービス層のエラーハンドリング

#### 404エラー（Not Found）
```csharp
try
{
    var response = await _tableClient.GetEntityAsync<T>(partitionKey, rowKey);
    return response.Value;
}
catch (Azure.RequestFailedException ex) when (ex.Status == 404)
{
    return null;  // エンティティが存在しない場合はnullを返す
}
```

#### 同時実行制御エラー（ETag不一致）
```csharp
await _tableClient.UpdateEntityAsync(entity, entity.ETag);
// ETagが一致しない場合、Azure.RequestFailedExceptionがスローされる
```

### 3.2 プレゼンテーション層のエラーハンドリング

#### データ取得エラー
```csharp
try
{
    expenseRequests = await ExpenseService.GetAllExpenseRequestsAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading expense requests: {ex.Message}");
    // エラーメッセージを表示（将来的にはユーザーフレンドリーなエラー表示を実装）
}
finally
{
    isLoading = false;
}
```

### 3.3 アプリケーションレベルのエラーハンドリング

#### 開発環境
- Developer Exception Page を表示

#### 本番環境
```csharp
app.UseExceptionHandler("/Error", createScopeForErrors: true);
```
- `/Error` ページにリダイレクト

## 4. セキュリティ詳細

### 4.1 CSRF対策
```csharp
app.UseAntiforgery();
```
- Blazor Serverは自動的にAntiforgeryトークンを使用

### 4.2 HTTPS強制（本番環境）
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
```

### 4.3 接続文字列の保護
- 開発環境: `appsettings.Development.json` (`.gitignore`で除外)
- 本番環境: 環境変数（Azure App Serviceの構成設定）

### 4.4 入力バリデーション
- Data Annotations を使用
- Blazor の EditForm コンポーネントで自動バリデーション

## 5. パフォーマンス設計

### 5.1 Azure Table Storageのクエリ最適化

#### PartitionKeyによるフィルタリング
```csharp
// 効率的: PartitionKeyでフィルタ（インデックス使用）
await foreach (var entity in _tableClient.QueryAsync<T>(
    e => e.PartitionKey == BusinessId))
{
    // 処理
}
```

#### 非効率なクエリの回避
```csharp
// 非効率: 全スキャン（避けるべき）
await foreach (var entity in _tableClient.QueryAsync<T>())
{
    if (entity.SomeProperty == someValue) { /* ... */ }
}
```

### 5.2 クライアント側の最適化

#### ローディング状態の表示
```razor
@if (isLoading)
{
    <div class="spinner-border" role="status">
        <span class="visually-hidden">読み込み中...</span>
    </div>
}
```

#### 非同期処理
- 全てのデータアクセスは非同期（`async/await`）

## 6. 拡張性設計

### 6.1 新規業務機能の追加手順

#### 1. モデルクラスの作成
```csharp
// Models/NewFeature.cs
public class NewFeature : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    public string BusinessId { get; set; } = string.Empty;
    // 業務プロパティ
}
```

#### 2. サービスクラスの作成
```csharp
// Services/NewFeatureService.cs
public class NewFeatureService
{
    private readonly TableClient _tableClient;
    private const string BusinessId = "new-feature";  // ケバブケース
    
    public NewFeatureService(string connectionString, string tableName)
    {
        var tableServiceClient = new TableServiceClient(connectionString);
        _tableClient = tableServiceClient.GetTableClient(tableName);
        _tableClient.CreateIfNotExists();
    }
    
    // CRUDメソッド実装
}
```

#### 3. Program.csに登録
```csharp
builder.Services.AddSingleton(new NewFeatureService(connectionString, tableName));
```

#### 4. Razorページの作成
```razor
@page "/new-feature"
@inject NewFeatureService NewFeatureService
```

### 6.2 既存機能の拡張

#### ステータスの追加
1. モデルクラスはそのまま（stringプロパティ）
2. 画面側で新しいステータス値を追加
3. バッジクラスの分岐を追加

#### プロパティの追加
1. モデルクラスにプロパティを追加
2. Azure Table Storageはスキーマレスなので、既存データに影響なし
3. サービスクラスはそのまま（自動マッピング）
4. 画面側で新しいプロパティを表示

## 7. テスト設計（将来実装）

### 7.1 ユニットテスト
- **対象**: Services層
- **フレームワーク**: xUnit, Moq
- **モック**: TableClient をモック化

### 7.2 統合テスト
- **対象**: Services層 + Azure Table Storage
- **環境**: Azure Storage Emulator / Azurite

### 7.3 E2Eテスト
- **対象**: Blazor Components
- **フレームワーク**: bUnit, Playwright

## 8. 監視・ログ設計（将来実装）

### 8.1 Application Insights統合
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### 8.2 構造化ログ
```csharp
builder.Logging.AddJsonConsole();
```

### 8.3 監視項目
- リクエスト数・レスポンスタイム
- Azure Table Storage操作の成功率
- エラー発生率
- SignalR接続状態

## 9. 参照ドキュメント

- [基本設計書](./Basic-Design.md)
- [プロジェクトREADME](../README.md)
- [Copilot開発ガイドライン](../.github/copilot-instructions.md)
- [Blazor公式ドキュメント](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/)
- [Azure Table Storage SDK ドキュメント](https://learn.microsoft.com/ja-jp/dotnet/api/azure.data.tables)

---

**作成日**: 2026-01-23  
**バージョン**: 1.0  
**更新履歴**:
- 2026-01-23: 初版作成
