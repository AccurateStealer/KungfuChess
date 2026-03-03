using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AK.Wwise.Event playMusic;
    public AK.Wwise.Event stopMusic;
    public AK.Wwise.Event playGrunt;
    public AK.Wwise.Event playHit;
    public AK.Wwise.Event playWinMusic;
    public AK.Wwise.Event stopWinMusic;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playMusic.Post(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMusic()
    {
        playMusic.Post(this.gameObject);
    }
    
    public void StopMusic()
    {
        stopMusic.Post(this.gameObject);
    }
    
    public void PlayGrunt()
    {
        playGrunt.Post(this.gameObject);
    }
    
    
    public void PlayHit()
    {
        playHit.Post(this.gameObject);
    }
    
    public void PlayWinMusic()
    {
        playWinMusic.Post(this.gameObject);
    }
    
    public void StopWinMusic()
    {
        stopWinMusic.Post(this.gameObject);
    }
}
