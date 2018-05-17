using UnityEngine;

public static class NumericExtensions
{
	private const float EPSILON = 0.01f;

	public static bool AlmostEquals(this float @this, float other, float epsilon = EPSILON)
	{
		return Mathf.Abs(@this - other) <= epsilon;
	}
}
