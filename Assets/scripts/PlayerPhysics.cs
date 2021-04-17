using UnityEngine;
using System.Collections;

public class PlayerPhysics : MonoBehaviour {

	[Tooltip ("Force to apply when walking, see also velocityStep")]
	public float[] walkForce;
	[Tooltip ("Velocity above which the respective walkForce will be applied when moving")]
	public float[] velocityStep;
	[Tooltip ("friction while standing")]
	public float standFriction = .8f;
	private bool[] moved;

	private Rigidbody rb;
	private Player player;

	// collision relative velocities for bump/pain (used for score deduction)
	public const float painVelocity = 12;
	public const float bumpVelocity = 8;

	void Awake () {

		if (walkForce.Length != velocityStep.Length) {
			Debug.LogError ("walkForce and velocityStep arrays must be of same size");
		}
		rb = GetComponent<Rigidbody> ();
		moved = new bool[3];

		player = GetComponent<Player> ();
	}

	void FixedUpdate () {

		// skip if game paused
		if (Time.timeScale <= 0) return;

		// dynamic friction
		Friction (moved[0], moved[2]);

		for (int i = 0; i < moved.Length; i++)
			moved[i] = false;
	}

	public void Force (Vector3 vec) {

		// skip if fell off
		if (transform.position.y < player.yFall) return;

		Vector3 force = Vector3.zero;
		int i = 0;

		// update motions
		if (Mathf.Abs (vec.x) > .1f)
			moved[0] = true;
		if (Mathf.Abs (vec.y) > .1f)
			moved[1] = true;
		if (Mathf.Abs (vec.z) > .1f)
			moved[2] = true;

		// X
		for (i = 0; i < walkForce.Length; i++) {
			if (Mathf.Abs (rb.velocity.x) < velocityStep[i] || i == walkForce.Length - 1)
				break;
		}
		force.x = -vec.x * walkForce[i];

		// Y
		for (i = 0; i < walkForce.Length; i++) {
			if (Mathf.Abs (rb.velocity.y) < velocityStep[i] || i == walkForce.Length - 1)
				break;
		}
		force.y = vec.y * walkForce[i];

		// Z
		for (i = 0; i < walkForce.Length; i++) {
			if (Mathf.Abs (rb.velocity.z) < velocityStep[i] || i == walkForce.Length - 1)
				break;
		}
		force.z = -vec.z * walkForce[i];

		// apply
		rb.AddForce (force);

	}

	public void ForceRaw (Vector3 vec) {

		vec.x = -vec.x;
		vec.z = -vec.z;
		rb.AddForce (vec * walkForce[0]);
	}

	void Friction (bool atX, bool atZ) {

		if (atX && atZ) return;

		Vector3 vel = rb.velocity;
		if (!atX) vel.x *= standFriction;
		if (!atZ) vel.z *= standFriction;
		rb.velocity = vel;
	}

	public void SetVelocity (Vector3 vel) {
		rb.velocity = vel;
	}

	void OnCollisionEnter (Collision collision) {

		if (collision.gameObject.layer == 10) { // car
			float sqrmag = collision.relativeVelocity.sqrMagnitude;
			if (sqrmag > painVelocity * painVelocity) {
				player.CarHit (1);
			}
			else if (sqrmag > bumpVelocity * bumpVelocity) {
				player.CarHit ();
			}
		}
		else if (player.fell == 0 && collision.gameObject.layer == 18) { // train
			float sqrmag2 = collision.relativeVelocity.sqrMagnitude;
			if (sqrmag2 > bumpVelocity * bumpVelocity) {
				player.CarHit (2);
			}
		}
	}
}
