using Stride.Engine;
using Stride.Graphics;
using StrideRiptideInterpolationExample.Systems;
using System;

namespace StrideRiptideInterpolationExample
{
    public class ExampleGame : Game
    {
        private readonly NetworkSystem _networkSystem;
        private readonly CharactersSystem _charactersSystem;

        public ExampleGame() : base()
        {
            _networkSystem = new NetworkSystem(Services);
            Services.AddService(_networkSystem);

            _charactersSystem = new CharactersSystem(Services);
            Services.AddService(_charactersSystem);
        }

        protected override void BeginRun()
        {
            MinimizedMinimumUpdateRate.MinimumElapsedTime = new TimeSpan(0);
            WindowMinimumUpdateRate.MinimumElapsedTime = new TimeSpan(0);
            GraphicsDevice.Presenter.PresentInterval = PresentInterval.Immediate;

            base.BeginRun();
        }

        protected override void Initialize()
        {
            GameSystems.Add(_networkSystem);
            GameSystems.Add(_charactersSystem);

            base.Initialize();
        }
    }
}
