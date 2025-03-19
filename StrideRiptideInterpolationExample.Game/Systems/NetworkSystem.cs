using Stride.Core;
using Stride.Games;
using Riptide;
using Stride.Core.Diagnostics;
using Riptide.Utils;

namespace StrideRiptideInterpolationExample.Systems
{
    public class NetworkSystem(IServiceRegistry registry) : GameSystemBase(registry)
    {
        public bool IsHost { get; set; }
        public string IpAddress { get; set; }
        public ushort PlayerClientId => Client.Id;
        public Server Server { get; } = new Server();
        public Client Client { get; } = new Client();

        public const string Localhost = "127.0.0.1";
        public const ushort Port = 7777;

        private static readonly Logger Log = GlobalLogger.GetLogger(nameof(NetworkSystem));

        public override void Initialize() =>
            RiptideLogger.Initialize(Debug, Info, Warning, Error, includeTimestamps: true);

        protected override void Destroy()
        {
            Server.Stop();
            Client.Disconnect();

            base.Destroy();
        }

        public override void Update(GameTime gameTime)
        {
            if (Server.IsRunning)
                Server.Update();

            Client.Update();

            base.Update(gameTime);
        }

        public void StartConnect()
        {
            if (IsHost)
                Server.Start(Port, maxClientCount: 16);

            Client.Connect($"{IpAddress}:{Port}", useMessageHandlers: false);
            Enabled = true;
        }

        public void SendMessageToServer(Message message) =>
            Client.Send(message);

        public void SendMessageToAllClients(Message message) =>
            Server.SendToAll(message);

        private void Debug(string msg) =>
            Log.Debug(msg);

        private void Info(string msg) =>
            Log.Info(msg);

        private void Warning(string msg) =>
            Log.Warning(msg);

        private void Error(string msg) =>
            Log.Error(msg);
    }
}
