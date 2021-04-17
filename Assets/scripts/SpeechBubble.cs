using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour {

	public static SpeechBubble me;
	private Text mytext;
	public Camera mycamera;
	public GameObject player;
	private Transform playerT;

	[Tooltip ("world-space y to add to position")]
	public float moveUp = 1f;

	public string[] texts;

	public float liveTime = 2.5f;
	private float mytime;

	private RectTransform rtr;

	void Awake () {

		me = this;
		playerT = player.transform;
		mytext = transform.GetChild (0).GetComponent<Text> ();
		mytext.text = string.Empty;
		rtr = GetComponent<RectTransform> ();
		gameObject.SetActive (false);
	}

	void Update () {

		// update screen position
		rtr.position = mycamera.WorldToScreenPoint (playerT.position + Vector3.up * moveUp);
		// timer before disappear
		if (mytime >= 0f) {
			mytime -= Time.deltaTime;
			if (mytime <= 0f) gameObject.SetActive (false);
		}
	}

	public void SetText (int i, string extra = "") {

		if (i < 0 || i >= texts.Length) return;
		mytext.text = texts[i] + extra;
		mytime = liveTime;
	}

	public void PinText (int i, string extra = "") {

		if (i < 0 || i >= texts.Length) return;
		mytext.text = texts[i] + extra;
		mytime = -1f;
	}
}
