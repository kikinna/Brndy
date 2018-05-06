using System;

public static class ActionExtensions
{
	public static void SafeInvoke(this Action @this)
	{
		if (@this == null)
			return;

		@this();
	}

	public static void SafeInvoke<T>(this Action<T> @this, T arg)
	{
		if (@this == null)
			return;

		@this(arg);
	}

	public static void SafeInvoke<T1, T2>(this Action<T1, T2> @this, T1 arg1, T2 arg2)
	{
		if (@this == null)
			return;

		@this(arg1, arg2);
	}

	public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> @this, T1 arg1, T2 arg2, T3 arg3)
	{
		if (@this == null)
			return;

		@this(arg1, arg2, arg3);
	}

	public static void SafeInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (@this == null)
			return;

		@this(arg1, arg2, arg3, arg4);
	}
}