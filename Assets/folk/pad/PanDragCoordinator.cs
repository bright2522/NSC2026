using UnityEngine;

public static class PanDragCoordinator
{
    private static MonoBehaviour activeInteractor;

    public static bool HasActiveInteraction => activeInteractor != null;

    public static bool IsOwner(MonoBehaviour interactor)
    {
        return activeInteractor == interactor;
    }

    public static bool CanInteract(MonoBehaviour interactor)
    {
        return activeInteractor == null || activeInteractor == interactor;
    }

    public static bool TryBegin(MonoBehaviour interactor)
    {
        if (!CanInteract(interactor))
        {
            return false;
        }

        activeInteractor = interactor;
        return true;
    }

    public static void Maintain(MonoBehaviour interactor)
    {
        if (activeInteractor == null || activeInteractor == interactor)
        {
            activeInteractor = interactor;
        }
    }

    public static void End(MonoBehaviour interactor)
    {
        if (activeInteractor == interactor)
        {
            activeInteractor = null;
        }
    }

    public static bool IsBlocked(MonoBehaviour interactor)
    {
        return activeInteractor != null && activeInteractor != interactor;
    }

    public static bool IsHitOnObject(RaycastHit hit, Transform target)
    {
        return hit.transform == target || hit.transform.IsChildOf(target);
    }
}
