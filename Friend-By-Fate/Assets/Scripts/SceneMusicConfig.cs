using UnityEngine;
using UnityEngine.Audio;

public class SceneMusicConfig : MonoBehaviour
{
    [SerializeField] private bool playOnSceneLoad = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioMixerGroup musicOutputGroup;

    public bool PlayOnSceneLoad => playOnSceneLoad;
    public bool Loop => loop;
    public AudioClip MusicClip => musicClip;
    public AudioMixerGroup MusicOutputGroup => musicOutputGroup;
}
