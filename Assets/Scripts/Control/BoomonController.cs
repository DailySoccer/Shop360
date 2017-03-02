﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;


// TODO Automatizar configuración (animators y lo que se pueda) a partir de esto
public enum BoomonRole
{
	None = 0,
	Artist = 1,
	Gamer = 2,
	Maker = 3,
	Music = 4,
	Naturalist = 5,
	FemaleSport = 6,
	MaleSport = 7
}



[RequireComponent(typeof(Animator), typeof(CharacterController))]
[RequireComponent(typeof(Toucher), typeof(FacialAnimator))]
public class BoomonController : MonoBehaviour, IObjectTouchListener, ITeleportable
{
	#region Public Fields
	public enum State
	{
		Idle = 0,
		Driven = 1,
		Emotion = 2,
		Move = 3,
		Throw = 4,
		StandUp = 5,
		Tickles = 6,
	};

	public enum Emotion
	{
		None = 0,
		Anger = 1,
		Shame = 2,
		Happiness = 3,
		Sadness = 4,
		Scare = 5,
	}

	public enum Sense
	{
		Left = -1,
		None = 0,
		Right = 1
	}


	public Transform Transform {
		get {
			return _ragdoll == null ?
				transform : _ragdoll.Transform;
		}
	}
	   

	public BoomonRole Role {
		get { return _role; }
	}


	public State CurrentState 
	{
		get { return _currentState; }
		set 
		{
			if(value == _currentState)
				return;

			State lastState = _currentState;
			_currentState = value;
			OnStateChange(lastState, _currentState);
		}
	}
								 

	public Emotion CurrentEmotion
	{
		get { return _currentEmotion; }
		set 
		{
			if(value == _currentEmotion)
				return;
			Emotion lastEmotion = _currentEmotion;
			_currentEmotion = value;
			OnEmotionChange(lastEmotion, _currentEmotion);
		}
	}




	public Sense MoveSense 
	{
		get { return _moveSense; }
		private set 
		{
			if(value == Sense.None) {
				transform.forward = _refSystem.ScreenDir;
				transform.position = _refSystem.ProjectOnPlane(transform.position);
				_velocity = Vector3.zero;

			} else {
				transform.forward = (int)value * _refSystem.Right;
				_velocity = _moveSpeed * transform.forward;
			}

			_animator.SetInteger("Direction", (int)value);
			_moveSense = value;
		}
	}

	public void SetIsControllable(bool value)
	{
		if(!value && CurrentState == State.Move)
			CurrentState = State.Idle;

		IsTouchEnabled = _isControllable = value;
	}


	public bool IsTouchEnabled {
		get { return _toucher.enabled; }
		private set {
			_toucher.enabled = value && _isControllable;
			if(_ragdoll != null)
				_ragdoll.IsTouchEnabled = value && _isControllable;
		}
	}


	public event Action<State, State> StateChange;
	public event Action<Emotion, Emotion> EmotionChange;

	public bool IsTeleporting { get; private set; }
	public ReferenceSystem ReferenceSystem { get { return _refSystem; } }

	#endregion

	//==============================================================

	#region Public Methods

	public void Move(Vector2 touchPos)
	{
		Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
		//*
		float moveValue = Vector3.Dot(Vector3.right, touchPos - screenPos) * 2f / Screen.dpi;
		/*/
				float moveValue = 2f * (touchPos - screenPos).x / Screen.width;
		/**/
		if(MoveSense == Sense.None || Mathf.Abs(moveValue) > _senseReversalInchesMin)
			MoveSense = (Sense)Mathf.Sign(moveValue);

		CurrentState = State.Move;
	}

	public void Throw(Vector3 velocity, Vector3? applyPosition = null)
	{
		if(CurrentState == State.Throw)
			return;

		Log("Throw", "@" + velocity, this);

		CurrentState = State.Throw;
		Ragdoll.Setup(transform, _refSystem);
		Ragdoll.Throw(velocity, applyPosition);
	}

	public void Throw(Vector2 swipePos, Vector2 swipeVector, float speedRatio)
	{
		if(CurrentState == State.Throw)
			return;

		Log("Throw", "caused by Swipe", this);

		CurrentState = State.Throw;
		Ragdoll.Setup(transform, _refSystem);
		Ragdoll.OnSwipe(null, swipePos, swipeVector, speedRatio);
	}

	public void TeleportTo(Teleport target)
	{
		IsTeleporting = true;
		CurrentState = State.Driven;

		transform.position = target.transform.position;
		_refSystem = new ReferenceSystem(transform.position, target.Right);

		GoTo(target.ExitPoint);
	}



