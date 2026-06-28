using UnityEngine;

public class SlicedFoodManager : MonoBehaviour
{
    public static SlicedFoodManager Instance { get; private set; }

    [SerializeField] private string containerName = "SlicedPieces";

    public static Transform Container
    {
        get
        {
            if (Instance != null)
                return Instance.transform;

            SlicedFoodManager existing = FindObjectOfType<SlicedFoodManager>();
            if (existing != null)
                return existing.transform;

            GameObject managerObject = new GameObject("SlicedFoodManager");
            return managerObject.AddComponent<SlicedFoodManager>().transform;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.name = containerName;
    }

    public void ClearAllSlicedPieces()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}
