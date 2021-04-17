using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraFollow : MonoBehaviour {

	// target & sensitivity
	public GameObject whoToFollow;
	public float sensitivity;

	// initial zoom out
	public float zoomOut = 1.25f;
	public float zoomOutTime = 5f;

	private Transform followT;
	private Vector3 offset;
	private readonly Vector3[] offsetBase = new Vector3[2];

	private float ylock;
	private readonly float[] ylockBase = new float[2];

	private Player player;
	private PlayerDance dance;

	void Awake () {

		// get follow transform
		followT = whoToFollow.transform;

		// get initial and final follow offset
		offsetBase[0] = transform.position - followT.position;
		offsetBase[1] = offsetBase[0] * zoomOut;
		offset = offsetBase[0];

		// get initial and final ylock
		ylockBase[0] = transform.position.y;
		ylockBase[1] = followT.position.y + offset.y * zoomOut;
		ylock = ylockBase[0];

		// get player components
		if (followT) {
			player = followT.GetComponent<Player> ();
			dance = followT.GetComponent<PlayerDance> ();
		}
	}

	void FixedUpdate () {

		// initial zoom out
		if (Time.time < zoomOutTime * 2f) {
			offset = Vector3.Lerp (offsetBase[0], offsetBase[1], Time.time / zoomOutTime);
			ylock = Mathf.Lerp (ylockBase[0], ylockBase[1], Time.time / zoomOutTime);
		}

		// in stage 2, don't follow player when hit by train
		if (player && player.fell > 0 && dance && dance.style == 1) return;

		// smooth follow
		Vector3 npos = followT.position + offset;
		npos.y = ylock;
		transform.position += sensitivity * (npos - transform.position);
	}
}