	public void GoTo(Vector3 position, float? speed = null, Action callback = null)
	{
		_drivenTarget = position;
		if(callback != null)
			_goToCallbacks.Add(callback);

		Vector3 move = _drivenTarget - transform.position;
		float moveSense = Vector3.Dot(move, _refSystem.Right);

		MoveSense = (Sense)Mathf.Sign(moveSense);
		if(speed.HasValue)
			_velocity = speed.Value * transform.forward;

		CurrentState = State.Driven;
	}

	#endregion

	//==============================================================

	#region Mono

	// TODO Extraer subinicializaicones
	private void Awake()
	{
		_isControllable = true;

		Debug.Assert(_right != Vector3.zero, "BoomonController::Awake>> Right move direction not defined!!");

		_role = (BoomonRole)Enum.Parse(typeof(BoomonRole), name.Split('(')[0]);

		_goToCallbacks = new List<Action>();
		_stateActions = new Dictionary<State, StateActions>
		{
			{ State.Driven,  new StateActions(OnDrivenStart, OnDrivenEnd, DrivenUpdate)},
			{ State.Emotion, new StateActions(OnEmotionStart, OnEmotionEnd) },
			{ State.Idle,    new StateActions(OnIdleStart, OnIdleEnd, MoveUpdate)},
			{ State.Move,    new StateActions(OnMoveStart, OnMoveEnd, MoveUpdate, OnMoveCollision)},
			{ State.Throw,   new StateActions(OnThrowStart, OnThrowEnd)},
			{ State.StandUp, new StateActions(OnStandUpStart, OnStandUpEnd)},
			{ State.Tickles, new StateActions(OnTickleStart, OnTickleEnd)},
		};


		_refSystem = new ReferenceSystem(transform.position, _right);
		transform.up = _refSystem.Up;

		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterController>();
		_toucher = GetComponent<Toucher>();
		_face = GetComponent<FacialAnimator>();

		_groundSlopeCosine = Mathf.Cos(_controller.slopeLimit * Mathf.Deg2Rad);
		_animator.SetFloat(_moveMultiplierParam, _moveSpeed * _moveAnimMultiplierBase);
	}



	private void OnDestroy()
	{
		StateChange = null;
		Ragdoll = null;

		_goToCallbacks = null;

		_toucher = null;
		_face = null;
		_ragdoll = null;
		//_bipedRoot = null;
		_animator = null;
		_controller = null;
	}

	private void Start()
	{
		MoveSense = Sense.None;
		CurrentState = State.Idle;
	}

	// TODO FRS 161027 Enfocar toda esta clase a un patrón estado inyectándonos en ellos
	private void OnEnable()
	{
		_animator.SetFloat(_moveMultiplierParam, _moveSpeed * _moveAnimMultiplierBase);
		_animator.GetBehaviour<BoomonIdleState>().Enter += OnIdleReady;
	}

