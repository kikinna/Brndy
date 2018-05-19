using UnityEngine;
using System.Collections.Generic;

public class AgentDirector : MonoBehaviour
{
	// CONSTANTS

	private const int   X_STATUS       = 0xB0;
	private const int   Z_STATUS       = 0xB1;
	private const int   DEAD_STATUS    = 0xB2;

	// CONFIGURATION

	[Header("Director Setup")]
	[SerializeField] float             m_AutoModeAfterSeconds = 5f;

	[Header("Mapping Setup")]
	[SerializeField] Vector2           m_XInMapping           = new Vector2(0f, 127f);
	[SerializeField] Vector2           m_XOutMapping          = new Vector2(-15f, 15f);
	[SerializeField] Vector2           m_ZInMapping           = new Vector2(0f, 127f);
	[SerializeField] Vector2           m_ZOutMapping          = new Vector2(-20f, 7f);

	// PRIVATE MEMBERS

	private AgentManager               m_AgentManager;

	private MidiReceiver               m_Receiver;
	private List<MidiAgentMessage>     m_AgentMessages        = new List<MidiAgentMessage>(48);

	private float                      m_LastAgentUpdate;

	// MONOBEHAVIOUR

	protected void Awake()
	{
		m_Receiver = FindObjectOfType<MidiReceiver>();
		m_AgentManager = GetComponent<AgentManager>();
	}

	protected void Update()
	{
		while (m_Receiver.IsEmpty == false)
		{
			MidiMessageReceived(m_Receiver.PopMessage());
		}

		if (m_AutoModeAfterSeconds > 0f && m_LastAgentUpdate + m_AutoModeAfterSeconds < Time.time)
		{
			m_AgentManager.StartAutoMode();
		}
	}

	// PRIVATE METHODS

	private void AgentDisappeared(int id)
	{
		Debug.Log("DISAPPEARED ID: " + id);

		m_AgentManager.AgentDisappeared(id);
	}

	private void AgentPositionUpdated(int id, Vector3 position)
	{
		Debug.Log("UPDATED ID: " + id + " POSITION: " + position);

		m_AgentManager.AgentPositionUpdated(id, position);
		m_AgentManager.StopAutoMode();

		m_LastAgentUpdate = Time.time;
	}

	private void MidiMessageReceived(MidiMessage midiMessage)
	{
		var agentMessage = m_AgentMessages.Find(t => t.ID == midiMessage.data1);

		if (agentMessage == null)
		{
			agentMessage = new MidiAgentMessage(midiMessage.data1);
			m_AgentMessages.Add(agentMessage);
		}

		switch (midiMessage.status)
		{
			case X_STATUS:
				agentMessage.SetX(MathUtils.Map(m_XInMapping, m_XOutMapping, midiMessage.data2));
				break;
			case Z_STATUS:
				agentMessage.SetZ(MathUtils.Map(m_ZInMapping, m_ZOutMapping, midiMessage.data2));
				break;
			case DEAD_STATUS:
				m_AgentMessages.Remove(agentMessage);
				AgentDisappeared(agentMessage.ID);
				return;
		}

		if (agentMessage.IsCompleted == true)
		{
			AgentPositionUpdated(agentMessage.ID, agentMessage.Position);
			agentMessage.Reset();
		}
	}
}
