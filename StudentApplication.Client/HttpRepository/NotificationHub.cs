using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace StudentApplication.Client.HttpRepository;

/// <summary>
///   Wrapper for a SignalR hub for auto-joining groups upon reconnecting
/// </summary>
public class NotificationHub
{
    private readonly HashSet<string> _joinedGroups = new();
    public HubConnection HubConnection { get; private set; } = null!;

    public async Task JoinGroupAsync(string group)
    {
        _joinedGroups.Add(group);
        await HubConnection.InvokeAsync("Subscribe", group);
    }

    public NotificationHub(Uri uri)
    {
        var builder = new HubConnectionBuilder();
        HubConnection = builder.WithUrl(uri).WithAutomaticReconnect().Build();
        HubConnection.Reconnected += async _ => await RejoinGroupsAsync();
    }

    public async Task LeaveGroupAsync(string group)
    {
        if (_joinedGroups.Remove(group))
            await HubConnection.InvokeAsync("Unsubscribe", group);
    }

    public async Task ConnectAsync()
    {
        if (HubConnection.State != HubConnectionState.Disconnected)
            return;
        
        await HubConnection.StartAsync();
    }

    private async Task RejoinGroupsAsync()
    {
        foreach (var group in _joinedGroups)
            await JoinGroupAsync(group);
    }
}