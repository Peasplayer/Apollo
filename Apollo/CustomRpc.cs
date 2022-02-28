using Reactor.Networking.MethodRpc;

namespace Apollo
{
    public class CustomRpc
    {
        public enum RpcCalls
        {
            HandShake,
            UseCustomMap,
            UsePlatform
        }

        [MethodRpc((uint) RpcCalls.HandShake)]
        public static void RpcSendHandShake(PlayerControl sender)
        {
            if (!AmongUsClient.Instance.AmHost) 
                return;
                
            if (sender.PlayerId == PlayerControl.LocalPlayer.PlayerId) 
                return;

            RpcUseCustomMap(PlayerControl.LocalPlayer, sender.PlayerId, CustomMap.UseCustomMap);
        }
        
        [MethodRpc((uint) RpcCalls.UseCustomMap)]
        public static void RpcUseCustomMap(PlayerControl sender, byte target, bool useCustomMap)
        {
            if (!AmongUsClient.Instance.AmHost) 
                return;
                
            if (target != PlayerControl.LocalPlayer.PlayerId && target != byte.MaxValue) 
                return;

            CustomMap.UseCustomMap = useCustomMap;
        }

        [MethodRpc((uint)RpcCalls.UsePlatform)]
        public static void RpcUsePlatform(PlayerControl sender, int platform)
        {
            var movingPlatform = MovingPlatformHandler.GetPlatform(platform);
            if (movingPlatform != null)
                movingPlatform.PlatformBehaviour.Use(sender);
        }
        
        /*[RegisterCustomRpc((uint) RpcCalls.HandShake)]
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
        }*/
    }
}