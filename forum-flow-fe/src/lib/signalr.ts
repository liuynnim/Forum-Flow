import * as signalR from "@microsoft/signalr";

class SignalRService {
  private chatConnection: signalR.HubConnection | null = null;

  /**
   * Khởi tạo kết nối tới ChatHub
   */
  public getChatConnection(): signalR.HubConnection {
    if (!this.chatConnection) {
      const hubUrl = `${process.env.NEXT_PUBLIC_SIGNALR_URL}/chat`;

      this.chatConnection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          // Gửi token xác thực qua WebSocket Query hoặc Header
          accessTokenFactory: () => localStorage.getItem("accessToken") || "",
        })
        .withAutomaticReconnect() // Tự động kết nối lại nếu mất mạng
        .configureLogging(signalR.LogLevel.Information)
        .build();
    }
    return this.chatConnection;
  }

  /**
   * Bắt đầu kết nối WebSocket
   */
  public async startChatConnection(): Promise<void> {
    const connection = this.getChatConnection();
    if (connection.state === signalR.HubConnectionState.Disconnected) {
      try {
        await connection.start();
        console.log("✅ SignalR Chat Hub Connected successfully!");
      } catch (err) {
        console.error("❌ SignalR Connection Error: ", err);
      }
    }
  }

  /**
   * Ngắt kết nối khi rời trang
   */
  public async stopChatConnection(): Promise<void> {
    if (
      this.chatConnection &&
      this.chatConnection.state !== signalR.HubConnectionState.Disconnected
    ) {
      await this.chatConnection.stop();
      console.log("🛑 SignalR Disconnected.");
    }
  }
}

export const signalRService = new SignalRService();
