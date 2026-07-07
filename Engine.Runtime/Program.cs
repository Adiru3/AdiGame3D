using System;
using Engine.Runtime.GameLoop;

namespace Engine.Runtime
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Аргументы:
            //   args[0] = путь к level.json (обязательно)
            //   args[1] = "host"   — запустить как сервер
            //   args[1] = "join"   — подключиться к серверу
            //   args[2] = IP (если join)
            //   args[3] = порт (опционально, по умолчанию 7777)

            string levelPath = null;
            bool   isHost    = false;
            string joinIp    = null;
            int    port      = 7777;

            // Ищем level.json в аргументах или рядом с exe
            if (args.Length > 0 && System.IO.File.Exists(args[0]))
            {
                levelPath = args[0];
            }
            else
            {
                // Ищем level.json рядом с exe
                string local = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "level.json");
                if (System.IO.File.Exists(local))
                    levelPath = local;
                else
                    levelPath = local; // Игра всё равно запустится, покажет ошибку
            }

            if (args.Length > 1)
            {
                if (args[1].ToLower() == "host")
                    isHost = true;
                else if (args[1].ToLower() == "join" && args.Length > 2)
                    joinIp = args[2];
            }

            if (args.Length > 3 && int.TryParse(args[3], out int p))
                port = p;

            Console.WriteLine($"=== Adigame3D Runtime ===");
            Console.WriteLine($"Level: {levelPath}");
            Console.WriteLine($"Mode:  {(isHost ? "HOST" : joinIp != null ? $"JOIN {joinIp}" : "SOLO")}");

            try
            {
                using (var window = new RuntimeWindow(levelPath, isHost, joinIp, port))
                {
                    // 60 FPS игра, 60 FPS рендер
                    window.Run(60.0, 60.0);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fatal error: {ex}");
                System.Windows.Forms.MessageBox.Show(
                    $"Fatal Error:\n{ex.Message}\n\n{ex.StackTrace}",
                    "Adigame3D Runtime Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
