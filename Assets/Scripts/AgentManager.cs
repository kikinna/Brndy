using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
	// INSPECTOR FIELDS

	[SerializeField] Vector2           m_IdleInterval       = new Vector2(3f, 5f);
	[SerializeField] Transform[]       m_InterestingPoints;
	[SerializeField] int               m_AgentsCount;
	[SerializeField] Agent[]           m_AgentPrefabs;
	[SerializeField] Transform[]       m_HandObjectPrefabs;

	// PRIVATE MEMBERS

	private List<Agent>                m_Agents             = new List<Agent>(48);
	private List<AgentPoint>           m_AgentPoints        = new List<AgentPoint>(48);
	private List<MidiColector>         m_MidiColectors      = new List<MidiColector>(48);

    // MONOBEHAVIOUR

    private void Awake()
	{
		for (int i = 0; i < m_InterestingPoints.Length; i++)
		{
			var point = m_InterestingPoints[i];

			if (point == null)
				continue;

			m_AgentPoints.Add(new AgentPoint(point));
		}

		if (m_InterestingPoints.Length < m_AgentsCount + 1)
		{
			Debug.LogError("Not enough interesting points");
			return;
		}

		int handObjectsCount = m_HandObjectPrefabs.Length;

		for (int i = 0; i < m_AgentsCount; i++)
		{
			var prefab = m_AgentPrefabs[Random.Range(0, m_AgentPrefabs.Length)];
			var agent = Instantiate(prefab, transform);

			if (handObjectsCount > 0 && agent.CanHoldObject == true)
			{
				var objectPrefab = m_HandObjectPrefabs[Random.Range(0, handObjectsCount)];
				var objectInstance = objectPrefab != null ? Instantiate(objectPrefab) : null;

				agent.SetHandObject(objectInstance);
			}

			agent.gameObject.SetActive(true);

			m_Agents.Add(agent);
		}
	}

	private void Start()
	{
		for (int i = 0; i < m_Agents.Count; i++)
		{
			FindNewPointForAgent(m_Agents[i], i);
		}
	}

	private void Update()
	{
		CheckAgentsPositions();
	}

	// PRIVATE MEMBERS

	private void CheckAgentsPositions()
	{
		for (int i = 0; i < m_Agents.Count; i++)
		{
			var agent = m_Agents[i];

			if (agent.IsFinished == false)
				continue;

			FindNewPointForAgent(agent, i);
		}
	}

	private void FindNewPointForAgent(Agent agent, int agentIndex)
	{
		var point = GetFreeAgentPoint();

		if (point == null)
		{
			Debug.LogWarning("No free agent point");
			return;
		}

		FreeAgentPoint(agentIndex);

		point.IsOccupied = true;
		point.AgentIndex = agentIndex;

		float idleTime = Random.Range(m_IdleInterval.x, m_IdleInterval.y);
        //Debug.Log("x: " + point.Transform.position.x + " " + "z: " + point.Transform.position.z);
        //agent.GoToPoint(point.Transform.position, point.Transform.rotation, idleTime);
    }

	private AgentPoint GetFreeAgentPoint()
	{
		if (m_AgentPoints.Count == 0)
			return null;

		int randomStart = Random.Range(0, m_AgentPoints.Count);

		for (int i = randomStart; i < m_AgentPoints.Count; i++)
		{
			var point = m_AgentPoints[i];

			if (point.IsOccupied == false)
				return point;
		}

		for (int i = 0; i < randomStart; i++)
		{
			var point = m_AgentPoints[i];

			if (point.IsOccupied == false)
				return point;
		}

		return null;
	}

	private void FreeAgentPoint(int agentIndex)
	{
		var agentPoint = m_AgentPoints.Find(t => t.AgentIndex == agentIndex);

		if (agentPoint == null)
			return;

		agentPoint.IsOccupied = false;
		agentPoint.AgentIndex = -1;
	}

	// HELPERS

	private class AgentPoint
	{
		public Transform      Transform;
		public bool           IsOccupied;
		public int            AgentIndex;

		public AgentPoint(Transform transform)
		{
			Transform = transform;
		}
	}

    // Listen to MIDI reciever
    void OnNoteOn(MidiMessage midi)
    {
        var newMidi = m_MidiColectors.Find(t => t.Id == midi.data1);
        if (newMidi == null)
        {
            newMidi = new MidiColector(midi.data1);
            m_MidiColectors.Add(newMidi);
        }

        if (midi.status == 0xB0) // x
        {
            newMidi.recievedMessages++;
            newMidi.X = midi.data2;
        }
        if (midi.status == 0xB1) // y => z
        {
            newMidi.recievedMessages++;
            newMidi.Z = midi.data2;
        }
        if (midi.status == 0xB2) // Dead agent message
        {
            m_MidiColectors.Remove(newMidi);
            var agent = m_Agents.Find(t => t.ID == newMidi.Id);
            agent.gameObject.SetActive(false);
            m_Agents.Remove(agent);
        }

        if (newMidi.Xset && newMidi.Zset && newMidi.recievedMessages == newMidi.ExpectedMessageCount)
        {
            var agent = m_Agents.Find(t => t.ID == newMidi.Id);
            if (agent == null)
            {
                var prefab = m_AgentPrefabs[Random.Range(0, m_AgentPrefabs.Length)];
                agent = Instantiate(prefab, new Vector3(newMidi.X, 0, newMidi.Z), new Quaternion());
                agent.ID = newMidi.Id;
                agent.gameObject.SetActive(true);

                m_Agents.Add(agent);
            }
            else
            {
                float idleTime = Random.Range(m_IdleInterval.x, m_IdleInterval.y);
                Debug.Log("ID: " + agent.ID + " x: " + newMidi.X + " z: " + newMidi.Z);
                agent.GoToPoint(new Vector3(newMidi.X, 0, newMidi.Z), new Quaternion(), idleTime);
            }

            newMidi.reset();
        }
    }
}

public class MidiColector
{
    public bool Xset = false;
    public bool Zset = false;
    public int ExpectedMessageCount = 2;
    public int recievedMessages = 0;

    public int  Id;
    public float X {
        get { return m_X; }
        set
        {
            m_X = -ConvertRange(0, 127, -15, 15, value); // first two values must be set by width and height of processing window
            Xset = true;
        }
    }
    public float Z
    {
        get { return m_Z; }
        set
        {
            m_Z = ConvertRange(0, 127, -20, 7, value);
            Zset = true;
        }
    }

    private float m_X;
    private float m_Z;

    public MidiColector(int id)
    {
        Id = id;
    }

    public void reset()
    {
        Xset = false;
        Zset = false;
        recievedMessages = 0;
    }

    private float ConvertRange(int originalStart, int originalEnd, // original range
                               float newStart, float newEnd, // desired range
                               float value) // value to convert
    {
        float scale = (float)(newEnd - newStart) / (originalEnd - originalStart);
        return (float)(newStart + ((value - originalStart) * scale));
    }
}
