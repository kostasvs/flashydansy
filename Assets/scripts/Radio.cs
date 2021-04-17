using UnityEngine;
using System.Collections;

public class Radio : MonoBehaviour {

	[Tooltip ("score duration in secs")]
	public float songDuration;
	[HideInInspector]
	public static bool playing;

	private AudioSource au;
	private ParticleSystem prt;

	[Tooltip ("beat duration in secs")]
	public float beatDuration = .49f;
	[Tooltip ("beats to count from song start before game starts")]
	public float beatsStart = 3f;
	[Tooltip ("beat offset to add (does not affect radio animation)")]
	public float beatCorrection;
	[HideInInspector]
	public float playTime;

	[Tooltip ("animation scaleup")]
	public float grow = .25f;
	[Tooltip ("animation curve scale")]
	public float curveScale = 2f;

	[HideInInspector]
	public float beats;
	private bool paused;

	void Start () {

		au = GetComponent<AudioSource> ();
		prt = GetComponent<ParticleSystem> ();
		//PlayMusic ();
	}

	void Update () {

		// check for paused state
		if (Time.timeScale <= 0) {
			if (!paused) {
				paused = true;
				if (au.isPlaying) au.Pause ();
			}
			return;
		}
		else {
			if (paused) {
				paused = false;
				au.UnPause ();
			}
		}

		// end song after songDuration
		if (playing && Time.time - playTime > songDuration) {
			playing = false;
			prt.Stop ();
		}

		// calculate beats via time
		beats = (Time.time - playTime) / beatDuration;
		// animate
		if (playing) {
			float sc = 1f;
			if (playing && beats >= beatsStart) sc += grow * Curve (BeatFraction ());
			transform.localScale = Vector3.one * sc;
		}
		// add correction
		beats += beatCorrection;
	}

	public void PlayMusic () {

		playing = true;
		prt.Play ();
		if (au.isPlaying) au.Stop ();
		au.Play ();
		playTime = Time.time;
		beats = 0f;
	}

	public float BeatFraction () {
		return beats - Mathf.Floor (beats);
	}

	float Curve (float x) {
		x *= curveScale;
		return Mathf.Max (0f, x - x * x);
	}
}
