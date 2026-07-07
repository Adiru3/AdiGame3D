namespace Engine.Core.Network
{
    /// <summary>
    /// Типы сетевых пакетов движка.
    /// </summary>
    public enum PacketType : byte
    {
        // Подключение
        PlayerJoin          = 1,
        PlayerLeave         = 2,
        SceneSnapshot       = 3,   // Полный снимок сцены при подключении

        // Игрок
        PlayerTransform     = 10,  // Позиция + поворот игрока
        PlayerInput         = 11,  // Нажатые клавиши

        // Редактор (синхронизация)
        PlaceBlock          = 20,
        RemoveBlock         = 21,
        UpdateBlock         = 22,

        // Сервер -> Клиенты
        ServerTick          = 30,  // Тик физики сервера
        ChatMessage         = 40,
        Ping                = 99,
        Pong                = 100,
    }
}
