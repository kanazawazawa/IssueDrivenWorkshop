using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace IssueDrivenWorkshop.Models;

public class Employee : ITableEntity
{
    // PartitionKey = 業務ID（BusinessId）
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // 業務ID（PartitionKeyと同じ値、表示・検索用）
    public string BusinessId { get; set; } = string.Empty;

    // 社員ID
    [Required(ErrorMessage = "社員IDは必須です")]
    public string EmployeeId { get; set; } = string.Empty;

    // 氏名
    [Required(ErrorMessage = "氏名は必須です")]
    public string FullName { get; set; } = string.Empty;

    // フリガナ
    public string Kana { get; set; } = string.Empty;

    // 部署
    [Required(ErrorMessage = "部署は必須です")]
    public string Department { get; set; } = string.Empty;

    // 役職
    public string Position { get; set; } = string.Empty;

    // メールアドレス
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    public string Email { get; set; } = string.Empty;

    // 入社日
    public DateTime HireDate { get; set; }

    // 生年月日
    public DateTime? DateOfBirth { get; set; }

    // ステータス（在職中、退職済みなど）
    public string Status { get; set; } = "在職中";
}
