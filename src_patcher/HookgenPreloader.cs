using BepInEx;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using System.Reflection;

namespace PartialityWrapper
{
    public class HookgenPreloader
    {
        // Paths
        internal const string PATCHER_FOLDER = "PartialityWrapper";
        public static string HookgenPatcherFolder => Path.Combine(Paths.PatcherPluginPath, PATCHER_FOLDER);
        public static string OutputPluginFolder => Path.Combine(Paths.PluginPath, PATCHER_FOLDER);

        public static string AsmCSharpFilePath => Path.Combine(Paths.ManagedPath, "Assembly-CSharp.dll");
        public static string HooksAsmFilePath => Path.Combine(OutputPluginFolder, "HOOKS-Assembly-CSharp.dll");

        // Log source
        internal static ManualLogSource Logging = Logger.CreateLogSource(PATCHER_FOLDER);

        // Required property for BepInEx preloader patcher.
        public static IEnumerable<string> TargetDLLs => new[] { "Assembly-CSharp.dll" };

        // Required method for preloader patcher, will be used as our entry point.
        // We only have one TargetDLL so this only runs once.
        public static void Patch(AssemblyDefinition _)
        {
            Logging.LogMessage($"BepInEx-Partiality-Wrapper initializing HOOKS...");

            if (!File.Exists(AsmCSharpFilePath))
            {
                Logging.LogMessage($"Could not find 'Assembly-CSharp.dll' file, aborting HOOKS generatiion.");
                return;
            }

            if (File.Exists(HooksAsmFilePath))
            {
                // if HOOKS file is older than the Assembly-Csharp file...
                if (File.GetLastWriteTime(HooksAsmFilePath) < File.GetLastWriteTime(AsmCSharpFilePath))
                {
                    Logging.LogMessage($"HOOKS file is outdated, deleting...");
                    File.Delete(HooksAsmFilePath);
                }
                else
                {
                    Logging.LogMessage($"HOOKS file is up to date!");
                    return;
                }
            }

            Logging.LogMessage("Generating new HOOKS file...");

            try
            {
                using (var modder = new MonoModder
                {
                    InputPath = AsmCSharpFilePath,
                    OutputPath = HooksAsmFilePath,
                    PublicEverything = true,
                    DependencyDirs = new List<string> { Paths.ManagedPath, HookgenPatcherFolder }
                })
                {
                    modder.Read();
                    modder.MapDependencies();
                    var generator = new HookGenerator(modder, Path.GetFileName(HooksAsmFilePath));
                    using (ModuleDefinition module = generator.OutputModule)
                    {
                        generator.HookPrivate = true;
                        generator.Generate();
                        module.Write(HooksAsmFilePath);
                    }
                }

                Logging.LogMessage("Done!");
            }
            catch (Exception ex)
            {
                Logging.LogWarning($"Exception running HOOKS generation!");
                Logging.LogMessage(ex);
            }
        }

    }
}
