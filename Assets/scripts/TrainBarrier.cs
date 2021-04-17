using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainBarrier : MonoBehaviour {

	public bool open = true;
	[Tooltip ("angle offset ahead of current angle to raycast for obstacles on")]
	public float checkAheadAngle = 10f;
	private float barRadius;
	private float barVectorMag;

	private CapsuleCollider col;
	public const float xRotOpen = -90f;
	public const float xRotClosed = 0;
	public float rotSpeed = 90f;

	private LayerMask lmask;
	//private Rigidbody rb;

	[Tooltip ("time before rechecking after an obstacle was met")]
	public float holdInterval = .4f;
	private float holdTimer;

	[Tooltip ("max times that an obstacle will be found before going up to unblock")]
	public int blockedMax = 3;
	private int blockedCount;
	[Tooltip ("duration of going up to unblock, before coming down again")]
	public float unblockDuration = .75f;
	private float unblockTimer;

	void Start () {

		col = GetComponent<CapsuleCollider> ();
		lmask = LayerMask.GetMask ("Player", "Car");
		barRadius = transform.TransformVector (Vector3.forward * col.radius).magnitude;
		barVectorMag = transform.TransformVector (Vector3.down * (col.height - col.radius * 2))
			.magnitude;
	}

	/*private void Update () {

		if (Input.GetKeyDown (KeyCode.B)) open = !open;
	}*/

	void FixedUpdate () {

		if (Time.timeScale == 0) return;

		// check if opening (normally or to unblock)
		bool tryopen = open;
		if (unblockTimer > 0) {
			unblockTimer -= Time.deltaTime;
			tryopen = true;
		}
		if (holdTimer > 0) {
			// skip until holdTimer ends
			holdTimer -= Time.deltaTime * (tryopen ? 2f : 1f);
			return;
		}

		// find current angle
		Vector3 leu = transform.localEulerAngles;
		float myrot = leu.x;
		if (myrot > 180) myrot -= 360;
		if (tryopen && (myrot <= xRotOpen || leu.y == 180 || leu.z == 180)) return;
		if (!tryopen && myrot >= xRotClosed) return;

		// get delta to apply
		float delta = tryopen ? -rotSpeed * Time.deltaTime : rotSpeed * Time.deltaTime;
		if (tryopen && myrot + delta < xRotOpen) delta = xRotOpen - myrot;
		else if (!tryopen && myrot + delta > xRotClosed) delta = xRotClosed - myrot;

		// raycast vector
		Vector3 rcast = Quaternion.Euler (delta + checkAheadAngle * Mathf.Sign (delta), 0, 0)
			* transform.TransformVector (Vector3.down * (col.height - col.radius * 2));
		Vector3 rorigin = transform.TransformPoint (Vector3.right * col.center.x);

		// preform raycast
		Ray ray = new Ray (rorigin, rcast);
		if (!Physics.SphereCast (ray, barRadius, barVectorMag, lmask)) {
			// no obstacles seen, rotate normally
			transform.Rotate (delta, 0, 0);
			blockedCount = 0;
			Debug.DrawLine (rorigin, rorigin + rcast, Color.blue, .1f, false);
		}
		else {
			// obstacles found, wait for holdInterval
			holdTimer = holdInterval;
			if (!open) {
				// increment blockedCount
				blockedCount++;
				if (blockedCount >= blockedMax) {
					// trigger unblocking
					blockedCount = 0;
					unblockTimer = unblockDuration;
				}
			}
			Debug.DrawLine (rorigin, rorigin + rcast, Color.red, .1f, false);
		}
	}
}
