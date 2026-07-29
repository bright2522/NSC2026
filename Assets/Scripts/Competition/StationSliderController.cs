using UnityEngine;

public class StationSliderController : MonoBehaviour
{
    [SerializeField] private Transform rowRoot;
    [SerializeField] private Transform[] stations;
    [SerializeField] private float spacing = 20f;
    [SerializeField] private int startIndex;
    [SerializeField] private float snapDuration = 0.35f;
    [SerializeField] private LeanTweenType snapEase = LeanTweenType.easeOutCubic;
    [SerializeField] private bool swipeEnabled = true;
    [SerializeField] private float switchThreshold = 0.2f;
    [SerializeField] private float flickSpeed = 4f;

    public int CurrentIndex { get; private set; }
    public int StationCount => stations != null ? stations.Length : 0;
    public bool IsEnabled => swipeEnabled;

    private Vector3 homeLocalPosition;
    private Camera cam;
    private bool dragging;
    private float dragStartWorldX;
    private float dragStartScreenX;
    private Vector3 dragStartRowLocal;
    private float lastWorldX;
    private float lastScreenX;
    private float velocity;
    private int activeTweenId = -1;
    private bool layoutReady;

    void Awake()
    {
        EnsureLayout();
    }

    void Start()
    {
        cam = Camera.main;
        EnsureLayout();
        GoTo(startIndex, instant: true);
    }

    void Update()
    {
        if (!swipeEnabled || StationCount <= 1) return;
        HandleDrag();
    }

    public void SetEnabled(bool enabled)
    {
        swipeEnabled = enabled;
        if (!enabled && dragging)
        {
            dragging = false;
            SnapToCurrent();
        }
    }

    public void Next()
    {
        if (StationCount <= 0) return;
        GoTo((CurrentIndex + 1) % StationCount);
    }

    public void Previous()
    {
        if (StationCount <= 0) return;
        GoTo((CurrentIndex - 1 + StationCount) % StationCount);
    }

    public void GoTo(int index, bool instant = false)
    {
        EnsureLayout();
        if (StationCount <= 0 || rowRoot == null) return;

        CurrentIndex = Mathf.Clamp(index, 0, StationCount - 1);
        Vector3 target = PositionForIndex(CurrentIndex);

        CancelTween();

        if (instant || snapDuration <= 0f)
        {
            rowRoot.localPosition = target;
            return;
        }

        activeTweenId = LeanTween.moveLocal(rowRoot.gameObject, target, snapDuration)
            .setEase(snapEase)
            .id;
    }

    void EnsureLayout()
    {
        if (layoutReady) return;

        if (rowRoot == null)
            rowRoot = transform;

        homeLocalPosition = rowRoot.localPosition;

        if (stations != null && stations.Length > 0)
        {
            for (int i = 0; i < stations.Length; i++)
            {
                if (stations[i] == null) continue;
                stations[i].localPosition = new Vector3(spacing * i, stations[i].localPosition.y, stations[i].localPosition.z);
            }
        }

        layoutReady = true;
    }

    void HandleDrag()
    {
        if (rowRoot == null) return;

        if (GetPressDown(out Vector2 downPos))
        {
            CancelTween();
            dragging = true;
            dragStartWorldX = ScreenToWorldX(downPos);
            dragStartScreenX = downPos.x;
            lastWorldX = dragStartWorldX;
            lastScreenX = dragStartScreenX;
            dragStartRowLocal = rowRoot.localPosition;
            velocity = 0f;
        }

        if (dragging && GetPressHeld(out Vector2 movePos))
        {
            float wx = ScreenToWorldX(movePos);
            float delta = wx - dragStartWorldX;
            rowRoot.localPosition = new Vector3(dragStartRowLocal.x + delta, dragStartRowLocal.y, dragStartRowLocal.z);
            velocity = (wx - lastWorldX) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastWorldX = wx;
            lastScreenX = movePos.x;
        }

        if (dragging && GetPressUp())
        {
            dragging = false;
            float movedSlots = Mathf.Abs(rowRoot.localPosition.x - dragStartRowLocal.x) / Mathf.Max(spacing, 0.0001f);
            bool swipedLeft = lastScreenX < dragStartScreenX;
            bool flick = Mathf.Abs(velocity) > flickSpeed;

            if (swipedLeft && (movedSlots > switchThreshold || flick))
                CurrentIndex = NextIndex();
            else if (!swipedLeft && (movedSlots > switchThreshold || flick))
                CurrentIndex = PreviousIndex();

            SnapToCurrent();
        }
    }

    void SnapToCurrent()
    {
        GoTo(CurrentIndex);
    }

    int NextIndex() => Mathf.Min(CurrentIndex + 1, StationCount - 1);
    int PreviousIndex() => Mathf.Max(CurrentIndex - 1, 0);

    Vector3 PositionForIndex(int index) => homeLocalPosition + Vector3.left * (spacing * index);

    float ScreenToWorldX(Vector2 screenPos)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return screenPos.x;

        float depth = Mathf.Abs(cam.transform.position.z - rowRoot.position.z);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));
        return world.x;
    }

    bool GetPressDown(out Vector2 pos)
    {
        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
            return true;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            pos = Input.GetTouch(0).position;
            return true;
        }

        pos = default;
        return false;
    }

    bool GetPressHeld(out Vector2 pos)
    {
        if (Input.GetMouseButton(0))
        {
            pos = Input.mousePosition;
            return true;
        }

        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                pos = t.position;
                return true;
            }
        }

        pos = default;
        return false;
    }

    bool GetPressUp()
    {
        if (Input.GetMouseButtonUp(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) return true;
        return false;
    }

    void CancelTween()
    {
        if (rowRoot != null)
            LeanTween.cancel(rowRoot.gameObject);

        if (activeTweenId >= 0)
        {
            LeanTween.cancel(activeTweenId);
            activeTweenId = -1;
        }
    }

    void OnDisable()
    {
        CancelTween();
        dragging = false;
    }
}
