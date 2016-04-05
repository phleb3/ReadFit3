using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Reflection;
using ReadFit;

namespace QA.FileDialogs
{
    // http://www.qa.com/about-qa/blogs/2011/july/creating-an-mvvm-friendly-openfiledialog-in-wpf/
    /// <summary>
    /// A wrapper around a <see cref="Microsoft.Win32.FileDialog"/> FileDialog class
    /// </summary>
    [DesignTimeVisible(false)]
    public abstract class DependencyFileDialog : Control
    {
        /// <summary>
        /// Overridden in derviced classes to provide access to the appropriate FileDialog subclass
        /// </summary>
        protected abstract FileDialog Dialog
        {
            get;
        }

        /// <summary>
        /// Constructor for the DependencyFileDialog. Hooks up the FileOk event to the <seealso cref="FileOkCommand"/> FileOkAction
        /// </summary>
        public DependencyFileDialog()
        {
            Visibility = System.Windows.Visibility.Collapsed;
            Dialog.FileOk += Dialog_FileOk;
        }

        /// <summary>
        /// Handle the FileDialog's "Save" or "Open" click event by firing the FileOkAction command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ExecuteFileOkCommand();
        }

        /// <summary>
        /// Executes the FileOkCommand using the current FileName property. Marked as virtual so it can be overridden in
        /// DependencyOpenFileDialog. Not happy with this situation.
        /// </summary>
        protected virtual void ExecuteFileOkCommand()
        {
            if (FileOkCommand != null)
            {
                FileOkCommand.Execute(Dialog.FileName);
            }
        }

