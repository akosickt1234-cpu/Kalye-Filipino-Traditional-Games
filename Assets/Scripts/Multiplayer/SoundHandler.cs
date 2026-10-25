using UnityEngine;
using Unity.Netcode;

public class SoundHandler : NetworkBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("AudioClips")]
    [SerializeField] private AudioClip thud;
    [SerializeField] private AudioClip walk;
    [SerializeField] private AudioClip run;

    private int currentMovementSound = -1;

    public void Thud()
    {
        PlaySoundRpc(0);
    }

    public void Walk()
    {
        SetMovementSound(1);
    }

    public void Run()
    {
        SetMovementSound(2);
    }

    public void Idle()
    {
        SetMovementSound(3);
    }

    private void SetMovementSound(int soundID)
    {
        if (currentMovementSound == soundID) return;

        currentMovementSound = soundID;
        PlaySoundRpc(soundID);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlaySoundRpc(int soundID)
    {
        if (soundID == 0)
        {
            if (thud != null)
                audioSource.PlayOneShot(thud);

            return;
        }

        AudioClip clip = soundID switch
        {
            1 => walk,
            2 => run,
            _ => null
        };

        if (clip == null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
}