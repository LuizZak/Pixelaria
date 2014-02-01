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

using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Pixelaria.Data;
using Pixelaria.Data.Exports;
using Pixelaria.Data.Persistence;
using Pixelaria.Data.Validators;

using Pixelaria.Importers;
using Pixelaria.Exporters;

using Pixelaria.Views;
using Pixelaria.Views.MiscViews;
using Pixelaria.Views.ModelViews;

using Pixelaria.Utils;

namespace Pixelaria.Controllers
{
    /// <summary>
    /// Main application controller
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// The currently opened bundle
        /// </summary>
        Bundle currentBundle;

        /// <summary>
        /// The main application form
        /// </summary>
        MainForm mainForm;

        /// <summary>
        /// The default animation importer
        /// </summary>
        IDefaultImporter defaultImporter;

        /// <summary>
        /// The default animation exporter
        /// </summary>
        IDefaultExporter defaultExporter;

        /// <summary>
        /// The Animation fields validator
        /// </summary>
        IAnimationValidator animationValidator;

        /// <summary>
        /// The AnimationSheet fields validator
        /// </summary>
        IAnimationSheetValidator animationSheetValidator;

        /// <summary>
        /// Whether the current bundle has unsaved changes
        /// </summary>
        bool unsavedChanges;

        /// <summary>
        /// Stores a list of the recently opened bundle files
        /// </summary>
        RecentFileList recentFileList;

        /// <summary>
        /// Gets the current bundle opened on the application
        /// </summary>
        public Bundle CurrentBundle { get { return currentBundle; } }

        /// <summary>
        /// Gets the current IDefaultImporter of the program
        /// </summary>
        public IDefaultImporter DefaultImporter { get { return defaultImporter; } }

        /// <summary>
        /// Gets the current IDefaultExporter of the program
        /// </summary>
        public IDefaultExporter DefaultExporter { get { return defaultExporter; } }

        /// <summary>
        /// Gets the current IAnimationValidator of the program
        /// </summary>
        public IAnimationValidator AnimationValidator { get { return animationValidator; } }

        /// <summary>
        /// Gets the current IAnimationSheetValidator of the program
        /// </summary>
        public IAnimationSheetValidator AnimationSheetValidator { get { return animationSheetValidator; } }

        /// <summary>
        /// Gets whether the current bundle has unsaved changes
        /// </summary>
        public bool UnsavedChanges { get { return unsavedChanges; } }

