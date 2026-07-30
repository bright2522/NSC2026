#if CMPSETUP_COMPLETE
using System;
using AvocadoShark;
using Fusion;
using UnityEngine;

public class FusionLobbyBridge : MonoBehaviour
{
    public static FusionLobbyBridge Instance { get; private set; }

    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private string gameSceneName = "Rpvp";
    [SerializeField] private int maxPlayers = 4;

    public string PendingRoomCode { get; private set; } = string.Empty;
    public string LocalDisplayName { get; private set; } = "Player";
    public bool HasDisplayName { get; private set; }

    public event Action<string> OnStatusChanged;
    public event Action<string> OnRoomError;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureFusionConnection();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetDisplayName(string rawName)
    {
        string trimmed = rawName?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (trimmed.Length > 16)
        {
            trimmed = trimmed[..16];
        }

        LocalDisplayName = trimmed;
        HasDisplayName = true;
        PlayerPrefs.SetString("MP_PLAYER_NAME", trimmed);
        PlayerPrefs.Save();

        EnsureFusionConnection();
        if (FusionConnection.Instance != null)
        {
            FusionConnection.Instance._playerName = trimmed;
        }

        SetStatus($"สวัสดี {trimmed}");
    }

    public void PrepareHostRoom()
    {
        if (!HasDisplayName)
        {
            RaiseError("กรุณาใส่ชื่อก่อน");
            return;
        }

        PendingRoomCode = RoomCodeGenerator.Generate();
        SetStatus($"รหัส {PendingRoomCode} — กด ENTER ROOM ก่อน แล้วให้เพื่อน Join");
    }

    public void EnterHostedRoom()
    {
        if (!HasDisplayName)
        {
            RaiseError("กรุณาใส่ชื่อก่อน");
            return;
        }

        if (string.IsNullOrEmpty(PendingRoomCode))
        {
            PendingRoomCode = RoomCodeGenerator.Generate();
        }

        var fusion = EnsureFusionConnection();
        if (fusion == null)
        {
            RaiseError("ไม่พบ FusionConnection");
            return;
        }

        fusion._playerName = LocalDisplayName;
        SetStatus("กำลังสร้างห้องและเข้า Rpvp...");

        if (!HasPhotonAppId())
        {
            RaiseError("Photon App Id ว่าง — ใส่ AppIdFusion ใน PhotonAppSettings ก่อน");
            return;
        }

        fusion.JoinRoom(PendingRoomCode, maxPlayers, string.Empty);
    }

    public void JoinRoom(string rawCode)
    {
        if (!HasDisplayName)
        {
            RaiseError("กรุณาใส่ชื่อก่อน");
            return;
        }

        string roomCode = RoomCodeGenerator.Normalize(rawCode);
        if (!RoomCodeGenerator.IsValid(roomCode))
        {
            RaiseError("รหัสห้องต้องมี 6 ตัว (a-z, A-Z, 0-9)");
            return;
        }

        if (!HasPhotonAppId())
        {
            RaiseError("Photon App Id ว่าง — ใส่ AppIdFusion ใน PhotonAppSettings ก่อน");
            return;
        }

        PendingRoomCode = roomCode;
        SetStatus($"กำลังเข้าห้อง {roomCode}...");

        var fusion = EnsureFusionConnection();
        if (fusion == null)
        {
            RaiseError("ไม่พบ FusionConnection");
            return;
        }

        fusion._playerName = LocalDisplayName;
        fusion.JoinRoom(roomCode);
    }

    private static bool HasPhotonAppId()
    {
        var settings = Fusion.Photon.Realtime.PhotonAppSettings.Global.AppSettings;
        return !string.IsNullOrWhiteSpace(settings.AppIdFusion)
               || !string.IsNullOrWhiteSpace(settings.AppIdRealtime);
    }

    public FusionConnection EnsureFusionConnection()
    {
        if (FusionConnection.Instance != null)
        {
            FusionConnection.Instance.Configure(runnerPrefab, gameSceneName);
            if (!string.IsNullOrEmpty(LocalDisplayName))
            {
                FusionConnection.Instance._playerName = LocalDisplayName;
            }

            return FusionConnection.Instance;
        }

        var existing = FindFirstObjectByType<FusionConnection>(FindObjectsInactive.Include);
        if (existing != null)
        {
            if (!existing.gameObject.activeSelf)
            {
                existing.gameObject.SetActive(true);
            }

            existing.Configure(runnerPrefab, gameSceneName);
            if (!string.IsNullOrEmpty(LocalDisplayName))
            {
                existing._playerName = LocalDisplayName;
            }

            return existing;
        }

        var go = new GameObject("FusionConnectionManager");
        var fusion = go.AddComponent<FusionConnection>();
        fusion.Configure(runnerPrefab, gameSceneName);
        if (!string.IsNullOrEmpty(LocalDisplayName))
        {
            fusion._playerName = LocalDisplayName;
        }

        return fusion;
    }

    private void SetStatus(string message)
    {
        Debug.Log($"[FusionLobbyBridge] {message}");
        OnStatusChanged?.Invoke(message);
    }

    private void RaiseError(string message)
    {
        Debug.LogError($"[FusionLobbyBridge] {message}");
        OnRoomError?.Invoke(message);
        OnStatusChanged?.Invoke(message);
    }
}
#endif
