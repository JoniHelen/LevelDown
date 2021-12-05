using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioHandler instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(string name, AudioSource source)
    {
        foreach (Sound s in sounds)
        {
            if (s.name == name)
            {
                source.clip = s.clip;
                source.volume = s.volume;
                source.pitch = s.pitch;

                source.Play();
            }
        }
    }

    public void PlaySound(string name, AudioSource source, float pitch)
    {
        foreach (Sound s in sounds)
        {
            if (s.name == name)
            {
                source.clip = s.clip;
                source.volume = s.volume;
                source.pitch = pitch;

                source.Play();
            }
        }
    }

    public void PlaySound(string name, AudioSource source, float pitch, bool looping)
    {
        foreach (Sound s in sounds)
        {
            if (s.name == name)
            {
                source.clip = s.clip;
                source.volume = s.volume;
                source.pitch = pitch;
                source.loop = looping;

                source.Play();
            }
        }
    }

}
