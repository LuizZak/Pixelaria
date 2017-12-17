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
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace PixCore.Undo
{
    /// <summary>
    /// Base interface for performing undo/redo in undo systems.
    /// 
    /// See <see cref="UndoSystem"/> class.
    /// </summary>
    public interface IUndoSystem
    {
        /// <summary>
        /// Gets whether this UndoSystem can currently undo a task
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// Gets whether this UndoSystem can currently redo a task
        /// </summary>
        bool CanRedo { get; }
        
        /// <summary>
        /// Undoes one task on this UndoSystem
        /// </summary>
        void Undo();

        /// <summary>
        /// Redoes one task on this UndoSystem
        /// </summary>
        void Redo();
    }

    /// <summary>
    /// Enables recording and performing of series of undo/redo tasks.
    /// </summary>
    public class UndoSystem : IUndoSystem
    {
        /// <summary>
        /// Set to true before any undo/redo task, and subsequently set to false before returning.
        /// 
        /// Used to detect incorrect reentry calls to this undo system
        /// </summary>
        private bool _isDoingWork;

        /// <summary>
        /// The list of tasks that can be undone/redone
        /// </summary>
        private readonly List<IUndoTask> _undoTasks;

        /// <summary>
        /// The index of the current redo task.
        /// From this index onwards, all UndoTasks registered on the undo task list are considered
        /// Redo tasks
        /// </summary>
        private int _currentTask;

        /// <summary>
        /// The current group undo task
        /// </summary>
        private GroupUndoTask _currentGroupUndoTask;

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
        /// Occurs whenever a task will be undone
        /// </summary>
        public event UndoEventHandler WillPerformUndo;

        /// <summary>
        /// Occurs whenever a task was undone
        /// </summary>
        public event UndoEventHandler UndoPerformed;

        /// <summary>
        /// Occurs whenever a task will be redone
        /// </summary>
        public event UndoEventHandler WillPerformRedo;

        /// <summary>
        /// Occurs whenever a task was redone
        /// </summary>
        public event UndoEventHandler RedoPerformed;

        /// <summary>
        /// Occurs whenever the undo system was cleared
        /// </summary>
        public event EventHandler Cleared;

        /// <summary>
        /// Gets the ammount of tasks currently held by this UndoSystem
        /// </summary>
        public int Count => _undoTasks.Count;

        /// <summary>
        /// Gets or sets the maximum ammount of tasks this UndoSystem can store
        /// </summary>
        public int MaximumTaskCount { get; set; }

        /// <summary>
        /// Gets whether this UndoSystem can currently undo a task
        /// </summary>
        public bool CanUndo => _currentTask > 0;

        /// <summary>
        /// Gets whether this UndoSystem can currently redo a task
        /// </summary>
        public bool CanRedo => _currentTask < _undoTasks.Count;

        /// <summary>
        /// Gets whether this UndoSystem is currently in group undo mode, recording all
        /// current undos into a group that will be stored as a single undo task later
        /// </summary>
        public bool InGroupUndo => _currentGroupUndoTask != null;

        /// <summary>
        /// Returns the next undo operation on the undo stack. If there's no undo operation available, null is returned
        /// </summary>
        [CanBeNull]
        public IUndoTask NextUndo => CanUndo ? _undoTasks[_currentTask - 1] : null;

        /// <summary>
        /// Returns the next redo operation on the undo stack. If there's no redo operation available, null is returned
        /// </summary>
        [CanBeNull]
        public IUndoTask NextRedo => CanRedo ? _undoTasks[_currentTask] : null;

        /// <summary>
        /// Initializes a new instance of the UndoSystem class
        /// </summary>
        public UndoSystem()
        {
            _undoTasks = new List<IUndoTask>();
            _currentTask = 0;
            MaximumTaskCount = 15;
        }

        /// <summary>
        /// Registers the given UndoTask on this UndoSystem
        /// </summary>
        /// <param name="task">The task to undo</param>
        /// <exception cref="ArgumentNullException">The undo task provided is null</exception>
        /// <exception cref="UndoSystemRecursivelyModifiedException">If called by an <see cref="IUndoTask"/>'s Undo/Redo method, either directly or indirectly, before <see cref="Undo"/>/<see cref="Redo"/> return (calling during <see cref="UndoPerformed"/>/<see cref="RedoPerformed"/> is safe, however).</exception>
        public void RegisterUndo([NotNull] IUndoTask task)
        {
            CheckReentry();

            if (task == null)
                throw new ArgumentNullException(nameof(task), @"The task cannot be null");

            // Grouped undos: record them inside the group undo
            if (InGroupUndo)
            {
                _currentGroupUndoTask.AddTask(task);
                return;
            }

            // Redo task clearing
            ClearRedos();

            // Task capping
            if (_undoTasks.Count >= MaximumTaskCount)
            {
                while (_undoTasks.Count >= MaximumTaskCount)
                {
                    _undoTasks[0].Clear();

                    _undoTasks.RemoveAt(0);
                }
            }
            else
            {
                _currentTask++;
            }

            _undoTasks.Add(task);

            UndoRegistered?.Invoke(this, new UndoEventArgs(task));
        }

        /// <summary>
        /// Undoes one task on this UndoSystem
        /// </summary>
        /// <exception cref="UndoSystemRecursivelyModifiedException">If called by an <see cref="IUndoTask"/>'s Undo/Redo method, either directly or indirectly, before <see cref="Undo"/>/<see cref="Redo"/> return (calling during <see cref="UndoPerformed"/>/<see cref="RedoPerformed"/> is safe, however).</exception>
        public void Undo()
        {
            CheckReentry();

            // Finish any currently opened group undos
            if (InGroupUndo)
            {
                FinishGroupUndo(_currentGroupUndoTask.DiscardOnOperation);
            }

            if (_currentTask == 0)
                return;

            _isDoingWork = true;

            // Get the task to undo
            var task = _undoTasks[_currentTask - 1];
            
            WillPerformUndo?.Invoke(this, new UndoEventArgs(task));

            _currentTask--;
            task.Undo();

            _isDoingWork = false;

            UndoPerformed?.Invoke(this, new UndoEventArgs(task));
        }

        /// <summary>
        /// Redoes one task on this UndoSystem
        /// </summary>
        /// <exception cref="UndoSystemRecursivelyModifiedException">If called by an <see cref="IUndoTask"/>'s Undo/Redo method, either directly or indirectly, before <see cref="Undo"/>/<see cref="Redo"/> return (calling during <see cref="UndoPerformed"/>/<see cref="RedoPerformed"/> is safe, however).</exception>
        public void Redo()
        {
            CheckReentry();

            // Finish any currently opened group undos
            if (InGroupUndo)
            {
                FinishGroupUndo(_currentGroupUndoTask.DiscardOnOperation);
            }
            
            if (_currentTask == _undoTasks.Count)
                return;
            
            _isDoingWork = true;

            // Get the task to undo
            var task = _undoTasks[_currentTask];
            
            WillPerformRedo?.Invoke(this, new UndoEventArgs(task));

            _currentTask++;
            task.Redo();

            _isDoingWork = false;
            
            RedoPerformed?.Invoke(this, new UndoEventArgs(task));
        }

        /// <summary>
        /// Starts a group undo task
        /// </summary>
        /// <param name="description">A description for the task</param>
        /// <param name="discardOnOperation">Whether to discard the undo group if it's opened on this UndoSystem while it receives an undo/redo call</param>
        /// <exception cref="UndoSystemRecursivelyModifiedException">If called by an <see cref="IUndoTask"/>'s Undo/Redo method, either directly or indirectly, before <see cref="Undo"/>/<see cref="Redo"/> return (calling during <see cref="UndoPerformed"/>/<see cref="RedoPerformed"/> is safe, however).</exception>
        public void StartGroupUndo(string description, bool discardOnOperation = false)
        {
            CheckReentry();

            if (InGroupUndo)
                return;

            _currentGroupUndoTask = new GroupUndoTask(description) { DiscardOnOperation = discardOnOperation };
        }

        /// <summary>
        /// Finishes and records the current grouped undo tasks
        /// </summary>
        /// <param name="cancel">Whether to cancel the undo operations currently grouped</param>
        /// <exception cref="UndoSystemRecursivelyModifiedException">If called by an <see cref="IUndoTask"/>'s Undo/Redo method, either directly or indirectly, before <see cref="Undo"/>/<see cref="Redo"/> return (calling during <see cref="UndoPerformed"/>/<see cref="RedoPerformed"/> is safe, however).</exception>
        public void FinishGroupUndo(bool cancel = false)
        {
            CheckReentry();

            if (!InGroupUndo)
                return;

            var task = _currentGroupUndoTask;
            _currentGroupUndoTask = null;

            if (task.UndoList.Count > 0 && !cancel)
            {
                RegisterUndo(task);
            }
            else
            {
                task.Clear();
            }
        }

        /// <summary>
        /// Removes and returns the next undo task from this UndoSystem's undo list without
        /// performing it.
        /// If no undo task is available, null is returned
        /// </summary>
        /// <returns>The next available undo operation if available, null otherwise</returns>
        [CanBeNull]
        public IUndoTask PopUndo()
        {
            if (!CanUndo)
                return null;

            var task = NextUndo;

            _undoTasks.Remove(task);
            _currentTask--;

            return task;
        }

        /// <summary>
        /// Removes and returns the next redo task from this UndoSystem's undo list without
        /// performing it.
        /// If no redo task is available, null is returned
        /// </summary>
        /// <returns>The next available redo operation if available, null otherwise</returns>
        [CanBeNull]
        public IUndoTask PopRedo()
        {
            if (!CanRedo)
                return null;

            var task = NextRedo;

            _undoTasks.Remove(task);

            return task;
        }

        /// <summary>
        /// Clears all operations on this UndoSystem
        /// </summary>
        public void Clear()
        {
            foreach (var task in _undoTasks)
            {
                task.Clear();
            }

            _undoTasks.Clear();

            _currentTask = 0;

            Cleared?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Clear all redo tasks currently stored on this UndoSystem
        /// </summary>
        private void ClearRedos()
        {
            if (_currentTask == _undoTasks.Count)
                return;

            for (int i = _currentTask; i < _undoTasks.Count; i++)
            {
                _undoTasks[i].Clear();
            }

            _undoTasks.RemoveRange(_currentTask, _undoTasks.Count - _currentTask);
        }

        /// <summary>
        /// Throws an <see cref="UndoSystemRecursivelyModifiedException"/> if recursive reentry was detected.
        /// </summary>
        private void CheckReentry()
        {
            if(_isDoingWork)
                throw new UndoSystemRecursivelyModifiedException();
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
    /// An undo task that encloses multiple IUndoTasks in it
    /// </summary>
    public class GroupUndoTask : IUndoTask
    {
        /// <summary>
        /// The list of undo tasks enclosed in this GroupUndoTask
        /// </summary>
        readonly List<IUndoTask> _undoList;

        /// <summary>
        /// The description for this GroupUndoTask instance
        /// </summary>
        private readonly string _description;

        /// <summary>
        /// Gets or sets a value specifying whether to discard the undo group if it's opened on an UndoSystem while it receives an undo/redo call
        /// </summary>
        public bool DiscardOnOperation { get; set; }

        /// <summary>
        /// Gets or sets whether to reverse the order of the operations on undo
        /// </summary>
        public bool ReverseOnUndo { get; set; }

        /// <summary>
        /// Gets a read-only version of the internal undo tasks list for this group undo task
        /// </summary>
        public ReadOnlyCollection<IUndoTask> UndoList => _undoList.AsReadOnly();

        /// <summary>
        /// Initializes a new instance of the GroupUndoTask class with a description
        /// </summary>
        /// <param name="description">The description for this GroupUndoTask</param>
        public GroupUndoTask(string description)
        {
            _undoList = new List<IUndoTask>();
            _description = description;
            ReverseOnUndo = true;
        }

        /// <summary>
        /// Initializes a new instance of the GroupUndoTask class with a list of tasks to perform and a description
        /// </summary>
        /// <param name="tasks">The tasks to perform</param>
        /// <param name="description">The description for this GroupUndoTask</param>
        /// <param name="discardOnOperation">Whether to reverse the order of the operations on undo</param>
        /// <param name="reverseOnUndo">Whether to perform the undo operations in the reverse order the trasks where added</param>
        public GroupUndoTask(IEnumerable<IUndoTask> tasks, string description, bool discardOnOperation = false, bool reverseOnUndo = true)
            : this(description)
        {
            if(tasks != null)
                AddTasks(tasks);

            DiscardOnOperation = discardOnOperation;
            ReverseOnUndo = reverseOnUndo;
        }

        /// <summary>
        /// Adds a new task on this GroupUndoTask
        /// </summary>
        /// <param name="task">The task to add to this GroupUndoTask</param>
        public void AddTask(IUndoTask task)
        {
            _undoList.Add(task);
        }

        /// <summary>
        /// Adds a list of tasks on this GroupUndoTask
        /// </summary>
        /// <param name="tasks">The tasks to add to this GroupUndoTask</param>
        public void AddTasks([NotNull] IEnumerable<IUndoTask> tasks)
        {
            foreach (var task in tasks)
            {
                AddTask(task);
            }
        }

        /// <summary>
        /// Clears this UndoTask object
        /// </summary>
        public void Clear()
        {
            foreach (var task in _undoList)
            {
                task.Clear();
            }

            _undoList.Clear();
        }

        /// <summary>
        /// Undoes this task
        /// </summary>
        public void Undo()
        {
            if (ReverseOnUndo)
            {
                // Undo in reverse order (last to first)
                for (int i = _undoList.Count - 1; i >= 0; i--)
                {
                    _undoList[i].Undo();
                }
            }
            else
            {
                foreach (var task in _undoList)
                {
                    task.Undo();
                }
            }
        }

        /// <summary>
        /// Redoes this task
        /// </summary>
        public void Redo()
        {
            foreach (var task in _undoList)
            {
                task.Redo();
            }
        }

        /// <summary>
        /// Returns a short string description of this UndoTask
        /// </summary>
        /// <returns>A short string description of this UndoTask</returns>
        public string GetDescription()
        {
            return _description;
        }
    }

    /// <summary>
    /// Arguments for the UndoRegistered event
    /// </summary>
    public class UndoEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the task associated with this event
        /// </summary>
        public IUndoTask Task { get; }

        /// <summary>
        /// Creates a new instance of the UndoEventArgs
        /// </summary>
        /// <param name="task">The task associated with this event</param>
        public UndoEventArgs(IUndoTask task)
        {
            Task = task;
        }
    }

    /// <summary>
    /// Exception that signals when a modifying call to an <see cref="UndoSystem"/> is made while an undo/redo
    /// task is being performed.
    /// 
    /// Usually this indicates that an undo task is calling code to undo work that itself is incorrectly trying to 
    /// register an undo task of its own to undo its work.
    /// </summary>
    public class UndoSystemRecursivelyModifiedException : Exception
    {
        public UndoSystemRecursivelyModifiedException()
            : base("A side-effect-containing call to UndoSystem was made while an undo operation was underway")
        {

        }
    }
}