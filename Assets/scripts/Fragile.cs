using UnityEngine;
using System.Collections;

public class Fragile : MonoBehaviour {

	// interval at which to check if we fell
	public const float checkInterval = .5f;
	private float checkTimer;

	// y below which we are considered broken
	private const float yFall = 10f;
	// y below which we immediately delete (should be less than yFall)
	private const float yDelete = 0f;
	
	public GameObject player;
	[Tooltip("message to show when broken")]
	public string textOnBreak;
	[Tooltip("score to add when broken")]
	public int scoreOnBreak;

	private PlayerScore score;
	private bool broken;
	private Vector3 myup;

	void Awake () {
	
		score = player.GetComponent<PlayerScore> ();
		checkTimer = checkInterval * Random.value;
		myup = transform.up;
	}
	
	void Update () {

		// skip if game paused
		if (Time.timeScale <= 0) return;
		if (broken) {
			if (transform.position.y < yDelete) Destroy (gameObject);
			return;
		}

		checkTimer -= Time.deltaTime;
		if (checkTimer > 0) return;

		checkTimer += checkInterval;
		// if we fell off or toppled over, mark as broken and add score
		if (transform.position.y < yFall || Vector3.Angle (transform.up, myup) > 45f) {
			broken = true;
			score.AddScore (textOnBreak, scoreOnBreak, 1);
		}
	}
}
