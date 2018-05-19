using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
	// CONSTANTS

	private static readonly int  ALPHA_ID     = Shader.PropertyToID("Vector1_6A5C60BC");

	// CONFIGURATION

	[SerializeField] int         m_SpeechAnimationsCount;
	[SerializeField] float       m_RotationSpeed           = 1f;

	[Header("Hand Setup")]
	[SerializeField] Transform   m_HandTransform;
	[SerializeField] Vector3     m_HandPositionOffset;
	[SerializeField] Vector3     m_HandRotationOffset;

	// PRIVATE MEMBERS

	private Animator             m_Animator;
	private NavMeshAgent         m_NavAgent;

	private Transform            m_HandObject;

	private Vector3              m_PointPosition;
	private Quaternion           m_PointRotation;

	private float                m_IdleStart;
	private float                m_IdleTime;

	private bool                 m_StateUpdated;

	private Material             m_AgentMaterial;
	private Material             m_HandObjectMaterial;

	private bool                 m_UpdateAlpha;
	private float                m_StartAlpha   = 1f;
	private float                m_TargetAlpha  = 1f;
	private float                m_FadeStart;
	private float                m_FadeDuration;

	// PUBLIC MEMBERS

	public int                   ID;
	public float                 Alpha              { set { SetAlpha(value); m_StartAlpha = m_TargetAlpha = value; } }
	public bool                  IsIdle             { get; private set; }
	public bool                  IsFinished         { get { return IsIdle == true && m_IdleStart + m_IdleTime < Time.time; } }
	public bool                  CanHoldObject      { get { return m_HandTransform != null; } }

	// SIGNALS

	public System.Action         StateUpdated;

	// PUBLIC METHODS

	public void GoToPoint(Vector3 position, Quaternion rotation, float idleTime = 0f)
	{
		m_NavAgent.SetDestination(position);

		m_PointPosition = position;
		m_PointRotation = rotation;
		m_IdleTime = idleTime;
	}

	public void SetHandObject(Transform handObject)
	{
		if (m_HandTransform == null)
			return;

		if (m_HandObject != null)
		{
			Destroy(m_HandObject);
			Destroy(m_HandObjectMaterial);

			m_HandObject = null;
			m_HandObjectMaterial = null;
		}

		if (handObject == null)
			return;

		m_HandObject = handObject;
		m_HandObjectMaterial = handObject.GetComponentInChildren<Renderer>().material;

		m_HandObject.SetParent(m_HandTransform);
		m_HandObject.gameObject.SetActive(true);

		m_HandObject.localPosition = m_HandPositionOffset;
		m_HandObject.localRotation = Quaternion.Euler(m_HandRotationOffset);
		m_HandObject.localScale = Vector3.one;
	}

	public void FadeIn(float duration = 0.5f)
	{
		m_StartAlpha = GetAlpha();
		m_TargetAlpha = 1f;

		m_FadeStart = Time.time;
		m_FadeDuration = duration;
	}

	public void FadeOut(float duration = 0.5f)
	{
		m_StartAlpha = GetAlpha();
		m_TargetAlpha = 0f;

		m_FadeStart = Time.time;
		m_FadeDuration = duration;
	}

	// MONOBEHAVIOUR

	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
		m_NavAgent = GetComponent<NavMeshAgent>();

		var renderer = GetComponentInChildren<Renderer>();
		m_AgentMaterial = renderer.material;
		m_UpdateAlpha = m_AgentMaterial.HasProperty(ALPHA_ID);

		m_NavAgent.updateRotation = false;
	}

	private void Start()
	{
		UpdateMovement(true);
	}

	private void Update()
	{
		m_PointPosition.y = transform.position.y;
		bool isIdle = m_NavAgent.velocity.AlmostEquals(Vector3.zero);

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

		UpdateAlpha();
	}

	private void OnDestroy()
	{
		Destroy(m_AgentMaterial);
		Destroy(m_HandObjectMaterial);

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

	private void UpdateAlpha()
	{
		if (m_UpdateAlpha == false)
			return;

		if (GetAlpha().AlmostEquals(m_TargetAlpha) == true)
			return;

		if (m_FadeDuration == 0f)
		{
			SetAlpha(m_TargetAlpha);
			return;
		}

		float progress = Mathf.Clamp01((Time.time - m_FadeStart) / m_FadeDuration);
		SetAlpha(Mathf.Lerp(m_StartAlpha, m_TargetAlpha, progress));
	}

	private float GetAlpha()
	{
		return m_AgentMaterial.GetFloat(ALPHA_ID);
	}

	private void SetAlpha(float alpha)
	{
		m_AgentMaterial.SetFloat(ALPHA_ID, alpha);

		if (m_HandObjectMaterial != null)
		{
			m_HandObjectMaterial.SetFloat(ALPHA_ID, alpha);
		}
	}
}
