using UnityEngine;
using System.Collections;

// creates <copies> copies of all children of this transform, spaced apart by <copyOffset>
public class BlockRows : MonoBehaviour {

	private GameObject[] startBlocks;
	private GameObject[] allBlocks;

	[Tooltip ("Number of copies per child")]
	public int copies;
	[Tooltip ("Offset to add to each copy")]
	public Vector3 copyOffset;

	void Start () {

		// collect all children gameObjects
		int n = transform.childCount;
		startBlocks = new GameObject[n];
		int j = 0;
		foreach (Transform tr in transform) {
			startBlocks[j] = tr.gameObject;
			j++;
		}

		// initialize allBlocks array, with first items being the original ones
		allBlocks = new GameObject[startBlocks.Length * (copies + 1)];
		for (int i = 0; i < startBlocks.Length; i++) {
			allBlocks[i] = startBlocks[i];
		}

		// create copies
		//n = startBlocks.Length; // already set above
		for (int i = 0; i < copies; i++) {
			foreach (GameObject go in startBlocks) {
				allBlocks[n] = Instantiate (go, go.transform.position + copyOffset * (i + 1),
					go.transform.rotation);
				n++;
			}
		}
	}
}
