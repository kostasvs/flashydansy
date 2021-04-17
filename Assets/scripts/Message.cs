using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Message : MonoBehaviour {

	private string msg;
	private int num;
	private bool changed;
	private float timer;
	public float msgDuration = 2f;

	public Color[] textColor;

	private Vector3 originPos;
	public float grow = .25f;
	public float curveScale = 2f;

	private Text mytext;

	void Awake () {
		
		mytext = GetComponent<Text>();
		originPos = transform.position;
	}

	void Update () {

		// skip if game paused
		if (Time.timeScale <= 0) return;

		// update position by curve
		float sc = 0f;
		if (timer > msgDuration - 1f) sc = grow * Curve (msgDuration - timer);
		transform.position = originPos + Vector3.up * sc;

		// timer before hiding
		if (timer > 0f) {
			if (changed) changed = false;
			else timer -= Time.deltaTime;
			if (timer <= 0f) {
				msg = string.Empty;
				mytext.text = string.Empty;
			}
		}

	}

	public void SetMessage (string text, int x, int col, bool forever = false) {

		changed = true;
		if (!forever) timer = msgDuration;
		else timer = 0f;
		if (text == msg) num += x;
		else {
			msg = text;
			num = x;
		}
		if (num > 0) mytext.text = msg + "\n+" + num;
		else if (num < 0) mytext.text = msg + "\n" + num;
		else mytext.text = msg;
		mytext.color = textColor [col];
	}

	float Curve (float x) {
		x *= curveScale;
		return Mathf.Max (0f, x - x*x);
	}
}
