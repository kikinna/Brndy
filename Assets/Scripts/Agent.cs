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

	private Quaternion     m_PointRotation;

	private float          m_IdleStart;
	private float          m_IdleTime;

	// PUBLIC MEMBERS

	public bool            IsIdle             { get; private set; }
	public bool            IsFinished         { get { return IsIdle == true && m_IdleStart + m_IdleTime < Time.time; } }

	// SIGNALS

	public System.Action   StateUpdated;

	// PUBLIC METHODS

	public void GoToPoint(Vector3 position, Quaternion rotation, float idleTime)
	{
		m_NavAgent.SetDestination(position);

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
		bool isIdle = m_NavAgent.destination.AlmostEquals(transform.position);

		if (isIdle != IsIdle)
		{
			UpdateMovement(isIdle);
		}

		Quaternion targetRotation = isIdle == true ? m_PointRotation : Quaternion.LookRotation(m_NavAgent.destination - transform.position, Vector3.up);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotationSpeed);
	}

	// PRIVATE METHODS

	private void UpdateMovement(bool isIdle)
	{
		int randomSpeech = Random.Range(0, m_SpeechAnimationsCount);

		m_Animator.SetInteger(AnimationID.Speech, isIdle == true ? randomSpeech : -1);
		m_Animator.SetBool(AnimationID.IsWalking, isIdle == false);

		IsIdle = isIdle;

		if (isIdle == true)
		{
			m_IdleStart = Time.time;
		}

		if (StateUpdated != null)
		{
			StateUpdated.Invoke();
		}
	}
}
