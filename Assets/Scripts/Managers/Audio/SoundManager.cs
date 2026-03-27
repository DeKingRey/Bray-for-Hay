using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class LevelSound
{
    public AudioClip musicClip;
    public AudioClip ambienceClip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Enemy")]
    [SerializeField] private LayerMask audioEnemyLayer;
    [SerializeField] private GameObject audioPrefab;

    [Space(10)]

    [Header("Music")]
    [Tooltip("Volume of music when dampened")]
    [SerializeField] private float dampVolume = 0.25f;
    public float maxMusicVolume;
    
    [SerializeField] private LevelSound[] levelSounds;
    [SerializeField] private AudioClip menuMusic;

    [SerializeField] private AudioSource ambienceSource;

    public AudioClip currentGameMusic;
    private AudioClip currentAmbience;

    private MusicFade fader;
    private AudioSource musicSource;
    private Transform player;

    private bool justPaused = false;

    // Dict for changing music states
    private Dictionary<GameManager.GameState, Action> stateActions;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        fader = GetComponent<MusicFade>();
        musicSource = GetComponent<AudioSource>();

        // Creates music state dict
        stateActions = new Dictionary<GameManager.GameState, Action>
        {
            { GameManager.GameState.Playing, () => StartCoroutine(WaitForFade(currentGameMusic)) },
            { GameManager.GameState.Menu, () => StartCoroutine(WaitForFade(menuMusic)) },
            { GameManager.GameState.LevelComplete, () => fader.StartFade(musicSource.volume, 0f, false) },
            { GameManager.GameState.GameOver, () => fader.StartFade(musicSource.volume, 0f, false) },
            { GameManager.GameState.Paused, () => fader.StartFade(musicSource.volume, dampVolume, true) }
        };

        GameManager.OnGameStateChanged += ChangeTrack;
        GameManager.OnSceneChanged += ChangeSceneTrack;

        #region Applying Current Music

        AudioClip newMusic = null;
        AudioClip newAmbience = null;

        // Changes tracks depending on level type
        if (GameManager.Instance.Level == GameManager.LevelType.Paddock)
        {
            newMusic = levelSounds[0].musicClip;
            newAmbience = levelSounds[0].ambienceClip;
        } else if (GameManager.Instance.Level == GameManager.LevelType.Mountains)
        {
            newMusic = levelSounds[1].musicClip;
            newAmbience = levelSounds[1].ambienceClip;
        }

        // Only switches if different
        if (currentGameMusic != newMusic)
        {
            currentGameMusic = newMusic;
            currentAmbience = newAmbience;
            //musicSource.clip = currentGameMusic;
            //fader.StartFade(0f, maxMusicVolume, false);
            
            ambienceSource.clip = currentAmbience;
            ambienceSource.Play();
        }
        #endregion
    }

    void Update()
    {
        if (GameManager.Instance.State == GameManager.GameState.Menu) return;
        if (!player) player = FindObjectOfType<PlayerController>().transform;
    }

    void OnDestroy()
    {
       GameManager.OnGameStateChanged -= ChangeTrack; 
    }

    void ChangeSceneTrack(int sceneIndex)
    {
        AudioClip newMusic = null;
        AudioClip newAmbience = null;

        // Changes tracks depending on level type
        if (sceneIndex > 3)
        {
            // Mountains Music
            newMusic = levelSounds[1].musicClip;
            newAmbience = levelSounds[1].ambienceClip;
        } else if (sceneIndex > 0)
        {
            // Paddock Music
            newMusic = levelSounds[0].musicClip;
            newAmbience = levelSounds[0].ambienceClip;
        } else 
        {
            // Main Menu
            newMusic = levelSounds[2].musicClip;
        }

        // Only switches if different
        if (currentGameMusic != newMusic)
        {
            currentGameMusic = newMusic;
            currentAmbience = newAmbience;
            StartCoroutine(WaitForFade(currentGameMusic));
            
            ambienceSource.clip = currentAmbience;
            ambienceSource.Play();
        }
    }

    void ChangeTrack(GameManager.GameState state)
    {
        // Smoothly fades between states when states are changed
        if (stateActions.TryGetValue(state, out var action))
        {
            // Only fades the music if the player pauses then resumes (no need to switch tracks)
            if (justPaused && state == GameManager.GameState.Playing)
            {
                justPaused = false;
                fader.StartFade(musicSource.volume, maxMusicVolume, true);

                return;
            } else justPaused = false;
            if (state == GameManager.GameState.Paused)
                justPaused = true;
            
            action.Invoke();
        }
    }

    // Waits for fade before switching tracks
    private IEnumerator WaitForFade(AudioClip clip)
    {
        // Fades out
        fader.StartFade(musicSource.volume, 0f, false);
        yield return new WaitForSeconds(fader.fadeDuration);

        currentGameMusic = clip;
        musicSource.clip = clip;
        musicSource.Play();

        // Fades in
        fader.StartFade(0f, maxMusicVolume, false);
    }

    // Instantiates an object to play a sound effect
    public void PlayAudio(AudioClip clip, float volume, Transform parent, float spatialBlend = 1)
    {
        AudioSource source = Instantiate(audioPrefab, parent).GetComponent<AudioSource>();

        // Determines whether the sound is 2D or 3D
        source.spatialBlend = spatialBlend;
        source.PlayOneShot(clip, volume);
    }

    public void CreateSoundBubble(float radius)
    {
        // Creates a sphere, checking if audio enemy is within, if so they will invesitigate the sound
        Collider[] targets = Physics.OverlapSphere(player.position, radius, audioEnemyLayer);
        foreach (Collider target in targets)
        {
            AudioEnemy enemy = target.gameObject.GetComponentInParent<AudioEnemy>();
            enemy.Investigate();
        }
    }
}
