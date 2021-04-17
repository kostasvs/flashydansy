using UnityEngine;
using System.Collections;

public class CarSensor : MonoBehaviour {

	private Car car;

	void Awake () {

		car = transform.parent.GetComponent<Car> ();
	}

	void OnTriggerEnter (Collider other) {
		if (other.transform == transform) return;
		//Debug.Log ("enter "+other.transform.name);
		car.avoids.Add (other.gameObject.transform);
	}

	void OnTriggerExit (Collider other) {
		if (other.transform == transform) return;
		//Debug.Log ("exit "+other.transform.name);
		car.avoids.Remove (other.gameObject.transform);
	}
}
