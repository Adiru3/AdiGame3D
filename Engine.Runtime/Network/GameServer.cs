using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using Engine.Core.Entities;
using Engine.Core.Network;
using Engine.Core.Scene;
using Newtonsoft.Json;

namespace Engine.Runtime.Network
{
    /// <summary>
    /// Авторитарный игровой сервер на LiteNetLib (UDP).
    /// Хранит физику всех игроков, принимает инпуты и рассылает трансформы.
    /// </summary>
    public class GameServer : INetEventListener, IDisposable
    {
        private NetManager _server;
        private SceneManager _scene;
        private uint _tick = 0;
        private bool _disposed;

        // Подключённые игроки: peerId -> данные
        private Dictionary<int, ServerPlayer> _players = new Dictionary<int, ServerPlayer>();

        public int Port       { get; }
        public bool IsRunning { get; private set; }

        public event Action<string> OnLog;

        public GameServer(SceneManager scene, int port = 7777)
        {
            Port   = port;
            _scene = scene;
            _server = new NetManager(this)
            {
                AutoRecycle             = true,
                UpdateTime              = 15,   // ~66 тиков/сек
                UnconnectedMessagesEnabled = false
            };
        }

        public void Start()
        {
            if (_server.Start(Port))
            {
                IsRunning = true;
                Log($"Server started on port {Port}");
            }
            else
            {
                Log($"Failed to start server on port {Port}");
            }
        }

        public void Update()
        {
            if (!IsRunning) return;
            _server.PollEvents();
            _tick++;
        }

        // ─── INetEventListener ────────────────────────────────────────────

        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (_server.ConnectedPeersCount < 16)
                request.AcceptIfKey("Adigame3d");
            else
                request.Reject();
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Log($"Player connected: {peer.Id} from {peer.EndPoint}");

            var player = new ServerPlayer
            {
                Id       = peer.Id,
                Name     = $"Player_{peer.Id}",
                Position = new Vec3(0, 3, 0),
                Peer     = peer
            };
            _players[peer.Id] = player;

            // Отправляем снимок сцены
            SendSceneSnapshot(peer);
            // Отправляем join всем
            BroadcastPlayerJoin(player);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log($"Player disconnected: {peer.Id}");
            _players.Remove(peer.Id);
            BroadcastPlayerLeave(peer.Id);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (reader.AvailableBytes == 0) return;
            var type = (PacketType)reader.GetByte();

            switch (type)
            {
                case PacketType.PlayerInput:
                    HandlePlayerInput(peer.Id, reader);
                    break;
                case PacketType.PlaceBlock:
                    HandlePlaceBlock(peer.Id, reader);
                    break;
                case PacketType.RemoveBlock:
                    HandleRemoveBlock(peer.Id, reader);
                    break;
                case PacketType.ChatMessage:
                    HandleChat(peer.Id, reader);
                    break;
            }
        }

        // ─── Обработка инпута ────────────────────────────────────────────

        private void HandlePlayerInput(int playerId, NetPacketReader reader)
        {
            if (!_players.TryGetValue(playerId, out var player)) return;
            player.InputForward = reader.GetBool();
            player.InputBack    = reader.GetBool();
            player.InputLeft    = reader.GetBool();
            player.InputRight   = reader.GetBool();
            player.InputJump    = reader.GetBool();
            player.InputSprint  = reader.GetBool();
            player.Yaw          = reader.GetFloat();
            player.Pitch        = reader.GetFloat();
        }

        private void HandlePlaceBlock(int playerId, NetPacketReader reader)
        {
            var pos   = new Vec3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            var btype = (EntityType)reader.GetInt();

            var e = _scene.AddEntity(btype, pos);

            // Рассылаем всем
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.PlaceBlock);
            writer.Put(playerId);
            writer.Put(pos.X); writer.Put(pos.Y); writer.Put(pos.Z);
            writer.Put((int)btype);
            writer.Put(e.Id.ToString());
            _server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }

        private void HandleRemoveBlock(int playerId, NetPacketReader reader)
        {
            string idStr = reader.GetString();
            if (!Guid.TryParse(idStr, out var guid)) return;
            _scene.RemoveEntity(guid);

            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.RemoveBlock);
            writer.Put(idStr);
            _server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }

        private void HandleChat(int playerId, NetPacketReader reader)
        {
            string msg = reader.GetString(256);
            string name = _players.TryGetValue(playerId, out var p) ? p.Name : "?";

            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.ChatMessage);
            writer.Put(playerId);
            writer.Put(name);
            writer.Put(msg);
            _server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }

        // ─── Отправка трансформов (вызывается каждый тик) ────────────────

        public void BroadcastTransforms()
        {
            if (!IsRunning || _players.Count == 0) return;

            foreach (var kv in _players)
            {
                var writer = new NetDataWriter();
                writer.Put((byte)PacketType.PlayerTransform);
                writer.Put(kv.Key);
                writer.Put(kv.Value.Position.X);
                writer.Put(kv.Value.Position.Y);
                writer.Put(kv.Value.Position.Z);
                writer.Put(kv.Value.Yaw);
                writer.Put(kv.Value.Pitch);
                writer.Put(_tick);
                _server.SendToAll(writer, DeliveryMethod.Sequenced);
            }
        }

        // ─── Сцена ───────────────────────────────────────────────────────

        private void SendSceneSnapshot(NetPeer peer)
        {
            string json = _scene.SerializeToJson();
            var writer  = new NetDataWriter();
            writer.Put((byte)PacketType.SceneSnapshot);
            writer.Put(json);
            writer.Put(_tick);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        // ─── Broadcast helpers ───────────────────────────────────────────

        private void BroadcastPlayerJoin(ServerPlayer player)
        {
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.PlayerJoin);
            writer.Put(player.Id);
            writer.Put(player.Name);
            writer.Put(player.Position.X);
            writer.Put(player.Position.Y);
            writer.Put(player.Position.Z);
            _server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }

        private void BroadcastPlayerLeave(int id)
        {
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.PlayerLeave);
            writer.Put(id);
            _server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }

        // ─── Неиспользуемые интерфейсные методы ──────────────────────────

        public void OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError) { }
        public void OnNetworkReceiveUnconnected(System.Net.IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
        public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

        private void Log(string msg) => OnLog?.Invoke($"[Server] {msg}");

        public void Dispose()
        {
            if (!_disposed)
            {
                _server?.Stop();
                _disposed = true;
            }
        }
    }

    public class ServerPlayer
    {
        public int     Id;
        public string  Name;
        public Vec3    Position = new Vec3(0, 2, 0);
        public float   Yaw, Pitch;
        public bool    InputForward, InputBack, InputLeft, InputRight;
        public bool    InputJump, InputSprint;
        public NetPeer Peer;
    }
}
