using System;
using Net.Message;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace Net
{
    public class Client : MonoBehaviour
    {
        #region Singleton implementation
        public static Client Instance { private set; get; }
        private void Awake() {
            Instance = this;
        }
        #endregion

        private NetworkDriver driver;
        private NetworkConnection connection;
        private bool isActive;
        private Action connectionDropped;

        public Client(Action connectionDropped)
        {
            this.connectionDropped = connectionDropped;
        }


        public void Init(string ip, ushort port) {
            driver = NetworkDriver.Create();
            NetworkEndpoint endpoint = NetworkEndpoint.Parse(ip, port);
        
            connection = driver.Connect(endpoint); // LOCAL HOST

            Debug.Log("Attempting to connect to Server on " + endpoint.Address);
            isActive = true;
            RegisterToEvent();
        }

        public void Shutdown() {
            if (isActive) {
                UnregisterToEvent();

                driver.Dispose();
                isActive = false;
                connection = default(NetworkConnection);
            }
        }

        public void OnDestroy() {
            Shutdown();
        }

        public void Update() {
            if (!isActive) {
                return;
            }

            driver.ScheduleUpdate().Complete();
            CheckAlive();
            UpdateMessagePump();
        }

        private void CheckAlive() {
            if (!connection.IsCreated && isActive) {
                Debug.Log("Something went wrong, lost connection to server");
                connectionDropped?.Invoke();
                Shutdown();
            }
        }

        private void UpdateMessagePump() {
            NetworkEvent.Type cmd;
            while ((cmd = connection.PopEvent(driver, out var stream)) != NetworkEvent.Type.Empty) {
                if (cmd == NetworkEvent.Type.Connect) {
                    SendToServer(new NetWelcome());
                    Debug.Log("Connected");
                } else if (cmd == NetworkEvent.Type.Data) {
                    NetUtility.OnData(stream, default(NetworkConnection));
                } else if (cmd == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Client got disconnected from server");
                    connection = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    Shutdown();
                }
            }
        }

        public void SendToServer(NetMessage msg) {
            driver.BeginSend(connection, out var writer);
            msg.Serialize(ref writer);
            driver.EndSend(writer);
        }

        // Event parsing
        private void RegisterToEvent() {
            NetUtility.CKeepAlive += OnKeepAlive;
        }

        private void UnregisterToEvent() {
            NetUtility.CKeepAlive -= OnKeepAlive;
        }

        private void OnKeepAlive(NetMessage msg) {
            // Send it back, to keep both side alive
            SendToServer(msg);
        }
    }
}
