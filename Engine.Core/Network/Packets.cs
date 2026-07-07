using System;
using Engine.Core.Entities;

namespace Engine.Core.Network
{
    // ═══════════════════════════════════════════════════════════════════════
    // Базовый пакет
    // ═══════════════════════════════════════════════════════════════════════

    public abstract class BasePacket
    {
        public abstract PacketType Type { get; }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Подключение / отключение
    // ═══════════════════════════════════════════════════════════════════════

    public class PacketPlayerJoin : BasePacket
    {
        public override PacketType Type => PacketType.PlayerJoin;
        public int   PlayerId   { get; set; }
        public string PlayerName { get; set; }
        public Vec3   SpawnPos   { get; set; }
    }

    public class PacketPlayerLeave : BasePacket
    {
        public override PacketType Type => PacketType.PlayerLeave;
        public int PlayerId { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Трансформация игрока (позиция + ротация)
    // ═══════════════════════════════════════════════════════════════════════

    public class PacketPlayerTransform : BasePacket
    {
        public override PacketType Type => PacketType.PlayerTransform;
        public int   PlayerId { get; set; }
        public Vec3  Position { get; set; }
        public float Yaw      { get; set; }   // Горизонтальный угол (градусы)
        public float Pitch    { get; set; }   // Вертикальный угол (градусы)
        public Vec3  Velocity { get; set; }
        public uint  Tick     { get; set; }   // Тик сервера
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Инпут клиента -> Сервер
    // ═══════════════════════════════════════════════════════════════════════

    public class PacketPlayerInput : BasePacket
    {
        public override PacketType Type => PacketType.PlayerInput;
        public int   PlayerId   { get; set; }
        public bool  MoveForward { get; set; }
        public bool  MoveBack    { get; set; }
        public bool  MoveLeft    { get; set; }
        public bool  MoveRight   { get; set; }
        public bool  Jump        { get; set; }
        public bool  Sprint      { get; set; }
        public float MouseYaw    { get; set; }
        public float MousePitch  { get; set; }
        public uint  Tick        { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Размещение / удаление блоков
    // ═══════════════════════════════════════════════════════════════════════

    public class PacketPlaceBlock : BasePacket
    {
        public override PacketType Type => PacketType.PlaceBlock;
        public int        PlayerId   { get; set; }
        public Vec3       Position   { get; set; }
        public EntityType BlockType  { get; set; }
        public ColorRGB   Color      { get; set; }
        public uint       Tick       { get; set; }
    }

    public class PacketRemoveBlock : BasePacket
    {
        public override PacketType Type => PacketType.RemoveBlock;
        public int  PlayerId { get; set; }
        public Guid EntityId { get; set; }
        public uint Tick     { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Снимок всей сцены при подключении
    // ═══════════════════════════════════════════════════════════════════════

    public class PacketSceneSnapshot : BasePacket
    {
        public override PacketType Type => PacketType.SceneSnapshot;
        public string SceneJson { get; set; }   // Полная сцена в JSON
        public uint   ServerTick { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Чат
    // ═══════════════════════════════════════════════════════════════════════

    public class PacketChatMessage : BasePacket
    {
        public override PacketType Type => PacketType.ChatMessage;
        public int    SenderId   { get; set; }
        public string SenderName { get; set; }
        public string Message    { get; set; }
    }
}
