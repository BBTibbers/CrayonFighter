using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BreathingSystemDemo : MonoBehaviour
{
    public float BreathValue = 0;
    [Range(2, 6)]
    public float BreathsFrequencyMultiplier = 2;
    public AudioClip[] InhaleNoseLvl1;
    public AudioClip[] InhaleNoseLvl2;
    public AudioClip[] InhaleMouthLvl1;
    public AudioClip[] InhaleMouthLvl2;
    public AudioClip[] InhaleMouthLvl3;
    public AudioClip[] ExhaleNoseLvl1;
    public AudioClip[] ExhaleNoseLvl2;
    public AudioClip[] ExhaleMouthLvl1;
    public AudioClip[] ExhaleMouthLvl2;
    public AudioClip[] ExhaleMouthLvl3;
    public GameObject[] Lungs;

    private AudioSource m_as;
    private bool m_IsInhale = true;
    private AudioClip[][] m_aSources;

    private void Start()
    {
        m_as = GetComponent<AudioSource>();
        m_aSources = new AudioClip[][] { InhaleNoseLvl1, InhaleNoseLvl2, InhaleMouthLvl1, InhaleMouthLvl2, InhaleMouthLvl3, ExhaleNoseLvl1, ExhaleNoseLvl2, ExhaleMouthLvl1, ExhaleMouthLvl2, ExhaleMouthLvl3 };
    }

    void Update()
    {
        BreathValue = Mathf.Lerp(BreathValue, Mathf.Sin(Time.time * BreathsFrequencyMultiplier), Time.deltaTime); // InHale on positive values, ExHale on negative values. I used Lerp to smooth the transition when changing BreathsFrequencyMultiplier
        float scaleValue = BreathValue * 0.1f + 1; //Scale value from ~0.9 to ~1.1
        foreach (GameObject lung in Lungs)
        {
            lung.transform.localScale = Vector3.one * scaleValue;//scale the lungs while breathing
            Color baseCol = lung.GetComponent<Renderer>().material.color;
            baseCol.r = (BreathValue + 1) * 0.2f + 0.6f; // red value of color between ~0.4 and ~1;
            lung.GetComponent<Renderer>().material.color = baseCol;
        }
        PlayBreathSound();
    }

    AudioClip GetRandomAudioClip(AudioClip[][] clipArray, int index)
    {
        return clipArray[index][Random.Range(0, clipArray[index].Length)];
    }

    void PlayBreathSound()
    {
        int breathIndex = 0;
        breathIndex = (int)BreathsFrequencyMultiplier - 2;
        if (BreathValue > 0 && m_IsInhale)
        {
            m_as.clip = GetRandomAudioClip(m_aSources, breathIndex);
            m_as.Play();
            m_IsInhale = !m_IsInhale;
        }

        if (BreathValue <= 0 && !m_IsInhale)
        {
            m_as.clip = GetRandomAudioClip(m_aSources, (int)(breathIndex + m_aSources.Length * 0.5f));
            m_as.Play();
            m_IsInhale = !m_IsInhale;
        }
    }

    float RemapValue(float value, float MinValIn, float MaxValIn, float MinValOut, float MaxValOut)
    {
        return MinValOut + (value - MinValIn) * (MaxValOut - MinValOut) / (MaxValIn - MinValIn);
    }
}
