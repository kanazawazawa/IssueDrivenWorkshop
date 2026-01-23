using Azure.Data.Tables;
using IssueDrivenWorkshop.Models;

namespace IssueDrivenWorkshop.Services;

public class EmployeeService
{
    private readonly TableClient _tableClient;
    
    // 業務ID（定数）
    private const string BusinessId = "employee-management";

    public EmployeeService(string connectionString, string tableName)
    {
        var tableServiceClient = new TableServiceClient(connectionString);
        _tableClient = tableServiceClient.GetTableClient(tableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        var employees = new List<Employee>();
        
        // 業務IDでフィルタリング
        await foreach (var entity in _tableClient.QueryAsync<Employee>(e => e.BusinessId == BusinessId))
        {
            employees.Add(entity);
        }

        return employees.OrderBy(e => e.EmployeeId).ToList();
    }

    public async Task<Employee?> GetEmployeeAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<Employee>(partitionKey, rowKey);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        // PartitionKeyに業務IDを設定
        employee.PartitionKey = BusinessId;
        employee.BusinessId = BusinessId;
        
        if (string.IsNullOrEmpty(employee.RowKey))
        {
            employee.RowKey = Guid.NewGuid().ToString();
        }

        // DateTimeをUTCに変換
        employee.HireDate = DateTime.SpecifyKind(employee.HireDate, DateTimeKind.Utc);
        if (employee.DateOfBirth.HasValue)
        {
            employee.DateOfBirth = DateTime.SpecifyKind(employee.DateOfBirth.Value, DateTimeKind.Utc);
        }

        await _tableClient.AddEntityAsync(employee);
        return employee;
    }

    public async Task<Employee> UpdateEmployeeAsync(Employee employee)
    {
        // DateTimeをUTCに変換
        employee.HireDate = DateTime.SpecifyKind(employee.HireDate, DateTimeKind.Utc);
        if (employee.DateOfBirth.HasValue)
        {
            employee.DateOfBirth = DateTime.SpecifyKind(employee.DateOfBirth.Value, DateTimeKind.Utc);
        }

        await _tableClient.UpdateEntityAsync(employee, employee.ETag);
        return employee;
    }

    public async Task DeleteEmployeeAsync(string partitionKey, string rowKey)
    {
        await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
    }
}
