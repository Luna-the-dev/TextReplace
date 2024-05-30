﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Windows;
using TextReplace.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Messages.Sources;
using TextReplace.Core.Validation;

namespace TextReplace.MVVM.ViewModel
{
    partial class TopBarViewModel : ObservableObject,
        IRecipient<DefaultSourceFileOptionsMsg>
    {

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ToggleCaseSensitiveCommand))]
        private Visibility _caseSensitive = Visibility.Hidden;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ToggleWholeWordCommand))]
        private Visibility _wholeWord = Visibility.Hidden;

        [ObservableProperty]
        private string _suffix = SourceFilesData.DefaultSourceFileOptions.Suffix ?? "";
        partial void OnSuffixChanged(string value)
        {
            SourceFilesData.UpdateAllSourceFileOptions(suffix: value);
        }

        // visibility flags for top bar components
        [ObservableProperty]
        private Visibility _replaceFileReadSuccess = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _replaceFileReadFail = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _sourceFileReadSuccess = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _sourceFileReadFail = Visibility.Hidden;
        [ObservableProperty]
        private bool _replaceIsClickable = false;

        // commands
        public RelayCommand Replace => new RelayCommand(ReplaceCmd);
        public RelayCommand ChangeOutputDirectory => new RelayCommand(ChangeOutputDirectoryCmd);

        public TopBarViewModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        /// <summary>
        /// Wrapper for ReplaceData.SetNewReplaceFile and updates the replace button clickability.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newDelimiter"></param>
        /// <returns>False if new replace file was not set.</returns>
        public bool SetNewReplaceFile(string fileName, string? newDelimiter = null)
        {
            // open a file dialogue for the user and update the replace file
            bool result = ReplaceData.SetNewReplaceFile(fileName, newDelimiter);

            if (result)
            {
                ReplaceFileReadSuccess = Visibility.Visible;
                ReplaceFileReadFail = Visibility.Hidden;
            }
            else
            {
                Debug.WriteLine("ReplaceFile either could not be read or parsed.");
                ReplaceFileReadSuccess = Visibility.Hidden;
                ReplaceFileReadFail = Visibility.Visible;
            }

            SetReplaceButtonClickability();

            return result;
        }

        public void SourceFiles(List<string> fileNames)
        {
            // open a file dialogue for the user and update the source files
            bool result = SourceFilesData.SetNewSourceFiles(fileNames);

            if (result == true)
            {
                Debug.Write("Source file name(s):");
                SourceFilesData.SourceFiles.ForEach(i => Debug.WriteLine($"\t{i.FileName}"));
                SourceFileReadSuccess = Visibility.Visible;
                SourceFileReadFail = Visibility.Hidden;
            }
            else
            {
                Debug.WriteLine("SourceFile could not be read.");
                SourceFileReadSuccess = Visibility.Hidden;
                SourceFileReadFail = Visibility.Visible;
            }

            SetReplaceButtonClickability();
        }

        private void ReplaceCmd()
        {
            if (ReplaceData.FileName == string.Empty || SourceFilesData.SourceFiles.Count == 0)
            {
                Debug.WriteLine("Replace file or source files were empty. This should never be reached...");
                return;
            }

            bool caseSensitive = (CaseSensitive == Visibility.Visible) ? true : false;
            ReplaceData replaceData = new ReplaceData(caseSensitive);

            // create a list of destination file names
            List<string> destFileNames = SourceFilesData.GenerateDestFileNames();

            // perform the text replacements
            bool wholeWord = (WholeWord == Visibility.Visible) ? true : false;
            bool result = replaceData.PerformReplacements(
                SourceFilesData.SourceFiles.Select(x => x.FileName).ToList(),
                destFileNames,
                wholeWord);
            Debug.WriteLine("Output file names:");
            destFileNames.ForEach(o => Debug.WriteLine($"\t{o}"));

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
            }
            else
            {
                Debug.WriteLine("Replacements successfully performed.");
            }
        }

        [RelayCommand]
        private void ToggleCaseSensitive()
        {
            CaseSensitive = (CaseSensitive == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
        }

        [RelayCommand]
        private void ToggleWholeWord()
        {
            WholeWord = (WholeWord == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
        }

        private void ChangeOutputDirectoryCmd()
        {
            // configure open file dialog box
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = "Select Folder",
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                Debug.WriteLine("Change default file path window was closed");
                return;
            }

            SourceFilesData.UpdateAllSourceFileOptions(outputDirectory: dialog.FileName);
        }

        /// <summary>
        /// Wrapper function for ReplaceData.SetDelimiter
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static bool SetDelimiter(string delimiter)
        {
            try
            {
                ReplaceData.Delimiter = delimiter;
                return true;
            }
            catch (ArgumentException e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Checks to see if the suffix is valid, and then sets the suffix
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public bool SetSuffix(string suffix)
        {
            if (DataValidation.IsSuffixValid(suffix))
            {
                Suffix = suffix;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the replace button visibility based on whether replace/source files were successfully uploaded.
        /// </summary>
        private void SetReplaceButtonClickability()
        {
            ReplaceIsClickable = (ReplaceFileReadSuccess == Visibility.Visible &&
                                  SourceFileReadSuccess == Visibility.Visible);
        }

        public void Receive(DefaultSourceFileOptionsMsg message)
        {
            Suffix = message.Value.Suffix;
        }
    }
}
