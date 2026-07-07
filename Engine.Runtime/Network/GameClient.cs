using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using Engine.Core.Entities;
using Engine.Core.Network;
using Engine.Core.Scene;

namespace Engine.Runtime.Network
{
    /// <summary>
    /// Игровой клиент: подключается к серверу, отправляет инпуты,
    /// получает трансформы других игроков и интерполирует их.
    /// </summary>
    public class GameClient : INetEventListener, IDisposable
    {
        private NetManager _client;
        private NetPeer    _server;
        private bool       _disposed;

        public bool IsConnected => _server?.ConnectionState == ConnectionState.Connected;
        public int  LocalPlayerId { get; private set; } = -1;

        // Состояния других игроков для интерполяции
        private Dictionary<int, RemotePlayerState> _remoteStates
            = new Dictionary<int, RemotePlayerState>();

        public IEnumerable<RemotePlayerState> RemoteStates => _remoteStates.Values;

        // ─── События ──────────────────────────────────────────────────────
        public event Action<string> OnLog;
        public event Action<Engine.Core.Scene.Scene> OnSceneReceived;
        public event Action<string> OnChatMessage;
        public event Action<int>    OnPlayerLeft;
        public event Action<int, string, Vec3> OnPlayerJoined;

        public GameClient()
        {
            _client = new NetManager(this)
            {
                AutoRecycle = true,
                UpdateTime  = 15
            };
            _client.Start();
        }

        public void Connect(string ip, int port = 7777)
        {
            _client.Connect(ip, port, "Adigame3d");
            Log($"Connecting to {ip}:{port}...");
        }

        public void Update()
        {
            _client.PollEvents();
            UpdateInterpolation();
        }

        // ─── Отправка инпута на сервер ────────────────────────────────────

        public void SendInput(
            bool fwd, bool back, bool left, bool right,
            bool jump, bool sprint,
            float yaw, float pitch, uint tick)
        {
            if (!IsConnected) return;
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.PlayerInput);
            writer.Put(fwd);   writer.Put(back);
            writer.Put(left);  writer.Put(right);
            writer.Put(jump);  writer.Put(sprint);
            writer.Put(yaw);   writer.Put(pitch);
            writer.Put(tick);
            _server.Send(writer, DeliveryMethod.Sequenced);
        }

        public void SendPlaceBlock(Vec3 pos, EntityType type)
        {
            if (!IsConnected) return;
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.PlaceBlock);
            writer.Put(pos.X); writer.Put(pos.Y); writer.Put(pos.Z);
            writer.Put((int)type);
            _server.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public void SendRemoveBlock(Guid entityId)
        {
            if (!IsConnected) return;
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.RemoveBlock);
            writer.Put(entityId.ToString());
            _server.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public void SendChat(string message)
        {
            if (!IsConnected) return;
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.ChatMessage);
            writer.Put(message);
            _server.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        // ─── INetEventListener ────────────────────────────────────────────

