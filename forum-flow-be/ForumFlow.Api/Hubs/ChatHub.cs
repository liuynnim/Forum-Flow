using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ForumFlow.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.FindFirst("username")?.Value ?? "Anonymous";
        Console.WriteLine($"🟢 {username} connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User?.FindFirst("username")?.Value ?? "Anonymous";
        Console.WriteLine($"🔴 {username} disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Tham gia một phòng chat</summary>
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserJoined", new
        {
            Username = Context.User?.FindFirst("username")?.Value,
            RoomId   = roomId,
            At       = DateTime.UtcNow
        });
    }

    /// <summary>Rời phòng chat</summary>
    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserLeft", new
        {
            Username = Context.User?.FindFirst("username")?.Value,
            RoomId   = roomId,
            At       = DateTime.UtcNow
        });
    }

    /// <summary>Gửi tin nhắn tới một phòng chat</summary>
    public async Task SendMessageToRoom(string roomId, string content)
    {
        var username = Context.User?.FindFirst("username")?.Value ?? "Anonymous";
        await Clients.Group(roomId).SendAsync("ReceiveMessage", new
        {
            Sender    = username,
            Content   = content,
            RoomId    = roomId,
            Timestamp = DateTime.UtcNow
        });
    }
}

