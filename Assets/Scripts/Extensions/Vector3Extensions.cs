using UnityEngine;

public static class Vector3Extensions
{
	private const float EPSILON = 0.01f;

	public static bool AlmostEquals(this Vector3 @this, Vector3 other, float epsilon = EPSILON)
	{
		if (Mathf.Abs(@this.x - other.x) > epsilon)
			return false;

		if (Mathf.Abs(@this.y - other.y) > epsilon)
			return false;

		if (Mathf.Abs(@this.z - other.z) > epsilon)
			return false;

		return true;
	}
}
