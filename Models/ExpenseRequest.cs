using Azure;
using Azure.Data.Tables;

namespace IssueDrivenWorkshop.Models;

public class ExpenseRequest : ITableEntity
{
    // PartitionKey = 業務ID（BusinessId）
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // 業務ID（PartitionKeyと同じ値、表示・検索用）
    public string BusinessId { get; set; } = string.Empty;

    // 経費発生日
    public DateTime ExpenseDate { get; set; }

    // 社員名
    public string EmployeeName { get; set; } = string.Empty;

    // 部署
    public string Department { get; set; } = string.Empty;

    // カテゴリ
    public string Category { get; set; } = string.Empty;

    // 金額
    public int Amount { get; set; }

    // 説明
    public string Description { get; set; } = string.Empty;

    // ステータス
    public string Status { get; set; } = string.Empty;
}
