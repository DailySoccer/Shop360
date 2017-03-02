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

	private Quaternion _originalRotation;
	private Vector3 _originalScale;

	void Start() {
		progressBar.fillAmount = 0;
		_originalRotation = transform.localRotation;
		_originalScale = transform.localScale;
		//VRSettings.renderScale = 1f;
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
		float planeDistance = 90000;
		ActionSpot actionItem = null;
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(getRaycastPointer(), results);
		Vector3 stickyPosition = Vector3.one;
		Vector3 stickyNormal = Vector3.one;
		Transform stickyTransform = null;
		
		if (results.Count > 0) {
			//stickyPosition = results[0].worldPosition;
			//stickyNormal = results[0].worldNormal;
			stickyTransform = results[0].gameObject.transform;
			stickyPosition = stickyTransform.position;
			planeDistance = Vector3.Distance(mainCamera.transform.position, stickyPosition);
		}

		results = results.Where((r) => { return r.gameObject.tag == tagName; }).ToList();

		if (results.Count > 0) {
			planeDistance = Vector3.Distance(mainCamera.transform.position, stickyPosition);
			//transform.rotation = Quaternion.FromToRotation(Vector3.back, stickyNormal);

			actionItem = results[0].gameObject.GetComponent<ActionSpot>();
			if (actionItem == null) {
				Debug.LogWarningFormat("{0} does not contains ActionSpot Component", results[0]);
			}
			results.Clear();
		} else {
			transform.localRotation = _originalRotation;
		}
		
		transform.localPosition = new Vector3(0, 0, planeDistance);
		transform.localScale = _originalScale * planeDistance;

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
