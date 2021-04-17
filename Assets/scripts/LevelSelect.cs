using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour {

	public static LevelSelect me;

	public const uint scoreToUnlockLv2 = 1000000;

	public GameObject[] buttons;
	public Text[] placeholders;
	private string[] defTexts;

	void Awake () {

		me = this;
		defTexts = new string[placeholders.Length];
		for (int i = 0; i < defTexts.Length; i++) defTexts[i] = placeholders[i].text;
		gameObject.SetActive (false);
	}

	private void OnEnable () {

		// enable button for stage 2 only if score reached or cheats enabled
		buttons[0].SetActive (SaveLoad.totalscore >= scoreToUnlockLv2 || MenuButton.cheatsEnabled);
		if (buttons[0].activeSelf) placeholders[0].gameObject.SetActive (false);
		else {
			placeholders[0].gameObject.SetActive (true);
			placeholders[0].text = defTexts[0] + SaveLoad.totalscore + " (" +
				Mathf.FloorToInt (SaveLoad.totalscore / (float)scoreToUnlockLv2 * 100f) +"%)";
				///+ (SaveLoad.unlocks - 1) + "/6 moves";
		}
	}

	public void LoadLevel (int level) {

		if (SceneManager.GetActiveScene ().buildIndex == level) return;
		SceneManager.LoadScene (level);
	}
}
