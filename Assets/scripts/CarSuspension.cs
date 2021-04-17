using UnityEngine;
using System.Collections;

public class CarSuspension : MonoBehaviour {

	// car rigidbody
	private Rigidbody rb;
	// car chassis
	private Transform chassis;
	// previous velocity vector
	private Vector3 velPrev;
	
	// current rotation to apply in degrees
	private Vector3 displace;
	[Tooltip("max rotation to apply per axis")]
	public Vector3 displaceFactor = new Vector3 (30, 0, 30);

	[Tooltip("displacing force coefficient")]
	public float forceFactor = .05f;
	[Tooltip ("restoring force coefficient")]
	public float restoreFactor = .01f;
	private Vector3 restore;
	
	[Tooltip ("damping factor for restoring force")]
	public float dampen = .95f;

	void Awake () {
	
		rb = GetComponent<Rigidbody>();
		chassis = transform.Find ("chassis");
	}
	
	void FixedUpdate () {

		// skip if game paused
		if (Time.timeScale <= 0) return;
		
		// apply external forces (accerelation)
		Vector3 velDiff = rb.velocity - velPrev;
		velPrev = rb.velocity;
		velDiff = Quaternion.Inverse (transform.rotation) * velDiff;
		displace -= velDiff * forceFactor;

		// restoring force
		restore -= displace * restoreFactor;
		restore *= dampen;
		displace += restore;

		// apply
		displace.x = Mathf.Clamp (displace.x, -1f, 1f);
		displace.y = Mathf.Clamp (displace.y, -1f, 1f);
		displace.z = Mathf.Clamp (displace.z, -1f, 1f);
		Vector3 dd = Vector3.zero;
		dd.x = displace.z * displaceFactor.x;
		dd.y = displace.y * displaceFactor.y;
		dd.z = displace.x * displaceFactor.z;
		chassis.localEulerAngles = dd;
	}
}
