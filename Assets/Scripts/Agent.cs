using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField] int   m_LaughAnimationsCount;

	// PRIVATE MEMBERS

	private Animator       m_Animator;
	private NavMeshAgent   m_NavAgent;

	// PUBLIC MEMBERS

	public bool            IsIdle             { get; private set; }

	// PUBLIC METHODS

	public void GoToPosition(Vector3 position)
	{
		m_NavAgent.SetDestination(position);
	}

	// MONOBEHAVIOUR

	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
		m_NavAgent = GetComponent<NavMeshAgent>();
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
