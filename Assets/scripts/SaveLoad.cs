using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoad {

	public static bool hasLoaded = false;
	public static int hiscore = 0;
	public static int unlocks = 1;
	public static int plays = 1;
	public static uint totalscore = 0;
	public static int unlocksLv2 = 1;
	public static int hiscoreLv2 = 0;
	public static int scaleUI = 0;

	public static bool Save () {

		//if (Application.isWebPlayer) return;

		bool success = false;
		SaveData sd = new SaveData (hiscore, unlocks, plays, totalscore,
			unlocksLv2, hiscoreLv2, scaleUI);
		BinaryFormatter bf = new BinaryFormatter ();
		try {
			using (FileStream file = File.Create (Application.persistentDataPath + "/save.gd")) {
				bf.Serialize (file, sd);
				success = true;
			}
		}
		catch (Exception e) {
			Debug.LogWarning ("Failed to save game data: " + e);
			return false;
		}
		return success;
	}

	public static void Load () {

		//if (Application.isWebPlayer) return;

		// Debug.Log(Application.persistentDataPath);
		if (!File.Exists (Application.persistentDataPath + "/save.gd")) return;

		try {
			using (FileStream file = File.Open (Application.persistentDataPath + "/save.gd",
				FileMode.Open)) {

				BinaryFormatter bf = new BinaryFormatter ();
				SaveData sd = (SaveData)bf.Deserialize (file);
				hiscore = sd.hiscore;
				unlocks = sd.unlocks;
				plays = sd.plays;
				totalscore = sd.totalscore;
				unlocksLv2 = sd.unlocksLv2;
				hiscoreLv2 = sd.hiscoreLv2;
				scaleUI = sd.scaleUI;
				//Debug.Log (totalscore);
				hasLoaded = true;
			}
		}
		catch (Exception e) {
			Debug.LogWarning ("Failed to load game data: " + e);
		}
	}

	public static void AddToTotal (uint amount) {

		checked {
			try {
				totalscore += amount;
			}
			catch (OverflowException) {
				totalscore = uint.MaxValue;
			}
		}
	}

	[Serializable]
	class SaveData {
		public int hiscore = 0;
		public int unlocks = 1;
		public int plays = 0;
		[OptionalField (VersionAdded = 2)]
		public uint totalscore = 0;
		[OptionalField (VersionAdded = 2)]
		public int unlocksLv2 = 1;
		[OptionalField (VersionAdded = 2)]
		public int hiscoreLv2 = 0;
		[OptionalField (VersionAdded = 2)]
		public int scaleUI = 0;

		public SaveData (int hiscore2, int unlocks2, int plays2, uint total2, int unlocksLv2, int hiscoreLv2,
			int scaleUI) {
			hiscore = hiscore2;
			unlocks = unlocks2;
			plays = plays2;
			totalscore = total2;
			this.unlocksLv2 = unlocksLv2;
			this.hiscoreLv2 = hiscoreLv2;
			this.scaleUI = scaleUI;
		}
	}
}
