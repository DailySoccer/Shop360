using UnityEngine;
using UnityEngine.Events;

public class LookableItem : MonoBehaviour {

	[HideInInspector]
	public UnityEvent _onHover;
	[HideInInspector]
	public UnityEvent _onLeave;

	public void Looked() {
		_onHover.Invoke();
	}

	public void Leave() {
		_onLeave.Invoke();
	}
}
