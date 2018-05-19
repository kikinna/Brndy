using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
	// INSPECTOR FIELDS

	[SerializeField] Vector2           m_IdleInterval       = new Vector2(3f, 5f);

	[SerializeField] Transform[]       m_InterestingPoints;
	[SerializeField] Transform[]       m_OutPoints;

	[SerializeField] AgentSetup[]      m_AgentSetups;

	// PRIVATE MEMBERS

	private List<AgentPoint>           m_AgentPoints        = new List<AgentPoint>(48);

	// PUBLIC METHODS

	public void StartAutoMode()
	{
	}

	public void StopAutoMode()
	{
	}

	public void AgentDisappeared(int id)
	{
		for (int i = 0; i < m_AgentSetups.Length; i++)
		{
			m_AgentSetups[i].Despawn(id);
		}
	}

	public void AgentPositionUpdated(int id, Vector3 position)
	{
		for (int i = 0; i < m_AgentSetups.Length; i++)
		{
			var setup = m_AgentSetups[i];

			if (setup.HasAgent(id) == true)
			{
				setup.UpdateAgentPosition(id, position);
				return;
			}
		}

		int randomStart = Random.Range(0, m_AgentSetups.Length);

		for (int i = randomStart; i < m_AgentSetups.Length; i++)
		{
			var setup = m_AgentSetups[i];

			if (setup.IsAutonomous == true || setup.CanSpawnAgent == false)
				continue;

			setup.UpdateAgentPosition(id, position);
			return;
		}

		for (int i = 0; i < randomStart; i++)
		{
			var setup = m_AgentSetups[i];

			if (setup.IsAutonomous == true || setup.CanSpawnAgent == false)
				continue;

			setup.UpdateAgentPosition(id, position);
			return;
		}
	}

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

		for (int i = 0; i < m_AgentSetups.Length; i++)
		{
			m_AgentSetups[i].Iniatialize(transform);
		}
	}

	private void Update()
	{
		CheckAutonomousAgentsPositions();

		for (int i = 0; i < m_AgentSetups.Length; i++)
		{
			m_AgentSetups[i].ReturnFinished();
		}
	}

	// PRIVATE MEMBERS

	private void CheckAutonomousAgentsPositions()
	{
		for (int i = 0; i < m_AgentSetups.Length; i++)
		{
			var setup = m_AgentSetups[i];

			var freeAgent = setup.GetFreeAutonomousAgent();

			if (freeAgent == null)
				continue;

			FindNewPointForAgent(freeAgent);
		}
	}

	private void FindNewPointForAgent(Agent agent)
	{
		var point = GetFreeAgentPoint();

		if (point == null)
		{
			Debug.LogWarning("No free agent point");
			return;
		}

		FreeAgentPoint(agent);
		point.Agent = agent;

		float idleTime = Random.Range(m_IdleInterval.x, m_IdleInterval.y);
		//Debug.Log("x: " + point.Transform.position.x + " " + "z: " + point.Transform.position.z);
		agent.GoToPoint(point.Transform.position, idleTime);
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

	private void FreeAgentPoint(Agent agent)
	{
		var agentPoint = m_AgentPoints.Find(t => t.Agent == agent);

		if (agentPoint == null)
			return;

		agentPoint.Agent = null;
	}

	// HELPERS

	private class AgentPoint
	{
		public Transform      Transform;
		public Agent          Agent;
		public bool           IsOccupied   { get { return Agent != null; } }

		public AgentPoint(Transform transform)
		{
			Transform = transform;
		}
	}

	[System.Serializable]
	private class AgentSetup
	{
		public Agent                    AgentPrefab;
		public bool                     IsAutonomous;
		public int                      MaxAgents = 5;
		public Transform[]              HandObjectPrefabs;

		public bool                     CanSpawnAgent              { get { return m_Agents.Count < MaxAgents; } }

		private ResourceCache<Agent>    m_AgentCache = new ResourceCache<Agent>();
		private List<Agent>             m_Agents     = new List<Agent>(12);

		private Transform               m_ParentTransform;

		public void Iniatialize(Transform parentTransform)
		{
			m_AgentCache.Initialize(AgentPrefab, Mathf.Min(MaxAgents, 2));

			m_ParentTransform = parentTransform;
		}

		public bool HasAgent(int id)
		{
			if (IsAutonomous == true)
				return false;

			return m_Agents.Find(t => t.ID == id) != null;
		}

		public void UpdateAgentPosition(int id, Vector3 position)
		{
			var agent = m_Agents.Find(t => t.ID == id);

			if (agent == null)
			{
				agent = GetAgent(position);
				agent.ID = id;
			}

			agent.GoToPoint(position);
		}

		public Agent GetFreeAutonomousAgent()
		{
			if (IsAutonomous == false)
				return null;

			for (int i = 0; i < m_Agents.Count; i++)
			{
				var agent = m_Agents[i];

				if (agent.IsWaiting == true)
					return agent;
			}

			return CanSpawnAgent == true ? GetAgent(m_ParentTransform.position) : null;
		}

		public void Despawn(int id)
		{
			var agent = m_Agents.Find(t => t.ID == id);

			if (agent == null)
				return;

			agent.Despawn();
		}

		public void ReturnFinished()
		{
			for (int i = m_Agents.Count - 1; i >= 0; i--)
			{
				var agent = m_Agents[i];

				if (agent.IsFinished == false)
					continue;

				agent.gameObject.SetActive(false);
				agent.RemoveHandObject();

				m_AgentCache.Return(agent);

				m_Agents.Remove(agent);
			}
		}

		private Agent GetAgent(Vector3 initialPosition)
		{
			var agent = m_AgentCache.Get();

			agent.transform.SetParent(m_ParentTransform);
			agent.transform.position = initialPosition;

			agent.gameObject.SetActive(true);
			agent.Spawn();

			int handObjectsCount = HandObjectPrefabs.Length;

			if (handObjectsCount > 0 && agent.CanHoldObject == true)
			{
				var objectPrefab = HandObjectPrefabs[Random.Range(0, handObjectsCount)];
				var objectInstance = objectPrefab != null ? Instantiate(objectPrefab) : null;

				agent.SetHandObject(objectInstance);
			}

			m_Agents.Add(agent);

			return agent;
		}
	}
}
