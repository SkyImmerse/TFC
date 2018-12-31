using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets;
using Game.DAO;
using GameClient.Network.Protocol;
using UnityEngine;

namespace GameClient.Network
{
    public delegate void CmdWorker(TCPClient client, InputMessage msg);
    public class TCPCmdHandler
    {
        private Dictionary<int, CmdWorker> mWorkers = new Dictionary<int, CmdWorker>();

        public event Func<int, InputMessage, bool> IsSkipWorker;

        public void RegisterCmd(int PacketCmdID, CmdWorker Callback)
        {
            mWorkers.Add(PacketCmdID, Callback);
        }
        public void ProcessCmd(object sender, SocketEventArgs e)
        {
            InputMessage msg = e.InMessage;
            MicroTasks.Dispatch(() =>
            {
                while (msg != null && !msg.Eof())
                {
                    try
                    {
                        int opcode = msg.GetU8();

                        if (IsSkipWorker != null && IsSkipWorker.Invoke(opcode, msg))
                        {
                            continue;
                        }

                        if (!mWorkers.ContainsKey(opcode))
                        {
                            Debug.Log(string.Format("UNHANDLED PACKET! Opcode: {0}", opcode));
                            continue;
                        }
                        else
                        {
                            mWorkers[opcode].Invoke(sender as TCPClient, msg);
                        }
                    }
                    catch(Exception ex)
                    {
                        // log Exception
                        Debug.LogError(ex);
                        // COMPLETLY DROP MESSAGE
                        break;
                    }
                }
            });
        }

    }
}
