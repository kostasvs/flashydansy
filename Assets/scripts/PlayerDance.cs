using UnityEngine;
using System.Collections;

public class PlayerDance : MonoBehaviour {

	public static PlayerDance me;
	// poses
	const int INIT = 0;
	const int STAND1 = 1;
	const int STAND2 = 2;
	const int WALK = 3;
	const int DIVA1 = 4;
	const int DIVA2 = 5;
	const int SLIDE = 6;
	const int TWISTER = 7;
	const int JUMP = 8;
	const int BANG1 = 9;
	const int BANG2 = 10;
	const int SPIN = 11;
	const int HEADSPIN = 12;
	const int FALLING = 13;
	private Transform cartwheelTr;

	private Player player;
	private PlayerPhysics phys;
	private PlayerScore scr;
	private Radio rdio;
	[Tooltip ("Current moveset")]
	public int style = 0;

	private Quaternion originRot;
	private Vector3 originScale;

	private int phase = 0;
	private int direction = -100;
	private int directionLast = -100;
	[HideInInspector]
	public int mode = 0;
	private int mybeat = 0;
	private int posePhase;
	private float lockTime;
	private Vector3 motion;
	private float motionAngle;
	[Tooltip ("turn speed for turning moves")]
	public float motionTurn;
	[Tooltip ("move speed")]
	public float motionForce;

	private int unlocks;

	void Start () {

		me = this;

		player = GetComponent<Player> ();
		phys = GetComponent<PlayerPhysics> ();
		scr = GetComponent<PlayerScore> ();
		rdio = player.rdio;
		direction = player.direction;

		originScale = player.poserT.localScale;
		originRot = player.poserT.localRotation;

		if (style == 0) unlocks = SaveLoad.unlocks;
		else unlocks = SaveLoad.unlocksLv2;
		if (MenuButton.cheatsEnabled) UnlockAll ();

		if (style == 1) cartwheelTr = player.poses[TWISTER].transform.GetChild (0);
	}

	void FixedUpdate () {

		// skip if game paused
		if (Time.timeScale <= 0) return;

		// idle
		if (phase != player.phase) {
			if (phase == 1 && player.phase != 1) {
				player.pose = 0;
				mode = 0;
			}
			phase = player.phase;
		}
		//if (phase != 1) return;

		int beat;
		float fr = 2f;
		Vector3 force = Vector3.zero;
		Vector3 forceraw = Vector3.zero;

		// check for control
		if (player.direction != direction) CheckControl ();

		// falling
		if (player.fell > 0) {
			player.pose = FALLING;
			TransformClear ();
		}

		// idle
		else if (mode == 0 && phase == 0) {
			player.pose = 0;
			TransformClear ();
		}

		// stand
		else if (mode == 0 && phase == 1) {
			beat = Mathf.FloorToInt (rdio.beats * 2f);
			if (mybeat != beat) {
				mybeat = beat;
				posePhase = (posePhase + 1) % 2;
				player.pose = posePhase + 1;
				TransformClear ();
			}
		}

		// moon walk (0) / tribal walk (1)
		else if (mode == 1 && (direction == 0 || direction == 2)) {
			beat = Mathf.FloorToInt (rdio.beats * 2f);
			if (mybeat != beat) {
				mybeat = beat;
				posePhase = (posePhase + 1) % 4;
				TransformClear ();
				if (direction == 0) TransformRot (style == 0 ? 90 : -90);
				else TransformRot (style == 0 ? -90 : 90);
				if (posePhase == 1 || posePhase == 3) player.pose = style == 0 ? STAND2 : WALK;
				else {
					player.pose = (style == 0 ? WALK : STAND2);
					//if (posePhase == 3) transformFlip ();
				}
				if (player.pose == WALK && posePhase == 3) TransformFlip ();
				player.UpdatePose ();
			}
			if (direction == 0) force.x += 1f;
			else force.x -= 1f;
			scr.AddScore (string.Empty, 2, 0);
		}

		// diva walk
		else if (mode == 1 && (direction == 1 || direction == 3)) {
			beat = Mathf.FloorToInt (rdio.beats * 2f);
			if (mybeat != beat) {
				mybeat = beat;
				TransformClear ();
				posePhase = (posePhase + 1) % 4;
				if (posePhase == 2 || posePhase == 0) player.pose = STAND2;
				else {
					if (style == 0) {
						if (posePhase == 1) player.pose = DIVA1;
						else player.pose = DIVA2;
					}
					else {
						player.pose = DIVA1;
						if (posePhase == 1) TransformFlip ();
					}
				}
				if (direction == 1) TransformRot (180f);
				player.UpdatePose ();
			}
			if (direction == 1) force.z += 1f;
			else force.z -= 1f;
			scr.AddScore (string.Empty, 3, 0);
		}

		// slide
		else if (mode == 2) {
			beat = Mathf.FloorToInt (rdio.beats * 4f);
			if (mybeat != beat) {
				mybeat = beat;
				posePhase = 0;
				player.pose = SLIDE;
				TransformClear ();
				if (direction == 2) TransformFlip ();
			}
			if (direction == 0) forceraw.x += fr;
			else forceraw.x -= fr;
			if (style == 0) scr.AddScore ("Slide", 4, 0);
			else scr.AddScore ("Cool Slide", 4, 0);
		}

		// twister
		else if (mode == 3) {
			beat = Mathf.FloorToInt (rdio.beats * 4f);
			if (mybeat != beat) {
				mybeat = beat;
				posePhase = (posePhase + 1) % 8;
				player.pose = TWISTER;
				TransformClear ();
				if (style == 0) TransformRot (360f * posePhase / 8f);
				else cartwheelTr.localEulerAngles = new Vector3 (0, 90, 360f * posePhase / 8f * (direction == 1 ? -1 : 1));
			}
			if (direction == 1) forceraw.z += fr;
			else forceraw.z -= fr;
			if (style == 0) scr.AddScore ("Twister", 5, 0);
			else scr.AddScore ("Cartwheel", 5, 0);
		}

		// jump
		else if (mode == 4) {
			beat = Mathf.FloorToInt (rdio.beats * 4f);
			mybeat = beat;
			posePhase = 0;
			if (lockTime > .75f) player.pose = DIVA2;
			else {
				player.pose = JUMP;
				forceraw += motion * fr;
			}
			TransformClear ();
			if (lockTime > .75f) TransformRot (motionAngle + 180f);
			else TransformRot (motionAngle + 90f);
			float pTime = lockTime;
			lockTime -= Time.deltaTime;
			if (lockTime <= 0f) mode = 0;
			if (pTime > .75f && lockTime <= .75f) {
				if (style == 0) scr.AddScore ("Joy Jump", 1000, 0);
				else scr.AddScore ("Joy Slide", 1000, 0);
			}
		}

		// bang the floor
		else if (mode == 5) {
			beat = Mathf.FloorToInt (rdio.beats * 2f);
			if (mybeat != beat) {
				mybeat = beat;
				posePhase = (posePhase + 1) % 2;
				if (posePhase == 0) player.pose = BANG1;
				else {
					player.pose = BANG2;
					scr.AddScore ("Bang The Floor", 200, 0);
				}
				TransformClear ();
				TransformRot (motionAngle);
			}
		}

		// spin
		else if (mode == 6 || mode == 7) {
			beat = Mathf.FloorToInt (rdio.beats * 4f);
			if (mybeat != beat) {
				mybeat = beat;
				posePhase = (posePhase + 1) % 8;
				if (mode == 6) player.pose = SPIN;
				else player.pose = HEADSPIN;

				TransformClear ();
				TransformRot (360f * posePhase / 8f);
			}
			if (mode == 6) motionAngle += motionTurn * Time.deltaTime;
			else motionAngle -= motionTurn * Time.deltaTime;
			//motionAngle = motionAngle % 360;
			motion = Vector3.forward * motionForce;
			motion = Quaternion.Euler (0f, motionAngle, 0f) * motion;
			forceraw += motion;
			if (mode == 6) {
				if (style == 0) scr.AddScore ("Spin", 15, 0);
				else scr.AddScore ("Dancing Queen", 15, 0);
			}
			else {
				if (style == 0) scr.AddScore ("Head Spin", 20, 0);
				else scr.AddScore ("Capoeira Spin", 20, 0);
			}
		}

		// apply force if requested
		if (force != Vector3.zero) phys.Force (force);
		if (forceraw != Vector3.zero) phys.ForceRaw (forceraw);

	}

