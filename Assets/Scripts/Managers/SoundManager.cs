using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    struct DebugSound
    {
        public Vector3 position;
        public float radius;
        public float timeCreated;
    }
    private List<DebugSound> debugSounds = new List<DebugSound>();
    private float debugDuration = 0.5f;

    public static SoundManager Instance;

    [SerializeField] private LayerMask audioEnemyLayer;
    
    
    
    private Transform player;
    

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
        }
    }

    void Update()
    {
        if (!player) player = FindObjectOfType<PlayerController>().transform;
    }

    public void CreateSoundBubble(float radius)
    {
        debugSounds.Add(new DebugSound
        {
            position = player.position,
            radius = radius,
            timeCreated = Time.time
        });

        // Creates a sphere, checking if audio enemy is within, if so they will invesitigate the sound
        Collider[] targets = Physics.OverlapSphere(player.position, radius, audioEnemyLayer);
        foreach (Collider target in targets)
        {
            AudioEnemy enemy = target.gameObject.GetComponentInParent<AudioEnemy>();
            enemy.Investigate();
        }
    }

    private void OnDrawGizmos()
    {
        if (debugSounds == null) return;

        for (int i = debugSounds.Count - 1; i >= 0; i--)
        {
            if (Application.isPlaying && Time.time - debugSounds[i].timeCreated > debugDuration)
            {
                debugSounds.RemoveAt(i);
                continue;
            }

            Gizmos.color = Color.Lerp(Color.green, Color.red, debugSounds[i].radius / 20f);
            Gizmos.DrawWireSphere(debugSounds[i].position, debugSounds[i].radius);
        }
    }
}
