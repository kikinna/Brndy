using UnityEngine;

public static class Vector3Extensions
{
	private const float EPSILON = 0.01f;

	public static bool AlmostEquals(this Vector3 @this, Vector3 other)
	{
		if (Mathf.Abs(@this.x - other.x) > EPSILON)
			return false;

		if (Mathf.Abs(@this.y - other.y) > EPSILON)
			return false;

		if (Mathf.Abs(@this.z - other.z) > EPSILON)
			return false;

		return true;
	}
}