	private void OnDisable()
	{
		_animator.GetBehaviour<BoomonIdleState>().Enter -= OnIdleReady;
	}
	/// <summary>
	/// 
	/// </summary>
	private void Update()
	{
		Action update = _stateActions[CurrentState].Update;
		if(update != null)
			update();

#if UNITY_EDITOR || UNITY_STANDALONE
		if(Input.GetKeyDown(KeyCode.E))
			CurrentEmotion = (Emotion)((int)(CurrentEmotion + 1)
				% Enum.GetValues(typeof(Emotion)).Length);
#endif
	}




	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Action<ControllerColliderHit> collisionEnter = _stateActions[CurrentState].OnCollisionEnter;
		if(collisionEnter != null)
			collisionEnter(hit);
	}



	#endregion

	//==============================================================

	#region Events

	private void OnStateChange(State lastState, State nextState)
	{
		_stateActions[lastState].OnEnd(nextState);
		_stateActions[nextState].OnStart(lastState);

		var e = StateChange;
		if(e != null)
			e(lastState, nextState);
	}

	private void OnEmotionChange(Emotion lastEmotion, Emotion nextEmotion)
	{
		CurrentState = nextEmotion == Emotion.None ?
				State.Idle : State.Emotion;

		if(nextEmotion != Emotion.None)
			PlayAnimation(nextEmotion.ToString());

		var e = EmotionChange;
		if (e != null)
			e(lastEmotion, nextEmotion);
	}

	private void OnRagdollGroundEnter(Vector3 groundPos)
	{
		Log("OnRagdollGroundEnter", this);
		transform.position = groundPos;
		CurrentState = State.Idle;
	}
		 
	//----------------------------------------------------------------------------------
		
	#region Events.Toucher

	public void OnTapStart(Toucher toucher, Vector2 touchPos)
	{
		if(toucher != null || CurrentState != State.Idle && CurrentState != State.Emotion)
			return;

		CurrentState = State.Move;
	}

	public void OnTapStop(Toucher toucher, Vector2 position)
	{
		if(CurrentState == State.Move)
			CurrentState = State.Idle;
	}

	public void OnTapStay(Toucher toucher, Vector2 touchPos)
	{
		if(CurrentState == State.Move)
			Move(touchPos);
	}

	public void OnDoubleTap(Toucher toucher, Vector2 position)
	{
		if((CurrentState == State.Idle || CurrentState == State.Emotion) && toucher == _toucher)
			CurrentState = State.Tickles;
	}

	public void OnSwipe(Toucher toucher, Vector2 position, Vector2 direction, float speedRatio)
	{
		Log("OnSwipe");

		if(toucher != _toucher)
			return;

		float throwDegress = Mathf.Atan(direction.y / Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
		if(throwDegress > _throwDegreesMin)
			Throw(position, direction, speedRatio);
		else
			CurrentState = State.Move;
	}

	#endregion

	//---------------------------------------------------------

	#region Events.Idle

	/// <summary>
	/// Animation Event
	/// Indicates that Idle animation is playing.
	/// </summary>
	// TODO FRS Pasar todo a behaviours o un mejor enfoque
	private void OnIdleReady()
	{
		IsTouchEnabled = true;
	}

	private void OnIdleStart(State lastState)
	{
		Log("OnIdleStart", "Last=" + lastState);


		switch(lastState) {
			case State.Throw:
			case State.StandUp:
				PlayAnimation(State.StandUp.ToString());
				break;
			case State.Tickles:
				PlayAnimation(lastState.ToString());
				break;
			default:
				PlayAnimation(State.Idle.ToString());
				break;
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="nextState"></param>
	private void OnIdleEnd(State nextState)
	{
		Log("OnIdleEnd", "Next=" + nextState);

		IsTouchEnabled = nextState == State.Tickles
						 || nextState == State.Move
						 || nextState == State.Emotion;
	}


	#endregion

	//----------------------------------------------------------------------

	#region Events.Move

	private void OnMoveStart(State lastState)
	{
		Log("OnMoveStart");
		PlayAnimation(State.Move.ToString());
	}

	private void OnMoveEnd(State nextState)
	{
		Log("OnMoveEnd");

		MoveSense = Sense.None;
	}

	private void MoveUpdate()
	{
#if UNITY_EDITOR
		_animator.SetFloat(_moveMultiplierParam, _moveSpeed * _moveAnimMultiplierBase);
#endif

		// TODO FRS 161115 Ahora mismo deja al boomon encajonado al primer contacto, pero seguir investigando
		//if ((_controller.collisionFlags & CollisionFlags.Sides) != 0) 
		//{
		//	CurrentState = State.Idle;
		//	return;
		//}

		if(_controller.isGrounded ||
			Physics.Raycast(transform.position, -_refSystem.Up, _fallHeightMin)) {
			_controller.SimpleMove(_velocity);
			Debug.DrawRay(transform.position, -_fallHeightMin * _refSystem.Up, Color.green);
		} else {
			Throw(_velocity);
			Debug.DrawRay(transform.position, -_fallHeightMin * _refSystem.Up, Color.red);
		}
	}


	private void OnMoveCollision(ControllerColliderHit hit)
	{
		//Log("OnMoveCollision", hit.gameObject.name);

#if UNITY_EDITOR
		_groundSlopeCosine = Mathf.Cos(_controller.slopeLimit * Mathf.Deg2Rad);
#endif

		Rigidbody rigid = hit.collider.attachedRigidbody;

		if(rigid != null) {
			Push(rigid);
			transform.position = _refSystem.ProjectOnPlane(transform.position);

		} else if(Vector3.Dot(hit.normal, _refSystem.Up) < _groundSlopeCosine) {
			CurrentState = State.Idle;
		}
	}

	#endregion

	//---------------------------------------------------------------------------------

	#region Events.Throw

	private void OnThrowStart(State lastState)
	{
		Log("OnThrowStart");
		gameObject.SetActive(false);
		Ragdoll.gameObject.SetActive(true);
	}

	private void OnThrowEnd(State nextState)
	{
		Log("OnThrowEnd");

		_face.Reset();
		Ragdoll = null;
		gameObject.SetActive(true);
	}

	#endregion

	//----------------------------------------------------

	#region Events.StandUp


	private void OnStandUpStart(State obj)
	{
		CurrentState = State.Idle;
	}

	private void OnStandUpEnd(State obj)
	{

	}

	#endregion

	//------------------------------------------------------------------

	#region Events.Tickle

	private void OnTickleStart(State lastState)
	{
		IsTouchEnabled = false;
		Log("OnTickleStart");
		CurrentState = State.Idle;
	}

	private void OnTickleEnd(State nextState)
	{
		Log("OnTickleEnd");
	}

	#endregion

	//-------------------------------------------------------------------

	#region Events.CodeDrive

	private void OnDrivenStart(State obj)
	{
		Log("OnDrivenStart");
		PlayAnimation(State.Move.ToString());
	}

	private void OnDrivenEnd(State obj)
	{
		Log("OnDrivenEnd");

		IsTeleporting = false;
		MoveSense = Sense.None;
	}

	private void DrivenUpdate()
	{
		bool hasReachedTarget = Vector3.Dot(_velocity, transform.position - _drivenTarget) >= 0f;
		if(hasReachedTarget)
			OnDrivenTargetReached();
		else
			_controller.SimpleMove(_velocity);
	}

	private void OnDrivenTargetReached()
	{
		CurrentState = State.Idle;

		int n = _goToCallbacks.Count;
		for(int i = 0; i < n; ++i)
			_goToCallbacks[i]();

		_goToCallbacks.RemoveRange(0, n);
	}

	#endregion

	//------------------------------------------------------------------

	#region Events.Emotion

	private void OnEmotionStart(State obj)
	{
	}

	private void OnEmotionEnd(State obj)
	{
		_currentEmotion = Emotion.None;
	}

	#endregion

	#endregion

	//============================================================

	#region Private Methods



	private void Push(Rigidbody rigid)
	{
		if(rigid == null || rigid.isKinematic)
			return;

		rigid.AddForce((1f - _bounciness) * _pushMass * _velocity, ForceMode.Impulse);
		_velocity *= _bounciness;

		var item = rigid.GetComponent<Item>();
		if(item != null)
			item.Play();
	}



	private void PlayAnimation(string trigger)
	{
		Log("PlayAnimation", trigger);
		_animator.SetTrigger(trigger);
	}


	[Conditional("DEBUG_BOOMON")]
	protected void Log(string method, object msg = null, UnityEngine.Object context = null)
	{
		string log = "BoomonController::" + method;
		if(msg != null && msg.ToString() != string.Empty)
			log += ">> " + msg;

		Debug.Log("<b><color=cyan>" + log + "</color></b>", context ?? this);
	}
	#endregion

	//=======================================================================

	#region Private Fields

	private Ragdoll Ragdoll {
		get {
			if(_ragdoll == null) {
				string ragdollPath = PathSolver.Instance.GetBoomonPath(_role, PathSolver.InstanceType.Ragdoll);
				var ragdollPrefab = Resources.Load<GameObject>(ragdollPath);
				_ragdoll = Instantiate(ragdollPrefab).GetComponent<BoomonRagdoll>();
				_ragdoll.GroundEnter += OnRagdollGroundEnter;
			}
			return _ragdoll;
		}

		set {
			if(_ragdoll == null)
				return;

			_ragdoll.GroundEnter -= OnRagdollGroundEnter;
			Destroy(_ragdoll.gameObject);
			_ragdoll = null;
		}
	}

				   
	//[SerializeField] private Transform _bipedRoot;

	[SerializeField]
	private string _moveMultiplierParam = "MoveMultiplier";
	[SerializeField]
	private Vector3 _right;
	[SerializeField, Range(0f, 1f)]
	private float _bounciness = 0.8f;
	[SerializeField, Range(0f, 20f)]
	private float _pushMass = 5f;
	[SerializeField, Range(0.5f, 50f)]
	private float _moveSpeed = 2.5f;
	[SerializeField, Range(0f, 1f)]
	private float _moveAnimMultiplierBase = .3f;
	[SerializeField, Range(0f, 90f)]
	private float _throwDegreesMin = 10f;
	[SerializeField, Range(0f, 10f)]
	private float _senseReversalInchesMin = 1f;
	[SerializeField, Range(0f, 100f)]
	private float _fallHeightMin = 100f;


	private Animator _animator;
	private CharacterController _controller;
	private BoomonRagdoll _ragdoll;
	private FacialAnimator _face;
	private Toucher _toucher;

	private Vector3 _velocity;
	private float _groundSlopeCosine;

	private Sense _moveSense;
	private State _currentState = State.Move;

	private struct StateActions
	{
		public readonly Action<State> OnStart;
		public readonly Action<State> OnEnd;
		public readonly Action Update;
		public readonly Action<ControllerColliderHit> OnCollisionEnter;

		public StateActions(
			Action<State> onStart,
			Action<State> onEnd,
			Action update = null,
			Action<ControllerColliderHit> onCollisionEnter = null)
		{
			OnStart = onStart;
			OnEnd = onEnd;
			Update = update;
			OnCollisionEnter = onCollisionEnter;
		}
	}


	private ReferenceSystem _refSystem;
	private Dictionary<State, StateActions> _stateActions;
	private List<Action> _goToCallbacks;
	private Vector3 _drivenTarget;
	private BoomonRole _role;
	private Emotion _currentEmotion;
	private bool _isControllable;

	#endregion

	
}