        /// <summary>
        /// Gets or sets the filter string that determines what types of files are displayed from either 
        /// the DependencyOpenFileDialog or DependencySaveFileDialog.
        /// </summary>
        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set
            {
                SetValue(FilterProperty, value);
            }
        }

        /// <summary>
        /// The Dependency Property for the Filter property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(DependencyFileDialog),
                new UIPropertyMetadata("", DialogPropertyChangedCallback));

        /// <summary>
        /// Gets or sets a string containing the full path of the file selected in a file dialog.
        /// </summary>
        public string FileName
        {
            get
            {
                return (string)GetValue(FileNameProperty);
            }
            set { SetValue(FileNameProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the FileName property
        /// </summary>
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(DependencyFileDialog),
            new UIPropertyMetadata(string.Empty, DialogPropertyChangedCallback));

        /// <summary>
        /// Gets or sets the text that appears in the title bar of a file dialog.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the Title property
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DependencyFileDialog),
            new UIPropertyMetadata("Select a file", DialogPropertyChangedCallback));

        /// <summary>
        /// Gets or sets a value indicating whether a file dialog automatically adds an extension to a file name if the user omits an extension.
        /// </summary>
        public bool AddExtension
        {
            get { return (bool)GetValue(AddExtensionProperty); }
            set { SetValue(AddExtensionProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the AddExtension property
        /// </summary>
        public static readonly DependencyProperty AddExtensionProperty =
            DependencyProperty.Register("AddExtension", typeof(bool), typeof(DependencyFileDialog), 
            new UIPropertyMetadata(true, DialogPropertyChangedCallback));        

        /// <summary>
        /// Gets or sets a value indicating whether a file dialog displays a warning if the user specifies a file name that does not exist.
        /// </summary>
        public bool CheckFileExists
        {
            get { return (bool)GetValue(CheckFileExistsProperty); }
            set { SetValue(CheckFileExistsProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the CheckFileExists property
        /// </summary>
        public static readonly DependencyProperty CheckFileExistsProperty = 
            DependencyProperty.Register("CheckFileExists", typeof(bool), typeof(DependencyFileDialog), 
            new UIPropertyMetadata(false, DialogPropertyChangedCallback));

        /// <summary>
        /// Gets or sets a value that specifies whether warnings are displayed if the user types invalid paths and file names.
        /// </summary>
        public bool CheckPathExists
        {
            get { return (bool)GetValue(CheckPathExistsProperty); }
            set { SetValue(CheckPathExistsProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the CheckPathExists property
        /// </summary>
        public static readonly DependencyProperty CheckPathExistsProperty =
            DependencyProperty.Register("CheckPathExists", typeof(bool), typeof(DependencyFileDialog), 
            new UIPropertyMetadata(true, DialogPropertyChangedCallback));

        /// <summary>
        /// Gets or sets the list of custom places for file dialog boxes. 
        /// </summary>
        public IList<FileDialogCustomPlace> CustomPlaces
        {
            get { return (IList<FileDialogCustomPlace>)GetValue(CustomPlacesProperty); }
            set { SetValue(CustomPlacesProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the CustomPlaces property
        /// </summary>
        public static readonly DependencyProperty CustomPlacesProperty =
            DependencyProperty.Register("CustomPlaces", typeof(IList<FileDialogCustomPlace>), typeof(DependencyFileDialog), 
            new UIPropertyMetadata(null, DialogPropertyChangedCallback));

        /// <summary>
        /// Gets or sets a value that specifies the default extension string to use to filter the list of files that are displayed.
        /// </summary>
        public string DefaultExt
        {
            get { return (string)GetValue(DefaultExtProperty); }
            set { SetValue(DefaultExtProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the DefaultExt property
        /// </summary>
        public static readonly DependencyProperty DefaultExtProperty =
            DependencyProperty.Register("DefaultExt", typeof(string), typeof(DependencyFileDialog), 
            new UIPropertyMetadata(string.Empty, DialogPropertyChangedCallback));

        /// <summary>
        /// Gets or sets a value indicating whether a file dialog returns either the location of the file referenced by a shortcut 
        /// or the location of the shortcut file (.lnk).
        /// </summary>
        public bool DereferenceLinks
        {
            get { return (bool)GetValue(DereferenceLinksProperty); }
            set { SetValue(DereferenceLinksProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the DereferenceLinks property
        /// </summary>
        public static readonly DependencyProperty DereferenceLinksProperty =
            DependencyProperty.Register("DereferenceLinks", typeof(bool), typeof(DependencyFileDialog), 
            new UIPropertyMetadata(false, DialogPropertyChangedCallback));

        /// <summary>
        /// Gets an array that contains one file name for each selected file.
        /// </summary>
        public string[] FileNames
        {
            get
            {
                return Dialog.FileNames;
            }
        }

        /// <summary>
        /// Gets or sets the index of the filter currently selected in a file dialog.
        /// </summary>
        public int FilterIndex
        {
            get { return (int)GetValue(FilterIndexProperty); }
            set { SetValue(FilterIndexProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the FilterIndex property
        /// </summary>
        public static readonly DependencyProperty FilterIndexProperty =
            DependencyProperty.Register("FilterIndex", typeof(int), typeof(DependencyFileDialog), 
            new UIPropertyMetadata(1, DialogPropertyChangedCallback));

        /// <summary>
        /// Gets or sets the initial directory that is displayed by a file dialog.
        /// </summary>
        public string InitialDirectory
        {
            get { return (string)GetValue(InitialDirectoryProperty); }
            set { SetValue(InitialDirectoryProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the InitialDirectory property
        /// </summary>
        public static readonly DependencyProperty InitialDirectoryProperty =
            DependencyProperty.Register("InitialDirectory", typeof(string), typeof(DependencyFileDialog), 
            new UIPropertyMetadata(string.Empty, DialogPropertyChangedCallback));

        /// <summary>
        /// Gets a string that only contains the file name for the selected file.
        /// </summary>
        public string SafeFileName
        {
            get
            {
                return Dialog.SafeFileName;
            }
        }

        /// <summary>
        /// Gets an array that contains one safe file name for each selected file.
        /// </summary>
        public string[] SafeFileNames
        {
            get
            {
                return Dialog.SafeFileNames;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog accepts only valid Win32 file names.
        /// </summary>
        public bool ValidateNames
        {
            get { return (bool)GetValue(ValidateNamesProperty); }
            set { SetValue(ValidateNamesProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the ValidateNames property
        /// </summary>
        public static readonly DependencyProperty ValidateNamesProperty =
            DependencyProperty.Register("ValidateNames", typeof(bool), typeof(DependencyFileDialog), 
            new UIPropertyMetadata(false, DialogPropertyChangedCallback));

        /// <summary>
        /// Uses reflection to set the property in the contained FileDialog
        /// </summary>
        /// <param name="obj">The FileDialog whose property has been changed</param>
        /// <param name="args">Information about the property and values that has been changed.</param>
        protected static void DialogPropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DependencyFileDialog dfd = obj as DependencyFileDialog;
            PropertyInfo changedProp = dfd.Dialog.GetType().GetProperty(args.Property.Name);
            if (changedProp.CanWrite)
            {
                changedProp.SetValue(dfd.Dialog, args.NewValue, null);
            }
        }

        /// <summary>
        /// This property should be bound to the Command in your ViewModel that is to be performed when the user clicks
        /// on either "Open" or "Save"
        /// </summary>
        public ICommand FileOkCommand
        {
            get
            {
                return (ICommand)GetValue(FileOkCommandProperty);
            }
            set
            {
                SetValue(FileOkCommandProperty, value);
            }
        }

        /// <summary>
        /// The Dependency Property for the FileOkCommand command
        /// </summary>
        public static readonly DependencyProperty FileOkCommandProperty =
            DependencyProperty.Register("FileOkCommand", typeof(ICommand), typeof(DependencyFileDialog));

        private ICommand showDialogCommand;

        /// <summary>
        /// When fired, this command causes the Dialog to be displayed
        /// </summary>
        public ICommand ShowDialogCommand
        {
            get
            {
                if (showDialogCommand == null)
                {
                    showDialogCommand = new RelayCommand(p => Dialog.ShowDialog());
                }
                return showDialogCommand;
            }
        }
    }
}
