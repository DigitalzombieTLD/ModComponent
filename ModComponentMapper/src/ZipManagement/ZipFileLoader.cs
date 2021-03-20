﻿using MelonLoader.ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace ModComponentMapper
{
    internal enum FileType
    {
        json,
        unity3d,
        txt,
        dll,
        bnk,
        other
    }

    internal static class ZipFileLoader
    {
        public const string ZIP_FOLDER_NAME = "ModComponentZips";

        internal static string GetZipFolderPath() => Path.Combine(Implementation.GetModsFolderPath(), ZIP_FOLDER_NAME);

        internal static void Initialize()
        {
            string zipFolderDirectory = GetZipFolderPath();
            if (!Directory.Exists(zipFolderDirectory))
            {
                Logger.Log("Directory '{0}' does not exist. Creating ...", zipFolderDirectory);
                Directory.CreateDirectory(zipFolderDirectory);
                return;
            }
            LoadZipFilesInDirectory(zipFolderDirectory);
        }
        internal static void LoadZipFilesInDirectory(string directory)
        {
            string[] directories = Directory.GetDirectories(directory);
            foreach (string eachDirectory in directories)
            {
                LoadZipFilesInDirectory(eachDirectory);
            }

            string[] files = Directory.GetFiles(directory);
            foreach (string eachFile in files)
            {
                if (eachFile.ToLower().EndsWith(".zip"))
                {
                    LoadZipFile(eachFile);
                }
            }
        }
        internal static void LoadZipFile(string zipFilePath)
        {
            Logger.Log("Loading zip file at: '{0}'", zipFilePath);
            var fileStream = File.OpenRead(zipFilePath);
            var zipInputStream = new ZipInputStream(fileStream);
            ZipEntry entry;
            while ((entry = zipInputStream.GetNextEntry()) != null)
            {
                string internalPath = entry.Name;
                string filename = Path.GetFileName(internalPath);
                FileType fileType = GetFileType(filename);
                if (fileType == FileType.other)
                    continue;


                using (var unzippedFileStream = new MemoryStream())
                {
                    int size = 0;
                    byte[] buffer = new byte[4096];
                    while (true)
                    {
                        size = zipInputStream.Read(buffer, 0, buffer.Length);
                        if (size > 0)
                            unzippedFileStream.Write(buffer, 0, size);
                        else
                            break;
                    }
                    string fullPath = Path.Combine(zipFilePath, internalPath);
                    switch (fileType)
                    {
                        case FileType.json:
                            HandleJson(internalPath,ReadToString(unzippedFileStream));
                            break;
                        case FileType.unity3d:
                            HandleUnity3d(internalPath, unzippedFileStream,fullPath);
                            break;
                        case FileType.txt:
                            HandleTxt(internalPath, ReadToString(unzippedFileStream), fullPath);
                            break;
                        case FileType.dll:
                            Logger.Log("Loading dll from zip at '{0}'", internalPath);
                            Assembly.Load(unzippedFileStream.ToArray());
                            break;
                        case FileType.bnk:
                            Logger.Log("Loading bnk from zip at '{0}'", internalPath);
                            AssetLoader.ModSoundBankManager.RegisterSoundBank(unzippedFileStream.ToArray());
                            break;
                    }
                }
            }
        }
        internal static Encoding GetEncoding(MemoryStream memoryStream)
        {
            using (var reader = new StreamReader(memoryStream, true))
            {
                reader.Peek();
                return reader.CurrentEncoding;
            }
        }
        internal static string ReadToString(MemoryStream memoryStream)
        {
            Encoding encoding = GetEncoding(memoryStream);
            return encoding.GetString(memoryStream.ToArray());
        }
        internal static FileType GetFileType(string filename)
        {
            if (String.IsNullOrEmpty(filename)) return FileType.other;
            if (filename.EndsWith(".unity3d")) return FileType.unity3d;
            if (filename.EndsWith(".json")) return FileType.json;
            if (filename.EndsWith(".txt")) return FileType.txt;
            if (filename.EndsWith(".dll")) return FileType.dll;
            if (filename.EndsWith(".bnk")) return FileType.bnk;
            return FileType.other;
        }
        private static void HandleJson(string internalPath,string text)
        {
            string filenameNoExtension = Path.GetFileNameWithoutExtension(internalPath);
            if (internalPath.StartsWith(@"auto-mapped/"))
            {
                Logger.Log("Loading automapped json from zip at '{0}'", internalPath);
                JsonHandler.RegisterJsonText(filenameNoExtension, text, JsonType.Automapped);
            }
            else if (internalPath.StartsWith(@"blueprints/"))
            {
                Logger.Log("Loading blueprint json from zip at '{0}'", internalPath);
                JsonHandler.RegisterJsonText(filenameNoExtension, text, JsonType.Blueprint);
            }
            else if (internalPath.StartsWith(@"existing-json/"))
            {
                Logger.Log("Loading existing json from zip at '{0}'", internalPath);
                JsonHandler.RegisterJsonText(filenameNoExtension, text, JsonType.Existing);
            }
        }
        private static void HandleTxt(string internalPath,string text,string fullPath)
        {
            if (internalPath.StartsWith(@"gear-spawns/"))
            {
                Logger.Log("Loading txt from zip at '{0}'", internalPath);
                string[] lines = Regex.Split(text, "\r\n|\r|\n");
                GearSpawnReader.ProcessLines(lines, fullPath);
            }
        }
        private static void HandleUnity3d(string internalPath,MemoryStream memoryStream,string fullPath)
        {
            if (internalPath.StartsWith(@"auto-mapped/"))
            {
                Logger.Log("Loading asset bundle from zip at '{0}'", internalPath);
                AssetBundle assetBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());
                string relativePath = GetPathRelativeToModsFolder(fullPath);
                AssetLoader.ModAssetBundleManager.RegisterAssetBundle(relativePath, assetBundle);
                AssetBundleManager.Add(relativePath);
            }
        }
        private static string GetPathRelativeToModsFolder(string fullPath)
        {
            return AutoMapper.GetRelativePath(fullPath, Implementation.GetModsFolderPath());
        }
    }
}