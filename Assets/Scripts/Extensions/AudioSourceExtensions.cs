using System.Collections;
using UnityEngine;

public static class AudioSourceExtensions
{
	public static void FadeOut(this AudioSource @this, float duration = 0.5f, MonoBehaviour behaviour = null)
	{
		if (@this == null)
			return;

		if (@this.isPlaying == false)
			return;

		if (duration < 0.01f)
			@this.Stop();

		if (behaviour != null)
		{
			behaviour.StartCoroutine(FadeOut_Coroutine(@this, duration));
		}
		else
		{
			Coroutines.StartCoroutine(FadeOut_Coroutine(@this, duration));
		}
	}

	private static IEnumerator FadeOut_Coroutine(AudioSource source, float duration)
	{
		float startTime = Time.time;
		float startVolume = source.volume;

		float progress = 0f;

		while (progress < 1f)
		{
			progress = (Time.time - startTime) / duration;
			source.volume = Mathf.Lerp(startVolume, 0f, progress);

			yield return null;
		}

		source.Stop();
		source.volume = startVolume;
	}
}
