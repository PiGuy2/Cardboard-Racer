using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	public static bool virtualRealityIsActive = (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android);
	public bool vrActiveSwitch;

	public float touchTimeThreshhold = 0.4f;

	private float touchStartTime;
	private bool wasTouched;

	public bool engineIsRunning = false;
	public bool isBraking = true;

	private Vector3 localForward;

	private Vector3 curPos;
	public bool isMovingBackward = false;
	private Vector3 loc;
	private Vector3 oldLoc;

	// Instance
	private Rigidbody rb;
	private GameObject mainCamera;
	private GameObject vrCamera;

	public float touchTime;

	public int brakeTimes = 0;

	// Use this for initialization
	void Start ()
	{
		rb = GetComponent<Rigidbody> ();

		mainCamera = GameObject.Find ("MainCamera");
		vrCamera = GameObject.Find ("GvrMain");

		if (vrActiveSwitch) {
			VrActiveSet ();
		}

		if (!virtualRealityIsActive) {
			mainCamera.SetActive (true);
			vrCamera.SetActive (false);
		} else {
			vrCamera.SetActive (true);
			mainCamera.SetActive (false);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		touchTime = Time.realtimeSinceStartup - touchStartTime;
		localForward = transform.forward;
		isBraking = false;

		if (virtualRealityIsActive) {
			if (Input.touchCount > 0) {
				Touch touch = Input.GetTouch (0);
				if (touch.phase == TouchPhase.Began) {
					touchStartTime = Time.realtimeSinceStartup;
					//wasTouched = true;
				}
				if (touch.phase == TouchPhase.Ended && !isBraking)
					StartEngine ();
				if ((Time.realtimeSinceStartup - touchStartTime) >= touchTimeThreshhold)
					UseBrake ();
			}

			/*if (wasTouched) {
				Touch touch = Input.GetTouch (0);
				if (touch.phase == TouchPhase.Ended)
					StartEngine ();
			}*/
		}
		// Input.GetKey ("space") only runs the first frame after space is pushed
		if (!virtualRealityIsActive) {
			if (Input.GetKey (KeyCode.Space)) {
				if (!wasTouched)
					touchStartTime = Time.realtimeSinceStartup;

				wasTouched = true;

				if (touchTime >= touchTimeThreshhold)
					UseBrake ();
			}
			if (wasTouched && !Input.GetKey ("space") && (Time.realtimeSinceStartup - touchStartTime) < touchTimeThreshhold) {
				wasTouched = false;
				StartEngine ();
			}
			if (wasTouched && !Input.GetKey ("space"))
				wasTouched = false;

			// Turn
			int zRot = 0;

			if (Input.GetKey (KeyCode.X)) {
				zRot += 1;
			}
			if (Input.GetKey (KeyCode.Z)) {
				zRot -= 1;
			}
			transform.Rotate (0, zRot, 0, Space.Self);
		}

		if (engineIsRunning) {
			rb.AddForce (localForward * 2);
		}
			
		isMovingBackward = false;
		loc = transform.position;
		if (movingBackwards (loc, oldLoc)) {
			isMovingBackward = true;
		}
		oldLoc = loc;
	}

	void StartEngine ()
	{
		if (!engineIsRunning) {
			engineIsRunning = true;
			rb.AddForce (localForward * 1);
		} else if (engineIsRunning) {
			engineIsRunning = false;
		}
	}

	void UseBrake ()
	{
		if (!isMovingBackward) {
			Vector3 localBackward = localForward * -1;
			rb.AddForce (localBackward * 7);
		}
		isBraking = true;
	}

	bool movingBackwards (Vector3 currentLoc, Vector3 oldLoc)
	{
		Rigidbody rb = GetComponent<Rigidbody> ();
		Vector3 loc = currentLoc - oldLoc;

		float rot = transform.eulerAngles.y - 180;
		float rotAbsolute = Mathf.Abs (rot);

		if ((loc.z < 0 && rotAbsolute > 90) || (loc.z > 0 && rotAbsolute < 90))
			return true;
		else if (rb.velocity.magnitude < 0.01)
			return true;
		else if (rotAbsolute == 90)
			return true;
		else
			return false; 	
	}

	void VrActiveSet ()
	{
		virtualRealityIsActive = !virtualRealityIsActive;
	}
}
