using TMPro;
using UnityEngine;
using System.Collections;

public class RoomCodeUI : MonoBehaviour
{
    public GameObject numberPad;
    public GameObject modePanel;
    public TMP_Text codeText;
    public TMP_Text warningText;

    private string roomCode = "";

    void Start()
    {
        numberPad.SetActive(false);

        codeText.text = "Enter Room Code";
        warningText.text = "";
    }

    public void OpenNumberPad()
{
    modePanel.SetActive(false);

    numberPad.SetActive(true);
}

    public void AddNumber(string number)
    {
        if (roomCode.Length >= 6)
            return;

        warningText.text = "";

        if (roomCode == "")
            codeText.text = "";

        roomCode += number;
        codeText.text = roomCode;
    }

    public void CloseNumberPad()
{
    roomCode = "";

    codeText.text = "Enter Room Code";

    warningText.text = "";

    numberPad.SetActive(false);

    modePanel.SetActive(true);
}

    public void EnterRoom()
    {
        if (roomCode != "123456")
        {
            StartCoroutine(WrongCodeMessage());
            return;
        }

        warningText.text = "";
        Debug.Log("เข้าห้องสำเร็จ");

        // ใส่โค้ดเข้าห้องจริงตรงนี้
    }

    IEnumerator WrongCodeMessage()
    {
        roomCode = "";

        codeText.text = "";
        warningText.text = "Not Found";

        yield return new WaitForSeconds(3f);

        warningText.text = "";
        codeText.text = "Enter Room Code";
    }

    public void ClearCode()
    {
        roomCode = "";
        warningText.text = "";
        codeText.text = "Enter Room Code";
    }

    public void BackSpace()
    {
        if (roomCode.Length == 0)
            return;

        roomCode = roomCode.Substring(0, roomCode.Length - 1);

        if (roomCode.Length == 0)
            codeText.text = "Enter Room Code";
        else
            codeText.text = roomCode;
    }
}