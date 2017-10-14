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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Controllers.Exporters;
using Pixelaria.Controllers.Importers;
using Pixelaria.Controllers.Validators;

using Pixelaria.Data;
using Pixelaria.Data.Exports;
using Pixelaria.Data.Factories;
using Pixelaria.Data.Persistence;

using Pixelaria.Properties;

using Pixelaria.Views;
using Pixelaria.Views.MiscViews;
using Pixelaria.Views.ModelViews;

using Pixelaria.Utils;

using Settings = Pixelaria.Utils.Settings;

namespace Pixelaria.Controllers
{
    /// <summary>
    /// Main application controller
    /// </summary>
    public sealed partial class Controller : IDisposable
    {
        /// <summary>
        /// The main application form
        /// </summary>
        private readonly MainForm _mainForm;

        /// <summary>
        /// Internal reactive binder
        /// </summary>
        private readonly Reactive _reactive = new Reactive();

        /// <summary>
        /// Gets the reactive interface object
        /// </summary>
        public IReactive Rx => _reactive;

        /// <summary>
        /// Gets the current bundle opened on the application
        /// </summary>
        public Bundle CurrentBundle { get; private set; }
        
        /// <summary>
        /// Gets the current IDefaultImporter of the program
        /// </summary>
        public IAnimationImporter AnimationImporter { get; }

        /// <summary>
        /// Gets the current IAnimationValidator of the program
        /// </summary>
        public IAnimationValidator AnimationValidator { get; }

        /// <summary>
        /// Gets the current IAnimationSheetValidator of the program
        /// </summary>
        public IAnimationSheetValidator AnimationSheetValidator { get; }

        /// <summary>
        /// Gets the current IFrameFactory of the program
        /// </summary>
        public FrameFactory FrameFactory { get; }

        /// <summary>
        /// Gets the interface state provider for this controller, which can be used
        /// anywhere on the gui interface to check states across forms
        /// </summary>
        public IInterfaceStateProvider InterfaceStateProvider { get; }

        /// <summary>
        /// Gets whether the current bundle has unsaved changes
        /// </summary>
        public bool UnsavedChanges { get; private set; }

        /// <summary>
        /// Gets the current RecentFileList for the program
        /// </summary>
        public RecentFileList CurrentRecentFileList { get; }

        /// <summary>
        /// The main application form
        /// </summary>
        public MainForm MainForm => _mainForm;

        #region Eventing

        /// <summary>
        /// Delegate for animation-related events
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="e">The arguments for the event</param>
        public delegate void AnimationEventHandler(object sender, AnimationEventArgs e);

        /// <summary>
        /// Delegate for animation sheet-related events
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="e">The arguments for the event</param>
        public delegate void AnimationSheetEventHandler(object sender, AnimationSheetEventArgs e);

        /// <summary>
        /// Event fired whenever an animation has been added to a bundle
        /// </summary>
        public event AnimationEventHandler AnimationAdded;

        /// <summary>
        /// Event fired whenever an animation has been removed from a bundle
        /// </summary>
        public event AnimationEventHandler AnimationRemoved;

        /// <summary>
        /// Event fired whenever an animation sheet has been added to a bundle
        /// </summary>
        public event AnimationSheetEventHandler AnimationSheetAdded;

        /// <summary>
        /// Event fired whenever an animation sheet has been removed from a bundle
        /// </summary>
        public event AnimationSheetEventHandler AnimationSheetRemoved;

        /// <summary>
        /// Event fired whenever a view has had its Modified state changed.
        /// This is a forwarded event subscriber, and the 'sender' argument
        /// matches the view that was modified, not this Controller instance.
        /// </summary>
        public event EventHandler ViewModifiedChanged
        {
            add => _mainForm.ChildViewModifiedChanged += value;
            remove => _mainForm.ChildViewModifiedChanged -= value;
        }

        /// <summary>
        /// Event fired whenever a view has opened or closed on the main form.
        /// This is a forwarded event subscriber, and the 'sender' argument matches the
        /// form view which raises this event, not this Controller instance.
        /// </summary>
        public event MainForm.ViewOpenedClosedEventDelegate ViewOpenedClosed
        {
            add => _mainForm.ViewOpenedClosed += value;
            remove => _mainForm.ViewOpenedClosed -= value;
        }

        #endregion

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="mainForm">The form to use as the main form of the application</param>
        public Controller(MainForm mainForm)
        {
            // Initialize the factories
            FrameFactory = new FrameFactory(null);

            // Initialize the validators and exporters
            var defValidator = new DefaultValidator(this);

            AnimationValidator = defValidator;
            AnimationSheetValidator = defValidator;
            InterfaceStateProvider = this;

            AnimationImporter = new AnimationPngImporter();

            // Initialize the Settings singleton
            Settings.GetSettings(Path.GetDirectoryName(Application.LocalUserAppDataPath) + "\\settings.ini");

            CurrentRecentFileList = new RecentFileList(10);

            if (mainForm != null)
            {
                mainForm.Controller = this;
                // Initialize the basic fields
                _mainForm = mainForm;
                _mainForm.UpdateRecentFilesList();

                // Start with a new empty bundle
                ShowNewBundle();
            }
        }
        
        public void Dispose()
        {
            _mainForm?.Dispose();
            _reactive?.Dispose();
            CurrentBundle?.Dispose();
        }

