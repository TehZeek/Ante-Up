using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public bool muteAudio = false;
    private AudioMan audioMan;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioMan = GetComponentInParent<AudioMan>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
