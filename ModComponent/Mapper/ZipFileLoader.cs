﻿using MelonLoader.ICSharpCode.SharpZipLib.Zip;
using MelonLoader.TinyJSON;
using MelonLoader.Utils;
using ModComponent.Utils;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ModComponent.Mapper;

internal static class ZipFileLoader
{
	internal static readonly List<byte[]> hashes = new();

	internal static void Initialize()
	{
		LoadZipFilesInDirectory(MelonEnvironment.ModsDirectory, false);
	}

	private static void LoadZipFilesInDirectory(string directory, bool recursive)
	{
		if (recursive)
		{
			string[] directories = Directory.GetDirectories(directory);
			foreach (string eachDirectory in directories)
			{
				LoadZipFilesInDirectory(eachDirectory, true);
			}
		}

		string[] files = Directory.GetFiles(directory,"*.modcomponent");

		Array.Sort(files);

		foreach (string eachFile in files)
		{
			if (eachFile.ToLower().EndsWith(".modcomponent"))
			{
				//PageManager.AddToItemPacksPage(new ItemPackData(eachFile));
				LoadZipFile(eachFile);
			}
		}
	}

	private static void LoadZipFile(string zipFilePath)
	{
		string zipFileName = Path.GetFileName(zipFilePath);
		string zipFileNameNoExt = Path.GetFileNameWithoutExtension(zipFilePath);

		Logger.Log($"Reading zip file at: '{zipFileName}'");
		FileStream fileStream = File.OpenRead(zipFilePath);

		hashes.Add(SHA256.Create().ComputeHash(fileStream));
		fileStream.Position = 0;

		ZipInputStream zipInputStream = new ZipInputStream(fileStream);
		ZipEntry entry;
		while ((entry = zipInputStream.GetNextEntry()) != null)
		{
			string internalPath = entry.Name;
			string filename = Path.GetFileName(internalPath);
			FileType fileType = GetFileType(filename);
			if (fileType == FileType.other)
			{
				continue;
			}

			using MemoryStream unzippedFileStream = new MemoryStream();
			int size = 0;
			byte[] buffer = new byte[4096];
			while (true)
			{
				size = zipInputStream.Read(buffer, 0, buffer.Length);
				if (size > 0)
				{
					unzippedFileStream.Write(buffer, 0, size);
				}
				else
				{
					break;
				}
			}
			if (!TryHandleFile(zipFilePath, internalPath, fileType, unzippedFileStream))
			{
				return;
			}
		}
	}

	private static string ReadToString(MemoryStream memoryStream)
	{
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}

