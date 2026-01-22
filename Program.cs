using IssueDrivenWorkshop.Components;
using IssueDrivenWorkshop.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register ExpenseRequestService with connection string from configuration
var connectionString = builder.Configuration.GetValue<string>("AzureTableStorage:ConnectionString") 
    ?? Environment.GetEnvironmentVariable("AZURE_TABLESTORAGE_CONNECTIONSTRING");
var tableName = builder.Configuration.GetValue<string>("AzureTableStorage:TableName") 
    ?? Environment.GetEnvironmentVariable("AZURE_TABLESTORAGE_TABLENAME") 
    ?? "Expenses";

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Azure Table Storage connection string is not configured. " +
        "Set 'AzureTableStorage:ConnectionString' in appsettings.json or " +
        "set 'AZURE_TABLESTORAGE_CONNECTIONSTRING' environment variable.");
}

builder.Services.AddSingleton(new ExpenseRequestService(connectionString, tableName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
