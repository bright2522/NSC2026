using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class PlayableDirectorActivateOnEnd : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private bool deactivateOnPlay = true;

    void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();
    }

    void OnEnable()
    {
        if (director != null)
            director.stopped += OnDirectorStopped;
    }

    void OnDisable()
    {
        if (director != null)
            director.stopped -= OnDirectorStopped;
    }

    void OnDirectorStopped(PlayableDirector _)
    {
        if (objectToActivate != null)
            objectToActivate.SetActive(true);
    }

    public void Play()
    {
        if (deactivateOnPlay && objectToActivate != null)
            objectToActivate.SetActive(false);

        director?.Play();
    }
}
