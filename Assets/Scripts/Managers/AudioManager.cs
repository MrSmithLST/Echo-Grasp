using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; //CREATING A SINGLETON

    private void Awake() 
    {
        if(!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);   

        if(bgm.Length <= 0) return; //IF THERE IS ANY MUSIC TO PLAY
        InvokeRepeating(nameof(PlayMusicIfNeeded), 0, 2); //MAKE SURE THAT THE MANAGER CHECKS IF THE MUSIC IS PLAYING EVERY 2 SECONDS
    }

    [Header("Audio Source")]
    [SerializeField] private AudioSource[] sfx; //ARRAY OF AUDIO SOURCES FOR SFX
    [SerializeField] private AudioSource[] bgm; //ARRAY OF AUDIO SOURCES FOR BGM
    private int bgmIndex; //CURNTLY PLAYING BGM INDEX

    //USED TO PLAY SFX ON COMMAND
    public void PlaySFX(int sfxToPlay, bool randomPitch = true)
    {
        if(sfxToPlay >= sfx.Length) return; //IF THE INDEX IS OUT OF BOUNDS THEN RETURN

        if(randomPitch) //USED TO RANDOMIZE THE PITCH OF SOUNDS THAT ARE PLAYING OFTENLY (LIKE JUMPING OR DASHING) TO MAKE THEM LESS REPETITIVE
            sfx[sfxToPlay].pitch = Random.Range(.9f, 1.1f); //RANDOMLY CHANGE THE PITCH OF THE SFX
        sfx[sfxToPlay].Play(); 
    }

    public void PlayMusicIfNeeded()
    {
        if(!bgm[bgmIndex].isPlaying) //EVERY 2 SECONDS IF THERE IS NO BGM PLAYING, THE RANDOM ONE IS BEING PLAYED
            PlayRandomBGM();
    }

    public void PlayRandomBGM()
    {
        bgmIndex = Random.Range(0, bgm.Length);
        PlayBGM(bgmIndex);
    }

    //USED TO PLAY BGM ON COMMAND
    public void PlayBGM(int bgmToPlay)
    {
        if(bgm.Length <= 0) return; //IF THE ARRAY IS EMPTY RETURN

        for (int i = 0; i < bgm.Length; i++) 
        {
            bgm[i].Stop(); //STOP ALL THE MUSIC THAT IS PLAYING
        }
        
        bgmIndex = bgmToPlay;
        bgm[bgmToPlay].Play(); //PLAY THE SELECTED MUSIC
    }

    public void StopSFX(int sfxToStop) => sfx[sfxToStop].Stop();
    
}
