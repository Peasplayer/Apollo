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
        public class RpcSendHandshake : PlayerCustomRpc<ApolloPlugin, byte>
        {
            public RpcSendHandshake(ApolloPlugin plugin, uint id) : base(plugin, id)
            {
            }

            public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

            public override void Write(MessageWriter writer, byte data)
            {
                writer.Write(data);
            }

            public override byte Read(MessageReader reader)
            {
                return reader.ReadByte();
            }

            public override void Handle(PlayerControl innerNetObject, byte data)
            {
                if (!AmongUsClient.Instance.AmHost) 
                    return;
                
                if (data == PlayerControl.LocalPlayer.PlayerId) 
                    return;

                Rpc<RpcUseCustomMap>.Instance.SendTo(data, CustomMap.UseCustomMap);
            }
        }
        
        [RegisterCustomRpc((uint) RpcCalls.UseCustomMap)]
        public class RpcUseCustomMap : PlayerCustomRpc<ApolloPlugin, bool>
        {
            public RpcUseCustomMap(ApolloPlugin plugin, uint id) : base(plugin, id)
            {
            }

            public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

            public override void Write(MessageWriter writer, bool data)
            {
                writer.Write(data);
            }

            public override bool Read(MessageReader reader)
            {
                return reader.ReadBoolean();
            }
            
            public override void Handle(PlayerControl innerNetObject, bool data)
            {
                if (AmongUsClient.Instance.AmHost) 
                    return;

                CustomMap.UseCustomMap = data;
            }
        }
    }
}