	private static string ReadToJsonString(MemoryStream memoryStream)
	{
		const byte leftCurlyBracket = (byte)'{';
		byte[] bytes = memoryStream.ToArray();
		int index = Array.IndexOf(bytes, leftCurlyBracket);
		if (index < 0)
		{
			throw new ArgumentException("MemoryStream has no Json content.", nameof(memoryStream));
		}
		return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(bytes, index, bytes.Length - index));
	}

	private static FileType GetFileType(string filename)
	{
		if (string.IsNullOrWhiteSpace(filename))
		{
			return FileType.other;
		}

		if (filename.EndsWith(".unity3d", StringComparison.Ordinal) || filename.EndsWith(".bundle", StringComparison.Ordinal))
		{
			return FileType.unity3d;
		}

		if (filename.EndsWith(".json", StringComparison.Ordinal))
		{
			return FileType.json;
		}

		if (filename.EndsWith(".txt", StringComparison.Ordinal))
		{
			return FileType.txt;
		}

		if (filename.EndsWith(".dll", StringComparison.Ordinal))
		{
			return FileType.dll;
		}

		if (filename.EndsWith(".bnk", StringComparison.Ordinal))
		{
			return FileType.bnk;
		}

		return FileType.other;
	}

	private static bool TryHandleFile(string zipFilePath, string internalPath, FileType fileType, MemoryStream unzippedFileStream)
	{
		switch (fileType)
		{
			case FileType.json:
				return TryHandleJson(zipFilePath, internalPath, ReadToJsonString(unzippedFileStream));
			case FileType.unity3d:
				return TryHandleUnity3d(zipFilePath, internalPath, unzippedFileStream.ToArray());
			case FileType.txt:
				return TryHandleTxt(zipFilePath, internalPath, ReadToString(unzippedFileStream));
			case FileType.dll:
				return TryLoadAssembly(zipFilePath, internalPath, unzippedFileStream.ToArray());
			case FileType.bnk:
				return TryRegisterSoundBank(zipFilePath, internalPath, unzippedFileStream.ToArray());
			default:
				string fullPath = Path.Combine(zipFilePath, internalPath);
				PackManager.SetItemPackNotWorking(zipFilePath, $"Could not handle asset '{fullPath}'");
				return false;
		}
	}

	private static bool TryLoadAssembly(string zipFilePath, string internalPath, byte[] data)
	{
		try
		{
			Logger.LogDebug($"Loading dll from zip at '{internalPath}'");
			Assembly.Load(data);
			return true;
		}
		catch (Exception e)
		{
			string fullPath = Path.Combine(zipFilePath, internalPath);
			PackManager.SetItemPackNotWorking(zipFilePath, $"Could not load assembly '{fullPath}'. {e.Message}");
			return false;
		}
	}

	private static bool TryRegisterSoundBank(string zipFilePath, string internalPath, byte[] data)
	{
		try
		{
			Logger.LogDebug($"Loading bnk from zip at '{internalPath}'");
			ModComponent.AssetLoader.ModSoundBankManager.RegisterSoundBank(data);
			return true;
		}
		catch (Exception e)
		{
			string fullPath = Path.Combine(zipFilePath, internalPath);
			PackManager.SetItemPackNotWorking(zipFilePath, $"Could not register sound bank '{fullPath}'. {e.Message}");
			return false;
		}
	}

	private static bool TryHandleJson(string zipFilePath, string internalPath, string text)
	{
		string bundleName = Path.GetFileNameWithoutExtension(zipFilePath);

		try
		{
			string filenameNoExtension = Path.GetFileNameWithoutExtension(internalPath);
			if (internalPath.StartsWith(@"auto-mapped/"))
			{
				Logger.LogDebug($"Reading automapped json from zip at '{internalPath}'");
				JsonHandler.RegisterJsonText(filenameNoExtension, text);
			}
			else if (internalPath.StartsWith(@"blueprints/"))
			{
				Logger.LogDebug($"Reading blueprint json from zip at '{internalPath}'");
				CraftingRevisions.BlueprintManager.AddBlueprintFromJson(text);
			}
			else if (internalPath.StartsWith(@"recipes/"))
			{
				Logger.LogDebug($"Reading recipes json from zip at '{internalPath}'");
				CraftingRevisions.RecipeManager.AddRecipeFromJson(text);
			}
			else if (internalPath.StartsWith(@"localizations/"))
			{
				// change emthod to ensure we go via the BOM fixed methods..
				LocalizationUtilities.LocalizationManager.LoadJsonLocalization(text);
			}
			else if (internalPath.StartsWith(@"bundle/"))
			{
				Logger.LogDebug($"Reading json catalog from zip at '{internalPath}'");
				string catalogFilename = Path.GetFileName(internalPath);
				AssetBundleProcessor.WriteCatalogToDisk(bundleName, catalogFilename, text);
			}
			else if (internalPath.ToLowerInvariant() == "buildinfo.json")
			{
				LogItemPackInformation(text);
			}
			else
			{
				throw new NotSupportedException($"Json file does not have a valid internal path: {internalPath}");
			}
			return true;
		}
		catch (Exception e)
		{
			string fullPath = Path.Combine(zipFilePath, internalPath);
			PackManager.SetItemPackNotWorking(zipFilePath, $"Could not load json '{fullPath}'. {e.Message}");
			return false;
		}
	}

	private static void LogItemPackInformation(string jsonText)
	{
		ProxyObject? dict = (ProxyObject)JSON.Load(jsonText);
		string modName = dict["Name"];
		string version = dict["Version"];
		Variant author;
		if (dict.TryGetValue("Author", out author)) {
			Logger.LogGreen($"Found: {modName} {version} by {author}");
		} else
		{
			Logger.LogGreen($"Found: {modName} {version}");
		}
	}

	private static bool TryHandleTxt(string zipFilePath, string internalPath, string text)
	{
		if (internalPath.StartsWith(@"gear-spawns/"))
		{
			try
			{
				Logger.LogDebug($"Reading txt from zip at '{internalPath}'");
				GearSpawner.SpawnManager.ParseSpawnInformation(text);
				return true;
			}
			catch (Exception e)
			{
				string fullPath = Path.Combine(zipFilePath, internalPath);
				PackManager.SetItemPackNotWorking(zipFilePath, $"Could not load gear spawn '{fullPath}'. {e.Message}");
				return false;
			}
		}
		else
		{
			string fullPath = Path.Combine(zipFilePath, internalPath);
			PackManager.SetItemPackNotWorking(zipFilePath, $"Txt file not in the gear-spawns folder: '{fullPath}'");
			return false;
		}
	}

	private static bool TryHandleUnity3d(string zipFilePath, string internalPath, byte[] data)
	{
		string bundleName = Path.GetFileNameWithoutExtension(zipFilePath);
		string fullPath = Path.Combine(zipFilePath, internalPath);
		if (internalPath.StartsWith(@"bundle/"))
		{
			Logger.LogDebug($"Loading asset bundle from zip at '{internalPath}'");

			try
			{
				string bundleFilename = Path.GetFileName(internalPath);
				AssetBundleProcessor.WriteAssettBundleToDisk(bundleName, bundleFilename, data);
				return true;
			}
			catch (Exception e)
			{
				PackManager.SetItemPackNotWorking(zipFilePath, $"Could not load asset bundle '{fullPath}'. {e.Message}");
				return false;
			}
		}
		else
		{
			PackManager.SetItemPackNotWorking(zipFilePath, $"Asset bundle not in the bundle folder: '{fullPath}'");
			return false;
		}
	}
}
