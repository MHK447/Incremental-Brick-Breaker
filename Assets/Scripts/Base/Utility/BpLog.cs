using System.Diagnostics;

public class BpLog
{
	[Conditional("BANPOFRI_LOG")]
	public static void Log(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("BANPOFRI_LOG")]
	public static void LogWarning(string message)
	{
		UnityEngine.Debug.LogWarning(message);
	}

	[Conditional("BANPOFRI_LOG")]
	public static void LogError(string message)
	{
		UnityEngine.Debug.LogError(message);
	}
}