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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Collection of assorted asynchronous task constructs
    /// </summary>
    public static class AsyncHelpers
    {
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

            using var throttler = new SemaphoreSlim(maxTasksToRunInParallel);
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
}