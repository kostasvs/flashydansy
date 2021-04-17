using UnityEngine;
using System.Collections;

public class CarSpawn : MonoBehaviour {

	[Tooltip("Prefab to spawn")]
	public GameObject prefab;
	[Tooltip("average time in secs between spawns (if area clear)")]
	public float timeClear = 3f;
	[Tooltip("spawn interval fluctuation in secs")]
	public float timeClearFluctuate = 2f;
	private float timer;

	void Awake () {
		RandomizeTimer ();
	}

	void Update () {

		if (timer <= 0f) {
			if (Radio.playing) Instantiate (prefab, transform.position, transform.localRotation);
			RandomizeTimer ();
		}
		timer -= Time.deltaTime;

	}

	void OnTriggerEnter () {
		RandomizeTimer ();
	}
	void OnTriggerStay () {
		RandomizeTimer ();
	}

	void RandomizeTimer () {
		timer = timeClear + Random.Range (-timeClearFluctuate, timeClearFluctuate);
	}

}
