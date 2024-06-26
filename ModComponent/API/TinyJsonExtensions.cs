﻿using MelonLoader.TinyJSON;
using ModComponent.Utils;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ModComponent.API;

internal static class TinyJsonExtensions
{
	internal static bool ContainsKey(this ProxyObject dict, string key)
	{
		foreach (KeyValuePair<string, Variant> pair in dict)
		{
			if (pair.Key == key)
			{
				return true;
			}
		}
		return false;
	}

	internal static Variant? GetVariant(this ProxyObject dict, string className, string fieldName)
	{
		Variant subDict;
		try
		{
			subDict = dict[className];
		}
		catch (KeyNotFoundException ex)
		{
			Logger.LogError($"The json doesn't have an entry for '{className}'");
			//			throw new Exception(, ex);
			return null;
		}
		try
		{
			return subDict[fieldName];
		}
		catch (KeyNotFoundException ex)
		{
			Logger.LogError($"The '{className}' entry in the json doesn't have a field for '{fieldName}'");
			//			throw new Exception($"The '{className}' entry in the json doesn't have a field for '{fieldName}'", ex);
			return null;
		}
	}

	internal static Variant? GetVariantOrNull(this ProxyObject dict, string className, string fieldName)
	{
		Variant subDict;
		try
		{
			subDict = dict[className];
		}
		catch (KeyNotFoundException ex)
		{
			return null;
		}
		try
		{
			return subDict[fieldName];
		}
		catch (KeyNotFoundException ex)
		{
			return null;
		}
	}

	internal static string? GetStringOrNull(this ProxyObject dict, string className, string fieldName)
	{
		Variant subDict;
		try
		{
			subDict = dict[className];
		}
		catch (KeyNotFoundException ex)
		{
			Logger.LogError($"The json doesn't have an entry for '{className}'");
			return null;
			//			throw new Exception($"The json doesn't have an entry for '{className}'", ex);
		}
		try
		{
			return subDict[fieldName].ToString();
		}
		catch (KeyNotFoundException ex)
		{
			return null;
		}
	}

	internal static bool GetBool(this ProxyObject dict, string className, string fieldName, bool _default = false)
	{
		Variant subDict;
		try
		{
			subDict = dict[className];
		}
		catch (KeyNotFoundException ex)
		{
			Logger.LogError($"The json doesn't have an entry for '{className}'");
			return _default;
		}
		try
		{
			return bool.Parse(subDict[fieldName]);
		}
		catch (KeyNotFoundException ex)
		{
			return _default;
		}
	}

	internal static int GetInt(this ProxyObject dict, string className, string fieldName, int _default = 0)
	{
		Variant subDict;
		try
		{
			subDict = dict[className];
		}
		catch (KeyNotFoundException ex)
		{
			Logger.LogError($"The json doesn't have an entry for '{className}'");
			//			throw new Exception($"The json doesn't have an entry for '{className}'", ex);
			return _default;
		}
		try
		{
			return int.Parse(subDict[fieldName], CultureInfo.InvariantCulture);
		}
		catch (KeyNotFoundException ex)
		{
			return _default;
		}
	}

	internal static float GetFloat(this ProxyObject dict, string className, string fieldName, float _default = 0f)
	{
		Variant subDict;
		try
		{
			subDict = dict[className];
		}
		catch (KeyNotFoundException ex)
		{
			Logger.LogError($"The json doesn't have an entry for '{className}'");
			//			throw new Exception($"The json doesn't have an entry for '{className}'", ex);
			return _default;
		}
		try
		{
			return float.Parse(subDict[fieldName], CultureInfo.InvariantCulture);
		}
		catch (KeyNotFoundException ex)
		{
			return _default;
		}
	}

	internal static T GetEnum<T>(this ProxyObject dict, string className, string fieldName) where T : Enum
	{
		return EnumUtils.ParseEnum<T>(dict.GetStringOrNull(className, fieldName));
	}

	internal static ProxyArray GetProxyArray(this ProxyObject dict, string className, string fieldName)
	{
		ProxyArray array = dict.GetVariant(className, fieldName) as ProxyArray;
		if (array == null)
		{
			Logger.LogError($"The field '{fieldName}' in entry '{className}' is not an array");
			return new ProxyArray();
		}

		return array;
	}

	internal static ProxyArray GetProxyArrayOrEmpty(this ProxyObject dict, string className, string fieldName)
	{
		var checkArray = dict.GetVariantOrNull(className, fieldName);
		if (checkArray == null)
		{
			return new ProxyArray();
		}

		ProxyArray array = checkArray as ProxyArray;

		return array;
	}

	private static float[] ConvertToFloatArray(this ProxyArray proxy)
	{
		List<float> result = new List<float>();
		foreach (Variant? item in proxy)
		{
			result.Add(item);
		}
		return result.ToArray();
	}

	internal static float[] GetFloatArray(this ProxyObject dict, string className, string fieldName)
	{
		return dict.GetProxyArray(className, fieldName).ConvertToFloatArray();
	}

	private static int[] ConvertToIntArray(this ProxyArray proxy)
	{
		List<int> result = new List<int>();
		foreach (Variant? item in proxy)
		{
			result.Add(item);
		}
		return result.ToArray();
	}

	internal static int[] GetIntArray(this ProxyObject dict, string className, string fieldName)
	{
		return dict.GetProxyArray(className, fieldName).ConvertToIntArray();
	}

	private static string[] ConvertToStringArray(this ProxyArray proxy)
	{
		List<string> result = new List<string>();
		foreach (Variant? item in proxy)
		{
			result.Add(item);
		}
		return result.ToArray();
	}

	internal static string[] GetStringArray(this ProxyObject dict, string className, string fieldName)
	{
		return dict.GetProxyArray(className, fieldName).ConvertToStringArray();
	}

	internal static string[] GetStringArrayOrEmpty(this ProxyObject dict, string className, string fieldName)
	{
		return dict.GetProxyArrayOrEmpty(className, fieldName).ConvertToStringArray();
	}

	private static Vector3 ConvertToVector3(this Variant array)
	{
		return new Vector3((float)array[0], (float)array[1], (float)array[2]);
	}

	internal static Vector3 GetVector3(this ProxyObject dict, string className, string fieldName)
	{
		return dict.GetVariant(className, fieldName).ConvertToVector3();
	}
}
