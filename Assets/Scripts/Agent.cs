using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField] int   m_SpeechAnimationsCount;
	[SerializeField] float m_RotationSpeed = 1f;

	// PRIVATE MEMBERS

	private Animator       m_Animator;
	private NavMeshAgent   m_NavAgent;

	private Vector3        m_PointPosition;
	private Quaternion     m_PointRotation;

	private float          m_IdleStart;
	private float          m_IdleTime;

	private bool           m_StateUpdated;

	// PUBLIC MEMBERS

	public bool            IsIdle             { get; private set; }
	public bool            IsFinished         { get { return IsIdle == true && m_IdleStart + m_IdleTime < Time.time; } }
    public int             Id;

    // SIGNALS

    public System.Action   StateUpdated;

	// PUBLIC METHODS

	public void GoToPoint(Vector3 position, Quaternion rotation, float idleTime)
	{
		m_NavAgent.SetDestination(position);

		m_PointPosition = position;
		m_PointRotation = rotation;
		m_IdleTime = idleTime;
	}

	// MONOBEHAVIOUR

	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
		m_NavAgent = GetComponent<NavMeshAgent>();

		m_NavAgent.updateRotation = false;
	}

	private void Start()
	{
		UpdateMovement(true);
	}

	private void Update()
	{
		m_PointPosition.y = transform.position.y;
		bool isIdle = m_PointPosition.AlmostEquals(transform.position, 0.1f);

		if (isIdle != IsIdle)
		{
			UpdateMovement(isIdle);
			UpdateSpeech();
		}

		Quaternion targetRotation = isIdle == true ? m_PointRotation : Quaternion.LookRotation(m_PointPosition - transform.position, Vector3.up);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotationSpeed);

		if (m_StateUpdated == true)
		{
			StateUpdated.SafeInvoke();
			m_StateUpdated = false;
		}
	}

	private void OnDestroy()
	{
		StateUpdated = null;
	}

	// PRIVATE METHODS

	private void UpdateMovement(bool isIdle)
	{
		m_Animator.SetBool(AnimationID.IsWalking, isIdle == false);

		IsIdle = isIdle;

		if (isIdle == true)
		{
			m_IdleStart = Time.time;
		}

		m_StateUpdated = true;
	}

	private void UpdateSpeech()
	{
		int speech = IsIdle == true ? Random.Range(0, m_SpeechAnimationsCount) : -1;
		m_Animator.SetInteger(AnimationID.Speech, speech);

		m_StateUpdated = true;
	}
}
