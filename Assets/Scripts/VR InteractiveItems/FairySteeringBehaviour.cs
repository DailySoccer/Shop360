using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PointOfInterestPointer))]
public class FairySteeringBehaviour : MonoBehaviour {

	public Transform fairy;

	public float idleDistance = 5;

	private PointOfInterestPointer _pointer;
	private CollisionData _currentCollisionData = null;

	// Use this for initialization
	void Start () {
		_pointer = GetComponent<PointOfInterestPointer>();

		//fairy.enabled = false;
		_currentCollisionData = new CollisionData();

		_pointer.OnEnterItem.AddListener(SendFairyToObject);
		_pointer.OnLeaveItem.AddListener(PullFairyFromObject);
		_pointer.OnHoverItem.AddListener(FairyMovingAtObject);
	}
	
	// Update is called once per frame
	void Update () {
		if (_currentCollisionData.isPointOfInterest) {
			FairyTargetMovement();
		} else {
			FairyIdleMovement();
		}
	}

	void FairyIdleMovement() {
		Transform camTransform = Camera.main.transform;
		Vector3 fairyNextPosition = camTransform.position + camTransform.forward * idleDistance;
        fairy.position = Vector3.Lerp(fairy.position, fairyNextPosition, 0.05f);
	}

	void FairyTargetMovement() {
		PointOfInterestIntern item = _currentCollisionData.item;
		Transform camTransform = Camera.main.transform;
		Vector3 fairyNextPosition = item.transform.position - Vector3.Normalize(item.transform.position - camTransform.position) * _currentCollisionData.item.sphereColliderRadius;
        fairy.position = Vector3.Lerp(fairy.position, fairyNextPosition, 0.1f);
	}

	void SendFairyToObject (CollisionData collisionData) {
		_currentCollisionData = collisionData;
	}

	void PullFairyFromObject (PointOfInterestIntern point) {
		_currentCollisionData = new CollisionData();
	}
	void FairyMovingAtObject (CollisionData collisionData) {
		_currentCollisionData = collisionData;
	}
}
