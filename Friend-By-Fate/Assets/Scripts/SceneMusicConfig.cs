using UnityEngine;
using UnityEngine.Audio;

public class SceneMusicConfig : MonoBehaviour
{
    [SerializeField] private bool playOnSceneLoad = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioMixerGroup musicOutputGroup;

    [Header("Additional Ambiences")]
    [SerializeField] private AudioClip extraAmbience2;
    [SerializeField] private bool loopExtraAmbience2 = true;

    [SerializeField] private AudioClip extraAmbience3;
    [SerializeField] private bool loopExtraAmbience3 = true;

    public bool PlayOnSceneLoad => playOnSceneLoad;
    public bool Loop => loop;
    public AudioClip MusicClip => musicClip;
    public AudioMixerGroup MusicOutputGroup => musicOutputGroup;
    public AudioClip ExtraAmbience2 => extraAmbience2;
    public bool LoopExtraAmbience2 => loopExtraAmbience2;
    public AudioClip ExtraAmbience3 => extraAmbience3;
    public bool LoopExtraAmbience3 => loopExtraAmbience3;
}