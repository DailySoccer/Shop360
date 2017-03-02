using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(PointOfInterestPointer))]
public class PointOfInterestDrawer : MonoBehaviour {

	public Image progressBar;
	public Image nearField;
	public Canvas canvas;

	private PointOfInterestPointer _pointer;

	private CollisionData _currentCollisionData = null;


	// Use this for initialization
	void Start () {
		_pointer = GetComponent<PointOfInterestPointer>();

		nearField.enabled = false;
		_currentCollisionData = new CollisionData();

		_pointer.OnEnterItem.AddListener(ShowNearField);
		_pointer.OnLeaveItem.AddListener(HideNearField);
		_pointer.OnHoverItem.AddListener(UpdateNearField);
	}
	
	// Update is called once per frame
	void Update() {
		canvas.transform.LookAt(Camera.main.transform.position, Camera.main.transform.rotation * Vector3.up);
		
		if (_currentCollisionData.isPointOfInterest) {
			Vector3 importanceScale = Vector3.one * _currentCollisionData.item.pointScale;

			nearField.enabled = true;
			nearField.transform.localScale = importanceScale * Mathf.Max(_pointer.centerRadius, _currentCollisionData.proportion);
			nearField.color = new Color(nearField.color.r, 
										nearField.color.g, 
										nearField.color.b, 
										(1 - _currentCollisionData.proportion) * (1 / (1 - _pointer.centerRadius)));

			canvas.transform.position = _currentCollisionData.item.sphereColliderCenter;
			canvas.transform.localScale = importanceScale * Vector3.Distance(canvas.transform.position, Camera.main.transform.position);

		} else {
			nearField.enabled = false;
			canvas.transform.position   = (Camera.main.stereoConvergence/2) * Camera.main.transform.forward + Camera.main.transform.position;
			canvas.transform.localScale = (Camera.main.stereoConvergence/2) * Vector3.one;
		}

		progressBar.fillAmount = _pointer.activationPercentage;
	}

	void ShowNearField(CollisionData collisionData) {
		_currentCollisionData = collisionData;
	}

	void HideNearField(PointOfInterestIntern point) {
		_currentCollisionData = new CollisionData();
	}
	void UpdateNearField(CollisionData collisionData) {
		_currentCollisionData = collisionData;
	}
}
