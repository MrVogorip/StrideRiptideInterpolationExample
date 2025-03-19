using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Events;
using StrideRiptideInterpolationExample.Systems;

namespace StrideRiptideInterpolationExample.UI
{
    public class LobbyPageScript : StartupScript
    {
        public CameraComponent UiCamera { get; set; }
        public UIComponent UIComponent { get; set; }

        private NetworkSystem _networkSystem;
        private EditText _ipAddress;
        private Button _create;
        private Button _join;

        public override void Start()
        {
            _networkSystem = Services.GetService<NetworkSystem>();

            _ipAddress = UIComponent.Page.RootElement.FindVisualChildOfType<EditText>("IpAddress");
            _create = UIComponent.Page.RootElement.FindVisualChildOfType<Button>("Create");
            _join = UIComponent.Page.RootElement.FindVisualChildOfType<Button>("Join");

            _ipAddress.Text = NetworkSystem.Localhost;
            _create.Click += CreateClicked;
            _join.Click += JoinClicked;

            base.Start();
        }

        public override void Cancel()
        {
            _join.Click -= JoinClicked;
            _create.Click -= CreateClicked;

            base.Cancel();
        }

        private void JoinClicked(object sender, RoutedEventArgs e) =>
            StartConnect();

        private void CreateClicked(object sender, RoutedEventArgs e)
        {
            _networkSystem.IsHost = true;
            StartConnect();
        }

        public void StartConnect()
        {
            _networkSystem.IpAddress = _ipAddress.Text;
            _networkSystem.StartConnect();
            UiCamera.Enabled = false;
            Entity.Scene.Entities.Remove(Entity);
        }
    }
}