        public void OnPeerConnected(NetPeer peer)
        {
            _server = peer;
            LocalPlayerId = peer.Id;
            Log($"Connected to server. Local ID: {LocalPlayerId}");
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
        {
            _server = null;
            Log($"Disconnected: {info.Reason}");
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (reader.AvailableBytes == 0) return;
            var type = (PacketType)reader.GetByte();

            switch (type)
            {
                case PacketType.PlayerTransform:
                    HandleTransform(reader);
                    break;
                case PacketType.SceneSnapshot:
                    HandleSceneSnapshot(reader);
                    break;
                case PacketType.PlayerJoin:
                    HandlePlayerJoin(reader);
                    break;
                case PacketType.PlayerLeave:
                    HandlePlayerLeave(reader);
                    break;
                case PacketType.ChatMessage:
                    HandleChat(reader);
                    break;
            }
        }

        // ─── Обработчики пакетов ──────────────────────────────────────────

        private void HandleTransform(NetPacketReader r)
        {
            int id     = r.GetInt();
            float x    = r.GetFloat(), y = r.GetFloat(), z = r.GetFloat();
            float yaw  = r.GetFloat(), pitch = r.GetFloat();
            uint  tick = r.GetUInt();

            if (id == LocalPlayerId) return; // Свои данные игнорируем

            if (!_remoteStates.TryGetValue(id, out var state))
            {
                state = new RemotePlayerState { Id = id };
                _remoteStates[id] = state;
            }

            state.TargetPos   = new Vec3(x, y, z);
            state.TargetYaw   = yaw;
            state.TargetPitch = pitch;
            state.LastTick    = tick;
        }

        private void HandleSceneSnapshot(NetPacketReader r)
        {
            string json = r.GetString();
            try
            {
                var scene = Newtonsoft.Json.JsonConvert.DeserializeObject<Engine.Core.Scene.Scene>(json);
                OnSceneReceived?.Invoke(scene);
                Log("Scene snapshot received.");
            }
            catch (Exception ex)
            {
                Log($"Scene parse error: {ex.Message}");
            }
        }

        private void HandlePlayerJoin(NetPacketReader r)
        {
            int id     = r.GetInt();
            string name = r.GetString();
            float x    = r.GetFloat(), y = r.GetFloat(), z = r.GetFloat();
            OnPlayerJoined?.Invoke(id, name, new Vec3(x, y, z));
            Log($"Player joined: {name} ({id})");
        }

        private void HandlePlayerLeave(NetPacketReader r)
        {
            int id = r.GetInt();
            _remoteStates.Remove(id);
            OnPlayerLeft?.Invoke(id);
            Log($"Player left: {id}");
        }

        private void HandleChat(NetPacketReader r)
        {
            int    id   = r.GetInt();
            string name = r.GetString();
            string msg  = r.GetString(256);
            OnChatMessage?.Invoke($"[{name}]: {msg}");
        }

        // ─── Интерполяция позиций других игроков ──────────────────────────

        private void UpdateInterpolation()
        {
            float lerpSpeed = 15f;
            float dt = 0.016f; // примерно 60 fps

            foreach (var s in _remoteStates.Values)
            {
                if (s.CurrentPos == null)
                    s.CurrentPos = s.TargetPos != null
                        ? new Vec3(s.TargetPos.X, s.TargetPos.Y, s.TargetPos.Z)
                        : new Vec3(0, 0, 0);

                if (s.TargetPos == null) continue;

                float t = Math.Min(1f, lerpSpeed * dt);
                s.CurrentPos.X = Lerp(s.CurrentPos.X, s.TargetPos.X, t);
                s.CurrentPos.Y = Lerp(s.CurrentPos.Y, s.TargetPos.Y, t);
                s.CurrentPos.Z = Lerp(s.CurrentPos.Z, s.TargetPos.Z, t);
                s.CurrentYaw   = LerpAngle(s.CurrentYaw, s.TargetYaw, t);
                s.CurrentPitch = Lerp(s.CurrentPitch, s.TargetPitch, t);
            }
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        private static float LerpAngle(float a, float b, float t)
        {
            float diff = b - a;
            while (diff > 180f)  diff -= 360f;
            while (diff < -180f) diff += 360f;
            return a + diff * t;
        }

        // ─── Заглушки интерфейса ──────────────────────────────────────────

        public void OnConnectionRequest(ConnectionRequest request) => request.Reject();
        public void OnNetworkError(System.Net.IPEndPoint ep, System.Net.Sockets.SocketError err) { }
        public void OnNetworkReceiveUnconnected(System.Net.IPEndPoint ep, NetPacketReader r, UnconnectedMessageType mt) { }
        public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

        private void Log(string msg) => OnLog?.Invoke($"[Client] {msg}");

        public void Dispose()
        {
            if (!_disposed)
            {
                _client?.Stop();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Интерполируемое состояние удалённого игрока.
    /// </summary>
    public class RemotePlayerState
    {
        public int   Id;
        public string Name = "Player";

        // Последнее принятое с сервера
        public Vec3  TargetPos;
        public float TargetYaw, TargetPitch;
        public uint  LastTick;

        // Текущие интерполированные
        public Vec3  CurrentPos;
        public float CurrentYaw, CurrentPitch;
    }
}
