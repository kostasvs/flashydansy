using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;

public class Score : MonoBehaviour {

	private int score;
	[Tooltip ("Score to show")]
	public int scoreGo;
	[Tooltip ("minimum delta to apply")]
	public int delta = 1;
	[Tooltip ("interpolation factor")]
	public float lerp = .1f;

	[Tooltip ("is this a timer display?")]
	public bool isTimer;
	public string strBefore;
	public string strAfter;

	private bool changed;
	private Text mytext;

	void Awake () {

		mytext = GetComponent<Text> ();
	}

	void FixedUpdate () {

		// update displayed value
		if (score != scoreGo) {
			int dt = Mathf.FloorToInt (Mathf.Abs (score - scoreGo) * lerp);
			if (dt < delta) dt = delta;
			if (score < scoreGo) score = Mathf.Min (score + dt, scoreGo);
			else score = Mathf.Max (score - dt, scoreGo);
			changed = true;
		}
	}

	void Update () {

		// update display
		if (changed) {
			NewScore (score);
			changed = false;
		}
	}

	public void NewScore (int x) {

		if (!isTimer) mytext.text = strBefore + string.Format ("{0:0000000}", x) + strAfter;
		else mytext.text = strBefore + string.Format ("{0:0}:{1:00}", Mathf.Floor (x / 60), x % 60) + strAfter;
	}
}
