﻿using MelonLoader.Utils;
using ModComponent.Mapper;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace ModComponent.Utils
{
	internal class AssetBundleProcessor
	{
		internal static string tempFolderName { get; set; } = "_ModComponentTemp";
		internal static string tempFolderPath { get; set; } = Path.Combine(MelonEnvironment.ModsDirectory, tempFolderName);

		internal static List<string> catalogFilePaths { get; set; } = new();

		internal static List<string> catalogsLoaded { get; set; } = new();
		internal static Dictionary<string, List<string>> catalogBundleList { get; set; } = new();
		internal static Dictionary<string, string> catalogTestList { get; set; } = new();

		internal static List<string> bundleFilePaths { get; set; } = new();
		internal static List<string> bundleNames { get; set; } = new();
		internal static Dictionary<string, List<string>> bundleAssetList { get; set; } = new();

		internal static void Initialize()
		{
			// ensure temp foler exists
			InitTempFolder();

			// process .modcomponent files & writeout bundles/catalogs
			ZipFileLoader.Initialize();

			// preload the bundles, populate bundleNames & bundleAssetList
			PreloadAssetBundles();

			// load the catalogs in a loop (prevent one from breaking all)
			LoadCatalogs();

			// test the catalogs in a loop (prevent one from breaking all)
			TestCatalogs();

			// now lets map those prefabs
			//MapPrefabs();

		}

		internal static void InitTempFolder()
		{
			if (!Directory.Exists(tempFolderPath))
			{
				Logger.LogDebug("Creating temp folder (" + tempFolderName + ")");
				Directory.CreateDirectory(tempFolderPath);
			}
		}

		internal static void InitTempBundleFolder(string bundleName)
		{
			string bundleFolder = Path.Combine(tempFolderPath, bundleName);
			if (!Directory.Exists(bundleFolder))
			{
				Logger.LogDebug("Creating temp bundle folder (" + bundleFolder + ")");
				Directory.CreateDirectory(bundleFolder);
			}
		}

		internal static void CleanupTempFolder()
		{
			foreach (string bundleFilePath in bundleFilePaths)
			{
				if (bundleFilePath != null && File.Exists(bundleFilePath))
				{
					File.Delete(bundleFilePath);
				}
			}
			foreach (string catalogFilePath in catalogFilePaths)
			{
				if (catalogFilePath != null && File.Exists(catalogFilePath))
				{
					File.Delete(catalogFilePath);
				}
			}

			if (Directory.Exists(tempFolderPath))
			{
				Directory.Delete(tempFolderPath, true);
			}
		}

		internal static void PreloadAssetBundles()
		{
			foreach (string bundleFilePath in bundleFilePaths)
			{

				if (bundleAssetList.ContainsKey(bundleFilePath))
				{
					continue;
				}

				if (bundleFilePath != null && File.Exists(bundleFilePath))
				{
					string bundleFileName = Path.GetFileName(bundleFilePath);
					Logger.LogDebug("Preloading (" + bundleFileName + ")");

					List<string> assetList = new();
					AssetBundle ab = AssetBundle.LoadFromFile(bundleFilePath);
					foreach (string assetName in ab.GetAllAssetNames())
					{
						assetList.Add(assetName);
					}
					bundleAssetList.Add(bundleFilePath, assetList);
					bundleNames.Add(ab.name);
					ab.Unload(true);
				}
			}
		}

		internal static void WriteAssettBundleToDisk(string bundleName, string filename, byte[] data)
		{
			InitTempBundleFolder(bundleName);

			string bundleFilePath = Path.Combine(tempFolderPath, bundleName, filename);
			if (File.Exists(bundleFilePath))
			{
				File.Delete(bundleFilePath);
			}
			FileStream fs = File.Create(bundleFilePath);
			fs.Write(data);
			fs.Close();
			Logger.LogDebug("Bundle Written (" + filename + ")");
			if (!bundleFilePath.Contains("unitybuiltinshaders"))
			{
				bundleFilePaths.Add(bundleFilePath);
			}
		}

		internal static void WriteCatalogToDisk(string bundleName, string filename, string data)
		{
			InitTempBundleFolder(bundleName);

			string catalogName = Path.GetFileNameWithoutExtension(filename);

			string catalogFilePath = Path.Combine(tempFolderPath, bundleName, filename);
			if (File.Exists(catalogFilePath))
			{
				File.Delete(catalogFilePath);
			}

			string? firstAsset = null;
			List<string> catalogBundles = new();
			ModContentCatalog? contentCatalog = System.Text.Json.JsonSerializer.Deserialize<ModContentCatalog>(data);
			if (contentCatalog == null)
			{
				Logger.LogError("Catalog Failed - Could not deserialize json (" + catalogName + ")");
				return;
			}
			if (contentCatalog.m_InternalIds == null || contentCatalog.m_InternalIds.Length <= 0)
			{
				Logger.LogError("Catalog Failed - InternalIds empty (" + catalogName + ")");
				return;
			}
			for (int i = 0; i < contentCatalog.m_InternalIds.Length; i++)
			{
				string line = contentCatalog.m_InternalIds[i];
				string assetExtension = Path.GetExtension(line);
				if (assetExtension == ".bundle" || assetExtension == ".unity3d")
				{
					contentCatalog.m_InternalIds[i] = Path.Combine(tempFolderPath, bundleName, Path.GetFileName(line));
					catalogBundles.Add(Path.GetFileName(line));
				}
				else if (firstAsset == null)
				{
					firstAsset = line;
				}
			}
			if (!catalogName.Contains("unitybuiltinshaders"))
			{
				catalogBundleList.Add(catalogFilePath, catalogBundles);
			}
			contentCatalog.m_LocatorId = catalogName;
			Logger.LogDebug("Catalog m_InternalIds Patched (" + catalogName + ")");

			data = System.Text.Json.JsonSerializer.Serialize<ModContentCatalog>(contentCatalog);

			File.WriteAllText(catalogFilePath, data);
			Logger.LogDebug("Catalog Written (" + catalogName + ")");
			catalogFilePaths.Add(catalogFilePath);
			if (firstAsset != null && !catalogName.Contains("unitybuiltinshaders"))
			{
				catalogTestList.Add(catalogFilePath, firstAsset);
			}

		}

		internal static void LoadCatalogs()
		{
			foreach (KeyValuePair<string, List<string>> item in catalogBundleList)
			{
				string catalogFilePath = item.Key;
				if (item.Value != null && item.Value.Count > 0)
				{

					bool catalogLoaded = LoadCatalog(catalogFilePath);
					if (catalogLoaded == true)
					{
						catalogsLoaded.Add(catalogFilePath);
					}

				}
			}
		}

		internal static bool LoadCatalog(string catalogFilePath)
		{
			if (catalogFilePath == null || catalogFilePath == "")
			{
				Logger.LogError("Catalog Loaded - No Catalog Path");
				return true;
			}

			string catalogName = Path.GetFileNameWithoutExtension(catalogFilePath);
			string catalogExtension = Path.GetExtension(catalogFilePath);

			if (catalogExtension != ".json")
			{
				Logger.LogError("Catalog Failed - Invalid extension (" + catalogExtension + ")");
				return true;
			}

			try
			{
				IResourceLocator catalogLocator = Addressables.LoadContentCatalogAsync(catalogFilePath).WaitForCompletion();
				if (catalogLocator != null && catalogLocator.Keys != null)
				{
					Logger.LogDebug("Catalog Loaded (" + catalogName + ") ");
					return true;
				}
			}
			catch (Exception e)
			{
				Logger.LogError("Catalog Failed (" + catalogName + ") " + e.ToString());
				return false;
			}
			return false;
		}

		internal static void TestCatalogs()
		{
			foreach (string catalogFilePath in catalogsLoaded)
			{
				bool catalogTest = TestCatalog(catalogFilePath);
			}
		}

		internal static bool TestCatalog(string catalogFilePath)
		{
			string catalogName = Path.GetFileNameWithoutExtension(catalogFilePath);

			if (catalogTestList.ContainsKey(catalogFilePath))
			{
				try
				{
					string testAssetPath = catalogTestList[catalogFilePath];
					string assetExtension = Path.GetExtension(testAssetPath).ToLowerInvariant();
					string testAssetName = Path.GetFileNameWithoutExtension(testAssetPath);

					if (assetExtension == ".mat")
					{
						Material? testObject = AssetBundleUtils.LoadAsset<Material>(testAssetName);
						if (testObject != null && testObject.name != null)
						{
							Logger.LogDebug("Catalog Test (" + catalogName + ") (" + testAssetName + ") OK");
							return true;
						}
						else
						{
							Logger.LogError("Catalog Test (" + catalogName + ") (" + testAssetName + ") Failed");
							return false;
						}

					}

					if (assetExtension == ".png" || assetExtension == ".jpg")
					{
						Texture2D? testObject = AssetBundleUtils.LoadAsset<Texture2D>(testAssetName);
						if (testObject != null && testObject.name != null)
						{
							Logger.LogDebug("Catalog Test (" + catalogName + ") (" + testAssetName + ") OK");
							return true;
						}
						else
						{
							Logger.LogError("Catalog Test (" + catalogName + ") (" + testAssetName + ") Failed");
							return false;
						}

					}
					if (assetExtension == ".prefab")
					{
						GameObject? testObject = AssetBundleUtils.LoadAsset<GameObject>(testAssetName);
						if (testObject != null && testObject.name != null)
						{
							Logger.LogDebug("Catalog Test (" + catalogName + ") (" + testAssetName + ") OK");
							return true;
						}
						else
						{
							Logger.LogError("Catalog Test (" + catalogName + ") (" + testAssetName + ") Failed");
							return false;
						}
					}
					Logger.LogError("Catalog Test Failed (" + catalogName + ") (" + testAssetName + assetExtension + ") Unknown asset extension");
					return false;
				}
				catch (Exception e)
				{
					Logger.LogError("Catalog Test Failed (" + catalogName + ") " + e.ToString());
					return false;
				}
			}
			else
			{
				Logger.LogError("Catalog Test Failed (" + catalogName + ") No test found");
				return false;
			}
		}

		internal static void MapPrefabs()
		{
			foreach (KeyValuePair<string, List<string>> item in bundleAssetList)
			{
				foreach (string assetName in item.Value)
				{
					if (assetName.ToLower().EndsWith(@".prefab"))
					{
						AutoMapper.AutoMapPrefab(Path.GetFileNameWithoutExtension(item.Key), Path.GetFileNameWithoutExtension(assetName));
					}
				}
			}
		}

		internal static bool IsModComponentPrefab(string name)
		{

			foreach (KeyValuePair<string, List<string>> item in bundleAssetList)
			{
				foreach (string assetName in item.Value)
				{
					if (Path.GetFileNameWithoutExtension(assetName).ToLower() == name.ToLower())
					{
						return true;
					}
				}
			}
			return false;
		}
		internal static string? GetPrefabBundlePath(string name)
		{

			foreach (KeyValuePair<string, List<string>> item in bundleAssetList)
			{
				foreach (string assetName in item.Value)
				{
					if (Path.GetFileNameWithoutExtension(assetName).ToLower() == name.ToLower())
					{
						return item.Key;
					}
				}
			}
			return null;
		}

	}

}
