using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
[RequireComponent(typeof(SphereCollider))]
public class PointOfInterestIntern : MonoBehaviour {

	[HideInInspector] public PointOfInterest pointOfInterestComponent;

	private SphereCollider _collider;

	private float _worldScale;
	private float _invWorldScale;
	private float _originalRadius = 0f;

	public bool isMultifire { get { return pointOfInterestComponent.isMultifire; } }
	public float pointScale { get { return pointOfInterestComponent.pointScale; } }

	public Vector3 sphereColliderCenter {
		get { return _collider.center + transform.position; }
	}
	public float sphereColliderRadius {
		get { return _collider.radius * _worldScale; }
	}

	// Use this for initialization
	void Start () {
		_collider = GetComponent<SphereCollider>();

		_worldScale = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3;
		_invWorldScale = 1 / _worldScale;

		if (_originalRadius == 0f) {
			_originalRadius = _collider.radius * pointOfInterestComponent.pointScale;
		}
	}
	
	// Update is called once per frame
	void Update () {
		float distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);

		_collider.radius = _originalRadius * distanceToCamera * _invWorldScale * 0.5f;
	}

	public void Leave() {
		pointOfInterestComponent.Leave();
	}

	public void Enter(CollisionData point) {
		pointOfInterestComponent.Enter(point);
	}

	public void Hover(CollisionData point) {
		pointOfInterestComponent.Hover(point);
	}
	public void FireAction() {
		pointOfInterestComponent.FireAction();
	}
}
