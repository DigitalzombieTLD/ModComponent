﻿using MelonLoader;
using UnityEngine;

namespace ModComponentMain
{
	internal class Implementation : MelonMod
	{
		public override void OnApplicationStart()
		{
			//ModComponentMapper.TestFunctions.TestFunction();

			InitialLogStatements();
			Settings.instance.AddToModSettings("ModComponent");

			//ModComponentMapper.TestFunctions.TestFunction();

			ModComponentMapper.MapperCore.InitializeAndMapAssets();
		}

		private void InitialLogStatements()
		{
			Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
			Logger.LogDebug("Debug Compilation");
			Logger.LogNotDebug("Release Compilation");
		}

		public static byte[][] GetItemPackHashes() => ModComponentMapper.ZipFileLoader.hashes.ToArray();
	}
}
