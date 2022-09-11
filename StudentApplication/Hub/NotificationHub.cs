namespace StudentApplication.Hub;

public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub
{
    private ILogger Logger { get; }
    public NotificationHub(ILogger logger)
    {
        Logger = logger;
    }
    
    public async Task Subscribe(string group)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        Logger.LogInformation($"{Context.User?.Identity?.Name ?? "Anonymous User"} joined the {group} group");
    }

    public async Task Unsubscribe(string group)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        Logger.LogInformation($"{Context.User?.Identity?.Name ?? "Anonymous User"} left the {group} group");
    }
}