using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
	// INSPECTOR FIELDS

	[SerializeField] float             m_CheckInterval      = 0.3f;
	[SerializeField] Transform[]       m_InterestingPoints;
	[SerializeField] int               m_AgentsCount;
	[SerializeField] Agent             m_AgentPrefab;

	// PRIVATE MEMBERS

	private List<Agent>                m_Agents             = new List<Agent>(48);
	private List<AgentPoint>           m_AgentPoints        = new List<AgentPoint>(48);

	private float                      m_LastCheck;

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
			var agent = i == 0 ? m_AgentPrefab : Instantiate(m_AgentPrefab, transform);
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
		if (m_LastCheck + m_CheckInterval < Time.time)
		{
			CheckAgentsPositions();
		}
	}

	// PRIVATE MEMBERS

	private void CheckAgentsPositions()
	{
		for (int i = 0; i < m_Agents.Count; i++)
		{
			var agent = m_Agents[i];

			if (agent.IsIdle == false)
				continue;

			FindNewPointForAgent(agent, i);
		}

		m_LastCheck = Time.time;
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

		agent.GoToPoint(point.Transform);
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
}
