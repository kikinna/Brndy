using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coroutines : MonoBehaviour
{
	private static Coroutines m_Instance;

	public static new Coroutine StartCoroutine(IEnumerator routine)
	{
		if (m_Instance == null)
		{
			m_Instance = new GameObject("Coroutines", typeof(Coroutines)).GetComponent<Coroutines>();
			m_Instance.hideFlags = HideFlags.HideAndDontSave;
		}

		return (m_Instance as MonoBehaviour).StartCoroutine(routine);
	}
}
