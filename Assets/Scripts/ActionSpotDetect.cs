using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.VR;
using UnityEngine.UI;

public class ActionSpotDetect : MonoBehaviour {

	public Camera mainCamera;
	public Image progressBar;
	[RangeAttribute(0.1f, 10.0f)]
	public float secsToActivate = 1;
	public string tagName = "Hotspot";
	private ActionSpot _lastActionItem;
	private bool _lastActionItemIsActivated = false;

	void Start() {
		progressBar.fillAmount = 0;
	}

	void Update () {
		RaycastWorldUI();

		if (_lastActionItem != null) {
			if (!_lastActionItemIsActivated || _lastActionItem.multifire) {
				progressBar.fillAmount += Time.deltaTime/secsToActivate;
				if (progressBar.fillAmount >= 1) {
					_lastActionItem.FireAction();
					_lastActionItemIsActivated = true;
					progressBar.fillAmount = 0;
				}
			}
		}
	}
	
	void RaycastWorldUI() {
		float planeDistance = 9;
		ActionSpot actionItem = null;
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(getRaycastPointer(), results);

		results = results.Where((r) => { return r.gameObject.tag == tagName; }).ToList();

		if (results.Count > 0) {
			planeDistance = Vector3.Distance(mainCamera.transform.position, results[0].worldPosition);
			Debug.Log("wp: " + results[0].worldPosition);
			Debug.Log("cam: " + results[0].worldPosition);

			actionItem = results[0].gameObject.GetComponent<ActionSpot>();
			if (actionItem == null) {
				Debug.LogWarningFormat("{0} does not contains ActionSpot Component", results[0]);
			}
			results.Clear();
		}
		
		transform.localPosition = new Vector3(0, 0, planeDistance);
		transform.localScale = (new Vector3(planeDistance, planeDistance, planeDistance)) * 0.75f;
		UpdateLookedItem(actionItem);
	}

	PointerEventData getRaycastPointer() {
		PointerEventData pointerData = new PointerEventData(EventSystem.current);
		pointerData.position = mainCamera.WorldToScreenPoint(mainCamera.transform.forward * 10);
		return pointerData;
	}

	private void UpdateLookedItem(ActionSpot actionItem) {
		if (_lastActionItem != actionItem) {
			progressBar.fillAmount = 0;
			if (_lastActionItem != null) {
				_lastActionItem.Leave();
			}
			if (actionItem != null) {
				actionItem.Look();
			}
			_lastActionItem = actionItem;
			_lastActionItemIsActivated = false;
		}
	}
}
