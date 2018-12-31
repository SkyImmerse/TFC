using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameClient.Network.Protocol;
using UnityEngine;

namespace GameClient.Network
{
    public enum NetSocketTypes { SOCKET_CONN = 0, SOCKT_CMD, SOCKT_ERR }
    public class SocketEventArgs : EventArgs
    {
        public int NetSocketType;
        public InputMessage InMessage;
        public string Error;
        public string ErrorStr;
        public string ErrorMsg;
    }

    public delegate void SocketEventHandler(object sender, SocketEventArgs e);

    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 65535;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        public List<byte> data = new List<byte>();
        internal MemoryStream stream;
        internal InputMessage InputMessage = new InputMessage();
    }


    public class TCPClient
    {

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);



        public event SocketEventHandler SocketEvent;
        public event Action OnConnect;

        public bool XteaEncryptionEnabled;
        public bool ChecksumEnabled;
        public UInt32[] XteaKey = new uint[4];

        public bool IsRun = false;


            private void ConnectCallback(IAsyncResult ar)
            {
                try
                {
                    // Retrieve the socket from the state object.
                    Socket client = (Socket)ar.AsyncState;

                    // Complete the connection.
                    client.EndConnect(ar);

                    OnConnect?.Invoke();

                Debug.Log(string.Format("Socket connected to {0}", client.RemoteEndPoint.ToString()));

                    // Signal that the connection has been made.
                    connectDone.Set();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }

            private void Receive()
            {
                try
                {
                    // Create the state object.
                    StateObject state = new StateObject();
                    state.workSocket = client;

                    // Begin receiving the data from the remote device.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                try
                {
                    // Retrieve the state object and the client socket 
                    // from the asynchronous state object.
                    StateObject state = (StateObject)ar.AsyncState;
                    Socket client = state.workSocket;

                    // Read data from the remote device.
                    int bytesRead = client.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                    // There might be more data, so store the data received so far.
                        state.data.AddRange(state.buffer.Take(bytesRead));
                        doProcessMsg(state);
                        Receive();
                    }
            }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }

            public void Send(byte[] byteData)
            {

                // Begin sending the data to the remote device.
                client.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), client);
            }

            private void SendCallback(IAsyncResult ar)
            {
                try
                {
                    // Retrieve the socket from the state object.
                    Socket client = (Socket)ar.AsyncState;

                    // Complete sending the data to the remote device.
                    int bytesSent = client.EndSend(ar);

                    // Signal that all bytes have been sent.
                    sendDone.Set();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }


            #region SocketListener
            public bool Connect(string host, int port)
        {
            try
            {
                XteaEncryptionEnabled = false;
                ChecksumEnabled = false;
                GenerateXteaKey();

                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(host), port);

                // Create a TCP/IP socket.
                client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                // Receive the response from the remote device.
                Receive();
            }
            catch { }

            return true;
        }

        public void Disconnect()
        {
            IsRun = false;
            if (client != null)
            {
                client.Close();
            }
        }

        public bool IsConnected => client!=null&& client.Connected;
        #endregion

        #region RecieveData

        private void doProcessMsg(StateObject state)
        {
            if (state.data.Count == 0) return;
            state.InputMessage.Reset();

            state.stream = new MemoryStream(state.data.ToArray());

            // first update message header size
            int headerSize = 2; // 2 bytes for message size
            if (ChecksumEnabled)
                headerSize += 4; // 4 bytes for checksum
            if (XteaEncryptionEnabled)
                headerSize += 2; // 2 bytes for XTEA encrypted message size
            state.InputMessage.SetHeaderSize((ushort)headerSize);

               // read the first 2 bytes which contain the message size
                var buffer = new byte[2];
                state.stream.Read(buffer, 0, 2);
                InternalRecvHeader(state, buffer, 2);
        }

        void InternalRecvHeader(StateObject state, byte[] buffer, UInt16 size)
        {
            // read message size
            state.InputMessage.FillBuffer(ref buffer, size);
            UInt16 remainingSize = state.InputMessage.ReadSize();

            // read remaining message data
            if (client != null)
            {
                var newBuffer = new byte[remainingSize];
                state.stream.Read(newBuffer, 0, remainingSize);

                InternalRecvData(state, newBuffer, remainingSize);
            }
        }

        public void InternalRecvData(StateObject state, byte[] buffer, UInt16 size)
        {
            //process data
            state.InputMessage.FillBuffer(ref buffer, size);

            if (ChecksumEnabled && !state.InputMessage.ReadChecksum())
            {
                Debug.LogError("Network message with invalid checksum");
                NotifyRecvData(new SocketEventArgs()
                {
                    Error = "Error",
                    ErrorMsg = "Network message with invalid checksum",
                    NetSocketType = (int)NetSocketTypes.SOCKT_ERR,
                });
                return;
            }

            if (XteaEncryptionEnabled)
            {
                if (!DecryptXtea(state.InputMessage, XteaKey))
                {
                    Debug.LogError("Failed to decrypt message");
                    NotifyRecvData(new SocketEventArgs()
                    {
                        Error = "Error",
                        ErrorMsg = "Failed to decrypt message",
                        NetSocketType = (int)NetSocketTypes.SOCKT_ERR,
                    });

                    return;
                }
            }

            NotifyRecvData(new SocketEventArgs()
            {
                Error = "Success",
                NetSocketType = (int)NetSocketTypes.SOCKT_CMD,
                InMessage = state.InputMessage
            });


        }

        public void NotifyRecvData(SocketEventArgs e)
        {
            this.SocketEvent?.Invoke(this, e);
        }
        #endregion

        #region Send Data
        public void Send(OutputMessage outputMessage)
        {
            // encrypt
            if (XteaEncryptionEnabled)
            {
                outputMessage.WriteMessageSize();
                EncryptXtea(outputMessage, XteaKey);
            }
            // write checksum
            if (ChecksumEnabled)
                outputMessage.WriteChecksum();

            // wirte message size
            outputMessage.WriteMessageSize();

            // send
            outputMessage.Flush(this);
            // reset message to allow reuse
            outputMessage.Reset();
        }
        #endregion

        #region Encryption
        System.Random _rand = new System.Random();
        private Socket client;

        public uint RandVal()
        {
            var buffer = new byte[sizeof(uint)];
            _rand.NextBytes(buffer);
            uint result = BitConverter.ToUInt32(buffer, 0);

            return (result % (0xFFFFFFFF - 0)) + 0;
        }

        public void GenerateXteaKey()
        {
            XteaKey[0] = (uint)RandVal();
            XteaKey[1] = (uint)RandVal();
            XteaKey[2] = (uint)RandVal();
            XteaKey[3] = (uint)RandVal();
        }
        public static unsafe bool DecryptXtea(InputMessage msg, uint[] key)
        {
            var encryptedSize = msg.GetUnreadSize();
            if ((encryptedSize) % 8 > 0 || key == null)
                return false;

            fixed (byte* bufferPtr = msg.GetReadBuffer())
            {
                uint* words = (uint*)(bufferPtr + msg.GetReadPos());
                int msgSize = msg.GetUnreadSize();

                for (int pos = 0; pos < msgSize / 4; pos += 2)
                {
                    uint xCount = 32, xSum = 0xC6EF3720, xDelta = 0x9E3779B9;

                    while (xCount-- > 0)
                    {
                        words[pos + 1] -= (words[pos] << 4 ^ words[pos] >> 5) + words[pos] ^ xSum
                                          + key[xSum >> 11 & 3];
                        xSum -= xDelta;
                        words[pos] -= (words[pos + 1] << 4 ^ words[pos + 1] >> 5) + words[pos + 1] ^ xSum
                                      + key[xSum & 3];
                    }
                }

                var decryptedSize = msg.GetU16() + 2;
                int sizeDelta = decryptedSize - encryptedSize;
                if (sizeDelta > 0 || -sizeDelta > encryptedSize)
                {
                    //Debug.LogError("invalid decrypted network message");
                    return false;
                }

                msg.SetMessageSize((ushort)(msg.GetMessageSize() + ((ushort)sizeDelta)));
            }

            return true;
        }

        public unsafe static bool EncryptXtea(OutputMessage msg, uint[] key)

        {

            if (key == null)

                return false;



            int pad = msg.GetMessageSize() % 8;

            if (pad > 0)

                msg.AddPaddingBytes(8 - pad);



            fixed (byte* bufferPtr = msg.GetDataBuffer())

            {

                uint* words = (uint*)(bufferPtr + msg.MHeaderPos);



                for (int pos = 0; pos < msg.GetMessageSize() / 4; pos += 2)

                {

                    uint xSum = 0, xDelta = 0x9e3779b9, xCount = 32;



                    while (xCount-- > 0)

                    {

                        words[pos] += (words[pos + 1] << 4 ^ words[pos + 1] >> 5) + words[pos + 1] ^ xSum

                                      + key[xSum & 3];

                        xSum += xDelta;

                        words[pos + 1] += (words[pos] << 4 ^ words[pos] >> 5) + words[pos] ^ xSum

                                          + key[xSum >> 11 & 3];

                    }

                }

            }



            return true;

        }
        #endregion

    }
}
