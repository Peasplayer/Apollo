using Apollo.ExtendedClasses;
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
            if (AmongUsClient.Instance.AmHost) 
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
    }
}