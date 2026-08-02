using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkScoreSync : MonoBehaviour
{
    [Header("Room Setup")]
    [Tooltip("ตั้งชื่อห้องให้เหมือนกันทั้ง 2 เครื่อง (เช่น Room123)")]
    public string roomCode = "CookingMatch01";

    [Header("Player Status (Auto Assigned)")]
    [Tooltip("ระบบจะกำหนดให้อัตโนมัติเมื่อเข้าซีน")]
    public int myPlayerID = 0; 
    public bool isRoleAssigned = false;

    private string deviceGuid;
    private int lastMyScore = -1;
    private int opponentScore = 0;

    // Server Key สำหรับเก็บข้อมูลคะแนนออนไลน์
    private string serverBaseUrl = "https://kvdb.io/4Tz91pMvZk8s33v44xX/";

    void Start()
    {
        // สร้าง ID สุ่มประจำเครื่องนี้
        deviceGuid = SystemInfo.deviceUniqueIdentifier + "_" + UnityEngine.Random.Range(1000, 9999);
        
        // เริ่มกระบวนการค้นหาบทบาท (P1 หรือ P2) อัตโนมัติ
        StartCoroutine(AssignPlayerRoleRoutine());
    }

    void Update()
    {
        // ถ้ายังลงทะเบียน P1/P2 ไม่เสร็จ หรือไม่มี ScoreManager ให้รอแป๊บนึง
        if (!isRoleAssigned || ScoreManager.Instance == null) return;

        // 1. ถ้าคะแนนเราเปลี่ยน ให้ส่งขึ้น Cloud
        int currentMyScore = ScoreManager.Instance.currentScore;
        if (currentMyScore != lastMyScore)
        {
            lastMyScore = currentMyScore;
            StartCoroutine(SendScoreToCloud(myPlayerID, currentMyScore));
        }

        // 2. อัปเดต UI คะแนนคู่แข่ง
        ScoreManager.Instance.SetOpponentScore(opponentScore);
    }

    // 🎯 ระบบจัดสรรบทบาท (Host/Client) ผ่าน Cloud อัตโนมัติ
    IEnumerator AssignPlayerRoleRoutine()
    {
        string p1Url = $"{serverBaseUrl}{roomCode}_P1_ID";

        // เช็กว่ามี P1 ลงทะเบียนไว้หรือยัง
        using (UnityWebRequest www = UnityWebRequest.Get(p1Url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(www.downloadHandler.text))
            {
                string existingP1 = www.downloadHandler.text;
                
                // ถ้า P1_ID ตรงกับเครื่องเรา -> เราคือ P1
                if (existingP1 == deviceGuid)
                {
                    myPlayerID = 1;
                }
                else
                {
                    // มี P1 เครื่องอื่นอยู่แล้ว -> เครื่องนี้กลายเป็น P2 ทันที
                    myPlayerID = 2;
                }
            }
            else
            {
                // ถ้ายังไม่มี P1 ในระบบ -> บันทึกเครื่องเราเป็น P1
                myPlayerID = 1;
                StartCoroutine(RegisterDeviceID(p1Url, deviceGuid));
            }
        }

        isRoleAssigned = true;
        Debug.Log($"<color=yellow>[Auto-Role] สำเร็จ! คุณได้รับการกำหนดเป็น Player {myPlayerID}</color>");

        // เริ่มวนลูปดึงคะแนนคู่แข่ง
        StartCoroutine(PollOpponentScoreRoutine());
    }

    IEnumerator RegisterDeviceID(string url, string id)
    {
        byte[] bodyData = System.Text.Encoding.UTF8.GetBytes(id);
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "text/plain");
            yield return www.SendWebRequest();
        }
    }

    // ลูปดึงคะแนนคู่แข่งทุกๆ 0.5 วินาที
    IEnumerator PollOpponentScoreRoutine()
    {
        while (true)
        {
            int opponentID = (myPlayerID == 1) ? 2 : 1;
            StartCoroutine(GetOpponentScoreFromCloud(opponentID));

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator SendScoreToCloud(int playerID, int score)
    {
        string url = $"{serverBaseUrl}{roomCode}_P{playerID}";
        byte[] bodyData = System.Text.Encoding.UTF8.GetBytes(score.ToString());

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "text/plain");

            yield return www.SendWebRequest();
        }
    }

    IEnumerator GetOpponentScoreFromCloud(int opponentID)
    {
        string url = $"{serverBaseUrl}{roomCode}_P{opponentID}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string textResult = www.downloadHandler.text;
                if (int.TryParse(textResult, out int fetchedScore))
                {
                    if (fetchedScore != opponentScore)
                    {
                        opponentScore = fetchedScore;
                        Debug.Log($"<color=orange>[Cloud Sync] ได้รับคะแนนคู่แข่ง (P{opponentID}): {opponentScore}</color>");
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}