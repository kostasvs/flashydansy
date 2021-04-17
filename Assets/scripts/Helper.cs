using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Helper : MonoBehaviour {

	[TextArea]
	[Tooltip("help messages for each page")]
	public string[] helps;
	[Tooltip ("current page")]
	public int current;
	
	[Tooltip("help message text")]
	public Text mytext;
	[Tooltip("current page indicator")]
	public Text page;

	void Start () {
	
		//mytext = transform.Find ("helpText").GetComponent<Text>();
		//page = transform.Find ("pageText").GetComponent<Text>();
		SetPage ();
	}
	
	public void Back () {
		
		if (current <= 0) return;
		current--;
		SetPage ();
	}

	public void Next () {
		
		if (current >= helps.Length - 1) return;
		current++;
		SetPage ();
	}

	void SetPage () {
		mytext.text = helps [current];
		page.text = "Page " + (current + 1) + "/" + helps.Length;
	}
}
