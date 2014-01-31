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

namespace Pixelaria.Data.Undo
{
    /// <summary>
    /// Enables performing of an undo/redo task
    /// </summary>
    public class UndoSystem
    {
        /// <summary>
        /// The list of tasks that can be undone/redone
        /// </summary>
        private List<IUndoTask> undoTasks;

        /// <summary>
        /// The index of the current redo task.
        /// From this index onwards, all UndoTasks registered on the undo task list are considered
        /// Redo tasks
        /// </summary>
        private int currentTask;

        /// <summary>
        /// The maximum ammount of tasks this UndoSystem can store
        /// </summary>
        private int maxTaskCount;

        /// <summary>
        /// Event handler for the events on the UndoSystem
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The arguments for the event</param>
        public delegate void UndoEventHandler(object sender, UndoEventArgs e);

        /// <summary>
        /// Occurs whenever a new Undo task is registered
        /// </summary>
        public event UndoEventHandler UndoRegistered;

        /// <summary>
        /// Occurs whenever a task was undone
        /// </summary>
        public event UndoEventHandler UndoPerformed;

        /// <summary>
        /// Occurs whenever a task was redone
        /// </summary>
        public event UndoEventHandler RedoPerformed;

        /// <summary>
        /// Gets the ammount of tasks currently held by this UndoSystem
        /// </summary>
        public int Count { get { return undoTasks.Count; } }

        /// <summary>
        /// Gets or sets the maximum ammount of tasks this UndoSystem can store
        /// </summary>
        public int MaximumTaskCount { get { return maxTaskCount; } set { maxTaskCount = value; } }

        /// <summary>
        /// Gets whether this UndoSystem can currently undo a task
        /// </summary>
        public bool CanUndo { get { return currentTask > 0; } }

        /// <summary>
        /// Gets whether this UndoSystem can currently redo a task
        /// </summary>
        public bool CanRedo { get { return currentTask < undoTasks.Count; } }

        /// <summary>
        /// Returns the next undo operation on the undo stack. If there's no undo operation available, null is returned
        /// </summary>
        public IUndoTask NextUndo { get { return CanUndo ? undoTasks[currentTask - 1] : null; } }

        /// <summary>
        /// Returns the next redo operation on the undo stack. If there's no redo operation available, null is returned
        /// </summary>
        public IUndoTask NextRedo { get { return CanRedo ? undoTasks[currentTask] : null; } }

        /// <summary>
        /// Initializes a new instance of the UndoSystem class
        /// </summary>
        public UndoSystem()
        {
            undoTasks = new List<IUndoTask>();
            currentTask = 0;
            maxTaskCount = 15;
        }

        /// <summary>
        /// Registers the given UndoTask on this UndoSystem
        /// </summary>
        /// <param name="task">The task to undo</param>
        public void RegisterUndo(IUndoTask task)
        {
            // Redo task clearing
            ClearRedos();

            // Task capping
            if (undoTasks.Count >= maxTaskCount)
            {
                while (undoTasks.Count >= maxTaskCount)
                {
                    undoTasks[0].Clear();

                    undoTasks.RemoveAt(0);
                }
            }
            else
            {
                currentTask++;
            }

            undoTasks.Add(task);

            if (UndoRegistered != null)
            {
                UndoRegistered.Invoke(this, new UndoEventArgs(task));
            }
        }

        /// <summary>
        /// Undoes one task on this UndoSystem
        /// </summary>
        public void Undo()
        {
            if (currentTask == 0)
                return;

            IUndoTask task = undoTasks[--currentTask];

            task.Undo();

            if (UndoPerformed != null)
            {
                UndoPerformed.Invoke(this, new UndoEventArgs(task));
            }
        }

        /// <summary>
        /// Redoes one task on this UndoSystem
        /// </summary>
        public void Redo()
        {
            if (currentTask == undoTasks.Count)
                return;

            IUndoTask task = undoTasks[currentTask++];
            task.Redo();

            if (RedoPerformed != null)
            {
                RedoPerformed.Invoke(this, new UndoEventArgs(task));
            }
        }

        /// <summary>
        /// Clears all operations on this UndoSystem
        /// </summary>
        public void Clear()
        {
            foreach (IUndoTask task in undoTasks)
            {
                task.Clear();
            }

            undoTasks.Clear();

            currentTask = 0;
        }

        /// <summary>
        /// Clear all redo tasks currently stored on this UndoSystem
        /// </summary>
        private void ClearRedos()
        {
            if (currentTask == undoTasks.Count)
                return;

            for (int i = currentTask; i < undoTasks.Count; i++)
            {
                undoTasks[i].Clear();
            }

            undoTasks.RemoveRange(currentTask, undoTasks.Count - currentTask);
        }
    }

    /// <summary>
    /// A task that is capable of being undone/redone
    /// </summary>
    public interface IUndoTask
    {
        /// <summary>
        /// Clears this UndoTask object
        /// </summary>
        void Clear();

        /// <summary>
        /// Undoes this task
        /// </summary>
        void Undo();

        /// <summary>
        /// Redoes this task
        /// </summary>
        void Redo();

        /// <summary>
        /// Returns a short string description of this UndoTask
        /// </summary>
        /// <returns>A short string description of this UndoTask</returns>
        string GetDescription();
    }

    /// <summary>
    /// Arguments for the UndoRegistered event
    /// </summary>
    public class UndoEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the task associated with this event
        /// </summary>
        public IUndoTask Task { get; private set; }

        /// <summary>
        /// Creates a new instance of the UndoEventArgs
        /// </summary>
        /// <param name="task">The task associated with this event</param>
        public UndoEventArgs(IUndoTask task)
        {
            this.Task = task;
        }
    }
}