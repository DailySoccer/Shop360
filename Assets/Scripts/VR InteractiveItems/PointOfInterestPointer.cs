using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class CollisionItemEvent : UnityEvent<CollisionData> {}
[Serializable] public class LeaveItemEvent : UnityEvent<PointOfInterestIntern> {}

public class PointOfInterestPointer : MonoBehaviour {

	[RangeAttribute(0, 1)] public float centerRadius = 0.3f;
	[RangeAttribute(0.1f, 10.0f)] public float secsToActivate = 1;
	public LayerMask cullingMask = 32;

	[SerializeField] public CollisionItemEvent OnHoverItem;
	[SerializeField] public LeaveItemEvent OnLeaveItem;
	[SerializeField] public CollisionItemEvent OnEnterItem;
	[SerializeField] public CollisionItemEvent OnFireAction;
		
	public float activationPercentage { get; private set; }
	public bool lastPointWasActivated { get; private set; }
	private CollisionData _lastRaycastData = new CollisionData();


	// Use this for initialization
	void Start () {
		InitializeEvents();
	}
	
	void InitializeEvents() {
		if (OnHoverItem == null) OnHoverItem = new CollisionItemEvent();
		if (OnLeaveItem == null) OnLeaveItem = new LeaveItemEvent();
		if (OnEnterItem == null) OnEnterItem = new CollisionItemEvent();
	}
	
	// Update is called once per frame
	void Update () {
		CollisionData collisionData = PerformRaycast();
		
		UpdateLookedItem(collisionData);
	}

	private CollisionData PerformRaycast() {
		CollisionData data = new CollisionData();

		RaycastHit hit;
		Transform camTransform = Camera.main.transform;
        Ray ray = new Ray(camTransform.position, camTransform.forward);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, cullingMask)) {
            GameObject objectHit = hit.transform.gameObject;
            
			data.item = objectHit.GetComponent<PointOfInterestIntern>();
			data.point = hit.point;
			data.isPointOfInterest = data.item != null;
        }

		return data;
	}

	private void UpdateLookedItem(CollisionData newRaycastData) {
		if (newRaycastData.isPointOfInterest) {
			// hard work
			FillCollisionScreenComputations(newRaycastData);
		}
		UpdateActivationState(newRaycastData);

		if (_lastRaycastData.item != newRaycastData.item) {
			LeaveLastItem();
			EnterItem(newRaycastData);
		}
		_lastRaycastData = newRaycastData;

		if (newRaycastData.isPointOfInterest) {
			HoverItem();
		}
	}

	private void UpdateActivationState(CollisionData newRaycastData)  {
		bool isLookingAtCenter = newRaycastData.isPointOfInterest && newRaycastData.proportion < centerRadius;

		if (isLookingAtCenter) {
			if (!lastPointWasActivated || newRaycastData.item.isMultifire) {
				activationPercentage += Time.deltaTime/secsToActivate;
				if (activationPercentage >= 1) {
					FireAction();
					lastPointWasActivated = true;
					activationPercentage = 0;
				}
			}
		} else {
			lastPointWasActivated = false;
			activationPercentage = 0;
		}
	}

	void FillCollisionScreenComputations(CollisionData raycastData) {
		Vector3 colliderScreenCenter = Camera.main.WorldToScreenPoint(raycastData.item.sphereColliderCenter);
		Vector3 colliderScreenMaxUp = Camera.main.WorldToScreenPoint(raycastData.item.sphereColliderCenter + (Camera.main.transform.up.normalized * raycastData.item.sphereColliderRadius));
		Vector3 collisionScreenPoint = Camera.main.WorldToScreenPoint(raycastData.point);
		colliderScreenCenter.z = 0;
		colliderScreenMaxUp.z = 0;
		collisionScreenPoint.z = 0;

		float maxScreenDistance = Vector3.Distance(colliderScreenCenter, colliderScreenMaxUp);
		float collisionScreenDistance = Vector3.Distance(colliderScreenCenter, collisionScreenPoint);

		float proportion = collisionScreenDistance / maxScreenDistance;
//		Debug.LogFormat("max: {0}, collision: {1}, proportion: {2}", maxScreenDistance, collisionScreenDistance, proportion);

		raycastData.proportion = proportion;
		raycastData.collisionScreenDistance = collisionScreenDistance;
		raycastData.maxScreenDistance = maxScreenDistance;

		raycastData.collisionScreenPoint = collisionScreenPoint;
		raycastData.colliderScreenCenter = colliderScreenCenter;
	}

	private void HoverItem() {
		_lastRaycastData.item.Hover(_lastRaycastData);
		Debug.Log("PointOfInterestPointer :: HOVER EVENT");
		OnHoverItem.Invoke(_lastRaycastData);
	}
	private void EnterItem(CollisionData newRaycastData) {
		if (newRaycastData != null && newRaycastData.isPointOfInterest) {
			newRaycastData.item.Enter(newRaycastData);
			Debug.Log("PointOfInterestPointer :: ENTER EVENT");
			OnEnterItem.Invoke(newRaycastData);
		}
		lastPointWasActivated = false;
	}
	private void LeaveLastItem() {
		if (_lastRaycastData != null && _lastRaycastData.isPointOfInterest) {
			_lastRaycastData.item.Leave();
			Debug.Log("PointOfInterestPointer :: LEAVE EVENT");
			OnLeaveItem.Invoke(_lastRaycastData.item);
		}
	}
	private void FireAction() {
		_lastRaycastData.item.FireAction();
		Debug.Log("PointOfInterestPointer :: FIRE EVENT");
		OnFireAction.Invoke(_lastRaycastData);
	}

	void OnDrawGizmos()	{
		if (Debug.isDebugBuild) {
			Transform camTransform = Camera.main.transform;
			Gizmos.color = !(_lastRaycastData != null && _lastRaycastData.isPointOfInterest)? Color.blue : Color.green;
			Gizmos.DrawRay(camTransform.position, camTransform.forward * 100);

			if(!_lastRaycastData.isPointOfInterest) return;

			Gizmos.color = Color.red;
			Gizmos.DrawLine(_lastRaycastData.item.sphereColliderCenter, 
							_lastRaycastData.item.sphereColliderCenter + (Camera.main.transform.up.normalized * _lastRaycastData.item.sphereColliderRadius));
		}
	}
	
}
