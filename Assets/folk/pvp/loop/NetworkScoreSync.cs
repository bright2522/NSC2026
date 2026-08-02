using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkScoreSync : MonoBehaviour
{
    [Header("Network Settings")]
    public int networkPort = 11235;

    [Header("Player Role (ระบบจะสุ่ม/เลือกให้อัตโนมัติเมื่อเข้าซีน)")]
    public int myPlayerID = 1;

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    private int lastMyScore = -1;
    private int opponentScore = 0;
    private float autoDetectTimer = 0f;
    private bool isRoleAssigned = false;

    void Start()
    {
        InitNetwork();
        // ลองส่งสัญญาณเช็กว่ามี Player 1 อยู่ในห้องหรือยัง
        SendCheckPing();
    }

    void InitNetwork()
    {
        try
        {
            udpClient = new UdpClient(networkPort);
            udpClient.EnableBroadcast = true;
            remoteEndPoint = new IPEndPoint(IPAddress.Any, networkPort);

            udpClient.BeginReceive(OnDataReceived, null);
            Debug.Log($"<color=cyan>[Network] เริ่มการเชื่อมต่อ LAN บน Port {networkPort}</color>");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network Error] เปิดพอร์ตไม่สำเร็จ: {e.Message}");
        }
    }

    void Update()
    {
        // 💡 นับเวลา 1 วินาทีแรกเพื่อตัดสินใจว่าเครื่องนี้จะเป็น P1 หรือ P2
        if (!isRoleAssigned)
        {
            autoDetectTimer += Time.deltaTime;
            if (autoDetectTimer >= 1.0f)
            {
                isRoleAssigned = true;
                Debug.Log($"<color=yellow>[Auto-Role] กำหนดบทบาทเรียบร้อย: คุณคือ Player {myPlayerID}</color>");
            }
        }

        if (ScoreManager.Instance == null) return;

        // 1. ส่งคะแนนของเราออกไป
        int currentMyScore = ScoreManager.Instance.currentScore;
        if (currentMyScore != lastMyScore)
        {
            lastMyScore = currentMyScore;
            SendScoreToNetwork(myPlayerID, currentMyScore);
        }

        // 2. อัปเดตคะแนนฝั่งตรงข้าม
        ScoreManager.Instance.SetOpponentScore(opponentScore);
    }

    void SendCheckPing()
    {
        SendRawMessage("PING_CHECK");
    }

    void SendScoreToNetwork(int playerID, int score)
    {
        SendRawMessage($"SCORE:{playerID}:{score}");
    }

    void SendRawMessage(string message)
    {
        if (udpClient == null) return;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            IPEndPoint broadcastEP = new IPEndPoint(IPAddress.Broadcast, networkPort);
            udpClient.Send(data, data.Length, broadcastEP);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Network Send Error] {e.Message}");
        }
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        try
        {
            byte[] receivedBytes = udpClient.EndReceive(ar, ref remoteEndPoint);
            string message = Encoding.UTF8.GetString(receivedBytes);

            // กรณีเจอสัญญาณทักทายจากเครื่องอื่น
            if (message == "PING_CHECK")
            {
                // ถ้ามีคน PING มาหาเรา และเราเข้าซีนมาก่อน (เป็น P1 อยู่แล้ว) ให้ตอบกลับไปบอกว่า "ฉันคือ P1"
                if (myPlayerID == 1)
                {
                    SendRawMessage("P1_ALREADY_EXISTS");
                }
            }
            else if (message == "P1_ALREADY_EXISTS")
            {
                // ถ้ามีข้อความบอกว่ามี P1 อยู่แล้ว และเรายังไม่ได้ล็อก Role ให้เราเปลี่ยนตัวเองเป็น P2 ทันที!
                if (!isRoleAssigned)
                {
                    myPlayerID = 2;
                    isRoleAssigned = true;
                    Debug.Log("<color=green>[Auto-Role] พบ Player 1 ในระบบ -> กำหนดตัวเองเป็น Player 2</color>");
                }
            }
            // กรณีเป็นข้อมูลคะแนน
            else if (message.StartsWith("SCORE:"))
            {
                string[] parts = message.Split(':');
                if (parts.Length == 3)
                {
                    int senderID = int.Parse(parts[1]);
                    int senderScore = int.Parse(parts[2]);

                    if (senderID != myPlayerID)
                    {
                        opponentScore = senderScore;
                    }
                }
            }

            udpClient.BeginReceive(OnDataReceived, null);
        }
        catch (Exception) { }
    }

    private void OnDestroy() { CloseNetwork(); }
    private void OnApplicationQuit() { CloseNetwork(); }

    void CloseNetwork()
    {
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
    }
}