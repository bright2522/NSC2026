using TMPro;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public TMP_Text roomCodeText;

    void Start()
    {
        string code = Random.Range(100000, 999999).ToString();

        roomCodeText.text = code;

        RoomData.CurrentRoomCode = code;
    }
}