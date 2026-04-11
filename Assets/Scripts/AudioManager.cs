using System;
using UnityEngine;

/// <summary>
/// Procedurally generates all game sounds — no audio files needed.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        sfxSource   = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop   = true;
        musicSource.volume = 0.25f;

        // Start ambient royal music
        musicSource.clip = GenerateAmbientMusic();
        musicSource.Play();
    }

    // ── PUBLIC SOUND TRIGGERS ─────────────────────────────────────────────

    public void PlayIntroFanfare()  => PlayClip(GenerateFanfare(), 0.7f);
    public void PlayCorrect()       => PlayClip(GenerateCorrect(), 0.8f);
    public void PlayWrong()         => PlayClip(GenerateWrong(), 0.7f);
    public void PlayCurtainOpen()   => PlayClip(GenerateCurtainWhoosh(), 0.6f);
    public void PlayDeath()         => PlayClip(GenerateDeath(), 0.8f);
    public void PlayWin()           => PlayClip(GenerateWin(), 0.8f);
    public void PlayButtonClick()   => PlayClip(GenerateClick(), 0.4f);
    public void PlayKingLaugh()     => PlayClip(GenerateLaugh(), 0.6f);

    void PlayClip(AudioClip clip, float volume = 1f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    // ── SOUND GENERATORS ─────────────────────────────────────────────────

    // Short royal fanfare — ascending notes
    AudioClip GenerateFanfare()
    {
        int sr = 44100;
        float dur = 1.2f;
        var data = new float[(int)(sr * dur)];
        float[] notes = { 261.6f, 329.6f, 392f, 523.2f }; // C E G C
        int noteSamples = data.Length / notes.Length;
        for (int n = 0; n < notes.Length; n++)
        {
            float freq = notes[n];
            for (int i = 0; i < noteSamples; i++)
            {
                int idx = n * noteSamples + i;
                float t = (float)i / sr;
                float env = Mathf.Clamp01(t * 8f) * Mathf.Clamp01(((float)noteSamples / sr - t) * 6f);
                data[idx] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.6f;
                // Add harmonic
                data[idx] += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * env * 0.2f;
            }
        }
        return MakeClip("fanfare", data, sr);
    }

    // Happy ascending ding
    AudioClip GenerateCorrect()
    {
        int sr = 44100;
        float dur = 0.6f;
        var data = new float[(int)(sr * dur)];
        float[] notes = { 523.2f, 659.2f };
        int ns = data.Length / 2;
        for (int n = 0; n < 2; n++)
        {
            float freq = notes[n];
            for (int i = 0; i < ns; i++)
            {
                int idx = n * ns + i;
                float t = (float)i / sr;
                float env = Mathf.Clamp01(t * 20f) * Mathf.Exp(-t * 5f);
                data[idx] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.7f;
            }
        }
        return MakeClip("correct", data, sr);
    }

    // Low descending buzzer
    AudioClip GenerateWrong()
    {
        int sr = 44100;
        float dur = 0.7f;
        var data = new float[(int)(sr * dur)];
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / sr;
            float freq = Mathf.Lerp(220f, 100f, t / dur);
            float env = Mathf.Clamp01(t * 10f) * Mathf.Clamp01((dur - t) * 6f);
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.5f;
            // Add growl
            data[i] += (UnityEngine.Random.value - 0.5f) * env * 0.15f;
        }
        return MakeClip("wrong", data, sr);
    }

    // Whooshing sweep
    AudioClip GenerateCurtainWhoosh()
    {
        int sr = 44100;
        float dur = 1.4f;
        var data = new float[(int)(sr * dur)];
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / sr;
            float env = Mathf.Clamp01(t * 3f) * Mathf.Clamp01((dur - t) * 2f);
            float noise = (UnityEngine.Random.value - 0.5f);
            float sweep = Mathf.Sin(2 * Mathf.PI * Mathf.Lerp(800f, 200f, t / dur) * t);
            data[i] = (noise * 0.4f + sweep * 0.3f) * env;
        }
        return MakeClip("whoosh", data, sr);
    }

    // Ominous death drum + descending tone
    AudioClip GenerateDeath()
    {
        int sr = 44100;
        float dur = 2.0f;
        var data = new float[(int)(sr * dur)];
        // Drum hits
        float[] drumTimes = { 0f, 0.3f, 0.55f, 0.75f };
        foreach (float dt in drumTimes)
        {
            int start = (int)(dt * sr);
            for (int i = 0; i < (int)(0.2f * sr) && start + i < data.Length; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Exp(-t * 20f);
                data[start + i] += (UnityEngine.Random.value - 0.5f) * env * 0.8f;
                data[start + i] += Mathf.Sin(2 * Mathf.PI * 60f * t) * env * 0.5f;
            }
        }
        // Descending tone
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / sr;
            float freq = Mathf.Lerp(400f, 80f, t / dur);
            float env = Mathf.Clamp01(t * 2f) * Mathf.Clamp01((dur - t) * 1.5f) * 0.3f;
            data[i] += Mathf.Sin(2 * Mathf.PI * freq * t) * env;
        }
        return MakeClip("death", data, sr);
    }

    // Triumphant win jingle
    AudioClip GenerateWin()
    {
        int sr = 44100;
        float dur = 1.8f;
        var data = new float[(int)(sr * dur)];
        float[] notes = { 392f, 523.2f, 659.2f, 783.9f, 1046.5f };
        int ns = (int)(sr * 0.3f);
        for (int n = 0; n < notes.Length; n++)
        {
            int start = n * (int)(sr * 0.28f);
            float freq = notes[n];
            for (int i = 0; i < ns && start + i < data.Length; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Clamp01(t * 15f) * Mathf.Exp(-t * 4f);
                data[start + i] += Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.5f;
                data[start + i] += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * env * 0.2f;
            }
        }
        return MakeClip("win", data, sr);
    }

    // Short UI click
    AudioClip GenerateClick()
    {
        int sr = 44100;
        float dur = 0.08f;
        var data = new float[(int)(sr * dur)];
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / sr;
            float env = Mathf.Exp(-t * 60f);
            data[i] = Mathf.Sin(2 * Mathf.PI * 800f * t) * env * 0.5f;
        }
        return MakeClip("click", data, sr);
    }

    // Silly laugh (tremolo wobble)
    AudioClip GenerateLaugh()
    {
        int sr = 44100;
        float dur = 1.0f;
        var data = new float[(int)(sr * dur)];
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / sr;
            float tremolo = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 8f * t);
            float env = Mathf.Clamp01(t * 5f) * Mathf.Clamp01((dur - t) * 3f);
            data[i] = Mathf.Sin(2 * Mathf.PI * 320f * t) * tremolo * env * 0.5f;
        }
        return MakeClip("laugh", data, sr);
    }

    // Ambient royal loop — slow arpeggiated chords
    AudioClip GenerateAmbientMusic()
    {
        int sr = 44100;
        float dur = 8f;
        var data = new float[(int)(sr * dur)];
        float[] chord = { 130.8f, 164.8f, 196f, 261.6f }; // C major
        float noteLen = 0.8f;
        int totalNotes = (int)(dur / noteLen);
        for (int n = 0; n < totalNotes; n++)
        {
            float freq = chord[n % chord.Length];
            int start = (int)(n * noteLen * sr);
            int len   = (int)(noteLen * sr);
            for (int i = 0; i < len && start + i < data.Length; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Clamp01(t * 4f) * Mathf.Clamp01((noteLen - t) * 2f);
                data[start + i] += Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.15f;
                data[start + i] += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * env * 0.05f;
            }
        }
        return MakeClip("ambient", data, sr, loop: true);
    }

    AudioClip MakeClip(string name, float[] data, int sampleRate, bool loop = false)
    {
        var clip = AudioClip.Create(name, data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
