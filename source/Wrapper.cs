using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using Partiality.Modloader;

/*
 *  ========= BepInEx-Partiality-Wrapper =========
 *  
 *  This is essentially Partiality packaged as a BepInEx mod.
 *  It serves as a dummy reference to Partiality.dll, containing only the PartialityMod class, and the loader as a BepInEx plugin.
 *  It also includes HookGen and will regenerate hooks when it is detected that they are out of date.
 *  
 *  This version also has a slightly different way of reading the Partiality assembly files, which would allow compatibility with Reloader.
 *
 *  ~~~~~~ Credits: ~~~~~~
 *  - Zandra: for Partiality
 *  - Ashnal: wrote original BepInEx-Partiality-Wrapper
 *  - notfood: rewrote the Wrapper for BepInEx 5.0
 *  - Sinai (me): bundled Partiality into this mod, and some small changes.
 *  - Laymain: helped with this version
*/

namespace Partiality
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class Wrapper : BaseUnityPlugin
    {
        const string ID = "com.sinai.PartialityWrapper";
        const string NAME = "Partiality Wrapper";
        const string VERSION = "2.1";

        public const string MODS_FOLDER = "Mods";

        public Wrapper()
        {
            // check the HOOKS file
            CheckHooks();

            // Read and Load PartialityMod types from the plugins folder
            LoadPartialityMods();
        }

        /// <summary>Generates the HOOKS-Assembly-CSharp.dll file, if it needs updating.</summary>
        void CheckHooks()
        {
            string asmPath = Path.Combine(Paths.ManagedPath, "Assembly-CSharp.dll");
            string hooksPath = Path.Combine(Path.GetDirectoryName(Info.Location), "HOOKS-Assembly-CSharp.dll");

            if (File.Exists(hooksPath)) 
            {
                // if HOOKS file is older than the Assembly-Csharp file...
                if (File.GetLastWriteTime(hooksPath) < File.GetLastWriteTime(asmPath))
                {
                    Logger.Log(LogLevel.Warning, "[HookGen] HOOKS file is out of date, generating a new one...");
                    File.Delete(hooksPath);
                }
                else // it's up to date. do nothing.
                {
                    return;
                }
            }

            using (MonoModder mm = new MonoModder 
            {
                InputPath = asmPath,
                OutputPath = hooksPath,
                PublicEverything = true,
                DependencyDirs = new List<string>() { Paths.ManagedPath, Paths.BepInExAssemblyDirectory },
            }) 
            {
                mm.Read();
                mm.MapDependencies();
                mm.Log("[HookGen] Starting HookGenerator");
                HookGenerator gen = new HookGenerator(mm, Path.GetFileName(hooksPath));
                using (ModuleDefinition mOut = gen.OutputModule) 
                {
                    gen.HookPrivate = true;
                    gen.Generate();
                    mOut.Write(hooksPath);
                }
                mm.Log("[HookGen] Done.");
            }
        }

        /// <summary>Sorts and then loads PartialityMod types from the Plugins folder</summary>
        void LoadPartialityMods()
        {
            List<PartialityMod> instances = GenerateInstances();

            LoadAndEnableMods(instances);
        }

        /// <summary>
        /// Loads a list of PartialityMods from the BepInEx\plugins\ directory.
        /// </summary>
        /// <returns>Returns a list of instances.</returns>
        List<PartialityMod> GenerateInstances()
        {
            List<PartialityMod> instances = new List<PartialityMod>();

            var list = Directory.GetFiles(Paths.PluginPath, "*.dll").ToList();

            if (Directory.Exists(MODS_FOLDER))
            {
                list.AddRange(Directory.GetFiles(MODS_FOLDER, "*.dll"));
            }

            foreach (string filepath in list) 
            {
                try 
                {
                    Assembly assembly = Assembly.Load(File.ReadAllBytes(filepath));

                    foreach (Type type in assembly.GetTypes()
                        .Where(x => 
                            x.BaseType == typeof(PartialityMod) &&
                            !x.IsInterface &&
                            !x.IsAbstract &&
                            x.Name != "PartialityMod")) // dont try to load the base class (although would this ever happen?)
                    {
                        var mod = (PartialityMod)Activator.CreateInstance(type);

                        if (mod.ModID == "NULL")
                        {
                            Logger.LogWarning("Mod with 'NULL' ID, assigning the Type as the ID");
                            mod.ModID = mod.GetType().Name;
                        }

                        instances.Add(mod);
                        Logger.LogInfo("Created instance of mod: " + mod.ModID);
                    }
                } 
                catch (BadImageFormatException) { } // unmanaged DLL
                catch (ReflectionTypeLoadException ex) 
                {
                    Logger.Log(LogLevel.Error, $"Could not load \"{Path.GetFileName(filepath)}\" as a plugin!");
                    Logger.Log(LogLevel.Debug, TypeLoadExceptionToString(ex));
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Unhandled exception loading \"{Path.GetFileName(filepath)}\"!");
                    Logger.Log(LogLevel.Debug, ex.StackTrace);
                }
            }

            return instances;
        }

        void LoadAndEnableMods(List<PartialityMod> instances)
        {
            // Sort mods by loadPriority (lower num = higher priority)

            var sorted = instances.OrderBy(mod => mod.loadPriority);

            // Call Init, OnLoad and OnEnable for the sorted instances

            foreach (var mod in sorted)
            {
                try
                {
                    mod.Init();
                    mod.OnLoad();
                    mod.OnEnable();

                    Logger.LogInfo("Loaded and Enabled mod " + mod.ModID);
                }
                catch (Exception e)
                {
                    Logger.LogError("Initialization error with mod: " + mod.ModID + "\r\n" + e);
                }
            }
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