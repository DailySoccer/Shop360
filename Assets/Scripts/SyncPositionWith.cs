using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SyncPosition : MonoBehaviour {

	public Transform with = null;

	void Awake() {
		enabled = !Application.isPlaying;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = with.position;
	}
}
