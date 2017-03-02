using UnityEngine;
using UnityEngine.UI;

public class HotSpot : LookableItem {
	
	public GameObject hotSpotContainer = null;
	public Image progressBar;
	[RangeAttribute(0.1f, 10.0f)]
	public float secsToActivate = 1;

	public bool isActiveHotspot = false;

	private bool _isCharging = false;
	

	// Use this for initialization
	void Start () {
		if (hotSpotContainer == null) {
			Debug.LogWarningFormat("{0}.hotspotContainer has not been assigned", name);
		} else {
			hotSpotContainer.SetActive(false);
		}
		_onHover.AddListener(Charge);
		_onLeave.AddListener(StopCharge);
	}

	void Update() {
		if (_isCharging) {
			progressBar.fillAmount += Time.deltaTime/secsToActivate;
			if (progressBar.fillAmount >= 1) {
				progressBar.fillAmount = 1;
				isActiveHotspot = true;

				if (hotSpotContainer != null) {
					hotSpotContainer.SetActive(true);
				}
			}
		}
	}

	private void Charge() {
		_isCharging = true;
	}

	private void StopCharge() {
		_isCharging = false;
		progressBar.fillAmount = 0;
		if (hotSpotContainer != null) {
			hotSpotContainer.SetActive(false);
		}
	}
}
