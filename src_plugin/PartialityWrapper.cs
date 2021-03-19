using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Partiality.Modloader;

namespace PartialityWrapper
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class PartialityWrapper : BaseUnityPlugin
    {
        private const string ID = "com.sinai.PartialityWrapper";
        private const string NAME = "Partiality Wrapper";
        private const string VERSION = "3.0";

        // Paths
        public const string PLUGIN_FOLDERNAME = "PartialityWrapper";
        public static string OurPluginFolder => Path.Combine(Paths.PluginPath, PLUGIN_FOLDERNAME);

        public static string HOOKSFilePath => Path.Combine(OurPluginFolder, "HOOKS-Assembly-CSharp.dll");

        public static string PartialityModsFolder => Path.Combine(Paths.PluginPath, "partiality-mods");
        public static string LegacyModsFolder => Path.Combine(Paths.GameRootPath, "Mods");

        public static PartialityWrapper Instance;

        internal void Awake()
        {
            Instance = this;

            if (File.Exists(HOOKSFilePath))
            {
                Assembly.Load(File.ReadAllBytes(HOOKSFilePath));
                Logger.LogMessage($"Loaded HOOKS file successfully");
            }
            else
                Logger.LogWarning($"Could not find HOOKS file at expected path: {HOOKSFilePath}");

            // Read and Load PartialityMod types from the 'BepInEx\plugins\' and 'Mods\' folders.
            LoadMods();            
        }

        /// <summary>
        /// Loads a list of PartialityMods from the BepInEx\plugins\ or Outward\Mods\ directory.
        /// </summary>
        private void LoadMods()
        {
            Logger.LogMessage("Loading Partiality Mods...");

            if (!Directory.Exists(PartialityModsFolder))
                Directory.CreateDirectory(PartialityModsFolder);

            // Get 'BepInEx\plugins\partiality-mods\' mods...
            var asmPaths = Directory.GetFiles(PartialityModsFolder, "*.dll", SearchOption.AllDirectories).ToList();

            // Get legacy 'Mods\' mods...
            if (Directory.Exists(LegacyModsFolder))
                asmPaths.AddRange(Directory.GetFiles(LegacyModsFolder, "*.dll", SearchOption.AllDirectories));

            // Load assemblies
            var assemblies = new List<Assembly>();
            foreach (string filepath in asmPaths)
            {
                try
                {
                    assemblies.Add(Assembly.Load(File.ReadAllBytes(filepath)));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Logger.LogError($"Could not load \"{Path.GetFileName(filepath)}\" as a PartialityMod!");
                    Logger.LogDebug(ex);
                }
                catch (BadImageFormatException) { } // unmanaged DLL
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Unhandled exception loading \"{Path.GetFileName(filepath)}\"!");
                    Logger.Log(LogLevel.Debug, ex.StackTrace);
                }
            }

            var mods = new List<PartialityMod>();
            foreach (var asm in assemblies)
            {
                try
                {
                    mods.AddRange(from type in GetTypesSafe(asm)
                                  where type.IsSubclassOf(typeof(PartialityMod))
                                  select (PartialityMod)Activator.CreateInstance(type));
                }
                catch (Exception ex)
                {
                    Logger.LogError("Exception loading assembly: " + asm.FullName);
                    Logger.LogDebug(ex.Message);
                    Logger.LogDebug(ex.StackTrace);
                }
            }

            // Load and enable mods
            foreach (PartialityMod mod in mods.OrderBy(mod => mod.loadPriority))
            {
                if (string.IsNullOrEmpty(mod.ModID) || mod.ModID == "NULL")
                    mod.ModID = mod.GetType().Name;

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

        public static IEnumerable<Type> GetTypesSafe(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return e.Types.Where(x => x != null); }
            catch { return Enumerable.Empty<Type>(); }
        }
    }
}