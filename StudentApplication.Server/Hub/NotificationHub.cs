using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StudentApplication.Server.Hub;

/// <summary>
///   Responsible for distributing CRUD operations to subscribed clients
/// </summary>
public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub
{
    private ILogger Logger { get; }
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        Logger = logger;
    }
    
    /// <summary>
    ///   Subscribes to a CRUD controller or to a specific item
    /// </summary>
    /// <example>Subscribe("courses")</example>
    /// <example>Subscribe("courses.12")</example>
    public async Task Subscribe(string group)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        Logger.LogInformation($"{Context.User?.Identity?.Name ?? "Anonymous User"} joined the {group} group");
    }

    /// <summary>
    ///   Unsubscribes from a CRUD controller.
    /// </summary>
    public async Task Unsubscribe(string group)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        Logger.LogInformation($"{Context.User?.Identity?.Name ?? "Anonymous User"} left the {group} group");
    }
}