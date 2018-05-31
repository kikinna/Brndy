using UnityEngine;

public class MidiAgentMessage
{
	// PUBLIC MEMBERS

	public int        ID               { get; private set; }
	public bool       IsCompleted      { get { return m_XSet == true && m_ZSet == true; } }
	public Vector3    Position         { get { return m_Position; } }

	// PRIVATE MEMBERS

	private bool      m_XSet;
	private bool      m_ZSet;

	private Vector3   m_Position;

	// C - TOR

	public MidiAgentMessage(int id)
	{
		ID = id;
	}

	// PUBLIC METHODS

	public void SetX(float value)
	{
        m_Position.x = -value;
		m_XSet = true;
	}

	public void SetZ(float value)
	{
		m_Position.z = value;
		m_ZSet = true;
	}

	public void Reset()
	{
		m_XSet = false;
		m_ZSet = false;
	}
}
