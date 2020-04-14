using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using Partiality;
using Partiality.Modloader;

/*
 *  Original PartialityWrapper written by Ashnal
 *  
 *  Forked by notfood and fixed for BepInEx 5.0 (rewrite)
 *  
 *  Re-forked by Sinai (me) for Outward again
*/

namespace BepInExPartialityWrapper
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class Wrapper : BaseUnityPlugin
    {
        const string ID = "github.sinaioutlander.BepInExPartialityWrapper";
        const string NAME = "Partiality Wrapper";
        const string VERSION = "2.01";

        public Wrapper()
        {
            GenerateHooks();
            BootstrapPartiality();
            LoadPartialityMods();
        }

        /// <summary>Generates the HOOKS-Assembly-CSharp.dll the same as Partiality</summary>
        void GenerateHooks()
        {
            string pathIn = Path.Combine(Paths.ManagedPath, "Assembly-CSharp.dll");
            string pathOut = Path.Combine(Path.GetDirectoryName(Info.Location), "HOOKS-Assembly-CSharp.dll");

            if (File.Exists(pathOut)) 
            {
                return;
            }

            using (MonoModder mm = new MonoModder 
            {
                InputPath = pathIn,
                OutputPath = pathOut,
                PublicEverything = true,
                DependencyDirs = new List<string>() { Paths.ManagedPath, Paths.BepInExAssemblyDirectory },
            }) 
            {
                mm.Read();
                mm.MapDependencies();
                mm.Log("[HookGen] Starting HookGenerator");
                HookGenerator gen = new HookGenerator(mm, Path.GetFileName(pathOut));
                using (ModuleDefinition mOut = gen.OutputModule) 
                {
                    gen.HookPrivate = true;
                    gen.Generate();
                    mOut.Write(pathOut);
                }
                mm.Log("[HookGen] Done.");
            }
        }

        /// <summary>Removes LoadAllMods from ModManager and initializates Partiality</summary>
        void BootstrapPartiality()
        {
            var harmony = new Harmony(ID);

            harmony.Patch(AccessTools.Method(typeof(ModManager), nameof(ModManager.LoadAllMods)),
                prefix: new HarmonyMethod(typeof(Wrapper), nameof(PrefixReturnFalse)));

            PartialityManager.CreateInstance();
        }

        static bool PrefixReturnFalse() => false;

        /// <summary>Loads the Partiality Mods after sorting</summary>
        void LoadPartialityMods()
        {
            var modTypes = LoadTypes<PartialityMod>(Paths.PluginPath);

            var loadedMods = PartialityManager.Instance.modManager.loadedMods;

            //Load mods from types
            foreach (Type t in modTypes) 
            {
                //Don't try to load the base class
                if (t.Name == "PartialityMod") 
                {
                    continue;
                }

                try 
                {
                    PartialityMod newMod = (PartialityMod) Activator.CreateInstance(t);

                    newMod.Init();

                    if (newMod.ModID == "NULL") 
                    {
                        Logger.LogWarning("Mod With NULL id, assigning the file as the ID");
                        newMod.ModID = t.Name;
                    }

                    loadedMods.Add(newMod);

                    Logger.LogInfo("Initialized mod " + newMod.ModID);

                } 
                catch (Exception e) 
                {
                    Logger.LogError("Could not instantiate Partiality Mod of Type: " + t.Name + "\r\n" + e);
                }
            }

            var loadedModsSorted = loadedMods.OrderBy(mod => mod.loadPriority);

            foreach (var pMod in loadedModsSorted)
            {
                try
                {
                    pMod.OnLoad();
                    pMod.OnEnable();

                    Logger.LogInfo("Loaded and Enabled mod " + pMod.ModID);
                }
                catch (Exception e)
                {
                    Logger.LogError("Initialization error with mod: " + pMod.ModID + "\r\n" + e);
                }
            }
        }

        /// <summary>
        /// Loads a list of types from a directory containing assemblies, that derive from a base type.
        /// </summary>
        /// <typeparam name="T">The specfiic base type to search for.</typeparam>
        /// <param name="directory">The directory to search for assemblies.</param>
        /// <returns>Returns a list of found derivative types.</returns>
        IEnumerable<Type> LoadTypes<T>(string directory)
        {
            List<Type> types = new List<Type>();
            Type pluginType = typeof(T);

            foreach (string dll in Directory.GetFiles(Path.GetFullPath(directory), "*.dll")) 
            {
                try 
                {
                    Assembly assembly = Assembly.LoadFile(dll);

                    foreach (Type type in assembly.GetTypes()) 
                    {
                        if (!type.IsInterface && !type.IsAbstract && type.BaseType == pluginType)
                            types.Add(type);
                    }
                } 
                catch (BadImageFormatException) { } //unmanaged DLL
                catch (ReflectionTypeLoadException ex) 
                {
                    Logger.Log(LogLevel.Error, $"Could not load \"{Path.GetFileName(dll)}\" as a plugin!");
                    Logger.Log(LogLevel.Debug, TypeLoadExceptionToString(ex));
                }
            }

            return types;
        }

        static string TypeLoadExceptionToString(ReflectionTypeLoadException ex)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Exception exSub in ex.LoaderExceptions) 
            {
                sb.AppendLine(exSub.Message);
                if (exSub is FileNotFoundException exFileNotFound) 
                {
                    if (!string.IsNullOrEmpty(exFileNotFound.FusionLog)) 
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exFileNotFound.FusionLog);
                    }
                } 
                else if (exSub is FileLoadException exLoad) 
                {
                    if (!string.IsNullOrEmpty(exLoad.FusionLog)) 
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exLoad.FusionLog);
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}