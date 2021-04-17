using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainControl : MonoBehaviour {

	public Rigidbody train;
	private Vector3 trainStartPos;
	public float trainVelocity = 15f;
	public float trainAcceleration = 1f;
	public float trainDelayStart = 3f;
	private float trainTimer;
	[Tooltip ("Z beyond which train disappears")]
	public float trainCutoffZ = 250f;
	[Tooltip ("Z beyond which bars can reopen, should be < trainCutoffZ")]
	public float trainReopenZ = 170f;
	private bool shouldReopen = false;

	public Transform[] trafficLight;
	private AudioSource[] trafficLightBell;
	public TrainBarrier[] barrier;
	private bool open = true;

	public const float trafficLightInterval = .5f;
	private bool trafficLightLastState;
	private float trafficLightLastToggle;

	public AudioSource trainPass;
	[Tooltip ("trainPass audio max heard dist (with min volume)")]
	public float trainPassMaxDist;
	[Tooltip ("trainPass audio min heard dist (with max volume)")]
	public float trainPassMinDist;
	private float trainPassMaxVol;
	//public float testOutput;

	[Tooltip ("train pass interval in secs")]
	public float trainInterval = 30f;
	private float trainIntervalTimer;
	private bool paused;

	// max time for which the train is allowed to have less than max speed before disappearing
	public const float holdMaxDelay = 3f;
	private float holdTimer;

	void Awake () {

		trainStartPos = train.transform.position;
		trafficLightBell = new AudioSource[trafficLight.Length];
		for (int i = 0; i < trafficLight.Length; i++)
			trafficLightBell[i] = trafficLight[i].GetComponent<AudioSource> ();
		trainPassMaxVol = trainPass.volume;
		trainIntervalTimer = trainInterval;
	}

	void Update () {

		// check paused state
		if (Time.timeScale <= 0) {
			if (!paused) {
				paused = true;
				foreach (var tlb in trafficLightBell) if (tlb && tlb.isPlaying) tlb.Pause ();
				if (trainPass.isPlaying) trainPass.Pause ();
			}
			return;
		}
		else {
			if (paused) {
				paused = false;
				foreach (var tlb in trafficLightBell) if (tlb) tlb.UnPause ();
				trainPass.UnPause ();
			}
		}

		bool shouldClose = false;
		if (open && Radio.playing) {
			// train pass interval
			trainIntervalTimer -= Time.deltaTime;
			if (trainIntervalTimer < 0) {
				// trigger train
				trainIntervalTimer = trainInterval;
				shouldClose = true;
			}
		}
		if (shouldClose || (!open && shouldReopen) /* || Input.GetKeyDown (KeyCode.B)*/) {
			// toggle bars
			open = !open;
			shouldReopen = false;
			foreach (var b in barrier) b.open = open;
			// toggle traffic lights
			if (open) trafficLightLastState = true;
			else trafficLightLastToggle = Time.time;
			foreach (var tl in trafficLight) {
				tl.GetChild (0).gameObject.SetActive (open);
				tl.GetChild (1).gameObject.SetActive (!open && trafficLightLastState);
				tl.GetChild (2).gameObject.SetActive (!open && !trafficLightLastState);
			}
			// toggle bells
			foreach (var tlb in trafficLightBell) if (tlb) {
					if (open) tlb.Stop ();
					else tlb.Play ();
				}
			// toggle train
			//if (open) train.gameObject.SetActive (false);
			if (!open) {
				train.transform.position = trainStartPos;
				train.velocity = Vector3.zero;
				train.gameObject.SetActive (true);
				trainPass.Play ();
				trainPass.volume = 0;
				trainTimer = 0;
			}
		}

		if (!open) {
			// traffic lights
			if (Time.time - trafficLightLastToggle > trafficLightInterval) {
				trafficLightLastToggle = Time.time;
				trafficLightLastState = !trafficLightLastState;
				foreach (var tl in trafficLight) {
					tl.GetChild (1).gameObject.SetActive (trafficLightLastState);
					tl.GetChild (2).gameObject.SetActive (!trafficLightLastState);
				}
			}
			// train delayed start
			if (trainTimer < trainDelayStart) {
				trainTimer += Time.deltaTime;
				if (trainTimer >= trainDelayStart) {
					train.velocity = Vector3.forward * trainVelocity;
				}
			}
			//else if (train.velocity.sqrMagnitude < trainVelocity * trainVelocity)
			//	train.AddForce (Vector3.forward * trainAcceleration * Time.deltaTime)
		}

		// disable train and reopen bars if train pass complete or stuck
		if (train.gameObject.activeSelf && (train.position.z > trainReopenZ || holdTimer > holdMaxDelay)) {
			shouldReopen = true;
			if (train.position.z > trainCutoffZ || holdTimer > holdMaxDelay) {
				train.gameObject.SetActive (false);
				trainPass.Stop ();
			}
			holdTimer = 0f;
		}

		// adjust train pass volume
		if (train.gameObject.activeSelf) {
			trainPass.volume = trainPassMaxVol *
				(1f - Mathf.Clamp01 ((Mathf.Abs (train.transform.position.z - trainPass.transform.position.z) - trainPassMinDist)
				/ (trainPassMaxDist - trainPassMinDist)));
			//testOutput = train.transform.position.z - trainPass.transform.position.z;
		}

		//if (Application.isEditor && Input.GetKey (KeyCode.F)) {
		//	train.velocity = Vector3.zero;
		//}
	}

	private void FixedUpdate () {

		// accerelate train and increase holdTimer if train below max speed
		if (train.gameObject.activeSelf && trainTimer > trainDelayStart
			&& train.velocity.sqrMagnitude < trainVelocity * trainVelocity) {
			train.AddForce (Vector3.forward * trainAcceleration * Time.deltaTime,
				ForceMode.Acceleration);
			holdTimer += Time.deltaTime;
		}
		else holdTimer = 0;
	}
}
