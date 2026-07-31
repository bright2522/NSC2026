using Unity.Netcode;
using UnityEngine;
#if CMPSETUP_COMPLETE
using AvocadoShark;
#endif

public static class CreateRoomSceneBootstrap
{
    public static bool EnsureSceneReady()
    {
        ActivateIfFound<NetworkManager>();
        ActivateIfFound<LobbyFlowController>();
        ActivateIfFound<LobbyManager>();
#if CMPSETUP_COMPLETE
        EnsureFusionLobby();
#endif
        return EnsureRoomService() != null;
    }

#if CMPSETUP_COMPLETE
    public static FusionLobbyBridge EnsureFusionLobby()
    {
        if (FusionLobbyBridge.Instance != null)
        {
            FusionLobbyBridge.Instance.EnsureFusionConnection();
            return FusionLobbyBridge.Instance;
        }

        var bridge = Object.FindFirstObjectByType<FusionLobbyBridge>(FindObjectsInactive.Include);
        if (bridge != null)
        {
            if (!bridge.gameObject.activeSelf)
            {
                bridge.gameObject.SetActive(true);
            }

            bridge.EnsureFusionConnection();
            return bridge;
        }

        var go = new GameObject("FusionLobbyBridge");
        bridge = go.AddComponent<FusionLobbyBridge>();
        bridge.EnsureFusionConnection();
        return bridge;
    }
#endif

    public static MultiplayerRoomService EnsureRoomService()
    {
        if (MultiplayerRoomService.Instance != null)
        {
            return MultiplayerRoomService.Instance;
        }

        var service = Object.FindFirstObjectByType<MultiplayerRoomService>(FindObjectsInactive.Include);
        if (service == null)
        {
            return null;
        }

        if (!service.gameObject.activeSelf)
        {
            service.gameObject.SetActive(true);
        }

        if (MultiplayerRoomService.Instance == null)
        {
            MultiplayerRoomService.BindInstance(service);
        }

        return MultiplayerRoomService.Instance;
    }

    private static void ActivateIfFound<T>() where T : Component
    {
        var component = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        if (component == null)
        {
            return;
        }

        if (!component.gameObject.activeSelf)
        {
            component.gameObject.SetActive(true);
        }
    }
}