	void CheckControl () {

		if (lockTime > 0f) {
			if (mode == 4) return;
			else lockTime = 0f;
		}

		mybeat = -1;
		posePhase = Mathf.FloorToInt (rdio.beats * 2f);
		direction = player.direction;
		if (player.direction == -1) {
			mode = 0;
			return;
		}

		int dirlast = directionLast;
		directionLast = player.direction;

		if (!player.directionTap) {
			mode = 1;
			return;
		}

		if (direction == dirlast) {
			if (direction == 0 || direction == 2) {
				if (unlocks < 2) mode = 1;
				else mode = 2;
			}
			else {
				if (unlocks < 3) mode = 1;
				else mode = 3;
			}
			return;
		}

		if (dirlast == 3 && direction == 1) {
			if (unlocks < 4) mode = 1;
			else {
				mode = 4;
				lockTime = 1f;
				motionAngle = Random.value * 360f;
				motion = Vector3.forward;
				motion = Quaternion.Euler (0f, motionAngle, 0f) * motion;
			}
		}
		if (dirlast == 1 && direction == 3) {
			if (unlocks < 5) mode = 1;
			else {
				mode = 5;
				if (Random.value < .5f) motionAngle = 0f;
				else motionAngle = 180f;
			}
		}

		if ((dirlast == 0 && direction == 2) || (dirlast == 2 && direction == 0)) {
			if (dirlast == 0 && direction == 2) {
				if (unlocks < 6) mode = 1;
				else mode = 6;
			}
			else {
				if (unlocks < 7) mode = 1;
				else mode = 7;
			}
			motionAngle = Random.value * 360f;
		}

	}

	void TransformClear () {
		player.poserT.localRotation = originRot;
		player.poserT.localScale = originScale;
		//Debug.Log (player.pose);
	}

	void TransformFlip () {
		Vector3 sc = player.poserT.localScale;
		sc.x = -sc.x;
		//Debug.Log (player.pose + " " + sc.x);
		player.poserT.localScale = sc;
	}

	void TransformRot (float angle) {
		player.poserT.Rotate (Vector3.up * angle);
	}

	public void ResetMode () {
		mode = 0;
	}

	public static void UnlockAll () {
		if (me) me.unlocks = 7;
	}
}
