using System;
using Net.Message;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace Net
{
    public class Server : MonoBehaviour
    {
        #region Singleton implementation
        public static Server Instance { private set; get; }
        private void Awake() {
            Instance = this;
        }
        #endregion

        private NetworkDriver driver;
        private NativeList<NetworkConnection> connections;
        private bool isActive;
        private const float KeepAliveTickRate = 20.0f;
        private float lastKeepAlive;
        private Action connectionDropped;

        public Server(Action connectionDropped)
        {
            this.connectionDropped = connectionDropped;
        }

        public void Init(ushort port) {
            driver = NetworkDriver.Create();
            NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.Port = port;
            if (driver.Bind(endpoint) != 0) {
                Debug.Log("Unable to bind on port " + endpoint.Port);
            } else {
                driver.Listen();
                Debug.Log("Currently listening on port " + endpoint.Port);
            }

            connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
            isActive = true;
        }

        public void Shutdown() {
            if (isActive) {
                driver.Dispose();
                connections.Dispose();
                isActive = false;
            }
        }

        public void OnDestroy() {
            Shutdown();
        }

        public void Update() {
            if (!isActive) {
                return;
            }
            KeepAlive();

            driver.ScheduleUpdate().Complete();
            CleanupConnections();
            AcceptNewConnections();
            UpdateMessagePump();
        }

        private void KeepAlive() {
            if (Time.time - lastKeepAlive > KeepAliveTickRate) {
                lastKeepAlive = Time.time;
                Broadcast(new NetKeepAlive());
            }
        }

        private void CleanupConnections() {
            for (int i = 0; i < connections.Length; i++) {
                if (!connections[i].IsCreated) {
                    connections.RemoveAtSwapBack(i);
                    --i;
                }
            }
        }

        private void AcceptNewConnections() {
            NetworkConnection c;
            while((c = driver.Accept()) != default(NetworkConnection)) {
                connections.Add(c);
            }
        }

        private void UpdateMessagePump() {
            for (int i = 0; i < connections.Length; i++) {
                NetworkEvent.Type cmd;
                while ((cmd = driver.PopEventForConnection(connections[i], out var stream)) != NetworkEvent.Type.Empty) {
                    if (cmd == NetworkEvent.Type.Data) {
                        NetUtility.OnData(stream, connections[i], this);
                    } else if (cmd == NetworkEvent.Type.Disconnect) {
                        Debug.Log("Client disconnected from server");
                        connections[i] = default(NetworkConnection);
                        connectionDropped?.Invoke();
                        Shutdown();
                    }
                }
            }
        }

        // Server specific
        public void SendToClient(NetworkConnection connection, NetMessage msg) {
            driver.BeginSend(connection, out var writer);
            msg.Serialize(ref writer);
            driver.EndSend(writer);
        }

        public void Broadcast(NetMessage msg)
        {
            foreach (var connection in connections)
            {
                if (connection.IsCreated) {
                    SendToClient(connection, msg);
                }
            }
        }
    }
}
