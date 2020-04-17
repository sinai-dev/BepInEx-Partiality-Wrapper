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

namespace Partiality
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class Wrapper : BaseUnityPlugin
    {
        const string ID = "com.sinai.PartialityWrapper";
        const string NAME = "Partiality Wrapper";
        const string VERSION = "2.1";

        public string PluginFolder { get => Path.GetDirectoryName(Info.Location); }
        public string ModsFolder { get => Path.Combine(Paths.GameRootPath, "Mods"); }

        internal void Awake()
        {
            // Load dependencies manually
            LoadDependencies();

            // check the HOOKS file
            CheckHooks();

            // Read and Load PartialityMod types from the plugins folder
            LoadMods();
        }

        private void LoadDependencies()
        {
            IEnumerable<string> dependencies = (
                from filepath in Directory.GetFiles(PluginFolder)
                where filepath.EndsWith(".dll") || filepath.EndsWith(".exe")
                select filepath
            ).AsEnumerable();

            foreach (string filepath in dependencies)
            {
                Logger.Log(LogLevel.Message, "Loading dependency " + Path.GetFileName(filepath));
                Assembly.Load(File.ReadAllBytes(filepath));
            }

            Logger.Log(LogLevel.Message, "Done loading dependencies");
        }

        private void CheckHooks()
        {
            string asmPath = Path.Combine(Paths.ManagedPath, "Assembly-CSharp.dll");
            string hooksPath = Path.Combine(PluginFolder, "HOOKS-Assembly-CSharp.dll");

            if (File.Exists(hooksPath))
            {
                // if HOOKS file is older than the Assembly-Csharp file...
                if (File.GetLastWriteTime(hooksPath) < File.GetLastWriteTime(asmPath))
                {
                    File.Delete(hooksPath);
                }
                else
                {
                    return;
                }
            }

            Logger.Log(LogLevel.Message, "Generating new HOOKS file...");

            using (var modder = new MonoModder
            {
                InputPath = asmPath,
                OutputPath = hooksPath,
                PublicEverything = true,
                DependencyDirs = new List<string> { Paths.ManagedPath, PluginFolder }
            })
            {
                modder.Read();
                modder.MapDependencies();
                var generator = new HookGenerator(modder, Path.GetFileName(hooksPath));
                using (ModuleDefinition module = generator.OutputModule)
                {
                    generator.HookPrivate = true;
                    generator.Generate();
                    module.Write(hooksPath);
                }
            }

            Assembly.Load(File.ReadAllBytes(hooksPath));
        }

        /// <summary>
        /// Loads a list of PartialityMods from the BepInEx\plugins\ or Outward\Mods\ directory.
        /// </summary>
        private void LoadMods()
        {
            Logger.Log(LogLevel.Message, "Loading Partiality Mods...");

            var asmPaths = Directory.GetFiles(Paths.PluginPath, "*.dll").ToList();
            if (Directory.Exists(ModsFolder))
            {
                asmPaths.AddRange(Directory.GetFiles(ModsFolder, "*.dll", SearchOption.AllDirectories));
            }

            // Load assemblies without getting types to avoid referencing issues
            var assemblies = new List<Assembly>();
            foreach (string filepath in asmPaths)
            {
                try
                {
                    assemblies.Add(Assembly.Load(File.ReadAllBytes(filepath)));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Logger.Log(LogLevel.Error, $"Could not load \"{Path.GetFileName(filepath)}\" as a plugin!");
                    Logger.Log(LogLevel.Debug, TypeLoadExceptionToString(ex));
                }
                catch (BadImageFormatException) { } // unmanaged DLL
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Unhandled exception loading \"{Path.GetFileName(filepath)}\"!");
                    Logger.Log(LogLevel.Debug, ex.StackTrace);
                }
            }

            // Build an ordered list of Partiality mods, and create instances of the mods.
            var mods = (
                from assembly in assemblies 
                    from type in assembly.GetTypes()
                    where type.IsSubclassOf(typeof(PartialityMod))
                    select (PartialityMod)Activator.CreateInstance(type)
            ).OrderBy(mod => mod.loadPriority);

            // Load and enable mods
            foreach (PartialityMod mod in mods)
            {
                if (mod.ModID == "NULL")
                {
                    mod.ModID = mod.GetType().Name;
                }
                string label = $"{mod.ModID}@{mod.Version}";

                try
                {
                    mod.Init();
                    mod.OnLoad();
                    mod.OnEnable();

                    Logger.LogInfo("Loaded and Enabled " + label);
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, $"Unhandled exception loading \"{label}\"!");
                    Logger.Log(LogLevel.Debug, e.ToString());
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