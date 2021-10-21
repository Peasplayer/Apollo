using Hazel;
using Reactor;
using Reactor.Networking;

namespace LevelCrewmate
{
    public class CustomRpc
    {
        public enum RpcCalls
        {
            HandShake,
            UseCustomMap
        }
        
        [RegisterCustomRpc((uint) RpcCalls.HandShake)]
        public class RpcSendHandshake : PlayerCustomRpc<LevelCrewmatePlugin, RpcSendHandshake.Data>
        {
            public RpcSendHandshake(LevelCrewmatePlugin plugin, uint id) : base(plugin, id)
            {
            }

            public readonly struct Data
            {
                public readonly byte Player;

                public Data(byte player)
                {
                    Player = player;
                }
            }

            public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

            public override void Write(MessageWriter writer, Data data)
            {
                writer.Write(data.Player);
            }

            public override Data Read(MessageReader reader)
            {
                return new Data(reader.ReadByte());
            }

            public override void Handle(PlayerControl innerNetObject, Data data)
            {
                if (!AmongUsClient.Instance.AmHost) 
                    return;
                
                if (data.Player == PlayerControl.LocalPlayer.PlayerId) 
                    return;

                Rpc<RpcUseCustomMap>.Instance.Send(new RpcUseCustomMap.Data(data.Player, CustomMap.UseCustomMap));
            }
        }
        
        [RegisterCustomRpc((uint) RpcCalls.UseCustomMap)]
        public class RpcUseCustomMap : PlayerCustomRpc<LevelCrewmatePlugin, RpcUseCustomMap.Data>
        {
            public RpcUseCustomMap(LevelCrewmatePlugin plugin, uint id) : base(plugin, id)
            {
            }

            public readonly struct Data
            {
                public readonly byte Player;
                public readonly bool UseCustomMap;

                public Data(byte player, bool useCustomMap)
                {
                    Player = player;
                    UseCustomMap = useCustomMap;
                }
            }

            public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

            public override void Write(MessageWriter writer, Data data)
            {
                writer.Write(data.Player);
                writer.Write(data.UseCustomMap);
            }

            public override Data Read(MessageReader reader)
            {
                return new Data(reader.ReadByte(), reader.ReadBoolean());
            }

            public override void Handle(PlayerControl innerNetObject, Data data)
            {
                if (AmongUsClient.Instance.AmHost) 
                    return;
                
                if (PlayerControl.LocalPlayer.PlayerId != data.Player) 
                    return;

                CustomMap.UseCustomMap = data.UseCustomMap;
            }
        }
    }
}