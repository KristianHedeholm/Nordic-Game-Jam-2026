using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    private AudioSource sfxSource;
    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private AudioClip kingSpeechClip;
    
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── PUBLIC SOUND TRIGGERS ─────────────────────────────────────────────
    public void PlayCorrect()       => PlayClip(GenerateCorrect(), 0.8f);
    public void PlayWrong()         => PlayClip(GenerateWrong(), 0.7f);
    public void PlayCurtainOpen()   => PlayClip(GenerateCurtainWhoosh(), 0.6f);
    public void PlayDeath()         => PlayClip(GenerateDeath(), 0.8f);
    public void PlayWin()           => PlayClip(GenerateWin(), 0.8f);
    public void PlayButtonClick()   => PlayClip(GenerateClick(), 0.4f);
    public void PlayKingLaugh()     => PlayClip(GenerateLaugh(), 0.6f);
    public void PlayKingTalk()
    {
	    if (kingSpeechClip != null)
	    {
		    PlayClip(kingSpeechClip, 1.0f);
	    }
	    else
	    {
		    UnityEngine.Debug.LogError("kingSpeechClip is null");
	    }
    }

    public void StopKingTalk()
    {
        // Fade out sfxSource quickly if king speech is playing
        StartCoroutine(FadeOutSFX(0.2f));
    }
    
    IEnumerator FadeOutSFX(float duration)
    {
        float startVol = sfxSource.volume;
        float elapsed  = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sfxSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }
        sfxSource.Stop();
        sfxSource.volume = startVol;
    }
    public void PlayTadaaa()        => PlayClip(GenerateTadaaa(), 0.9f);

    public void PlayDrumrollThenReveal(Action onReveal)
        => StartCoroutine(DrumrollRoutine(onReveal));

    void PlayClip(AudioClip clip, float volume = 1f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    // ── COROUTINE ─────────────────────────────────────────────────────────

    IEnumerator DrumrollRoutine(Action onReveal)
    {
        PlayClip(GenerateDrumroll(), 0.85f);
        yield return new WaitForSeconds(2.5f);   // drumroll duration
        PlayTadaaa();
        yield return new WaitForSeconds(0.3f);
        onReveal?.Invoke();
    }

    // ── SOUND GENERATORS ─────────────────────────────────────────────────

    // Short royal fanfare — ascending notes
    /*AudioClip GenerateFanfare()
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
    }*/

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
    /*AudioClip GenerateAmbientMusic()
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
    }*/

    // Accelerating snare drumroll building to a crescendo
    AudioClip GenerateDrumroll()
    {
        int sr = 44100;
        float dur = 2.5f;
        var data = new float[(int)(sr * dur)];

        // Accelerating hit intervals: start at 0.3s apart, end at 0.04s
        float t = 0f;
        float interval = 0.30f;
        while (t < dur)
        {
            // Progress 0→1
            float progress = t / dur;
            // Each hit: short snare burst
            int start = (int)(t * sr);
            int hitLen = (int)(0.05f * sr);
            float vol = 0.4f + progress * 0.5f; // gets louder
            for (int i = 0; i < hitLen && start + i < data.Length; i++)
            {
                float tt = (float)i / sr;
                float env = Mathf.Exp(-tt * 80f);
                data[start + i] += (UnityEngine.Random.value - 0.5f) * env * vol;
                data[start + i] += Mathf.Sin(2 * Mathf.PI * 180f * tt) * env * vol * 0.4f;
            }
            interval = Mathf.Lerp(0.30f, 0.035f, progress);
            t += interval;
        }
        return MakeClip("drumroll", data, sr);
    }

    // Big triumphant TADAAAA
    AudioClip GenerateTadaaa()
    {
        int sr = 44100;
        float dur = 1.5f;
        var data = new float[(int)(sr * dur)];
        // Brass-like chord: C E G
        float[] freqs = { 523.2f, 659.2f, 783.9f, 1046.5f };
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / sr;
            float env = Mathf.Clamp01(t * 15f) * Mathf.Clamp01((dur - t) * 2f);
            foreach (float f in freqs)
            {
                data[i] += Mathf.Sin(2 * Mathf.PI * f * t) * env * 0.18f;
                data[i] += Mathf.Sin(2 * Mathf.PI * f * 2f * t) * env * 0.06f;
            }
        }
        return MakeClip("tadaaa", data, sr);
    }

    // Onion King style: wobbly warbling voice sound (Overcooked-inspired)
    AudioClip GenerateOnionKingTalk()
    {
        int sr = 44100;
        float dur = 0.4f + UnityEngine.Random.value * 0.4f; // variable length each time
        var data = new float[(int)(sr * dur)];
        float baseFreq = 180f + UnityEngine.Random.value * 80f;
        float wobbleRate = 6f + UnityEngine.Random.value * 4f;
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / sr;
            float env = Mathf.Clamp01(t * 20f) * Mathf.Clamp01((dur - t) * 10f);
            float wobble = 1f + 0.18f * Mathf.Sin(2 * Mathf.PI * wobbleRate * t);
            float freq = baseFreq * wobble;
            // Voiced sound with harmonics (vocalish)
            data[i]  = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.4f;
            data[i] += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.2f;
            data[i] += Mathf.Sin(2 * Mathf.PI * freq * 3f * t) * 0.1f;
            data[i] += Mathf.Sin(2 * Mathf.PI * freq * 4f * t) * 0.05f;
            data[i] *= env * 0.55f;
        }
        return MakeClip("kingtalk", data, sr);
    }

    // Crowd reaction — many overlapping voices
    /*AudioClip GenerateCrowdCheer(bool cheer)
    {
        int sr = 44100;
        float dur = 1.6f;
        var data = new float[(int)(sr * dur)];

        // Simulate ~30 individual voices
        int voices = 30;
        for (int v = 0; v < voices; v++)
        {
            float baseFreq = cheer
                ? 300f + UnityEngine.Random.value * 400f   // higher excited voices for cheer
                : 120f + UnityEngine.Random.value * 150f;  // lower grumbling voices for boo

            float vibRate  = 4f + UnityEngine.Random.value * 6f;
            float vibDepth = 0.05f + UnityEngine.Random.value * 0.15f;
            float delay    = UnityEngine.Random.value * 0.15f; // stagger entry
            float vol      = 0.025f + UnityEngine.Random.value * 0.02f;

            int startSample = (int)(delay * sr);
            for (int i = startSample; i < data.Length; i++)
            {
                float t   = (float)(i - startSample) / sr;
                float tNorm = t / (dur - delay);

                float env;
                if (cheer)
                    // Cheer: quick attack, sustained, fast decay
                    env = Mathf.Clamp01(t * 12f) * Mathf.Clamp01((dur - delay - t) * 3f);
                else
                    // Boo: slower build, long hold, gradual fade
                    env = Mathf.Clamp01(t * 5f) * Mathf.Clamp01((dur - delay - t) * 2f);

                float vib  = 1f + vibDepth * Mathf.Sin(2 * Mathf.PI * vibRate * t);
                float freq = baseFreq * vib;

                // Voice: fundamental + harmonics (gives vowel-like quality)
                float sample  = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.5f;
                sample       += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.25f;
                sample       += Mathf.Sin(2 * Mathf.PI * freq * 3f * t) * 0.12f;
                sample       += Mathf.Sin(2 * Mathf.PI * freq * 4f * t) * 0.06f;

                // Add a little breathiness
                sample += (UnityEngine.Random.value - 0.5f) * 0.08f;

                data[i] += sample * env * vol;
            }
        }

        // Normalise to prevent clipping
        float peak = 0f;
        foreach (float s in data) peak = Mathf.Max(peak, Mathf.Abs(s));
        if (peak > 0.9f)
            for (int i = 0; i < data.Length; i++) data[i] /= peak / 0.9f;

        return MakeClip(cheer ? "cheer" : "boo", data, sr);
    }*/

    AudioClip MakeClip(string name, float[] data, int sampleRate)
    {
        var clip = AudioClip.Create(name, data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
