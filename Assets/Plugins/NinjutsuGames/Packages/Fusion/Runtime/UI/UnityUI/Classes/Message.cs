using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class Message
    {
        public Transform instance;
        public Transform target;
        public Vector3 offset;
        public float duration;
        public float fadeOutTime = 0.5f;
        public bool forceExpire;
        public Text text;
        public TMP_Text tmpText;
        public float startTime;
        public CanvasGroup canvasGroup;
    }
}