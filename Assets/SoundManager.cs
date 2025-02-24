using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip NotYourmoveSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip drawSound;
    [SerializeField] private AudioClip turnChangeSound;
    [SerializeField] private AudioClip chatNotificationSound;

    private AudioSource audioSource;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep SoundManager persistent
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayButtonClick() => PlaySound(buttonClickSound);
    public void PlayMoveSound() => PlaySound(moveSound);
    public void PlayNotYourMoveSound() => PlaySound(NotYourmoveSound);
    public void PlayWinSound() => PlaySound(winSound);
    public void PlayDrawSound() => PlaySound(drawSound);
    public void PlayTurnChangeSound() => PlaySound(turnChangeSound);
    public void PlayChatNotificationSound() => PlaySound(chatNotificationSound);
}