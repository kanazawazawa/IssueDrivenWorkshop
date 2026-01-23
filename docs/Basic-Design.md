# 基本設計書 (Basic Design Document)

## 1. システム概要

### 1.1 プロジェクト名
**IssueDrivenWorkshop** - 経費申請システム

### 1.2 システムの目的
企業における経費申請業務をデジタル化し、効率的な経費管理を実現するWebアプリケーションです。社員管理機能と経費申請機能を提供し、申請から承認までのプロセスを一元管理します。

### 1.3 主要機能
- **ダッシュボード**: 経費申請の統計情報をリアルタイムで表示
- **経費申請管理**: 経費の申請、一覧表示、帳票出力
- **社員管理**: 社員情報の登録、一覧表示、編集

## 2. 技術スタック

### 2.1 フレームワーク・ライブラリ
| 技術 | バージョン | 用途 |
|------|-----------|------|
| .NET | 8.0 | ベースフレームワーク |
| Blazor Server | - | UIフレームワーク |
| Azure Data Tables | 12.11.0 | データストレージ |
| Bootstrap | 5.x | CSSフレームワーク |

### 2.2 開発言語
- **C# 12.0** (with Nullable Reference Types enabled)
- **Razor** (Blazor Components)

### 2.3 データストア
- **Azure Table Storage**: NoSQLデータベースとして使用
  - スケーラブルで低コスト
  - Key-Value形式のデータ管理

## 3. システムアーキテクチャ

### 3.1 全体構成
```
┌─────────────────────────────────────────────────────┐
│                   ユーザー（ブラウザ）                  │
└──────────────────────┬──────────────────────────────┘
                       │ HTTPS
                       ↓
┌─────────────────────────────────────────────────────┐
│              Blazor Server Application               │
│  ┌─────────────────────────────────────────────┐   │
│  │         Components/Pages (UI Layer)         │   │
│  │  - Home.razor (ダッシュボード)                │   │
│  │  - ExpenseList.razor (経費一覧)              │   │
│  │  - ExpenseCreate.razor (経費作成)            │   │
│  │  - EmployeeList.razor (社員一覧)             │   │
│  │  - EmployeeCreate.razor (社員登録)           │   │
│  └─────────────────┬───────────────────────────┘   │
│                    │                                 │
│  ┌─────────────────▼───────────────────────────┐   │
│  │      Services (Business Logic Layer)        │   │
│  │  - ExpenseRequestService                    │   │
│  │  - EmployeeService                          │   │
│  └─────────────────┬───────────────────────────┘   │
│                    │                                 │
│  ┌─────────────────▼───────────────────────────┐   │
│  │         Models (Data Layer)                 │   │
│  │  - ExpenseRequest                           │   │
│  │  - Employee                                 │   │
│  └─────────────────────────────────────────────┘   │
└──────────────────────┬──────────────────────────────┘
                       │ SDK (Azure.Data.Tables)
                       ↓
┌─────────────────────────────────────────────────────┐
│            Azure Table Storage                      │
│  - Expenses Table (経費申請・社員データ)              │
└─────────────────────────────────────────────────────┘
```

### 3.2 アーキテクチャパターン
- **3層アーキテクチャ**
  1. **プレゼンテーション層**: Blazor Components (Razor ファイル)
  2. **ビジネスロジック層**: Services クラス
  3. **データアクセス層**: Models + Azure Table Storage Client

### 3.3 レンダリングモード
- **Blazor Server**: サーバーサイドレンダリング with SignalR
  - リアルタイムUI更新
  - サーバーとクライアント間でSignalR接続を維持

## 4. データ設計

### 4.1 データストア構造
Azure Table Storageを使用し、単一のテーブル「Expenses」に複数の業務データを格納します。

#### テーブル設計の方針
- **PartitionKey**: 業務ID（BusinessId）を設定し、データを論理的に分割
- **RowKey**: エンティティごとに一意のGUIDを設定
- **BusinessId**: 業務識別用のフィールド（PartitionKeyと同値）

