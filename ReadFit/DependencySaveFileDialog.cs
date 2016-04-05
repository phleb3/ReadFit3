using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows;

namespace QA.FileDialogs
{
    public class DependencySaveFileDialog : DependencyFileDialog
    {
        private SaveFileDialog dialog;

        /// <summary>
        /// Overridden from DependencyFileDialog. Provides access to an instance of the Microsoft.Win32.SaveFileDialog class.
        /// </summary>
        protected override FileDialog Dialog
        {
            get
            {
                if (dialog == null)
                {
                    dialog = new SaveFileDialog();

                }
                return dialog;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether DependencySaveFileDialog prompts the user for permission 
        /// to create a file if the user specifies a file that does not exist. 
        /// </summary>
        public bool CreatePrompt
        {
            get { return (bool)GetValue(CreatePromptProperty); }
            set { SetValue(CreatePromptProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the CreatePrompt property
        /// </summary>
        public static readonly DependencyProperty CreatePromptProperty =
            DependencyProperty.Register("CreatePrompt", typeof(bool), typeof(DependencySaveFileDialog),
            new UIPropertyMetadata(false, DialogPropertyChangedCallback));

        /// <summary>
        /// Gets or sets a value indicating whether SaveFileDialog displays a warning if the user specifies the name of a file that already exists. 
        /// </summary>
        public bool OverwritePrompt
        {
            get { return (bool)GetValue(OverwritePromptProperty); }
            set { SetValue(OverwritePromptProperty, value); }
        }

        /// <summary>
        /// The Dependency Property for the OverwritePrompt property
        /// </summary>
        public static readonly DependencyProperty OverwritePromptProperty =
            DependencyProperty.Register("OverwritePrompt", typeof(bool), typeof(DependencySaveFileDialog), 
            new UIPropertyMetadata(true, DialogPropertyChangedCallback));        
    }
}
