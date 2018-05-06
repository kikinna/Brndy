using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]
public class AgentAudio : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField] float        m_FadeOutTime = 0.3f;
	[SerializeField] ClipSetup[]  m_SpeechClips;

	// PRIVATE MEMBERS

	private Agent          m_Agent;
	private Animator       m_Animator;

	private AudioSource    m_FootAudioSource;
	private AudioSource    m_HeadAudioSource;
	private AudioSource    m_SpeechAudioSource;

	// MONOBEHAVIOUR

	private void Awake()
	{
		m_Agent = GetComponent<Agent>();
		m_Animator = GetComponent<Animator>();

		m_FootAudioSource = this.GetChild<AudioSource>("FootAudioSource");
		m_HeadAudioSource = this.GetChild<AudioSource>("HeadAudioSource");
		m_SpeechAudioSource = this.GetChild<AudioSource>("SpeechAudioSource");

		m_Agent.StateUpdated += OnAgentStateUpdated;
	}

	private void OnEnable()
	{
		UpdateState();
	}

	private void OnDestroy()
	{
		if (m_Agent != null)
		{
			m_Agent.StateUpdated -= OnAgentStateUpdated;
		}
	}

	// HANDLERS

	private void OnAgentStateUpdated()
	{
		UpdateState();
	}

	// PRIVATE METHODS

	private void UpdateState()
	{
		// foot

		bool isWalking = m_Animator.GetBool(AnimationID.IsWalking);

		if (isWalking == true)
		{
			PlaySound(m_FootAudioSource);
		}
		else
		{
			StopSound(m_FootAudioSource, m_FadeOutTime);
		}

		// speech

		int speech = m_Animator.GetInteger(AnimationID.Speech);
		var speechSetup = GetClipSetup(m_SpeechClips, speech);

		if (speechSetup != null)
		{
			PlaySound(m_SpeechAudioSource, speechSetup.Clip, speechSetup.Delay);
		}
		else
		{
			StopSound(m_SpeechAudioSource, m_FadeOutTime);
		}
	}

	// HELPERS

	private void PlaySound(AudioSource audioSource, AudioClip clip, float delay = 0f)
	{
		if (audioSource == null)
			return;

		audioSource.clip = clip;
		PlaySound(audioSource, delay);
	}

	private void PlaySound(AudioSource audioSource, float delay = 0f)
	{
		if (audioSource == null)
			return;

		if (delay < 0.01f)
		{
			audioSource.Play();
		}
		else
		{
			audioSource.PlayDelayed(delay);
		}
	}

	private void StopSound(AudioSource audioSource, float fadeDuration = 0f)
	{
		if (audioSource == null)
			return;

		audioSource.FadeOut(fadeDuration, this);
	}

	private ClipSetup GetClipSetup(ClipSetup[] clips, int index)
	{
		if (clips == null || index < 0 || index >= clips.Length)
			return null;

		return clips[index];
	}

	[System.Serializable]
	public class ClipSetup
	{
		public AudioClip Clip;
		public float     Delay;
	}
}
