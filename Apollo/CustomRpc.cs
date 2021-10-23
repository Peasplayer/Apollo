using Hazel;
using Reactor;
using Reactor.Networking;

namespace Apollo
{
    public class CustomRpc
    {
        public enum RpcCalls
        {
            HandShake,
            UseCustomMap
        }
        
        [RegisterCustomRpc((uint) RpcCalls.HandShake)]
        public class RpcSendHandshake : PlayerCustomRpc<ApolloPlugin, RpcSendHandshake.Data>
        {
            public RpcSendHandshake(ApolloPlugin plugin, uint id) : base(plugin, id)
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

                Rpc<RpcUseCustomMap>.Instance.SendTo(data.Player, new RpcUseCustomMap.Data(CustomMap.UseCustomMap));
            }
        }
        
        [RegisterCustomRpc((uint) RpcCalls.UseCustomMap)]
        public class RpcUseCustomMap : PlayerCustomRpc<ApolloPlugin, RpcUseCustomMap.Data>
        {
            public RpcUseCustomMap(ApolloPlugin plugin, uint id) : base(plugin, id)
            {
            }

            public readonly struct Data
            {
                public readonly bool UseCustomMap;

                public Data(bool useCustomMap)
                {
                    UseCustomMap = useCustomMap;
                }
            }

            public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

            public override void Write(MessageWriter writer, Data data)
            {
                writer.Write(data.UseCustomMap);
            }

            public override Data Read(MessageReader reader)
            {
                return new Data(reader.ReadBoolean());
            }

            public override void Handle(PlayerControl innerNetObject, Data data)
            {
                if (AmongUsClient.Instance.AmHost) 
                    return;

                CustomMap.UseCustomMap = data.UseCustomMap;
            }
        }
    }
}