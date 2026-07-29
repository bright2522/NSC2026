using Unity.Netcode;
using UnityEngine;

public static class CreateRoomSceneBootstrap
{
    public static bool EnsureSceneReady()
    {
        ActivateIfFound<NetworkManager>();
        ActivateIfFound<LobbyFlowController>();
        ActivateIfFound<LobbyManager>();

        return EnsureRoomService() != null;
    }

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
