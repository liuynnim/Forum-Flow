export default function ChatPage() {
  return (
    <div className="flex h-screen bg-gray-100 dark:bg-zinc-900">
      <div className="w-1/4 border-r bg-white dark:bg-zinc-800 p-4">
        <h2 className="font-bold text-lg mb-4">Danh sách Phòng Chat</h2>
      </div>
      <div className="flex-1 p-6 flex items-center justify-center text-gray-400">
        Chọn một phòng chat để bắt đầu nhắn tin thời gian thực (SignalR)
      </div>
    </div>
  );
}
