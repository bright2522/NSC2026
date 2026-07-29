#if CMPSETUP_COMPLETE
using Fusion;
using UnityEngine;

public class PlayerUI : NetworkBehaviour
{
    public GameObject playerCanvas;

    public override void Spawned()
    {
        if (playerCanvas != null)
            playerCanvas.SetActive(HasInputAuthority);
    }
}
#endif
