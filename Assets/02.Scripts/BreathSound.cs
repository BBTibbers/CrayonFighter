using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BreathSound : MonoBehaviour
{
    public AudioClip[] InhaleMouthLvl1;
    public AudioClip[] InhaleMouthLvl2;
    public AudioClip[] InhaleMouthLvl3;
    public AudioClip[] ExhaleMouthLvl1;
    public AudioClip[] ExhaleMouthLvl2;
    public AudioClip[] ExhaleMouthLvl3;

    private AudioSource m_as;
    private AudioClip[][] m_aSources;

    public static BreathSound Instance;
    private void Start()
    {
        Instance = this;
        m_as = GetComponent<AudioSource>();
        m_aSources = new AudioClip[][] { InhaleMouthLvl1, InhaleMouthLvl2, InhaleMouthLvl3, ExhaleMouthLvl1, ExhaleMouthLvl2, ExhaleMouthLvl3 };
        m_as.volume = 0.2f;
    }


    public void PlayBreathInhale(int health)
    {
        if (health < 0) health = 0;
        if (health > 2) health = 2;
        m_as.clip = m_aSources[health][Random.Range(0, m_aSources[health].Length)];
        m_as.Play();
    }
    public void PlayBreathExhale(int health)
    {
        if (health < 0) health = 0;
        if (health > 2) health = 2;
        m_as.clip = m_aSources[health+3][Random.Range(0, m_aSources[health+3].Length)];
        m_as.Play();
    }


}
