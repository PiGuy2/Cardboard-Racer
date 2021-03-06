// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissioßns and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using proto;

/// @cond
namespace Gvr.Internal
{
	class EmulatorManager : MonoBehaviour
	{
		public static EmulatorManager Instance {
			get {
				if (instance == null) {
					var gameObject = new GameObject ("PhoneRemote");
					instance = gameObject.AddComponent<EmulatorManager> ();
					// This object should survive all scene transitions.
					GameObject.DontDestroyOnLoad (instance);
				}
				return instance;
			}
		}

		private static EmulatorManager instance = null;

		public delegate void OnGyroEvent (EmulatorGyroEvent gyroEvent);

		public event OnGyroEvent gyroEventListeners {
			add {
				if (value != null) {
					value (currentGyroEvent);
				}
				gyroEventListenersInternal += value;
			}

			remove {
				gyroEventListenersInternal -= value;
			}
		}

		public delegate void OnAccelEvent (EmulatorAccelEvent accelEvent);

		public event OnAccelEvent accelEventListeners {
			add {
				if (value != null) {
					value (currentAccelEvent);
				}
				accelEventListenersInternal += value;
			}

			remove {
				accelEventListenersInternal -= value;
			}
		}

		public delegate void OnTouchEvent (EmulatorTouchEvent touchEvent);

		public event OnTouchEvent touchEventListeners {
			add {
				if (value != null
				    && currentTouchEvent.pointers != null /* null only during init */) {
					value (currentTouchEvent);
				}
				touchEventListenersInternal += value;
			}

			remove {
				touchEventListenersInternal -= value;
			}
		}

		public delegate void OnOrientationEvent (EmulatorOrientationEvent orientationEvent);

		public event OnOrientationEvent orientationEventListeners {
			add {
				if (value != null) {
					value (currentOrientationEvent);
				}
				orientationEventListenersInternal += value;
			}

			remove {
				orientationEventListenersInternal -= value;
			}
		}

		public delegate void OnButtonEvent (EmulatorButtonEvent buttonEvent);

		public event OnButtonEvent buttonEventListeners {
			add {
				if (value != null) {
					value (currentButtonEvent);
				}
				buttonEventListenersInternal += value;
			}

			remove {
				buttonEventListenersInternal -= value;
			}
		}


		private void onGyroEvent (EmulatorGyroEvent e)
		{
			currentGyroEvent = e;
			if (gyroEventListenersInternal != null) {
				gyroEventListenersInternal (e);
			}
		}

		private void onAccelEvent (EmulatorAccelEvent e)
		{
			currentAccelEvent = e;
			if (accelEventListenersInternal != null) {
				accelEventListenersInternal (e);
			}
		}

		private void onTouchEvent (EmulatorTouchEvent e)
		{
			currentTouchEvent = e;
			if (touchEventListenersInternal != null) {
				touchEventListenersInternal (e);
			}
		}

		private void onOrientationEvent (EmulatorOrientationEvent e)
		{
			currentOrientationEvent = e;
			if (orientationEventListenersInternal != null) {
				orientationEventListenersInternal (e);
			}
		}

		private void onButtonEvent (EmulatorButtonEvent e)
		{
			currentButtonEvent = e;
			if (buttonEventListenersInternal != null) {
				buttonEventListenersInternal (e);
			}
		}

		EmulatorGyroEvent currentGyroEvent;
		EmulatorAccelEvent currentAccelEvent;
		EmulatorTouchEvent currentTouchEvent;
		EmulatorOrientationEvent currentOrientationEvent;
		EmulatorButtonEvent currentButtonEvent;

		private event OnGyroEvent gyroEventListenersInternal;
		private event OnAccelEvent accelEventListenersInternal;
		private event OnTouchEvent touchEventListenersInternal;
		private event OnOrientationEvent orientationEventListenersInternal;
		private event OnButtonEvent buttonEventListenersInternal;

		private Queue pendingEvents = Queue.Synchronized (new Queue ());
		private long lastDownTimeMs;

		public void Awake ()
		{
			if (instance == null) {
				instance = this;
			}
			if (instance != this) {
				Debug.LogWarning ("PhoneRemote must be a singleton.");
				enabled = false;
				return;
			}
			StartCoroutine ("EndOfFrame");
		}

		IEnumerator EndOfFrame ()
		{
			while (true) {
				yield return new WaitForEndOfFrame ();

				lock (pendingEvents.SyncRoot) {
					while (pendingEvents.Count > 0) {
						PhoneEvent phoneEvent = (PhoneEvent)pendingEvents.Dequeue ();
						ProcessEventAtEndOfFrame (phoneEvent);
					}
				}
			}
		}

		public void OnPhoneEvent (PhoneEvent e)
		{
			pendingEvents.Enqueue (e);
		}

		private void ProcessEventAtEndOfFrame (PhoneEvent e)
		{
			switch (e.Type) {
				case PhoneEvent.Types.Type.MOTION:
					EmulatorTouchEvent touchEvent = new EmulatorTouchEvent (e.MotionEvent, lastDownTimeMs);
					onTouchEvent (touchEvent);
					if (touchEvent.getActionMasked () == EmulatorTouchEvent.Action.kActionDown) {
						lastDownTimeMs = e.MotionEvent.Timestamp;
					}
					break;
				case PhoneEvent.Types.Type.GYROSCOPE:
					EmulatorGyroEvent gyroEvent = new EmulatorGyroEvent (e.GyroscopeEvent);
					onGyroEvent (gyroEvent);
					break;
				case PhoneEvent.Types.Type.ACCELEROMETER:
					EmulatorAccelEvent accelEvent = new EmulatorAccelEvent (e.AccelerometerEvent);
					onAccelEvent (accelEvent);
					break;
				case PhoneEvent.Types.Type.ORIENTATION:
					EmulatorOrientationEvent orientationEvent =
						new EmulatorOrientationEvent (e.OrientationEvent);
					onOrientationEvent (orientationEvent);
					break;
				case PhoneEvent.Types.Type.KEY:
					EmulatorButtonEvent buttonEvent = new EmulatorButtonEvent (e.KeyEvent);
					onButtonEvent (buttonEvent);
					break;
				default:
					Debug.Log ("Unsupported PhoneEvent type: " + e.Type);
					break;
			}
		}
	}
}
/// @endcond
