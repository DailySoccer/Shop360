using UnityEngine;
using UnityEngine.Events;

public class PointOfInterest : MonoBehaviour {

	public float pointScale = 1;
	public bool isMultifire = false;
	public UnityEvent OnFireAction;
	public UnityEvent OnLookAction;
	public UnityEvent OnLeaveAction;
	public UnityEvent OnNearAction;

	//private PointOfInterestIntern _collisionChild;

	// Use this for initialization
	void Start () {
		InitializeEvents();
		UpdateGUICollider();
	}

	void InitializeEvents() {
		if (OnFireAction == null) OnFireAction = new UnityEvent();
		if (OnLookAction == null) OnLookAction = new UnityEvent();
		if (OnLeaveAction == null) OnLeaveAction = new UnityEvent();
		if (OnNearAction == null) OnNearAction = new UnityEvent();
	}

	void UpdateGUICollider () {
		PointOfInterestIntern collisionChild = transform.GetComponentInChildren<PointOfInterestIntern>();

		if (collisionChild == null) {
			GameObject child = new GameObject();
			collisionChild = child.AddComponent<PointOfInterestIntern>();

			child.name = "GUI Auxiliar Collider";
			child.layer = LayerMask.NameToLayer("PointOfInterest");
			child.transform.SetParent(transform);

			child.transform.localPosition = Vector3.zero;
			child.transform.localScale = Vector3.one;
			child.transform.localRotation = Quaternion.identity;
		}

		collisionChild.pointOfInterestComponent = this;
	}

	public void Leave() {
		Debug.LogFormat("PointOfInterest({0}) :: LEAVE EVENT", name);
		OnLeaveAction.Invoke();
	}

	public void Enter(CollisionData point) {
		Debug.LogFormat("PointOfInterest({0}) :: ENTER EVENT", name);
		OnLookAction.Invoke();
	}

	public void Hover(CollisionData point) {
		Debug.LogFormat("PointOfInterest({0}) :: HOVER EVENT", name);
		OnNearAction.Invoke();
	}

	public void FireAction() {
		Debug.LogFormat("PointOfInterest({0}) :: FIRE ACTION EVENT", name);
		OnFireAction.Invoke();
	}
}
