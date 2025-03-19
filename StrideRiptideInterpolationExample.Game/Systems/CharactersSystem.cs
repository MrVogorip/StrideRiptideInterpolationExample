using Riptide;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Stride.Games;
using StrideRiptideInterpolationExample.Character;
using StrideRiptideInterpolationExample.Messages;
using System;
using System.Collections.Generic;

namespace StrideRiptideInterpolationExample.Systems
{
    public class CharactersSystem(IServiceRegistry registry) : GameSystemBase(registry)
    {
        private const string PathPrefab = "Prefabs/Character";

        private readonly Dictionary<ushort, CharacterController> _characters = [];
        private CharacterController _localCharacter;

        private SceneSystem _sceneSystem;
        private NetworkSystem _networkSystem;
        private IContentManager _contentManager;

        public override void Initialize()
        {
            _sceneSystem = Services.GetService<SceneSystem>();
            _networkSystem = Services.GetService<NetworkSystem>();
            _contentManager = Services.GetService<IContentManager>();

            _networkSystem.Server.RelayFilter = new MessageRelayFilter(typeof(GameMessageId), GameMessageId.CharacterAction);
            _networkSystem.Server.MessageReceived += ReceivedMessageFromClient;
            _networkSystem.Server.ClientConnected += SpawnCharacterToAllClients;

            _networkSystem.Client.MessageReceived += ReceivedMessageFromServer;
            _networkSystem.Client.ClientDisconnected += RemoveClientCharacter;
            _networkSystem.Client.Disconnected += RemoveAllCharacters;

            Enabled = true;

            base.Initialize();
        }

        protected override void Destroy()
        {
            _networkSystem.Server.MessageReceived -= ReceivedMessageFromClient;
            _networkSystem.Server.ClientConnected -= SpawnCharacterToAllClients;

            _networkSystem.Client.MessageReceived -= ReceivedMessageFromServer;
            _networkSystem.Client.ClientDisconnected -= RemoveClientCharacter;
            _networkSystem.Client.Disconnected -= RemoveAllCharacters;

            base.Destroy();
        }

        public override void Update(GameTime gameTime)
        {
            if (_localCharacter == null)
                return;

            _networkSystem.SendMessageToServer(new CharacterActionMessage().Create(new CharacterActionData
            {
                RemoteTime = DateTime.UtcNow.ToOADate(),
                PlayerId = _localCharacter.PlayerId,
                RunSpeed = _localCharacter.RunSpeed,
                IsJumped = _localCharacter.IsJumped,
                IsGrounded = _localCharacter.IsGrounded,
                Position = _localCharacter.CurrentPosition,
                Rotation = _localCharacter.CurrentRotation,
            }));

            base.Update(gameTime);
        }

        private void SpawnCharacter(CharacterSpawnData data)
        {
            if (_characters.ContainsKey(data.PlayerId))
                return;

            var character = LoadCharacter();
            SetupCharacter(character, data);
            if (character.IsLocalPlayer)
                _localCharacter = character;

            _characters.Add(data.PlayerId, character);
        }

        private CharacterController LoadCharacter()
        {
            var characterEntities = _contentManager.Load<Prefab>(PathPrefab).Instantiate();
            var character = characterEntities[0].Get<CharacterController>();
            _sceneSystem.SceneInstance.RootScene.Entities.AddRange(characterEntities);

            return character;
        }

        private void SetupCharacter(CharacterController character, CharacterSpawnData data)
        {
            character.PlayerId = data.PlayerId;
            character.CurrentPosition = data.Position;
            character.CurrentRotation = data.Rotation;
            character.IsLocalPlayer = data.PlayerId == _networkSystem.PlayerClientId;

            if (character.IsLocalPlayer)
            {
                character.Entity.Remove(character.RigidbodyComponent);
            }
            else
            {
                character.Entity.Remove(character.CharacterComponent);
                character.CharacterCamera.Entity.Remove(character.CharacterCamera.CameraComponent);
                character.CharacterCamera.Entity.Remove(character.CharacterCamera);
                character.CharacterInput.Entity.Remove(character.CharacterInput);
            }
        }

        private void RemoveCharacter(ushort playerId)
        {
            if (!_characters.TryGetValue(playerId, out CharacterController character))
                return;

            _characters.Remove(playerId);
            _sceneSystem.SceneInstance.RootScene.Entities.Remove(character.Entity);
        }

        private void CharacterAction(CharacterActionData data)
        {
            if (_networkSystem.PlayerClientId == data.PlayerId
                || !_characters.TryGetValue(data.PlayerId, out CharacterController character))
                return;

            character.FutureActions[data.RemoteTime] = data;
        }

        private void ReceivedMessageFromClient(object sender, MessageReceivedEventArgs e)
        {
            if ((GameMessageId)e.MessageId == GameMessageId.CharacterAction)
                _networkSystem.SendMessageToAllClients(e.Message);
        }

        private void SpawnCharacterToAllClients(object sender, ServerConnectedEventArgs e)
        {
            SpawnCharacter(new CharacterSpawnData
            {
                PlayerId = e.Client.Id,
                Position = Vector3.Zero,
                Rotation = Quaternion.Zero,
            });

            foreach (var character in _characters.Values)
            {
                _networkSystem.SendMessageToAllClients(new CharacterSpawnMessage().Create(new CharacterSpawnData
                {
                    PlayerId = character.PlayerId,
                    Position = character.CurrentPosition,
                    Rotation = character.CurrentRotation,
                }));
            }
        }

        private void ReceivedMessageFromServer(object sender, MessageReceivedEventArgs e)
        {
            switch ((GameMessageId)e.MessageId)
            {
                case GameMessageId.CharacterSpawn:
                    SpawnCharacter(new CharacterSpawnMessage().From(e.Message));
                    break;
                case GameMessageId.CharacterAction:
                    CharacterAction(new CharacterActionMessage().From(e.Message));
                    break;
                default:
                    break;
            }
        }

        private void RemoveClientCharacter(object sender, ClientDisconnectedEventArgs e) =>
            RemoveCharacter(e.Id);

        private void RemoveAllCharacters(object sender, DisconnectedEventArgs e)
        {
            foreach (var character in _characters.Values)
                RemoveCharacter(character.PlayerId);
        }
    }
}
