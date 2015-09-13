/*
This class is modification of ServerApi.cs from TerrariaServerApi
Copyright (C) 2011-2015 Nyx Studios (fka. The TShock Team)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;
using CSScriptLibrary;
using CSharpScript;

namespace CSharpScript
{
    // TODO: Maybe re-implement a reload functionality for plugins, but you'll have to load all assemblies into their own
    // AppDomain in order to unload them again later. Beware that having them in their own AppDomain might cause threading 
    // problems as usual locks will only work in their own AppDomains.
    public static class ScriptLoader
    {
        public const string ScriptsPath = "Scripts";
        private static readonly List<ScriptContainer> scripts = new List<ScriptContainer>();
        public static string ServerPluginsDirectoryPath
        {
            get;
            private set;
        }
        public static ReadOnlyCollection<ScriptContainer> Plugins
        {
            get { return new ReadOnlyCollection<ScriptContainer>(scripts); }
        }
        internal static void Initialize()
        {
            ServerPluginsDirectoryPath = Path.Combine(Environment.CurrentDirectory, ScriptsPath);
            if (!Directory.Exists(ServerPluginsDirectoryPath))
            {
                string lcDirectoryPath =
                    Path.Combine(Path.GetDirectoryName(ServerPluginsDirectoryPath), ScriptsPath.ToLower());

                if (Directory.Exists(lcDirectoryPath))
                {
                    Directory.Move(lcDirectoryPath, ServerPluginsDirectoryPath);
                    Console.WriteLine("Case sensitive filesystem detected, Scripts directory has been renamed.", TraceLevel.Warning);
                }
                else
                {
                    Directory.CreateDirectory(ServerPluginsDirectoryPath);
                    Console.WriteLine(string.Format(
                    "Folder Scripts does not exist. Creating now."),
                    TraceLevel.Info);
                }
            }

            // Add assembly resolver instructing it to use the server plugins directory as a search path.
            // TODO: Either adding the server plugins directory to PATH or as a privatePath node in the assembly config should do too.
            LoadPlugins();
        }

        internal static void DeInitialize()
        {
            UnloadScripts();
        }

        internal static void LoadPlugins()
        {
            string ignoredPluginsFilePath = Path.Combine(ServerPluginsDirectoryPath, "ignoredplugins.txt");

            List<string> ignoredFiles = new List<string>();
            if (File.Exists(ignoredPluginsFilePath))
                ignoredFiles.AddRange(File.ReadAllLines(ignoredPluginsFilePath));
            string[] ValidExtensions = new string[] { ".cs", ".csscript", ".script" }; //is it .script or script?
            List<FileInfo> dif = new DirectoryInfo(ScriptsPath).GetFiles().Where(f => ValidExtensions.Contains(f.Extension)).ToList();
            foreach (FileInfo fileInfo in dif)
            {
                try
                {
                    dynamic possiblyScript;
                    try
                    {
                        possiblyScript = CSScript.Evaluator.ReferenceAssembly(Assembly.GetCallingAssembly()).ReferenceAssembly(Assembly.GetExecutingAssembly()).ReferenceAssembly(Assembly.GetEntryAssembly()).ReferenceAssemblyByName(@"TShockAPI").ReferenceAssemblyByName("TerrariaServer").ReferenceDomainAssemblies().LoadFile(fileInfo.Name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(Path.GetFileNameWithoutExtension(fileInfo.Name) + " couldn't be loaded. You fucked something up");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine(ex.Source);
                        continue;
                    }
                    scripts.Add(new ScriptContainer(possiblyScript));
                }
                catch (Exception ex)
                {
                    // Broken assemblies / plugins better stop the entire server init.
                    Console.WriteLine(string.Format("Failed to load script \"{0}\".", fileInfo.Name) + ex);
                }
            }
            IOrderedEnumerable<ScriptContainer> orderedPluginSelector =
                from x in Plugins
                orderby x.Script.Order, x.Script.Name
                select x;
            try
            {
                int count = 0;
                foreach (ScriptContainer current in orderedPluginSelector)
                {
                    count++;
                }
                foreach (ScriptContainer current in orderedPluginSelector)
                {
                    try
                    {
                        current.Initialize();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.Source);
                        Console.WriteLine(ex.HelpLink);
                        Console.WriteLine(ex.StackTrace);
                        // Broken Scripts better stop the entire server init.
                        break;
                    }
                    Console.WriteLine(string.Format(
                        "Script {0} v{1} (by {2}) initiated.", current.Script.Name, current.Script.Version, current.Script.Author),
                        TraceLevel.Info);
                }
            }
            catch
            {
            }
        }

        internal static void UnloadScripts()
        {
            foreach (ScriptContainer scriptContainer in scripts)
            {
                try
                {
                    scriptContainer.DeInitialize();
                    Console.WriteLine(string.Format(
                        "Script \"{0}\" was deinitialized", scriptContainer.Script.Name),
                        TraceLevel.Error);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format(
                        "Script \"{0}\" has thrown an exception while being deinitialized:\n{1}", scriptContainer.Script.Name, ex),
                        TraceLevel.Error);
                }
            }

            foreach (ScriptContainer scriptContainer in scripts)
            {


                try
                {
                    scriptContainer.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format(
                        "Script \"{0}\" has thrown an exception while being disposed:\n{1}", scriptContainer.Script.Name, ex),
                        TraceLevel.Error);
                }
            }
        }
    }
}