        /// <summary>
        /// Gets the current RecentFileList for the program
        /// </summary>
        public RecentFileList CurrentRecentFileList { get { return recentFileList; } }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="mainForm">The form to use as the main form of the application</param>
        public Controller(MainForm mainForm)
        {
            this.mainForm = mainForm;
            mainForm.controller = this;

            ShowNewBundle();

            DefaultValidator defValidator = new DefaultValidator(this);

            animationValidator = defValidator;
            animationSheetValidator = defValidator;

            defaultImporter = new DefaultPngImporter();
            defaultExporter = new DefaultPngExporter();

            // Initialize the Settings singleton
            Settings.GetSettings(Path.GetDirectoryName(Application.ExecutablePath) + "\\settings.ini");

            recentFileList = new RecentFileList(10);

            mainForm.UpdateRecentFilesList();
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
        public void SaveBundle(string savePath)
        {
            currentBundle.SaveFile = savePath;
            PixelariaSaverLoader.SaveBundleToDisk(currentBundle, savePath);

            MarkUnsavedChanges(false);
        }

        /// <summary>
        /// Opens a loaded bundle from the given path on disk
        /// </summary>
        /// <param name="savePath">The path to load the bundle from</param>
        public void LoadBundleFromFile(string savePath)
        {
            // Dispose of the current bundle if it's present
            if (currentBundle != null)
                currentBundle.Dispose();

            Bundle newBundle = PixelariaSaverLoader.LoadBundleFromDisk(savePath);
            newBundle.SaveFile = savePath;

            LoadBundle(newBundle);

            // Store the file now
            recentFileList.StoreFile(savePath);
            mainForm.UpdateRecentFilesList();
        }

        /// <summary>
        /// Loads the given bundle into the interface.
        /// This method disposes of the current bundle
        /// </summary>
        /// <param name="newBundle">The new bundle to load</param>
        public void LoadBundle(Bundle newBundle)
        {
            currentBundle = newBundle;

            mainForm.LoadBundle(currentBundle);

            // Update the Unsaved Changes flag to false
            MarkUnsavedChanges(false);
        }

        /// <summary>
        /// Loads a bundle from the list of recent files list
        /// </summary>
        /// <param name="index">The index to get the file path from</param>
        public void LoadBundleFromRecentFileList(int index)
        {
            if (!File.Exists(recentFileList[index]))
            {
                if (MessageBox.Show("The file does not exists. Remove from the file list?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    recentFileList.RemoveFromList(index);
                    mainForm.UpdateRecentFilesList();
                }

                return;
            }

            // Cancel on changes saving confirmation quits the method
            if (ShowConfirmSaveChanges() == DialogResult.Cancel)
                return;

            LoadBundleFromFile(recentFileList[index]);
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
            if (currentBundle == null || isUnsaved == unsavedChanges)
                return;

            unsavedChanges = isUnsaved;

            mainForm.UnsavedChangesUpdated(isUnsaved);
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
        public Animation CreateAnimation(string name, int width, int height, int fps, bool frameskip, bool openOnForm, AnimationSheet parentSheet = null)
        {
            Animation anim = new Animation(name, width, height);

            anim.PlaybackSettings.FPS = fps;
            anim.PlaybackSettings.FrameSkip = frameskip;

            // Create a dummy frame
            anim.CreateFrame();

            AddAnimation(anim, openOnForm, parentSheet);

            return anim;
        }

        /// <summary>
        /// Adds the given Animation into the current bundle
        /// </summary>
        /// <param name="anim">The animation to add to the bundle</param>
        /// <param name="openOnForm">Whether to open the newly added animation on the main form</param>
        /// <param name="parentSheet">Optional AnimationSheet that will own the newly created animation</param>
        public void AddAnimation(Animation anim, bool openOnForm, AnimationSheet parentSheet = null)
        {
            currentBundle.AddAnimation(anim, parentSheet);

            if (openOnForm)
            {
                mainForm.AddAnimation(anim, true);
                mainForm.OpenViewForAnimation(anim);
            }
            else
            {
                mainForm.AddAnimation(anim);
            }

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Removes the given Animation from the current bundle
        /// </summary>
        /// <param name="anim">The Animation to remove from the bundle</param>
        public void RemoveAnimation(Animation anim)
        {
            currentBundle.RemoveAnimation(anim);

            mainForm.RemoveAnimation(anim);

            MarkUnsavedChanges(true);

            // Dispose of the animation
            anim.Dispose();
        }

        /// <summary>
        /// Method to be called whenever changes have been made to the fields of an Animation
        /// </summary>
        /// <param name="anim">The Animation that was modified</param>
        public void UpdatedAnimation(Animation anim)
        {
            mainForm.UpdateAnimation(anim);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Gets the index of the given Animation object inside its current parent container
        /// </summary>
        /// <param name="anim">The animation to get the index of</param>
        /// <returns>The index of the animation in its current parent container</returns>
        public int GetAnimationIndex(Animation anim)
        {
            return currentBundle.GetAnimationIndex(anim);
        }

        /// <summary>
        /// Rearranges the index of an Animation in the animation's current storing container
        /// </summary>
        /// <param name="anim">The animation to rearrange</param>
        /// <param name="newIndex">The new index to place the animation at</param>
        public void RearrangeAnimationsPosition(Animation anim, int newIndex)
        {
            currentBundle.RearrangeAnimationsPosition(anim, newIndex);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Creates and returns a new Animation Sheet
        /// </summary>
        /// <param name="name">The name for the animation sheet</param>
        /// <param name="openOnForm">Whether to open the newly added animation sheet on the main form</param>
        public AnimationSheet CreateAnimationSheet(string name, bool openOnForm)
        {
            AnimationSheet sheet = new AnimationSheet(name);

            AddAnimationSheet(sheet, openOnForm);

            return sheet;
        }

        /// <summary>
        /// Adds the given Animation Sheet into the current bundle
        /// </summary>
        /// <param name="sheet">The sheet to load into the current bundle</param>
        /// <param name="openOnForm">Whether to open the newly added animation sheet on the main form</param>
        public void AddAnimationSheet(AnimationSheet sheet, bool openOnForm)
        {
            currentBundle.AddAnimationSheet(sheet);

            if (openOnForm)
            {
                mainForm.AddAnimationSheet(sheet, true);
                mainForm.OpenViewForAnimationSheet(sheet);
            }
            else
            {
                mainForm.AddAnimationSheet(sheet, false);
            }

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Removes the given AnimationSeet from the current bundle
        /// </summary>
        /// <param name="sheet">The sheet to remove from the bundle</param>
        /// <param name="deleteAnimations">Whether to delete the nested animations as well. If set to false, the animations will be moved to the bundle's root</param>
        public void RemoveAnimationSheet(AnimationSheet sheet, bool deleteAnimations)
        {
            // Remove/relocate animations
            if (deleteAnimations)
            {
                foreach (Animation anim in sheet.Animations)
                {
                    RemoveAnimation(anim);
                }
            }

            // Remove the sheet
            currentBundle.RemoveAnimationSheet(sheet, false);

            mainForm.RemoveAnimationSheet(sheet);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Method to be called whenever changes have been made to the fields of an AnimationSheet
        /// </summary>
        /// <param name="sheet">The AnimationSheet that was modified</param>
        public void UpdatedAnimationSheet(AnimationSheet sheet)
        {
            mainForm.UpdateAnimationSheet(sheet);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Gets the index of the given AnimationSheet object inside its current parent container
        /// </summary>
        /// <param name="sheet">The sheet to get the index of</param>
        /// <returns>The index of the sheet in its current parent container</returns>
        public int GetAnimationSheetIndex(AnimationSheet sheet)
        {
            return currentBundle.GetAnimationSheetIndex(sheet);
        }

        /// <summary>
        /// Rearranges the index of an AnimationSheets in the sheets's current storing container
        /// </summary>
        /// <param name="sheet">The sheet to rearrange</param>
        /// <param name="newIndex">The new index to place the sheet at</param>
        public void RearrangeAnimationSheetsPosition(AnimationSheet sheet, int newIndex)
        {
            currentBundle.RearrangeAnimationSheetsPosition(sheet, newIndex);

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
            currentBundle.AddAnimationToAnimationSheet(anim, sheet);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Gets the AnimationSheet that currently owns the given Animation object.
        /// If the Animation is not inside any AnimationSheet, null is returned
        /// </summary>
        /// <param name="anim">The animation object to get the animation sheet of</param>
        /// <returns>The AnimationSheet that currently owns the given Animation object. If the Animation is not inside any AnimationSheet, null is returned</returns>
        public AnimationSheet GetOwningAnimationSheet(Animation anim)
        {
            return currentBundle.GetOwningAnimationSheet(anim);
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

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "Pixelaria Bundle (*.pxl)|*.pxl";

            //ofd.FileName = @"C:\Users\Luiz Fernando\Desktop\asasdads.plx";
            if (ofd.ShowDialog(mainForm) == DialogResult.OK)
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
            if (currentBundle.AnimationSheets.Length == 0)
            {
                MessageBox.Show("There are no animation sheets to export! Create a sheet with at least one animation in it before exporting.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                bool validSheet = false;
                foreach (AnimationSheet sheet in currentBundle.AnimationSheets)
                {
                    if (sheet.Animations.Length != 0)
                    {
                        validSheet = true;
                        break;
                    }
                }

                if (!validSheet)
                {
                    MessageBox.Show("There are no animations in any of the animation sheets! Create a sheet with at least one animation in it before exporting.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            // The bundle path must be valid
            if (currentBundle.ExportPath.Trim() == "" || !Directory.Exists(currentBundle.ExportPath))
            {
                if (MessageBox.Show("The bundle's export path is not set or is not valid.\nDo you wish to edit the export path now?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    mainForm.OpenBundleSettings(currentBundle);

                    if (currentBundle.ExportPath.Trim() == "" || !Directory.Exists(currentBundle.ExportPath))
                        return;
                }
                else
                {
                    return;
                }
            }

            BundleExportProgressView progressForm = new BundleExportProgressView(currentBundle, defaultExporter);

            progressForm.ShowDialog(this.mainForm);
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
            string savePath = currentBundle.SaveFile;

            if (savePath == "" || forceNew)
            {
                SaveFileDialog svd = new SaveFileDialog();

                svd.Filter = "Pixelaria Bundle (*.pxl)|*.pxl";

                if (svd.ShowDialog(mainForm) == DialogResult.OK)
                {
                    savePath = svd.FileName;
                }
                else
                {
                    return DialogResult.Cancel;
                }

                // Store the file now
                recentFileList.StoreFile(savePath);
                mainForm.UpdateRecentFilesList();
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
            if (!unsavedChanges)
            {
                return DialogResult.Yes;
            }

            DialogResult saveResult = MessageBox.Show("There are unsaved changes to the currently opened bundle. Save the current bundle first?", "Save Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (saveResult == DialogResult.Yes)
            {
                return ShowSaveBundle();
            }

            return saveResult;
        }

        /// <summary>
        /// Shows the interface for new Animation creation
        /// </summary>
        /// <param name="ownerSheet">Optional AnimationSheet that will own the newly created Animation</param>
        public void ShowCreateAnimation(AnimationSheet parentSheet = null)
        {
            NewAnimationView nav = new NewAnimationView(this, parentSheet);

            nav.ShowDialog(mainForm);
        }

        /// <summary>
        /// Shows an interface to duplicate the given animation
        /// </summary>
        /// <param name="animation">The animation to duplicate</param>
        public void ShowDuplicateAnimation(Animation animation)
        {
            Animation dup = currentBundle.DuplicateAnimation(animation, null);

            mainForm.AddAnimation(dup, true);
            mainForm.OpenViewForAnimation(dup);

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Shows the interface for Animation import
        /// </summary>
        /// <param name="ownerSheet">Optional AnimationSheet that will own the newly imported Animation</param>
        public void ShowImportAnimation(AnimationSheet parentSheet = null)
        {
            ImportAnimationView imp = new ImportAnimationView(this, parentSheet);

            imp.ShowDialog(mainForm);
        }

        /// <summary>
        /// Shows the interface for a new Animation Sheet creation
        /// </summary>
        public void ShowCreateAnimationSheet()
        {
            AnimationSheetView ed = new AnimationSheetView(this);

            if (ed.ShowDialog(mainForm) == DialogResult.OK)
            {
                AddAnimationSheet(ed.GenerateAnimationSheet(), true);
            }
        }

        /// <summary>
        /// Shows an interface to duplicate the given AnimationSheet object
        /// </summary>
        /// <param name="sheet">The animation sheet to duplicate</param>
        public void ShowDuplicateAnimationSheet(AnimationSheet sheet)
        {
            AnimationSheet dup = currentBundle.DuplicateAnimationSheet(sheet);

            mainForm.AddAnimationSheet(dup, true);
            mainForm.OpenViewForAnimationSheet(dup);

            // Add the cloned animations as well
            foreach (Animation anim in dup.Animations)
            {
                mainForm.AddAnimation(anim);
            }

            MarkUnsavedChanges(true);
        }

        /// <summary>
        /// Shows an interface to save an animation sheet's generated texture to disk
        /// </summary>
        /// <param name="sheet">The animation sheet to save to disk</param>
        public void ShowExportAnimationSheetImage(AnimationSheet sheet)
        {
            if (sheet.AnimationCount == 0)
            {
                MessageBox.Show("There are no animations on the sheet! Add at least one animation to the sheet before exporting an image.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get a file name
            string saveName = ShowSaveImage(null, sheet.Name, mainForm);

            if (saveName != "")
            {
                SheetExportProgressView exportView = new SheetExportProgressView(sheet, saveName, defaultExporter);

                exportView.ShowDialog(mainForm);
            }
        }

        /// <summary>
        /// Shows a dialog to save an image to disk
        /// </summary>
        /// <param name="imageToSave">The image to save to disk</param>
        /// <param name="fileName">An optional file name to display as default name when the dialog shows up</param>
        /// <param name="owner">An optional owner for the file dialog</param>
        /// <returns>The selected save path, or an empty string if the user has not chosen a save path</returns>
        public string ShowSaveImage(Image imageToSave = null, string fileName = "", IWin32Window owner = null)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "PNG Image (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp|GIF Image (*.gif)|*.gif|JPEG Image (*.jpg)|*.jpg|TIFF Image (*.tiff)|*.tiff";
            sfd.FileName = fileName;

            if (sfd.ShowDialog(owner) == DialogResult.OK)
            {
                if (imageToSave != null)
                {
                    string savePath = sfd.FileName;

                    imageToSave.Save(savePath);
                }

                return sfd.FileName;
            }

            return string.Empty;
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
        /// Generates an export image for the given AnimationSheet
        /// </summary>
        /// <param name="sheet">The animation sheet to generate the export of</param>
        /// <returns>An Image that represents the exported image for the animation sheet</returns>
        public Image GenerateExportForAnimationSheet(AnimationSheet sheet)
        {
            return defaultExporter.ExportAnimationSheet(sheet);
        }

        /// <summary>
        /// Exports the given animations into an image sheet and returns the created sheet
        /// </summary>
        /// <param name="exportSettings">The export settings for the sheet</param>
        /// <param name="anims">The list of animations to export</param>
        /// <returns>An image sheet representing the animations passed</returns>
        public Image GenerateExport(AnimationExportSettings exportSettings, params Animation[] anims)
        {
            return defaultExporter.ExportAnimationSheet(exportSettings, anims);
        }

        /// <summary>
        /// Generates a BundleSheetExport object that contains information about the export of a sheet
        /// </summary>
        /// <param name="exportSettings">The export settings for the sheet</param>
        /// <param name="anims">The list of animations to export</param>
        /// <returns>A BundleSheetExport object that contains information about the export of the sheet</returns>
        public BundleSheetExport GenerateBundleSheet(AnimationExportSettings exportSettings, params Animation[] anims)
        {
            BundleSheetExport bse = defaultExporter.ExportBundleSheet(exportSettings, anims);

            return bse;
        }

        /// <summary>
        /// Generates a BundleSheetExport object that contains information about the export of a sheet, using a custom event handler
        /// for export progress callback
        /// </summary>
        /// <param name="exportSettings">The export settings for the sheet</param>
        /// <param name="anims">The list of animations to export</param>
        /// <returns>A BundleSheetExport object that contains information about the export of the sheet</returns>
        public BundleSheetExport GenerateBundleSheet(AnimationExportSettings exportSettings, BundleExportProgressEventHandler callback, params Animation[] anims)
        {
            BundleSheetExport bse = defaultExporter.ExportBundleSheet(exportSettings, anims, callback);

            return bse;
        }
    }
}