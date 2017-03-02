﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidThrower : Toucher
{
	#region Public Methods

	public override bool IsTouchEnabled 
	{
		get { return base.IsTouchEnabled; }
		set
		{
			if (!value)  
				_rigid.velocity = Vector3.zero;
			
			base.IsTouchEnabled = value;
		}
	}

	public override void OnSwipe(Toucher toucher, Vector2 position, Vector2 direction, float speedRatio)
	{
		if (toucher != gameObject)
		{
			if (toucher != null)
				return;
			
			Vector2 myScreenPos = Camera.main.WorldToScreenPoint(_rigid.position);
			if ((myScreenPos - position).sqrMagnitude > _inchesSqrMax)
				return;
		}

		Vector3 touchPosition =  Camera.main.ScreenToWorldPoint(position);
		Throw(CalcThrowVelocity(direction, speedRatio), touchPosition);
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="velocity"></param>
	public virtual void Throw(Vector3 velocity, Vector3? applyPosition = null)
	{
		//Debug.Log("RigidThrower::Throw>> " + name + " throwed @ " + velocity, this);
		IsTouchEnabled = _isRethrowable;

		if(applyPosition.HasValue)
			_rigid.AddForceAtPosition(velocity, applyPosition.Value, ForceMode.VelocityChange);
		else
			_rigid.AddForce(velocity, ForceMode.VelocityChange);
	}

	

	#endregion

	//==============================================================

	#region Mono

	protected override void Awake()
	{
		base.Awake();

		if (_rigid == null)
			_rigid = GetComponentInChildren<Rigidbody>();

		_inchesSqrMax = Mathf.Pow(_touchDistanceInchesMax*Screen.dpi, 2f);

		//_throwSpeedMax = CalcVerticalSpeed(_throwPeakHeight);
	}



	protected override void OnDestroy()
	{
		_rigid = null;
		base.OnDestroy();
	}

	#endregion

	//==============================================================

	#region Private Methods

	/// <summary>
	/// 
	/// </summary>
	/// <param name="swipeVector"></param>
	/// <param name="swipeSpeedRatio"></param>
	/// <returns></returns>
	private Vector3 CalcThrowVelocity(Vector2 swipeVector, float swipeSpeedRatio)
	{
		Camera camera = Camera.main;
/*
		Vector3 dir = swipeVector.x * camera.transform.right 
					+ swipeVector.y * camera.transform.up;

		swipeSpeedRatio = .25f + .75f*swipeSpeedRatio;

		return swipeSpeedRatio * _throwSpeedMax * dir.normalized;
/*/

		if (swipeVector.y <= 0f) {
			Debug.LogWarning("RigidThrower::CalcThrowVelocity>> Throw degrees <= 0f; " +
			                 "horizontal velocity should be infinity to reach Peak Heigh", this);
			swipeVector.y = 0.1f;
		}

		float tan = swipeVector.x/swipeVector.y;
		return _throwSpeedMax * (camera.transform.up + tan*camera.transform.right);
/**/

	}


	private static float CalcVerticalSpeed(float parabolePeakHeight)
	{
		return Mathf.Sqrt(2f * parabolePeakHeight * Physics.gravity.magnitude);
	}
	#endregion


	//==============================================================

	#region Private Fields

	protected Rigidbody Rigid
	{
		get { return _rigid; }
	}
	
	

	private Rigidbody _rigid;

	[SerializeField, Range(0f, 100f)] private float _touchDistanceInchesMax = 1f;
	[SerializeField] private bool _isRethrowable = false;
	[SerializeField, Range(0.5f, 50f)] private float _throwSpeedMax = 20f;



	private float _inchesSqrMax;
	//private float _verticalSpeed;

	#endregion





}
