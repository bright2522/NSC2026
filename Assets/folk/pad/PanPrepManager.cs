using UnityEngine;

public class PanPrepManager : MonoBehaviour
{
    private static PanPrepManager instance;
    public static PanPrepManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PanPrepManager>();
                if (instance == null)
                {
                    var go = new GameObject("PanPrepManager");
                    instance = go.AddComponent<PanPrepManager>();
                }
            }
            return instance;
        }
    }

    private bool eggDone;
    private bool sausageDone;
    private bool prepBonusAwarded;

    public bool IsEggDone => eggDone;
    public bool IsSausageDone => sausageDone;
    public bool IsAllPrepDone => eggDone && sausageDone;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void MarkEggDone()
    {
        if (eggDone) return;
        eggDone = true;
        Debug.Log("[PanPrep] ใส่ไข่ครบแล้ว");
        LogStatus();
    }

    public void MarkSausageDone()
    {
        if (sausageDone) return;
        sausageDone = true;
        Debug.Log("[PanPrep] ใส่ไส้กรอกครบแล้ว");
        LogStatus();
    }

    void LogStatus()
    {
        if (!IsAllPrepDone) return;

        if (!prepBonusAwarded)
        {
            prepBonusAwarded = true;
            GameplayScore.Instance?.AddScore(40);
        }

        Debug.Log("[PanPrep] วัตถุดิบครบแล้ว — หยิบตะหลิวผัดได้");
    }
}
