using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TrClient
{
    public class AutoClient : TrClient
    {
        public AutoClient(bool debug, string uuid = "", string serverPassword = "") : base(debug, uuid, serverPassword) {
        }

        public void GameLoop(string host, int port) {
            Connect(host, port);
            GameLoopInternal();
        }

        private void GameLoopInternal() {
            Task.Run(() => {

                while (true) {
                    ProcessPackets();

                    Thread.Sleep(8);
                }
            });
        }
    }
}
