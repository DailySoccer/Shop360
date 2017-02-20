using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.VR;

public class UIWorldDetect : MonoBehaviour {

	public Camera mainCamera;
	public string tagName = "";
	private LookableItem _lastLookedItem;

	void Start() {
		Debug.Log(VRSettings.supportedDevices.Length);
		Debug.Log(VRSettings.supportedDevices[0]);
		Debug.Log(VRSettings.supportedDevices[1]);
		
		//StartCoroutine(LoadDevice(VRSettings.supportedDevices[0]));
        VRSettings.LoadDeviceByName(VRSettings.supportedDevices[0]);
    }

    IEnumerator<Object> LoadDevice(string newDevice) {
        VRSettings.LoadDeviceByName(newDevice);
        yield return null;
        VRSettings.enabled = true;
    }

	void Update () {
		RaycastWorldUI();
	}

	void RaycastWorldUI() {
		LookableItem lookedItem = null;
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(getRaycastPointer(), results);

		results = results.Where((r) => { return r.gameObject.tag == "Hotspot"; }).ToList();

		if (results.Count > 0) {
			lookedItem = results[0].gameObject.GetComponent<LookableItem>();
			results.Clear();
		}
		
		UpdateLookedItem(lookedItem);
	}
	PointerEventData getRaycastPointer() {
		PointerEventData pointerData = new PointerEventData(EventSystem.current);
		pointerData.position = mainCamera.WorldToScreenPoint(mainCamera.transform.forward * 10);
		return pointerData;
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