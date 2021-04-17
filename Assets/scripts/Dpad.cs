using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Dpad : MonoBehaviour, IPointerDownHandler {

	[Tooltip ("Player")]
	public GameObject man;
	private Player player;

	private bool playing = true;
	[Tooltip ("UI gamepad images")]
	public GameObject[] pads;
	private Image[] tex;

	// screen position
	private Vector2 basepos;
	private int lastScreenWidth;
	private int lastScreenHeight;

	private bool pressed;
	private int lastdir = -1;

	void Awake () {
	
		player = man.GetComponent<Player> ();

		// find images
		tex = new Image[pads.Length];
		for (int i = 0; i < pads.Length; i++) tex [i] = pads [i].GetComponent<Image> ();

		// get basepos (gamepad central point in screen space)
		RefreshBasePos ();
	}

	private void Start () {

		if (!Player.onMobile) gameObject.SetActive (false);
	}

	void Update () {

		// update basepos on screen size change
		if (lastScreenWidth != Screen.width ||
			lastScreenHeight != Screen.height) {
			RefreshBasePos ();
		}

		// check if game active
		bool shouldbeplaying = player.phase == 1 && Time.timeScale > 0;
		if (!playing && shouldbeplaying) {
			// enable pad
			playing = true;
			ShowPad (0);
		}
		if (playing && !shouldbeplaying) {
			// disable pad and reset
			playing = false;
			player.TapTo (-1);
			for (int i = 0; i < tex.Length; i++) {
				tex [i].enabled = false;
			}
		}

		if (!playing) return;

		/*if (Input.GetMouseButtonUp (0)) {
			player.tapTo (-1);
			showPad (0);
		}
		else if (Input.GetMouseButtonDown (0)) if (tex [0].rec.HitTest (Input.mousePosition)) {
			int dir = -1;
			Vector2 tap = (Vector2)Input.mousePosition - basepos;
			if (Mathf.Abs (tap.y) < Mathf.Abs (tap.x)) {
				if (tap.x > 0f) dir = 0;
				else dir = 2;
			}
			else {
				if (tap.y > 0f) dir = 1;
				else dir = 3;
			}
			player.tapTo (dir);
			showPad (dir + 1);
		}*/
		if (pressed && !Input.GetMouseButton(0)) {
			// if released, reset pad
			player.TapTo (-1);
			lastdir = -1;
			ShowPad (0);
			pressed = false;
			//Debug.Log ("release");
		}
		if (pressed) {
			int dir = -1;
			// calculate pressed dir by touch point delta from base point
			Vector2 tap = (Vector2)Input.mousePosition - basepos;
			if (Mathf.Abs (tap.y) < Mathf.Abs (tap.x)) {
				// horizontal direction
				if (tap.x > 0f) dir = 0;
				else dir = 2;
			}
			else {
				// vertical direction
				if (tap.y > 0f) dir = 1;
				else dir = 3;
			}
			// if direction changed, update player and refresh pad image
			if (lastdir != dir) {
				lastdir = dir;
				player.TapTo (dir);
				ShowPad (dir + 1);
			}
		}
	}

	public void OnPointerDown (PointerEventData e) {

		pressed = true;
	}

	void ShowPad (int pad) {
		// show correct image for given dir
		for (int i = 0; i < tex.Length; i++) {
			tex [i].enabled = i == pad;
		}
	}
	
	void RefreshBasePos () {

		lastScreenWidth = Screen.width;
		lastScreenHeight = Screen.height;

		// set basepos to center of 1st texture's screen rect
		Vector3[] corners = new Vector3[4];
		tex[0].GetComponent<RectTransform> ().GetWorldCorners (corners);
		basepos = Vector2.zero;
		foreach (var c in corners) basepos += (Vector2)c;
		basepos *= .25f;
	}
}
