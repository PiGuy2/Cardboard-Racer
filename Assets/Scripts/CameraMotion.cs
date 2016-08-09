using UnityEngine;
using System.Collections;

public class CameraMotion : MonoBehaviour
{
	public float amountToRotate;
	private bool vrActive;

	// Use this for initialization
	void Start ()
	{
		vrActive = PlayerController.virtualRealityIsActive;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!vrActive) {
			if (Input.GetKey (KeyCode.LeftArrow)) {
				transform.Rotate (0, -amountToRotate, 0, Space.Self);
			}
			if (Input.GetKey (KeyCode.RightArrow)) {
				transform.Rotate (0, amountToRotate, 0, Space.Self);
			}
			if (Input.GetKey (KeyCode.UpArrow)) {
				transform.Rotate (-amountToRotate, 0, 0, Space.Self);
			}
			if (Input.GetKey (KeyCode.DownArrow)) {
				transform.Rotate (amountToRotate, 0, 0, Space.Self);
			}
		}
	}
}