### 4.2 業務ID一覧
| 業務ID | 説明 | エンティティ |
|--------|------|-------------|
| `expense-request` | 経費申請 | ExpenseRequest |
| `employee-management` | 社員管理 | Employee |

### 4.3 主要エンティティ

#### ExpenseRequest（経費申請）
- **PartitionKey**: `expense-request` (固定)
- **RowKey**: GUID
- **主要フィールド**:
  - ExpenseDate: 経費発生日
  - EmployeeName: 社員名
  - Department: 部署
  - Category: カテゴリ
  - Amount: 金額
  - Description: 説明
  - Status: ステータス（申請中/承認済/却下）

#### Employee（社員）
- **PartitionKey**: `employee-management` (固定)
- **RowKey**: GUID
- **主要フィールド**:
  - EmployeeId: 社員ID
  - FullName: 氏名
  - Kana: フリガナ
  - Department: 部署
  - Position: 役職
  - Email: メールアドレス
  - HireDate: 入社日
  - DateOfBirth: 生年月日
  - Status: ステータス（在職中/退職済み/休職中）

## 5. セキュリティ

### 5.1 データ通信
- **HTTPS**: 全ての通信を暗号化
- **HSTS**: HTTP Strict Transport Security（本番環境）

### 5.2 認証・認可
- 現状: 認証機能なし（将来実装予定）
- CSRF対策: `app.UseAntiforgery()` を使用

### 5.3 接続文字列管理
- 開発環境: `appsettings.Development.json` (Git除外)
- 本番環境: 環境変数 (`AZURE_TABLESTORAGE_CONNECTIONSTRING`)

## 6. デプロイ・運用

### 6.1 開発環境
- ローカル実行: `dotnet run`
- ポート: HTTPS 7123

### 6.2 本番環境
- **ホスティング**: Azure App Service
- **CI/CD**: GitHub Actions
  - main ブランチへのプッシュで自動デプロイ
  - ワークフローファイル: `.github/workflows/deploy.yml`

### 6.3 環境設定
| 設定項目 | 開発環境 | 本番環境 |
|---------|---------|---------|
| 接続文字列 | appsettings.Development.json | 環境変数 |
| テーブル名 | appsettings.json (デフォルト: Expenses) | 環境変数 |
| HTTPS Redirect | No | Yes |
| Exception Handler | Developer Page | /Error |

## 7. 開発規約

### 7.1 コーディング規約
- **Nullable Reference Types**: 有効化必須
- **Implicit Usings**: 有効化
- **DateTime**: UTC必須（`DateTime.UtcNow` または `DateTimeKind.Utc`）

### 7.2 業務ID（BusinessId）規約
新規機能追加時は **必ず定数でBusinessIdを設定**:

```csharp
private const string BusinessId = "feature-name";  // ケバブケース
```

### 7.3 ファイル構成規約
```
IssueDrivenWorkshop/
├── Components/
│   ├── Pages/          # ページコンポーネント
│   ├── Layout/         # レイアウトコンポーネント
│   └── _Imports.razor  # 共通インポート
├── Models/             # データモデル
├── Services/           # ビジネスロジック
├── wwwroot/            # 静的ファイル
└── Program.cs          # アプリケーションエントリーポイント
```

## 8. 今後の拡張予定

### 8.1 機能拡張
- 認証・認可機能（Azure AD B2C統合）
- 承認ワークフロー
- ファイル添付機能（領収書画像など）
- メール通知機能

### 8.2 技術的改善
- ユニットテスト・統合テストの追加
- ロギング機能の強化（Application Insights）
- パフォーマンス監視
- エラーハンドリングの改善

## 9. 参照ドキュメント

- [プロジェクトREADME](../README.md)
- [Copilot開発ガイドライン](../.github/copilot-instructions.md)
- [詳細設計書](./Detailed-Design.md)
- [Blazor公式ドキュメント](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/)
- [Azure Table Storage ドキュメント](https://learn.microsoft.com/ja-jp/azure/storage/tables/)

---

**作成日**: 2026-01-23  
**バージョン**: 1.0  
**更新履歴**:
- 2026-01-23: 初版作成
