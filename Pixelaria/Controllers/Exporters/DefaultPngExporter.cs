/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Pixelaria.Data;
using Pixelaria.Data.Exports;
using Pixelaria.Utils;

namespace Pixelaria.Controllers.Exporters
{
    /// <summary>
    /// Default exporter that uses PNG as the texture format
    /// </summary>
    public class DefaultPngExporter : IBundleExporter
    {
        private readonly ISheetExporter _sheetExporter;

        /// <summary>
        /// Dictionary that maps Animation Sheet IDs to a completion progress, ranging from 0 to 1 inclusive.
        /// </summary>
        private readonly Dictionary<int, float> _sheetProgress = new Dictionary<int, float>();

        public DefaultPngExporter(ISheetExporter sheetExporter)
        {
            _sheetExporter = sheetExporter;
        }

        /// <summary>
        /// Exports a given Bundle asynchronously, calling a progress handler along the way
        /// </summary>
        /// <param name="bundle">The bundle to export</param>
        /// <param name="cancellationToken">A cancellation token that is passed to the exporters and can be used to cancel the export process mid-way</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A task representing the concurrent export progress</returns>
        public async Task ExportBundleConcurrent(Bundle bundle, CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null)
        {
            // Start with initial values for the progress export of every sheet
            var stageProgresses = new float[bundle.AnimationSheets.Count];
            var exports = new List<BundleSheetJson>();
            
            var progressAction = new Action<AnimationSheet>(sheet =>
            {
                if (progressHandler == null)
                    return;

                // Calculate total progress
                int total = (int) Math.Floor(stageProgresses.Sum() / stageProgresses.Length * 100);
                progressHandler(new SheetGenerationBundleExportProgressEventArgs(sheet, BundleExportStage.TextureAtlasGeneration, total, total / 2, "Generating sheets"));
            });

            var generationList = new List<Task>();

            for (int i = 0; i < bundle.AnimationSheets.Count; i++)
            {
                var sheet = bundle.AnimationSheets[i];
                int j = i;
                generationList.Add(new Task(() =>
                {
                    var exp = _sheetExporter.ExportBundleSheet(sheet, cancellationToken, args =>
                    {
                        stageProgresses[j] = (float)args.StageProgress / 100;
                        _sheetProgress[sheet.ID] = args.TotalProgress / 100.0f;
                        progressAction(sheet);
                    });

                    try
                    {
                        var sheetJson = new BundleSheetJson(exp.Result, sheet.Name, Path.GetFullPath(bundle.ExportPath) + "\\" + sheet.Name);
                        exports.Add(sheetJson);
                    }
                    catch (TaskCanceledException)
                    {
                        // unused
                    }
                }));
            }

            var concurrent = new Task(() => { StartAndWaitAllThrottled(generationList, 7, cancellationToken); });

            concurrent.Start();

            await concurrent;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            //
            // 2. Save the sheets to disk
            //
            for (int i = 0; i < exports.Count; i++)
            {
                var exp = exports[i];

                if (progressHandler != null)
                {
                    int progress = (int) ((float) i / exports.Count * 100);
                    progressHandler.Invoke(new BundleExportProgressEventArgs(BundleExportStage.SavingToDisk, progress, 50 + progress / 2));
                }

                exp.BundleSheet.SaveToDisk(exp.ExportPath);
            }

            //
            // 3. Compose the main bundle .json
            //

            if (exports.Any(export => export.BundleSheet.ExportSettings.ExportJson))
            {
                var jsonSheetList = new List<Dictionary<string, object>>();
                
                foreach (var export in exports)
                {
                    if (!export.BundleSheet.ExportSettings.ExportJson)
                        continue;
                    
                    var sheet = new Dictionary<string, object>();

                    // Path of final JSON file
                    string jsonName = Path.ChangeExtension(export.ExportPath, "json");
                    Debug.Assert(jsonName != null, "jsonName != null");
                    string filePath = Utilities.GetRelativePath(jsonName, bundle.ExportPath);

                    sheet["name"] = export.SheetName;
                    sheet["sprite_file"] = filePath;
                    
                    // Export animation list
                    var names = export.BundleSheet.Animations.Select(anim => anim.Name);

                    sheet["animations"] = names;

                    jsonSheetList.Add(sheet);
                }

                var json = new Dictionary<string, object>
                {
                    ["sheets"] = jsonSheetList
                };
                
                string finalPath = Path.ChangeExtension(Path.GetFullPath(bundle.ExportPath) + "\\" + bundle.Name, "json");
                string output = JsonConvert.SerializeObject(json);

                File.WriteAllText(finalPath, output, Encoding.UTF8);
            }

            progressHandler?.Invoke(new BundleExportProgressEventArgs(BundleExportStage.Ended, 100, 100));
        }

