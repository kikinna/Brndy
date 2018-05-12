﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
	// INSPECTOR FIELDS

	[SerializeField] Vector2           m_IdleInterval       = new Vector2(3f, 5f);
	[SerializeField] Transform[]       m_InterestingPoints;
	[SerializeField] int               m_AgentsCount;
	[SerializeField] Agent[]           m_AgentPrefabs;

	// PRIVATE MEMBERS

	private List<Agent>                m_Agents             = new List<Agent>(48);
	private List<AgentPoint>           m_AgentPoints        = new List<AgentPoint>(48);

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

		for (int i = 0; i < m_AgentsCount; i++)
		{
			var prefab = m_AgentPrefabs[Random.Range(0, m_AgentPrefabs.Length)];
			var agent = Instantiate(prefab, transform);

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

        agent.GoToPoint(point.Transform.position, point.Transform.rotation, idleTime);
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
        float x = 0, z = 0, r = 0;
        float y = 0;
        //if (midi.status == 0xB0) // x
        //{
        //    Debug.Log(midi.data2);
        //    x = ConvertRange(0, 127, -9, 9, midi.data2);
        //    Debug.Log("new Value: " + x);
        //}
        if (midi.status == 0xB1) // y => z
        {
            Debug.Log(midi.data2);
            z = ConvertRange(0, 127, -9, 9, midi.data2);
            Debug.Log("new Value: " + z);
        }
        //if (midi.status == 0xB2) // r
        //{
        //    Debug.Log(midi.data2);
        //    r = ConvertRange(0, 127, -9, 9, midi.data2);
        //    Debug.Log("new Value: " + r);
        //}
        var agent = m_Agents[0];
        float idleTime = Random.Range(m_IdleInterval.x, m_IdleInterval.y);
        x = Random.Range(-9, 9);
        agent.GoToPoint(new Vector3(x, 0, z), new Quaternion(), idleTime);
    }

    private float ConvertRange(int originalStart, int originalEnd, // original range
                               float newStart, float newEnd, // desired range
                               int value) // value to convert
    {
        float scale = (float)(newEnd - newStart) / (originalEnd - originalStart);
        return (float)(newStart + ((value - originalStart) * scale));
    }
}
