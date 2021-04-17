using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHorn : MonoBehaviour {

	[Tooltip("delay in secs")]
	public float firstHornDelay = .5f;
	[Tooltip ("delay in secs")]
	public float secondHornDelay = 6f;
	[Tooltip("fluctuation in secs")]
	public float hornVariation = .5f;
	
	private AudioSource hornAudio;
	private AudioLowPassFilter hornFilter;
	
	private float hornTimer;
	private int lastHorn;
	private float currentVariation;
	
	public float trainRadius;
	private bool paused;

	void Awake () {

		hornAudio = GetComponent<AudioSource> ();
		hornFilter = GetComponent<AudioLowPassFilter> ();
		
		// get world-space train radius
		var ccol = GetComponent<CapsuleCollider> ();
		trainRadius = transform.TransformVector (Vector3.right * ccol.radius).magnitude;
	}

	private void OnEnable () {

		hornTimer = 0;
		lastHorn = 0;
		currentVariation = Random.Range (-hornVariation, hornVariation);
	}

	void Update () {

		// check pause state
		if (Time.timeScale <= 0) {
			if (!paused) {
				paused = true;
				if (hornAudio.isPlaying) hornAudio.Pause ();
			}
			return;
		}
		else {
			if (paused) {
				paused = false;
				hornAudio.UnPause ();
			}
		}

		hornTimer += Time.deltaTime;
		if (lastHorn == 0 && hornTimer > firstHornDelay + currentVariation) {
			// sound first horn
			lastHorn++;
			hornFilter.enabled = true;
			hornAudio.Play ();
			currentVariation = Random.Range (-hornVariation, hornVariation);
		}
		else if (lastHorn == 1 && hornTimer > secondHornDelay + currentVariation) {
			// sound second horn
			lastHorn++;
			hornFilter.enabled = false;
			hornAudio.Play ();
		}
	}
}
