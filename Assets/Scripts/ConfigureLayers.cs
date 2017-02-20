using UnityEngine;
using System.Collections;
using UnityEngine.VR;

public class ConfigureLayers : MonoBehaviour {

	void Start() {
		//VR
	}
	/*
	// Use this for initialization
	IEnumerator Start() {
		
		//GvrEye rightObjectCam = null;
		//GvrEye leftObjectCam = null;
		Object[] objects = null;
		bool prepared = false;

		Debug.Log("1");
 		while(!prepared) {
			objects = GameObject.FindObjectsOfType(typeof(Camera));
			prepared = objects.Length >= 2;

			Debug.Log("1.2 prepared:" + prepared + " || count:" + objects.Length);
			if(!prepared) yield return new WaitForEndOfFrame();
			Debug.Log("1.3");
		}
		Debug.Log("2");
		yield return new WaitForEndOfFrame();
		Debug.Log("4");

		VR
	}*/
	

	/*
	// Use this for initialization
	IEnumerator Start() {
		
		GvrEye rightObjectCam = null;
		GvrEye leftObjectCam = null;
		Object[] objects = null;
		bool prepared = false;

		Debug.Log("1");
 		while(!prepared) {
			objects = GameObject.FindObjectsOfType(typeof(GvrEye));
			prepared = objects.Length >= 2;

			Debug.Log("1.2 prepared:" + prepared + " || count:" + objects.Length);
			if(!prepared) yield return null;
			Debug.Log("1.3");
		}

		Debug.Log("2");

		rightObjectCam = (GvrEye) (((GvrEye)objects[0]).eye == GvrViewer.Eye.Right? objects[0] : objects[1]);
		leftObjectCam = (GvrEye) (((GvrEye)objects[0]).eye == GvrViewer.Eye.Left? objects[0] : objects[1]);

		rightObjectCam.toggleCullingMask = 1 << LayerMask.NameToLayer("LeftEye");
		leftObjectCam.toggleCullingMask = 1 << LayerMask.NameToLayer("RightEye");
		GetComponent<StereoController>().keepStereoUpdated = true;
		Debug.Log("3");
		yield return null;
		GetComponent<StereoController>().keepStereoUpdated = false;
		Debug.Log("4");
	}
	
	*/
}
