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
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace PixCore.Undo
{
    /// <summary>
    /// An undo task that encloses multiple IUndoTasks in it
    /// </summary>
    public class GroupUndoTask : IUndoTask
    {
        /// <summary>
        /// The list of undo tasks enclosed in this GroupUndoTask
        /// </summary>
        private readonly List<IUndoTask> _undoList;

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
        /// <param name="discardOnOperation">Whether to discard the undo group if it's opened on an UndoSystem while it receives an undo/redo call</param>
        /// <param name="reverseOnUndo">Whether to perform the undo operations in the reverse order the tasks where added</param>
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
}