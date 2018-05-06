using UnityEngine;

public static class ComponentExtensions
{
	public static T GetChild<T>(this Component parent, bool necessary = false) where T : Component
	{
		return GetChild<T>(parent, "", necessary);
	}

	public static T GetChild<T>(this Component parent, string name = "", bool necessary = false) where T : Component
	{
		if (parent != null)
		{
			var children = parent.GetComponentsInChildren<T>(true);

			bool ignoreName = string.IsNullOrEmpty(name);

			for (int i = 0; i < children.Length; i++)
			{
				if (ignoreName == true || children[i].name == name)
					return children[i];
			}
		}

		if (necessary == true)
		{
			Debug.LogErrorFormat("The child of object '{0}' with name '{1}' and type '{2}' could not be found.", parent.name, name, typeof(T).Name);
		}

		return null;
	}

	public static T Duplicate<T>(this T @this) where T : Component
	{
		return GameObject.Instantiate<T>(@this, @this.transform.position, @this.transform.rotation, @this.transform.parent);
	}
}
