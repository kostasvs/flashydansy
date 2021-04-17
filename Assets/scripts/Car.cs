using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {

	[Tooltip ("Normal acceleration")]
	public float accerelation = 400f;
	[Tooltip ("Speed below which acceleration is boosted")]
	public float lowSpeed = 10f;
	[Tooltip ("Acceleration multiplier when speed < lowSpeed")]
	public float lowSpeedBoost = 1.25f;

	[Tooltip ("Steering speed coefficient")]
	public float steerFactor = 3000f;
	[Tooltip ("Max steering torque to apply")]
	public float maxSteer = 400f;
	[Tooltip ("Min steering torque when steering, [0] for normal, [1] for hard steer")]
	public float[] minSteerSpeed = new float[2];
	[Tooltip ("Max hard steering torque to apply")]
	public float maxHardSteer = 1000f;
	private float hardSteer;
	[Tooltip ("Rate of transitioning into hard steer")]
	public float hardSteerFactor = .15f;
	[Tooltip ("Rate of transitioning out of hard steer")]
	public float hardUnsteerFactor = .025f;

	[Tooltip ("Normal braking rate")]
	public float brake = .9f;
	[Tooltip ("Braking rate when speed > brakeFailSpeed")]
	public float brakeOnFail = .997f;
	[Tooltip ("Speed above which brakeOnFail is used")]
	public float brakeFailSpeed = 10f;

	// Delay before deleting after out of bounds
	private const float deathDelay = 1f;
	private float deathtimer;
	// x limit beyond which we die
	private float xdeath;

	[Tooltip ("y below which we are considered dead")]
	public float yFall = 13f;

	// angle in degrees from upright position beyond which we are considered toppled over
	private const float toppleAngle = 30f;
	// vertical speed above which we are considered falling
	private const float fallSpeed = 2f;

	private Rigidbody rb;

	// list of possible materials to choose from, and respective probability weights
	[Tooltip ("list of possible materials to choose from")]
	public Material[] materials;
	[Tooltip ("respective probability weights for materials array")]
	public float[] materialProb;

	// obstacles to try and avoid (dynamically updated by CarSensor)
	[HideInInspector]
	public HashSet<Transform> avoids = new HashSet<Transform> ();

	// last position where we have been remaining without moving
	private Vector3 holdPos;
	// max time to stay in same position before dying
	private const float holdMaxDelay = 10f;
	private float holdTimer;
	// squared distance that we need to move in order to update holdPos and reset holdTimer
	private const float holdMinDistanceSqr = 1f;
	[Tooltip ("Pos/Neg x limit within which the holdTimer doesn't increase")]
	public float noHoldBelowX;

	// collision relative velocities for crash/bump (used for scores)
	private const float crashVelocity = 14f;
	private const float bumpVelocity = 4f;

	// wheel transforms
	private readonly Transform[] wheelT = new Transform[2];
	[Tooltip ("Max angle to turn wheel when steering")]
	public float wheelTurn = 30f;
	[Tooltip ("child transform on which accerelation force is applied (optional)")]
	public Transform accerelationOrigin;

	[Tooltip ("Whether to enable z-axis course correction")]
	public bool courseCorrection = false;
	[Tooltip ("max z offset to correct")]
	public float maxCourseErrorToCorrect = 8f;
	[Tooltip ("error divider for smoother correction, should be > maxCourseErrorToCorrect")]
	public float maxCourseCorrectionToRequest = 20f;
	// course Z to maintain
	private float courseZ;
	// max difference from original transform.forward.x to correct
	private const float maxHeadingErrorToCorrect = .4f;
	// original transform.forward.x
	private float courseInitial;

	// becomes true when we first touch the ground
	private bool touchdown = false;
	// stores whether we got hit by train
	private bool trainCrashed = false;

	// min interval between crashes (used for score)
	private float crashTimer = 0;
	private const float crashInterval = .5f;

	//public float hsdebug;
	//public float stdebug;
	//public bool hsAllow = true;
	//public bool stAllow = true;

	void Awake () {

		rb = GetComponent<Rigidbody> ();
		xdeath = -transform.position.x;

		// apply random material
		int m = Choose (materialProb);
		Material mat = materials[m];
		MeshRenderer[] rends = GetComponentsInChildren<MeshRenderer> ();
		foreach (MeshRenderer rend in rends)
			rend.sharedMaterial = mat;
		//if (m == materialProb.Length - 1) accerelation *= 1.25f;

		holdPos = transform.position;

		// wheel transforms
		wheelT[0] = transform.Find ("wheelFL");
		wheelT[1] = transform.Find ("wheelFR");

		// course correction setup
		courseZ = transform.position.z;
		//courseZ = (transform.position + transform.right * 4f).z; // for testing
		courseInitial = transform.forward.x;
	}

	void FixedUpdate () {

		// skip if game paused
		if (Time.timeScale <= 0) return;
		// skip if haven't touched ground yet, or if falling
		if (!touchdown || Mathf.Abs (rb.velocity.y) > fallSpeed) return;
		// skip if toppled
		if (Vector3.Angle (transform.up, Vector3.up) > toppleAngle) return;

		// get max steering torque
		float maxsteer = maxSteer;
		float maxhardsteer = maxHardSteer;
		float myvel = rb.velocity.magnitude;
		// if speed < minSteerSpeeds, decrease max steers proportionally
		if (myvel < minSteerSpeed[0]) maxsteer *= myvel / minSteerSpeed[0];
		if (myvel < minSteerSpeed[1]) maxhardsteer *= myvel / minSteerSpeed[1];

		// remove deleted obstacles
		avoids.RemoveWhere (i => !i);

		// check for obstacles
		if (avoids.Count > 0f) {

			// brake
			if (myvel < brakeFailSpeed) rb.velocity *= brake;
			else rb.velocity *= brakeOnFail;

			// get mean pos of all obstacles
			int nonBarriers = 0;
			Vector3 avoidpos = Vector3.zero;
			foreach (Transform tr in avoids) {
				if (tr.gameObject.layer == 17) {
					// don't attempt to steer away from barriers
					continue;
				}
				avoidpos += tr.position;
				nonBarriers++;
			}
			if (nonBarriers > 0) {
				// get mean pos
				avoidpos /= nonBarriers;
				if (avoidpos != transform.position) {

					// hard steer to avoid mean pos
					Quaternion topos = Quaternion.LookRotation (avoidpos - transform.position);
					float angletopos = topos.eulerAngles.y;
					angletopos = angle_difference (angletopos, transform.eulerAngles.y);
					//if (angletopos < 4f) angletopos = -1f; // bias to right turn
					hardSteer -= hardSteerFactor * Mathf.Sign (angletopos);
					hardSteer = Mathf.Clamp (hardSteer, -1f, 1f);
				}
			}
		}

		else {
			// accerelate if no obstacles, and fade out of hard steer state
			float acc = accerelation;
			if (myvel < lowSpeed) acc *= lowSpeedBoost;
			if (accerelationOrigin == null) rb.AddForce (transform.forward * acc);
			else rb.AddForceAtPosition (transform.forward * acc, accerelationOrigin.position);
			hardSteer = Mathf.MoveTowards (hardSteer, 0f, hardUnsteerFactor);
		}

		// apply hard steer
		float ang = maxhardsteer * hardSteer;
		//hsdebug += ang;
		rb.AddTorque (Vector3.up * ang);
		WheelRotate (0);
		WheelRotate (1);

		// correct course (straighten) if not in hard steer state
		if (Mathf.Abs (hardSteer) == 1f) return;

		float angleCorrect = 0f;
		// if course correction enabled, no obstacles, and not excessive heading error, attempt correction
		if (courseCorrection && avoids.Count == 0 &&
			Mathf.Abs (transform.forward.x - courseInitial) < maxHeadingErrorToCorrect) {

			float courseError = courseZ - transform.position.z;
			if (Mathf.Abs (courseError) < maxCourseErrorToCorrect) {
				// request correction proportional to error, applying coefficients
				angleCorrect -= courseError / maxCourseErrorToCorrect
					* maxCourseCorrectionToRequest;
			}
		}
		// get angle offset from current heading to 90 degrees
		ang = angle_difference (90f + angleCorrect, transform.eulerAngles.y);
		if (Mathf.Abs (ang) < 90f) {
			// correct towards 90 degrees
			ang *= steerFactor;
			ang *= 1f - Mathf.Abs (hardSteer);
			if (Mathf.Abs (ang) > maxsteer) ang = maxsteer * Mathf.Sign (ang);
			rb.AddTorque (Vector3.up * ang);
			
			//stdebug += ang;
			if (angleCorrect != 0) {
				Debug.DrawLine (transform.position,
					transform.position + Vector3.up * 15 + Vector3.forward * (15 * angleCorrect),
					Color.green);
			}
		}
		else {
			// correct towards 270 degrees
			ang = angle_difference (270f - angleCorrect, transform.eulerAngles.y);
			ang *= steerFactor;
			ang *= (1f - Mathf.Abs (hardSteer));
			if (Mathf.Abs (ang) > maxsteer) ang = maxsteer * Mathf.Sign (ang);
			rb.AddTorque (Vector3.up * ang);
			
			//stdebug += ang;
			if (angleCorrect != 0) Debug.DrawLine (transform.position, transform.position + Vector3.up * 15 + Vector3.forward * (15 * angleCorrect), Color.red);
		}
	}

	void Update () {

		// skip if game paused
		if (Time.timeScale <= 0) return;
		// decrease crashTimer
		if (crashTimer > 0) crashTimer = Mathf.Max (0, crashTimer - Time.deltaTime);

		// find whether we passed x limit
		bool quit;
		if (xdeath < 0f) quit = transform.position.x < xdeath;
		else quit = transform.position.x > xdeath;

		// death timer
		if (quit && deathtimer <= 0f) deathtimer = deathDelay;

		// die from falling
		if (transform.position.y < yFall && deathtimer <= 0f) {
			deathtimer = deathDelay;
			if (PlayerScore.instance.carFallScore > 0)
				PlayerScore.instance.AddScore (PlayerScore.instance.carFallLabel,
					PlayerScore.instance.carFallScore, 1);
		}

		// delete due to x limit
		if (deathtimer > 0f) {
			deathtimer -= Time.deltaTime;
			if (deathtimer <= 0f) Destroy (gameObject);
			return;
		}

		// check of we moved from last holdPos
		Vector3 moved = transform.position - holdPos;
		if (moved.sqrMagnitude > holdMinDistanceSqr) {
			// reset holdTimer & holdPos
			holdTimer = 0f;
			holdPos = transform.position;
		}
		// if x pos outside noHoldBelowX, increase holdTimer over time
		// (noHoldBelowX is used to avoid deleting cars while they are on the train tracks)
		if (Mathf.Abs (transform.position.x) > noHoldBelowX) holdTimer += Time.deltaTime;
		
		// delete if holdMaxDelay exceeded (car stationary for too long)
		if (holdTimer > holdMaxDelay) Destroy (gameObject);
	}

	float angle_difference (float angto, float angfrom) {
		return ((((angto - angfrom) % 360) + 540) % 360) - 180;
	}

	// returns a random index of the array,
	// assuming each of the array items is the probability weight
	int Choose (float[] probs) {

		float total = 0;

		foreach (float elem in probs) {
			total += elem;
		}

		float randomPoint = Random.value * total;

		for (int i = 0; i < probs.Length; i++) {
			if (randomPoint < probs[i])
				return i;
			else
				randomPoint -= probs[i];
		}

		return probs.Length - 1;
	}

	void OnCollisionEnter (Collision collision) {

		if (collision.gameObject.layer == 10) { // car
			Car othercar = collision.gameObject.GetComponent<Car> ();
			float sqrmag = collision.relativeVelocity.sqrMagnitude;
			//Debug.Log (collision.relativeVelocity.magnitude);
			if (sqrmag > crashVelocity * crashVelocity) {
				if (crashTimer == 0 || othercar.crashTimer == 0) {
					PlayerScore.instance.AddScore ("Cars Crash-Collided", 2000, 1);
					crashTimer = crashInterval;
					othercar.crashTimer = crashInterval;
				}
			}
			else if (sqrmag > bumpVelocity * bumpVelocity) {
				if (crashTimer == 0 || othercar.crashTimer == 0) {
					PlayerScore.instance.AddScore ("Cars Collided", 1000, 1);
					crashTimer = crashInterval;
					othercar.crashTimer = crashInterval;
				}
			}
		}
		else if (collision.gameObject.layer == 15) { // tree
			if (collision.relativeVelocity.sqrMagnitude > bumpVelocity * bumpVelocity) {
				if (crashTimer == 0) {
					PlayerScore.instance.AddScore ("Car hit tree", 1000, 1);
					crashTimer = crashInterval;
				}
			}
		}
		else if (collision.gameObject.layer == 16) { // rock
			if (collision.relativeVelocity.sqrMagnitude > bumpVelocity * bumpVelocity) {
				if (crashTimer == 0) {
					PlayerScore.instance.AddScore ("Car hit rock", 1000, 1);
					crashTimer = crashInterval;
				}
			}
		}
		else if (collision.gameObject.layer == 18) { // train
			if (!trainCrashed && collision.relativeVelocity.sqrMagnitude > bumpVelocity * bumpVelocity) {
				PlayerScore.instance.AddScore ("Train crash", 2500, 1);
				trainCrashed = true;
			}
		}
		else touchdown = true;
	}

	void WheelRotate (int i) {

		Vector3 ang = wheelT[i].localEulerAngles;
		ang.y = wheelTurn * hardSteer;
		wheelT[i].localEulerAngles = ang;
	}
}
