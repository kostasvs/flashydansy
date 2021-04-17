using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaledUI : MonoBehaviour {

	public static List<ScaledUI> list = new List<ScaledUI> ();

	// global scale
	public static float scale = 1f;
	public static float startScaleIfStandalone = 1.7f;

	// delta to apply on increase/decrease
	public const float delta = .1f;
	// min/max
	public const float minScale = .5f;
	public const float maxScale = 2f;

	private Vector3 initScale;

	void Start () {

		if (SaveLoad.scaleUI > 0) {
			// load saved scale
			scale = SaveLoad.scaleUI * .01f;
		}
		else if (startScaleIfStandalone != 0 && !Application.isMobilePlatform) {
			// set default scale
			scale = startScaleIfStandalone;
			startScaleIfStandalone = 0;
		}
		// initialize this UI item with set scale
		initScale = transform.localScale;
		list.Add (this);
		transform.localScale = initScale * scale;
	}

	private void OnDestroy () {

		list.Remove (this);
	}

	public static void Refresh () {

		foreach (var item in list) if (item) {
				item.transform.localScale = item.initScale * scale;
			}
	}
}
