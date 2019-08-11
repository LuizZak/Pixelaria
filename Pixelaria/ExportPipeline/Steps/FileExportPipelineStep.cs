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
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Disposables;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Data.Exports;
using PixPipelineGraph;

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// A pipeline step that exports any and all resulting BundleSheetExports
    /// that come in.
    /// </summary>
    public sealed class FileExportPipelineStep : IPipelineEnd
    {
        private readonly CompositeDisposable _disposeBag = new CompositeDisposable();

        public PipelineNodeId Id { get; } = new PipelineNodeId(Guid.NewGuid());

        public string Name { get; } = "Export to File";

        public IReadOnlyList<IPipelineInput> Input { get; }

        /// <summary>
        /// Gets or sets the target file path for this file export pipeline step.
        /// </summary>
        [Editor(typeof(PathStringEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(TypeConverter))]
        public string FilePath { get; set; }

        public FileExportPipelineStep()
        {
            Input = new[]
            {
                new FileExportPipelineInput(this, new PipelineInput())
            };
        }

        public void Begin()
        {
            /*
            Input[0]
                .Connections
                .ToObservable()
                .SelectMany(o => o.GetObservable())
                .OfType<BundleSheetExport>()
                .Subscribe(sheet =>
                {
                    System.Diagnostics.Debug.WriteLine(sheet.Atlas.Name);
                }, error =>
                {
                    System.Diagnostics.Debug.WriteLine(error);
                }, () =>
                {
                    System.Diagnostics.Debug.WriteLine("Completed.");
                }).AddToDisposable(_disposeBag);
            */
        }

        public void Dispose()
        {
            _disposeBag.Dispose();
        }

        public IPipelineMetadata GetMetadata()
        {
            return new PipelineMetadata(new Dictionary<string, object>
            {
                {
                    PipelineMetadataKeys.EditableProperties,
                    new EditableTypePropertyProxy(this,
                        GetType().GetProperty(nameof(FilePath)) ?? throw new InvalidOperationException())
                }
            });
        }

        public sealed class FileExportPipelineInput : IPipelineInput
        {
            public PipelineNodeId NodeId => Node.Id;
            public string Name { get; } = "Generated Sprite Sheet";
            public IPipelineNode Node { get; }

            public PipelineInput Id { get; }
            public IReadOnlyList<Type> DataTypes { get; } = new []{typeof(BundleSheetExport)};

            public FileExportPipelineInput([NotNull] IPipelineNode step, PipelineInput id)
            {
                Node = step;
                Id = id;
            }

            public IPipelineMetadata GetMetadata()
            {
                return PipelineMetadata.Empty;
            }
        }

        private class PathStringEditor : UITypeEditor
        {
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.FileName = (string) value;
                    if (dlg.ShowDialog(Application.OpenForms[0]) == DialogResult.OK)
                        value = dlg.FileName;
                }

                return value;
            }
        }
    }
}