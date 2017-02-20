using UnityEngine;
using System.Collections;

public class LookAtChecker : MonoBehaviour {

	public Camera mainCamera;
	public string tagName;

	private LookableItem _lastLookedItem;
	
	void Update () {
		RaycastHit hit;
		Transform camTransform = mainCamera.transform;
        Ray ray = new Ray(camTransform.position, camTransform.forward);
		LookableItem lookedItem = null;

        if (Physics.Raycast(ray, out hit)) {
            GameObject objectHit = hit.transform.gameObject;
            
            if (objectHit.tag == tagName) {
				lookedItem = objectHit.GetComponent<LookableItem>();
			}
        }
		
		UpdateLookedItem(lookedItem);
	}

	void OnDrawGizmos()	{
		if (Debug.isDebugBuild) {
			Transform camTransform = mainCamera.transform;
			Gizmos.color = _lastLookedItem == null? Color.blue : Color.green;
			Gizmos.DrawRay(camTransform.position, camTransform.forward * 100);
		}
	}

	private void UpdateLookedItem(LookableItem lookedItem) {
		if (_lastLookedItem != lookedItem) {
			if (_lastLookedItem != null) {
				_lastLookedItem.Leave();
			}
			if (lookedItem != null) {
				lookedItem.Looked();
			}
			_lastLookedItem = lookedItem;
		}
	}
}
