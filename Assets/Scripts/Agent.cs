using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField] int   m_LaughAnimationsCount;
	[SerializeField] float m_RotationSpeed = 1f;

	// PRIVATE MEMBERS

	private Animator       m_Animator;
	private NavMeshAgent   m_NavAgent;

	private Quaternion     m_PointRotation;

	// PUBLIC MEMBERS

	public bool            IsIdle             { get; private set; }

	// PUBLIC METHODS

	public void GoToPoint(Transform point)
	{
		m_NavAgent.SetDestination(point.position);
		m_PointRotation = point.rotation;
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
		int randomLaugh = Random.Range(0, m_LaughAnimationsCount + 1);

		m_Animator.SetInteger(AnimationID.Laugh, randomLaugh);
		m_Animator.SetBool(AnimationID.IsWalking, isIdle == false);

		IsIdle = isIdle;
	}
}
