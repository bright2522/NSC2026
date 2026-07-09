using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerRoomService : MonoBehaviour
{
    public static MultiplayerRoomService Instance { get; private set; }

    [Header("Session")]
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Debug")]
    [SerializeField] private bool loadGameplaySceneOnConnect = true;

    public string CurrentRoomCode { get; private set; } = string.Empty;
    public bool IsInRoom => _currentSession != null;
    public bool IsHost => _currentSession != null && _currentSession.IsHost;

    public event Action<string> OnRoomCodeChanged;
    public event Action<string> OnStatusChanged;
    public event Action OnRoomJoined;
    public event Action<string> OnRoomError;

    private ISession _currentSession;
    private bool _isBusy;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public async void HostRoom()
    {
        await RunRoomAction(async () =>
        {
            SetStatus("กำลังสร้างห้อง...");

            string roomCode = await CreateUniqueRoomAsync();
            CurrentRoomCode = roomCode;
            OnRoomCodeChanged?.Invoke(roomCode);
            OnRoomJoined?.Invoke();
            SetStatus($"สร้างห้องสำเร็จ: {roomCode}");

            if (loadGameplaySceneOnConnect)
            {
                LoadGameplayScene();
            }
        });
    }

    public async void JoinRoom(string rawCode)
    {
        await RunRoomAction(async () =>
        {
            string roomCode = RoomCodeGenerator.Normalize(rawCode);

            if (!RoomCodeGenerator.IsValid(roomCode))
            {
                throw new InvalidOperationException("รหัสห้องต้องมี 6 ตัว (a-z, A-Z, 0-9)");
            }

            SetStatus("กำลังเข้าห้อง...");

            var options = new JoinSessionOptions();
            _currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(roomCode, options);

            CurrentRoomCode = roomCode;
            OnRoomCodeChanged?.Invoke(roomCode);
            OnRoomJoined?.Invoke();
            SetStatus($"เข้าห้อง {roomCode} สำเร็จ");

            if (loadGameplaySceneOnConnect)
            {
                LoadGameplayScene();
            }
        });
    }

    public async void LeaveRoom()
    {
        if (_currentSession == null)
        {
            return;
        }

        await RunRoomAction(async () =>
        {
            SetStatus("กำลังออกจากห้อง...");

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            await _currentSession.LeaveAsync();
            _currentSession = null;
            CurrentRoomCode = string.Empty;
            OnRoomCodeChanged?.Invoke(string.Empty);
            SetStatus("ออกจากห้องแล้ว");
        });
    }

    private async Task<string> CreateUniqueRoomAsync()
    {
        var options = new SessionOptions
        {
            MaxPlayers = maxPlayers,
            IsPrivate = true,
            Name = "Cooking Room"
        }.WithRelayNetwork();

        _currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        string joinCode = _currentSession.Code;

        if (string.IsNullOrEmpty(joinCode))
        {
            throw new InvalidOperationException("ไม่ได้รับรหัสห้องจาก Unity Services");
        }

        return joinCode;
    }

    private async Task EnsureServicesReadyAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private static string ToFriendlyMessage(Exception exception)
    {
        string message = exception.Message ?? string.Empty;
        string lower = message.ToLowerInvariant();

        if (lower.Contains("lobby not found") || lower.Contains("session not found"))
        {
            return "ไม่พบห้องนี้ ตรวจสอบรหัสอีกครั้ง";
        }

        if (lower.Contains("rate limit") || lower.Contains("too many requests"))
        {
            return "ลองบ่อยเกินไป รอสักครู่แล้วลองใหม่";
        }

        if (lower.Contains("already in") && lower.Contains("session"))
        {
            return "คุณอยู่ในห้องอยู่แล้ว ออกจากห้องก่อนแล้วลองใหม่";
        }

        if (lower.Contains("full") || lower.Contains("lobby is full"))
        {
            return "ห้องเต็มแล้ว";
        }

        return message;
    }

    private async Task RunRoomAction(Func<Task> action)
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;

        try
        {
            await EnsureServicesReadyAsync();
            await action();
        }
        catch (Exception exception)
        {
            string message = ToFriendlyMessage(exception);
            Debug.LogError($"[MultiplayerRoomService] {exception.Message}");
            OnRoomError?.Invoke(message);
            SetStatus(message);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void LoadGameplayScene()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            return;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            if (IsHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
            }

            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    private void SetStatus(string message)
    {
        Debug.Log($"[MultiplayerRoomService] {message}");
        OnStatusChanged?.Invoke(message);
    }
}
