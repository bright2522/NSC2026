using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameplaySystemsBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureGameplaySystems()
    {
        if (SceneManager.GetActiveScene().name != "Gameplay") return;
        if (Object.FindFirstObjectByType<GameplayScore>() != null) return;

        var go = new GameObject("GameplaySystems");
        go.AddComponent<GameplayScore>();
        go.AddComponent<GameplayTimer>();
        go.AddComponent<GameplayHUD>();
    }
}