        /// <summary>
        /// Gets the export progress for a given animation sheet.
        /// </summary>
        /// <param name="sheet">The sheet to get the current export progress of</param>
        /// <returns>A value from 0-1 specifying the current export progress for the sheet. In case the sheet is not currently being exported, 0 is returned.</returns>
        public float ProgressForAnimationSheet(AnimationSheet sheet)
        {
            return _sheetProgress.TryGetValue(sheet.ID, out float p) ? p : 0;
        }

        /// <summary>
        /// Starts the given tasks and waits for them to complete. This will run, at most, the specified number of tasks in parallel.
        /// <para>NOTE: If one of the given tasks has already been started, an exception will be thrown.</para>
        /// </summary>
        /// <param name="tasksToRun">The tasks to run.</param>
        /// <param name="maxTasksToRunInParallel">The maximum number of tasks to run in parallel.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static void StartAndWaitAllThrottled([NotNull] IEnumerable<Task> tasksToRun, int maxTasksToRunInParallel, CancellationToken cancellationToken = new CancellationToken())
        {
            StartAndWaitAllThrottled(tasksToRun, maxTasksToRunInParallel, -1, cancellationToken);
        }

        /// <summary>
        /// Starts the given tasks and waits for them to complete. This will run, at most, the specified number of tasks in parallel.
        /// <para>NOTE: If one of the given tasks has already been started, an exception will be thrown.</para>
        /// </summary>
        /// <param name="tasksToRun">The tasks to run.</param>
        /// <param name="maxTasksToRunInParallel">The maximum number of tasks to run in parallel.</param>
        /// <param name="timeoutInMilliseconds">The maximum milliseconds we should allow the max tasks to run in parallel before allowing another task to start. Specify -1 to wait indefinitely.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static void StartAndWaitAllThrottled([NotNull] IEnumerable<Task> tasksToRun, int maxTasksToRunInParallel, int timeoutInMilliseconds, CancellationToken cancellationToken = new CancellationToken())
        {
            // Convert to a list of tasks so that we don&#39;t enumerate over it multiple times needlessly.
            var tasks = tasksToRun.ToList();

            using (var throttler = new SemaphoreSlim(maxTasksToRunInParallel))
            {
                var postTaskTasks = new List<Task>();

                // Have each task notify the throttler when it completes so that it decrements the number of tasks currently running.
                // ReSharper disable once AccessToDisposedClosure See bellow: This is never disposed before leaving method
                tasks.ForEach(t => postTaskTasks.Add(t.ContinueWith(tsk => throttler.Release(), cancellationToken)));

                // Start running each task.
                foreach (var task in tasks)
                {
                    // Increment the number of tasks currently running and wait if too many are running.
                    throttler.Wait(timeoutInMilliseconds, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                    task.Start();
                }

                // Wait for all of the provided tasks to complete.
                // We wait on the list of "post" tasks instead of the original tasks, otherwise there is a potential
                // race condition where the throttler&#39;s using block is exited before some Tasks have had their "post"
                // action completed, which references the throttler, resulting in an exception due to accessing a disposed object.
                Task.WaitAll(postTaskTasks.ToArray(), cancellationToken);
            }
        }
        
        /// <summary>
        /// Bundles an exported BundleSheet and a path into one structure
        /// </summary>
        private struct BundleSheetJson
        {
            public readonly BundleSheetExport BundleSheet;
            public readonly string SheetName;
            public readonly string ExportPath;

            public BundleSheetJson(BundleSheetExport bundleSheet, string sheetName, string exportPath)
            {
                BundleSheet = bundleSheet;
                ExportPath = exportPath;
                SheetName = sheetName;
            }
        }
    }
}