        ////////////////////////////////////////////////////////////////////////////////
        //////////
        ////////// Bundle Related Methods
        //////////
        /////
        ///// Methods that change the state of the program by manipulating the Bundle
        ///// directly.
        /////
        ////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saves the currently loaded bundle to the given path on disk
        /// </summary>
        /// <param name="savePath">The path to save the currently bundle to</param>
        public void SaveBundle([NotNull] string savePath)
        {
            CurrentBundle.SaveFile = savePath;
            PixelariaSaverLoader.SaveBundleToDisk(CurrentBundle, savePath);

            MarkUnsavedChanges(false);
        }

        /// <summary>
        /// Opens a loaded bundle from the given path on disk
        /// </summary>
        /// <param name="savePath">The path to load the bundle from</param>
        public void LoadBundleFromFile([NotNull] string savePath)
        {
            var bundle = PixelariaSaverLoader.LoadBundleFromDisk(savePath);

            if (bundle == null)
            {
                MessageBox.Show(Resources.ErrorLoadingFile, Resources.Error_AlertTile, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Dispose of the current bundle if it's present
            if (CurrentBundle != null)
            {
                CloseBundle(CurrentBundle);
            }
            
            bundle.SaveFile = savePath;

            LoadBundle(bundle);

            // Store the file now
            CurrentRecentFileList.StoreFile(savePath);
            _mainForm.UpdateRecentFilesList();
        }

        /// <summary>
        /// Loads the given bundle into the interface.
        /// This method disposes of the current bundle
        /// </summary>
        /// <param name="newBundle">The new bundle to load</param>
        public void LoadBundle([NotNull] Bundle newBundle)
        {
            CurrentBundle = newBundle;

            _mainForm.LoadBundle(CurrentBundle);

            // Update the Unsaved Changes flag to false
            MarkUnsavedChanges(false);
        }

        /// <summary>
        /// Loads a bundle from the list of recent files list
        /// </summary>
        /// <param name="index">The index to get the file path from</param>
        public void LoadBundleFromRecentFileList(int index)
        {
            if (!File.Exists(CurrentRecentFileList[index]))
            {
                if (MessageBox.Show(Resources.UnexistingFileInFileList_RemoveQuestion, Resources.Question_AlertTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    CurrentRecentFileList.RemoveFromList(index);
                    _mainForm.UpdateRecentFilesList();
                }

                return;
            }

            // Cancel on changes saving confirmation quits the method
            if (ShowConfirmSaveChanges() == DialogResult.Cancel)
                return;

            var file = CurrentRecentFileList[index];
            if (file != null)
                LoadBundleFromFile(file);
        }

        /// <summary>
        /// Closes the given bundle from the controller
        /// </summary>
        /// <param name="bundle">The bundle to close</param>
        public void CloseBundle([NotNull] Bundle bundle)
        {
            bundle.Dispose();
        }

        /// <summary>
        /// Marks whether or not the current bundle has unsaved changes.
        /// This method alters the interface to display unsaved changes accordingly.
        /// This method does not change anything if the new unsaved changes flag is the same
        /// as the current one
        /// </summary>
        /// <param name="isUnsaved">The new value for the Unsaved Changes flag</param>
        public void MarkUnsavedChanges(bool isUnsaved)
        {
            if (CurrentBundle == null || isUnsaved == UnsavedChanges)
                return;

            UnsavedChanges = isUnsaved;

            _mainForm.UnsavedChangesUpdated(isUnsaved);
        }

        /// <summary>
        /// Creates and returns a new Animation.
        /// This method also adds the newly created animation to the currently loaded bundle
        /// </summary>
        /// <param name="name">The name of the new animation</param>
        /// <param name="width">The width of the animation</param>
        /// <param name="height">The height of the animation</param>
        /// <param name="fps">The FPS for the animation</param>
        /// <param name="frameskip">Whether the animation should frameskip</param>
        /// <param name="openOnForm">Whether to open the newly added animation on the main form</param>
        /// <param name="parentSheet">Optional AnimationSheet that will own the newly created animation</param>
        /// <returns>The newly created animation</returns>
        public Animation CreateAnimation(string name, int width, int height, int fps, bool frameskip, bool openOnForm,
            [CanBeNull] AnimationSheet parentSheet = null)
        {
            var anim = new Animation(name, width, height)
            {
                PlaybackSettings = new AnimationPlaybackSettings { FPS = fps, FrameSkip = frameskip }
            };

            // Create a dummy frame
            new AnimationController(CurrentBundle, anim).CreateFrame();

            AddAnimation(anim, openOnForm, parentSheet);

            return anim;
        }

        /// <summary>
        /// Adds the given Animation into the current bundle
        /// </summary>
        /// <param name="anim">The animation to add to the bundle</param>
        /// <param name="openOnForm">Whether to open the newly added animation on the main form</param>
        /// <param name="parentSheet">Optional AnimationSheet that will own the newly created animation</param>
        public void AddAnimation([NotNull] Animation anim, bool openOnForm, [CanBeNull] AnimationSheet parentSheet = null)
        {
            CurrentBundle.AddAnimation(anim, parentSheet);

            if (openOnForm)
            {
                _mainForm.AddAnimation(anim, true);
                _mainForm.OpenViewForAnimation(anim);
            }
            else
            {
                _mainForm.AddAnimation(anim);
            }

            AnimationAdded?.Invoke(this, new AnimationEventArgs(anim));

            MarkUnsavedChanges(true);

            if (parentSheet != null)
            {
                _reactive.RxOnAnimationSheetUpdate.OnNext(parentSheet);
            }
        }

        /// <summary>
        /// Removes the given Animation from the current bundle
        /// </summary>
        /// <param name="anim">The Animation to remove from the bundle</param>
        public void RemoveAnimation([NotNull] Animation anim)
        {
            var sheet = GetOwningAnimationSheet(anim);

            CurrentBundle.RemoveAnimation(anim);

            _mainForm.RemoveAnimation(anim);

            AnimationRemoved?.Invoke(this, new AnimationEventArgs(anim));

            MarkUnsavedChanges(true);

            // Dispose of the animation
            anim.Dispose();

            if (sheet != null)
            {
                _reactive.RxOnAnimationSheetUpdate.OnNext(sheet);
            }
        }

        /// <summary>
        /// Method to be called whenever changes have been made to the fields of an Animation
        /// </summary>
        /// <param name="anim">The Animation that was modified</param>
        public void UpdatedAnimation([NotNull] Animation anim)
        {
            _mainForm.UpdateAnimation(anim);

            _reactive.RxOnAnimationUpdate.OnNext(anim);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Gets the index of the given Animation object inside its current parent container
        /// </summary>
        /// <param name="anim">The animation to get the index of</param>
        /// <returns>The index of the animation in its current parent container</returns>
        public int GetAnimationIndex(Animation anim)
        {
            return CurrentBundle.GetAnimationIndex(anim);
        }

        /// <summary>
        /// Rearranges the index of an Animation in the animation's current storing container
        /// </summary>
        /// <param name="anim">The animation to rearrange</param>
        /// <param name="newIndex">The new index to place the animation at</param>
        public void RearrangeAnimationsPosition(Animation anim, int newIndex)
        {
            if (!CurrentBundle.RearrangeAnimationsPosition(anim, newIndex))
                return;

            var sheet = GetOwningAnimationSheet(anim);
            if (sheet != null)
                _reactive.RxOnAnimationSheetUpdate.OnNext(sheet);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Creates and returns a new Animation Sheet
        /// </summary>
        /// <param name="name">The name for the animation sheet</param>
        /// <param name="openOnForm">Whether to open the newly added animation sheet on the main form</param>
        public AnimationSheet CreateAnimationSheet(string name, bool openOnForm)
        {
            var sheet = new AnimationSheet(name);

            AddAnimationSheet(sheet, openOnForm);

            return sheet;
        }

        /// <summary>
        /// Adds the given Animation Sheet into the current bundle
        /// </summary>
        /// <param name="sheet">The sheet to load into the current bundle</param>
        /// <param name="openOnForm">Whether to open the newly added animation sheet on the main form</param>
        [CanBeNull]
        public AnimationSheetView AddAnimationSheet([NotNull] AnimationSheet sheet, bool openOnForm)
        {
            CurrentBundle.AddAnimationSheet(sheet);

            if (openOnForm)
            {
                _mainForm.AddAnimationSheet(sheet, true);
                return _mainForm.OpenViewForAnimationSheet(sheet);
            }

            _mainForm.AddAnimationSheet(sheet);

            AnimationSheetAdded?.Invoke(this, new AnimationSheetEventArgs(sheet));

            MarkUnsavedChanges(true);

            return null;
        }

        /// <summary>
        /// Removes the given AnimationSeet from the current bundle
        /// </summary>
        /// <param name="sheet">The sheet to remove from the bundle</param>
        /// <param name="deleteAnimations">Whether to delete the nested animations as well. If set to false, the animations will be moved to the bundle's root</param>
        public void RemoveAnimationSheet([NotNull] AnimationSheet sheet, bool deleteAnimations)
        {
            // Remove/relocate animations
            var animations = sheet.Animations;
            if (deleteAnimations)
                foreach (var anim in animations)
                    RemoveAnimation(anim);

            // Remove the sheet
            CurrentBundle.RemoveAnimationSheet(sheet, false);

            _mainForm.RemoveAnimationSheet(sheet);

            AnimationSheetRemoved?.Invoke(this, new AnimationSheetEventArgs(sheet));

            MarkUnsavedChanges(true);

            if (deleteAnimations)
                foreach (var anim in animations)
                    _reactive.RxOnAnimationUpdate.OnNext(anim);
        }

        /// <summary>
        /// Method to be called whenever changes have been made to the fields of an AnimationSheet
        /// </summary>
        /// <param name="sheet">The AnimationSheet that was modified</param>
        public void UpdatedAnimationSheet([NotNull] AnimationSheet sheet)
        {
            _mainForm.UpdateAnimationSheet(sheet);

            _reactive.RxOnAnimationSheetUpdate.OnNext(sheet);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Gets the index of the given AnimationSheet object inside its current parent container
        /// </summary>
        /// <param name="sheet">The sheet to get the index of</param>
        /// <returns>The index of the sheet in its current parent container</returns>
        public int GetAnimationSheetIndex(AnimationSheet sheet)
        {
            return CurrentBundle.GetAnimationSheetIndex(sheet);
        }

        /// <summary>
        /// Rearranges the index of an AnimationSheets in the sheets's current storing container
        /// </summary>
        /// <param name="sheet">The sheet to rearrange</param>
        /// <param name="newIndex">The new index to place the sheet at</param>
        public void RearrangeAnimationSheetsPosition(AnimationSheet sheet, int newIndex)
        {
            CurrentBundle.RearrangeAnimationSheetsPosition(sheet, newIndex);
            
            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Adds the given Animation object into the given AnimationSheet object
        /// If null is provided as animation sheet, the animation is removed from it's current animation sheet, if it's inside one
        /// </summary>
        /// <param name="anim">The animation to add to the animation sheet</param>
        /// <param name="sheet">The AnimationSheet to add the animation to</param>
        public void AddAnimationToAnimationSheet(Animation anim, AnimationSheet sheet)
        {
            var curSheet = CurrentBundle.GetOwningAnimationSheet(anim);
            if (ReferenceEquals(curSheet, sheet))
                return;

            CurrentBundle.AddAnimationToAnimationSheet(anim, sheet);

            _reactive.RxOnAnimationSheetUpdate.OnNext(sheet);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Gets the AnimationSheet that currently owns the given Animation object.
        /// If the Animation is not inside any AnimationSheet, null is returned
        /// </summary>
        /// <param name="anim">The animation object to get the animation sheet of</param>
        /// <returns>The AnimationSheet that currently owns the given Animation object. If the Animation is not inside any AnimationSheet, null is returned</returns>
        [CanBeNull]
        public AnimationSheet GetOwningAnimationSheet(Animation anim)
        {
            return CurrentBundle.GetOwningAnimationSheet(anim);
        }

        /// <summary>
        /// Returns a unique animation name, used for filling in default animation names
        /// </summary>
        /// <returns>A unique animation name to use as a default name</returns>
        public string GetUniqueUntitledAnimationName()
        {
            string prefix = "Untitled-";
            int postfix = 1;

            while (CurrentBundle.GetAnimationByName(prefix + postfix) != null)
            {
                postfix++;
            }

            return prefix + postfix;
        }

        /// <summary>
        /// Returns a unique animation sheet name, used for filling in default animation sheet names
        /// </summary>
        /// <returns>A unique animation sheet name to use as a default name</returns>
        public string GetUniqueUntitledAnimationSheetName()
        {
            string prefix = "Untitled-";
            int postfix = 1;

            while (CurrentBundle.GetAnimationSheetByName(prefix + postfix) != null)
            {
                postfix++;
            }

            return prefix + postfix;
        }

        ////////////////////////////////////////////////////////////////////////////////
        //////////
        ////////// Interface Related Methods
        //////////
        /////
        ///// Methods that rely on interface output to change the state of the program.
        ///// These methods will usually use interface to confirm interactions with the
        ///// user before calling the direct bundle manipulation methods.
        /////
        ////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Shows an interface for creating a new Bundle to the user. If there are unsaved changes on the current bundle,
        /// an interface for saving changes is shown.
        /// </summary>
        public void ShowNewBundle()
        {
            // Cancel on changes saving confirmation quits the method
            if (ShowConfirmSaveChanges() == DialogResult.Cancel)
                return;

            // Create a new bundle
            LoadBundle(new Bundle("Untitled Bundle"));
        }

        /// <summary>
        /// Shows an interface for Bundle loading to the user. If there are unsaved changes on the current bundle,
        /// an interface for saving changes is shown.
        /// </summary>
        public void ShowLoadBundle()
        {
            // Cancel on changes saving confirmation quits the method
            if (ShowConfirmSaveChanges() == DialogResult.Cancel)
                return;

            var ofd = new OpenFileDialog { Filter = @"Pixelaria Bundle (*.pxl)|*.pxl" };

            if (ofd.ShowDialog(_mainForm) == DialogResult.OK)
            {
                LoadBundleFromFile(ofd.FileName);
            }
        }

        /// <summary>
        /// Shows an interface for Bundle exporting to the user. If the bundle has no export path set, an interface for editing the
        /// bundle is shown.
        /// </summary>
        public void ShowExportBundle()
        {
            // The bundle needs at least one valid animation sheet with one animation in it before exporting
            if (CurrentBundle.AnimationSheets.Count == 0)
            {
                MessageBox.Show(Resources.NoAnimationSheetsToExportInfo, Resources.Information_AlertTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Whether there are any animation sheets with at least one animation in it
            bool validSheet = CurrentBundle.AnimationSheets.Any(sheet => sheet.Animations.Length != 0);

            if (!validSheet)
            {
                MessageBox.Show(Resources.NoAnimationsInSheetsAlert, Resources.Information_AlertTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // The bundle path must be valid
            try
            {
                var fullPath = Path.GetFullPath(CurrentBundle.ExportPath);

                if (!Directory.Exists(fullPath))
                {
                    if (MessageBox.Show(Resources.NonExistantBundleExportPathAlert_AskCreate, Resources.Question_AlertTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception)
            {
                if (MessageBox.Show(Resources.InvalidBundleExportPathAlert_AskEdit, Resources.Question_AlertTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _mainForm.OpenBundleSettings(CurrentBundle);
                    return;
                }
                else
                {
                    return;
                }
            }

            var progressForm = new BundleExportProgressView(CurrentBundle, GetExporter());

            progressForm.ShowDialog(_mainForm);
        }

        /// <summary>
        /// Displays an interface for saving the currently opened bundle.
        /// The method returns the DialogResult of the SaveFileDialog that was opened. If no dialog was opened (because the file was already saved on disk),
        /// DialogResult.OK is returned anyways.
        /// </summary>
        /// <param name="forceNew">Whether to show an interface to choose a new save location even if the current bundle already has been saved to disk</param>
        /// <returns>The DialogResult of the SaveFileDialog</returns>
        public DialogResult ShowSaveBundle(bool forceNew = false)
        {
            string savePath = CurrentBundle.SaveFile;

            if (savePath == "" || forceNew)
            {
                SaveFileDialog svd = new SaveFileDialog { Filter = @"Pixelaria Bundle (*.pxl)|*.pxl" };

                if (svd.ShowDialog(_mainForm) == DialogResult.OK)
                {
                    savePath = svd.FileName;
                }
                else
                {
                    return DialogResult.Cancel;
                }

                // Store the file now
                CurrentRecentFileList.StoreFile(savePath);
                _mainForm.UpdateRecentFilesList();
            }

            SaveBundle(savePath);

            return DialogResult.OK;
        }

        /// <summary>
        /// Shows a confirmation of changes save interface to the user if changes have been made to the bundle.
        /// If no changes have been made to the bundle, DialogResult.Yes is returned anyways.
        /// This method saves the changes made to disk as well.
        /// </summary>
        /// <returns>The DialogResult of the confirmation MessageBox. If no changes have been made to the bundle, DialogResult.Yes is returned anyways</returns>
        public DialogResult ShowConfirmSaveChanges()
        {
            if (!UnsavedChanges)
            {
                return DialogResult.Yes;
            }

            DialogResult saveResult = MessageBox.Show(Resources.UnsavedChangesAlert_AskSave, Resources.SaveConfirmation_AlertTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (saveResult == DialogResult.Yes)
            {
                return ShowSaveBundle();
            }

            return saveResult;
        }

        /// <summary>
        /// Shows the interface for new Animation creation
        /// </summary>
        /// <param name="parentSheet">Optional AnimationSheet that will own the newly created Animation</param>
        public void ShowCreateAnimation([CanBeNull] AnimationSheet parentSheet = null)
        {
            var nav = new NewAnimationView(this, parentSheet);

            nav.ShowDialog(_mainForm);
        }

        /// <summary>
        /// Shows an interface to duplicate the given animation
        /// </summary>
        /// <param name="animation">The animation to duplicate</param>
        public void ShowDuplicateAnimation([NotNull] Animation animation)
        {
            var dup = CurrentBundle.DuplicateAnimation(animation, null);

            _mainForm.AddAnimation(dup, true);
            _mainForm.OpenViewForAnimation(dup);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Shows the interface for Animation import
        /// </summary>
        /// <param name="parentSheet">Optional AnimationSheet that will own the newly imported Animation</param>
        public void ShowImportAnimation([CanBeNull] AnimationSheet parentSheet = null)
        {
            var imp = new ImportAnimationView(this, parentSheet);

            imp.ShowDialog(_mainForm);
        }

        /// <summary>
        /// Shows the interface for a new Animation Sheet creation
        /// </summary>
        public void ShowCreateAnimationSheet()
        {
            var ed = new AnimationSheetView(this);

            if (ed.ShowDialog(_mainForm) == DialogResult.OK)
            {
                AddAnimationSheet(ed.GenerateAnimationSheet(), true);
            }
        }

        /// <summary>
        /// Shows an interface to duplicate the given AnimationSheet object
        /// </summary>
        /// <param name="sheet">The animation sheet to duplicate</param>
        public AnimationSheetView ShowDuplicateAnimationSheet([NotNull] AnimationSheet sheet)
        {
            var dup = CurrentBundle.DuplicateAnimationSheet(sheet);

            _mainForm.AddAnimationSheet(dup, true);
            var view = _mainForm.OpenViewForAnimationSheet(dup);

            // Add the cloned animations as well
            foreach (var anim in dup.Animations)
            {
                _mainForm.AddAnimation(anim);
            }

            MarkUnsavedChanges(true);

            return view;
        }

        /// <summary>
        /// Shows an interface to save an animation sheet's generated texture to disk
        /// </summary>
        /// <param name="sheet">The animation sheet to save to disk</param>
        public void ShowExportAnimationSheetImage([NotNull] AnimationSheet sheet)
        {
            if (sheet.AnimationCount == 0)
            {
                MessageBox.Show(Resources.ExportSheetImage_NoAnimationsInSheet, Resources.Information_AlertTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get a file name
            string saveName = ShowSaveImage(null, sheet.Name, _mainForm);

            if (saveName == "") return;

            var exportView = new SheetExportProgressView(sheet, saveName, GetExporter());

            exportView.ShowDialog(_mainForm);
        }

        /// <summary>
        /// Shows a dialog to save an image to disk, and returns the selected path.
        /// Returns string.Empty if the user has canceled
        /// </summary>
        /// <param name="imageFormat">The ImageFormat associated with the file format chosen by the user</param>
        /// <param name="imageToSave">The image to save to disk</param>
        /// <param name="fileName">An optional file name to display as default name when the dialog shows up</param>
        /// <param name="owner">An optional owner for the file dialog</param>
        /// <returns>The selected save path, or an empty string if the user has not chosen a save path</returns>
        public string ShowSaveImage(out ImageFormat imageFormat, Image imageToSave = null, string fileName = "", IWin32Window owner = null)
        {
            imageFormat = ImageFormat.Png;

            var sfd = new SaveFileDialog
            {
                Filter = @"PNG Image (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp|GIF Image (*.gif)|*.gif|JPEG Image (*.jpg)|*.jpg|TIFF Image (*.tiff)|*.tiff",
                FileName = fileName
            };

            if (sfd.ShowDialog(owner) != DialogResult.OK)
                return string.Empty;

            string savePath = sfd.FileName;

            imageFormat = ImageFormatForExtension(Path.GetExtension(fileName), imageFormat);

            imageToSave?.Save(savePath, imageFormat);

            return savePath;
        }

        /// <summary>
        /// Shows a dialog to save an image to disk, and returns the selected path.
        /// Returns string.Empty if the user has canceled
        /// </summary>
        /// <param name="imageToSave">The image to save to disk</param>
        /// <param name="fileName">An optional file name to display as default name when the dialog shows up</param>
        /// <param name="owner">An optional owner for the file dialog</param>
        /// <returns>The selected save path, or an empty string if the user has not chosen a save path</returns>
        public string ShowSaveImage(Image imageToSave = null, string fileName = "", IWin32Window owner = null)
        {
            return ShowSaveImage(out ImageFormat _, imageToSave, fileName, owner);
        }

        /// <summary>
        /// Returns the ImageFormat associated with a given file extension
        /// </summary>
        /// <param name="extension">The extension of the file format, with or without the precending '.'</param>
        /// <param name="defaultFormat">The default format, if the extension is not valid</param>
        /// <returns>The ImageFormat associated with a given file extension</returns>
        public ImageFormat ImageFormatForExtension(string extension, ImageFormat defaultFormat)
        {
            if (extension.StartsWith("."))
                extension = extension.Substring(1);
            extension = extension.ToLower();

            switch (extension.ToLower())
            {
                case @"bmp":
                    return ImageFormat.Bmp;

                case @"gif":
                    return ImageFormat.Gif;

                case @"ico":
                    return ImageFormat.Icon;

                case @"jpg":
                case @"jpeg":
                    return ImageFormat.Jpeg;

                case @"png":
                    return ImageFormat.Png;

                case @"tif":
                case @"tiff":
                    return ImageFormat.Tiff;

                case @"wmf":
                    return ImageFormat.Wmf;

                default:
                    return defaultFormat;
            }
        }

        /// <summary>
        /// Shows a dialog to load an image from disk, and returns the loaded image file.
        /// Returns null if the user has canceled
        /// </summary>
        /// <param name="fileName">An optional file name to display as default name when the dialog shows up</param>
        /// <param name="owner">An optional owner for the file dialog</param>
        /// <returns>The selected image, or null if the user has not chosen an image</returns>
        public Image ShowLoadImage(string fileName = "", IWin32Window owner = null)
        {
            return ShowLoadImage(out string _, fileName, owner);
        }

        /// <summary>
        /// Shows a dialog to load an image from disk, and returns the loaded image file.
        /// Returns null if the user has canceled.
        /// The image loaded is automatically converted into a 32bpp transparent bitmap image format
        /// </summary>
        /// <param name="filePath">The file path that was chosen for the file. Returned as an empty string when no file was chosen</param>
        /// <param name="fileName">An optional file name to display as default name when the dialog shows up</param>
        /// <param name="owner">An optional owner for the file dialog</param>
        /// <returns>The selected image, or null if the user has not chosen an image</returns>
        public Image ShowLoadImage(out string filePath, string fileName = "", IWin32Window owner = null)
        {
            filePath = string.Empty;

            var ofd = new OpenFileDialog
            {
                Filter = @"PNG Image (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp|GIF Image (*.gif)|*.gif|JPEG Image (*.jpg)|*.jpg|TIFF Image (*.tiff)|*.tiff|All image formats (*.png, *.jpg, *.gif, *.tiff, *.bmp)|*.png;*.jpg;*.gif;*.tiff;*.bmp",
                FileName = fileName
            };

            if (ofd.ShowDialog(owner) == DialogResult.OK)
            {
                filePath = ofd.FileName;

                try
                {
                    using (var img = Image.FromFile(ofd.FileName))
                    {
                        return PreparedImage(img);
                    }
                }
                catch (Exception)
                {
                    filePath = "";
                    MessageBox.Show(@"Error loading selected image. It may not be in a valid image file format.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return null;
        }

        /// <summary>
        /// Shows a dialog to load multiple images from disk, and returns the loaded image files.
        /// Returns null if the user has canceled
        /// </summary>
        /// <param name="owner">An optional owner for the file dialog</param>
        /// <returns>The images the user opened, or null, if no images were chosen</returns>
        [CanBeNull]
        public Image[] ShowLoadImages([CanBeNull] IWin32Window owner = null)
        {
            return ShowLoadImages(out string[] _, owner);
        }

        /// <summary>
        /// Shows a dialog to load multiple images from disk, and returns the loaded image files.
        /// Returns null if the user has canceled.
        /// The images loaded are automatically converted into 32bpp transparent bitmap image format
        /// </summary>
        /// <param name="filePaths">The file paths that were chosen for the files. Returned as an empty array when no files were chosen</param>
        /// <param name="owner">An optional owner for the file dialog</param>
        /// <returns>The images the user opened, or null, if no images were chosen</returns>
        [CanBeNull]
        public Image[] ShowLoadImages([NotNull] out string[] filePaths, [CanBeNull] IWin32Window owner = null)
        {
            filePaths = new string[0];

            var ofd = new OpenFileDialog
            {
                Filter = @"PNG Image (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp|GIF Image (*.gif)|*.gif|JPEG Image (*.jpg)|*.jpg|TIFF Image (*.tiff)|*.tiff|All image formats (*.png, *.jpg, *.gif, *.tiff, *.bmp)|*.png;*.jpg;*.gif;*.tiff;*.bmp",
                Multiselect = true
            };

            if (ofd.ShowDialog(owner) != DialogResult.OK)
                return null;
            
            filePaths = ofd.FileNames;

            try
            {
                var sources = filePaths.Select(Image.FromFile).ToArray();
                var baked = sources.Select(PreparedImage).ToArray();
                    
                // Dispose of the images
                Array.ForEach(sources.ToArray(), image => image.Dispose());

                return baked;
            }
            catch (Exception)
            {
                filePaths = new string[0];
                MessageBox.Show(@"Error loading selected images. They may not all be valid image file formats.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        /// <summary>
        /// Returns an Image object that contains the contents of a given image baked into a 32bpp bitmap image
        /// </summary>
        /// <param name="image">The image to prepare</param>
        /// <returns>The image that was prepared from the given image</returns>
        private static Image PreparedImage([NotNull] Image image)
        {
            var bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(image, 0, 0, image.Width, image.Height);
                g.Flush();
            }

            return bitmap;
        }

        ////////////////////////////////////////////////////////////////////////////////
        //////////
        ////////// Misc Methods
        //////////
        /////
        ///// Miscelaneous methods not strictly related to bundles or interface
        /////
        ////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or generates a new exporter that is fit to be used during new fresh export operations
        /// </summary>
        public IBundleExporter GetExporter()
        {
            return new DefaultPngExporter();
        }

        /// <summary>
        /// Gets a dynamic animation provider for a given combination of animation sheet and export settings
        /// The provider is able to pull unsaved changes from animations within that sheet from the interface and when queried, the array of animations
        /// will then return always the most up-to-date data from the views.
        /// </summary>
        /// <param name="sheet">The animation sheet to wrap on the dynamic provider</param>
        /// <param name="settings">An overrided set of export settings to use</param>
        /// <returns>An <see cref="IAnimationProvider"/> instance that provides still unsaved changes from animation views when the property <see cref="IAnimationProvider.GetAnimations"/> is called.</returns>
        public IAnimationProvider GetDynamicProviderForSheet(AnimationSheet sheet, AnimationSheetExportSettings settings)
        {
            return new DynamicAnimationProvider(this, sheet, settings);
        }

        /// <summary>
        /// Generates an export image for the given AnimationSheet
        /// </summary>
        /// <param name="sheet">The animation sheet to generate the export of</param>
        /// <returns>An Image that represents the exported image for the animation sheet</returns>
        public Task<BundleSheetExport> GenerateExportForAnimationSheet(AnimationSheet sheet)
        {
            return GetExporter().ExportBundleSheet(sheet);
        }

        /// <summary>
        /// Generates a BundleSheetExport object that contains information about the export of a sheet, using a custom event handler
        /// for export progress callback
        /// </summary>
        /// <param name="sheetExportSettings">The export settings for the sheet</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the process mid-way</param>
        /// <param name="callback">The callback delegate to be used during the generation process</param>
        /// <param name="anims">The list of animations to export</param>
        /// <returns>A BundleSheetExport object that contains information about the export of the sheet</returns>
        public Task<BundleSheetExport> GenerateBundleSheet(AnimationSheetExportSettings sheetExportSettings, CancellationToken cancellationToken, BundleExportProgressEventHandler callback, params IAnimation[] anims)
        {
            return GetExporter().ExportBundleSheet(new BasicAnimationProvider(anims, sheetExportSettings, ""), cancellationToken, callback);
        }

        /// <summary>
        /// Generates a BundleSheetExport object that contains information about the export of a sheet, using a custom event handler
        /// for export progress callback
        /// </summary>
        /// <param name="provider">The provider for the animations to be generated</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the process mid-way</param>
        /// <param name="callback">The callback delegate to be used during the generation process</param>
        /// <returns>A BundleSheetExport object that contains information about the export of the sheet</returns>
        public Task<BundleSheetExport> GenerateBundleSheet(IAnimationProvider provider, CancellationToken cancellationToken, BundleExportProgressEventHandler callback)
        {
            return GetExporter().ExportBundleSheet(provider, cancellationToken, callback);
        }

        /// <summary>
        /// Shows an interface for saving a sprite strip version of the specified animation
        /// </summary>
        /// <param name="animation">The animation to save a sprite strip out of</param>
        public void ShowSaveAnimationStrip([NotNull] AnimationController animation)
        {
            using (var stripImage = GetExporter().GenerateSpriteStrip(animation))
            {
                ShowSaveImage(stripImage, animation.GetAnimationView().Name);
            }
        }

        /// <summary>
        /// Dynamic animation provider used to access unsaved animation states from the interface
        /// </summary>
        private class DynamicAnimationProvider : IAnimationProvider
        {
            private readonly Controller _controller;
            private readonly AnimationSheet _sheet;

            public IAnimation[] GetAnimations()
            {
                var anims =
                    new List<AnimationController>(
                        _sheet
                            .Animations
                            .Select(anim =>
                                new AnimationController(_controller.CurrentBundle, anim)
                            )
                    );

                // Get all currently opened animation sheet views from the main form
                foreach (var animation in _sheet.Animations)
                {
                    var view = _controller._mainForm.GetOpenedViewForAnimation(animation);
                    if (view == null)
                        continue;

                    var index = anims.FindIndex(cont => cont.MatchesController(view.ViewAnimation));
                    if (index != -1)
                    {
                        anims[index] = view.ViewAnimation;
                    }
                }

                return anims.Select(cont => cont.GetAnimationView()).ToArray();
            }

            public AnimationSheetExportSettings SheetExportSettings { get; }
            public string Name => _sheet.Name;

            public DynamicAnimationProvider(Controller controller, AnimationSheet sheet, AnimationSheetExportSettings sheetExportSettings)
            {
                _controller = controller;
                _sheet = sheet;
                SheetExportSettings = sheetExportSettings;
            }
        }

        /// <summary>
        /// Reactive binder for the controller
        /// </summary>
        public interface IReactive
        {
            /// <summary>
            /// Updates whenever any of the public properties of an animation instance change.
            /// 
            /// Only changes that where persisted (i.e. they are not unsaved changes to a form) are performed.
            /// </summary>
            IObservable<Animation> AnimationUpdate { get; }

            /// <summary>
            /// Updates whenever any of the public properties of an animation sheet instance change
            /// 
            /// Only changes that where persisted (i.e. they are not unsaved changes to a form) are performed.
            /// </summary>
            IObservable<AnimationSheet> AnimationSheetUpdate { get; }
        }

        /// <summary>
        /// Reactive binder for the controller
        /// </summary>
        private sealed class Reactive : IReactive, IDisposable
        {
            public readonly Subject<Animation> RxOnAnimationUpdate = new Subject<Animation>();
            public readonly Subject<AnimationSheet> RxOnAnimationSheetUpdate = new Subject<AnimationSheet>();

            public IObservable<Animation> AnimationUpdate => RxOnAnimationUpdate;
            public IObservable<AnimationSheet> AnimationSheetUpdate => RxOnAnimationSheetUpdate;

            public void Dispose()
            {
                RxOnAnimationUpdate?.Dispose();
                RxOnAnimationSheetUpdate?.Dispose();
            }
        }
    }

    /// <summary>
    /// Partial class for methods related to interface displaying
    /// </summary>
    public partial class Controller
    {
        /// <summary>
        /// Opens the interface for the given animation
        /// </summary>
        public AnimationView OpenAnimationView(Animation animation, int selectedFrameIndex = -1)
        {
            return _mainForm.OpenViewForAnimation(animation, selectedFrameIndex);
        }
    }
    
    /// <summary>
    /// Interface state provider implementation
    /// </summary>
    public partial class Controller : IInterfaceStateProvider
    {
        public bool HasUnsavedChangesForAnimation(Animation animation)
        {
            return _mainForm.GetOpenedViewForAnimation(animation)?.Modified ?? false;
        }

        public bool HasUnsavedChangesForAnimationSheet(AnimationSheet sheet)
        {
            return _mainForm.GetOpenedViewForAnimationSheet(sheet)?.Modified ?? false;
        }
    }

    /// <summary>
    /// Event arguments for an animation-related event
    /// </summary>
    public class AnimationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the animation binded to this event
        /// </summary>
        public Animation Animation { get; }

        /// <summary>
        /// Initializes a new instance of the AnimationEventArgs class with an animation to attach to this event argument
        /// </summary>
        public AnimationEventArgs(Animation animation)
        {
            Animation = animation;
        }
    }

    /// <summary>
    /// Event arguments for an animation sheet-related event
    /// </summary>
    public class AnimationSheetEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the animation sheet binded to this event
        /// </summary>
        public AnimationSheet AnimationSheet { get; }

        /// <summary>
        /// Initializes a new instance of the AnimationSheetEventArgs class with an animation sheet to attach to this event argument
        /// </summary>
        public AnimationSheetEventArgs(AnimationSheet animationSheet)
        {
            AnimationSheet = animationSheet;
        }
    }

    /// <summary>
    /// Interface for objects that can provide information about 
    /// </summary>
    public interface IInterfaceStateProvider
    {
        /// <summary>
        /// Returns whether, to the knowledge of this interface state provider, the given animation
        /// is currently opened in a view with pending changes to save
        /// </summary>
        /// <returns>A value specifying whether the animation has unsaved changes in any view this interface state provider is able to reach</returns>
        bool HasUnsavedChangesForAnimation(Animation animation);

        /// <summary>
        /// Returns whether, to the knowledge of this interface state provider, the given animation sheet
        /// is currently opened in a view with pending changes to save
        /// </summary>
        /// <returns>A value specifying whether the animation sheet has unsaved changes in any view this interface state provider is able to reach</returns>
        bool HasUnsavedChangesForAnimationSheet(AnimationSheet sheet);
    }
}