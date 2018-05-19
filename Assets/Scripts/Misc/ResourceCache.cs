using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceCache<T> where T : Component
{
	private List<T> m_Cache;

	private int m_Increment;
	private T m_Prefab;

	// PUBLIC METHODS

	public void Initialize(T prefab, int amount, int increment = 1)
	{
		if (prefab == null)
		{
			Debug.LogError("ResourceCache::Cache initialization failed - prefab is null.");
			return;
		}

		m_Prefab = prefab;
		m_Increment = increment > 0 ? increment : 1;

		m_Cache = new List<T>(amount);

		for (int i = 0; i < amount; i++)
		{
			var obj = InstantiateObject(m_Prefab);
			m_Cache.Add(obj);
		}
	}

	public void Initialize(string resourcePath, int amount, int increment = 1)
	{
		if (string.IsNullOrEmpty(resourcePath) == true)
		{
			Debug.LogError("ResourceCache::Cache initialization failed - parameter resourcePath is null or empty.");
			return;
		}

		var prefab = Resources.Load(resourcePath, typeof(T)) as T;

		Initialize(prefab, amount, increment);
	}

	public void Deinitialize()
	{
		for (int i = 0; i < m_Cache.Count; i++)
		{
			GameObject.Destroy(m_Cache[i]);
		}

		m_Cache.Clear();
	}

	public T Get()
	{
		if (m_Cache.Count == 0)
		{
			for (int i = 0; i < m_Increment; i++)
			{
				m_Cache.Add(InstantiateObject(m_Prefab));
			}
		}

		var obj = m_Cache[m_Cache.Count - 1];
		m_Cache.Remove(obj);

		return obj;
	}

	//public T[] GetRange(int count)
	//{
	//	if (count < 0)
	//		return null;


	//}

	public void Return(T obj)
	{
		obj.gameObject.SetActive(false);
		m_Cache.Add(obj);
	}

	// PRIVATE METHODS

	private T InstantiateObject(T prefab)
	{
		var obj = GameObject.Instantiate(prefab);
		obj.gameObject.SetActive(false);

		return obj;
	}
}
