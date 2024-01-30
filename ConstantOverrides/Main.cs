using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FullSerializer;
using UnityEngine;

namespace ConstantOverrides
{
    [BepInPlugin("Aidanamite.ConstantOverrides", "ConstantOverrides", "1.0.1")]
    public class Main : BaseUnityPlugin
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{Environment.CurrentDirectory}\\BepInEx\\{modName}";

        

        void Awake()
        {
            new Harmony($"com.Aidanamite.{modName}").PatchAll(modAssembly);
            StartCoroutine(FileWatcher());
            Logger.LogInfo($"{modName} has loaded");
        }
        IEnumerator FileWatcher()
        {
            var file = Environment.CurrentDirectory + "\\constants.json";
            var info = new FileInfo(file);
            var last = info.Exists ? info.LastWriteTime : default;
            while (true)
            {
                info = new FileInfo(file);
                if (!info.Exists)
                {
                    if (!File.Exists(file))
                        File.WriteAllBytes(file, GetResourceBytes("constants.json"));
                    info = new FileInfo(file);
                }
                if (info.Exists && info.LastWriteTime != last)
                {
                    last = info.LastWriteTime;
                    LoadConstants();
                }
                yield return null;
            }
        }
        static byte[] GetResourceBytes(string filename)
        {
            var stream = modAssembly.GetManifestResourceStream(modName + "." + filename);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Dispose();
            return buffer;
        }
        static string GetResourceText(string filename) => System.Text.Encoding.UTF8.GetString(GetResourceBytes(filename));
        static Assembly GetResourceAssembly(string filename)
        {
            try
            {
                return Assembly.Load(GetResourceBytes(filename));
            } catch
            {
                return null;
            }
        }
        public static void LoadConstants()
        {
            if (Constants.Instance)
                FsJSONManager.RawDeserialize(File.ReadAllText(Environment.CurrentDirectory + "\\constants.json"), out Constants.Instance._values);
        }
    }

    [HarmonyPatch(typeof(Constants),"ReadDefaultFile")]
    class Patch_ConstantRead
    {
        static bool Prefix()
        {
            Main.LoadConstants();
            return false;
        }
    }
}