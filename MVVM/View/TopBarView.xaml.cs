﻿using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TextReplace.MVVM.ViewModel;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for TopBarView.xaml
    /// </summary>
    public partial class TopBarView : UserControl
    {
        public TopBarView()
        {
            InitializeComponent();
        }

        private void UploadReplaceFile_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Upload";
            string body = "Upload a file for the replacement phrases.";

            var dialog = new PopupWindows.UploadReplacementsInputWindow(window, title, body);

            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                ((TopBarViewModel)DataContext).SetNewReplaceFile(dialog.FullFileName);
                TopBarViewModel.SetActiveContentView("replace");
            }
        }

        private void UploadSourceFile_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Upload";
            string body = "Upload Text or Document files for the sources to perform the replacements on.";

            var dialog = new PopupWindows.UploadSourceFilesInputWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                ((TopBarViewModel)DataContext).SourceFiles(dialog.FullFileNames);
                TopBarViewModel.SetActiveContentView("sources");
            }
        }

        private async void PerformReplacementsOnAllFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Replacify";
            string body = "Perform replacements on all files.";

            var dialog = new PopupWindows.ReplaceFilesWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnCancel.IsChecked == false)
            {
                return;
            }

            string filePath = await TopBarViewModel.ReplaceAll(dialog.OpenFileLocation);

            // if the user selected to open the file location,
            // open the file explorer and highlight the first generated file
            if (dialog.OpenFileLocation)
            {
                Process.Start("explorer.exe", "/select, " + filePath);
            }

            TopBarViewModel.SetActiveContentView("output");
        }

        private void OpenFileSuffixInputWindow(object sender, RoutedEventArgs e)
        {
            var viewModel = (TopBarViewModel)(DataContext);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            var window = Window.GetWindow(sender as DependencyObject);
            string title = textInfo.ToTitleCase(suffixMenuOption.Text);
            string body;
            if (viewModel.Suffix != string.Empty)
            {
                body = $"<u>Current suffix:</u> {viewModel.Suffix}";
            }
            else
            {
                body = "Enter suffix which will be appended onto the output file names.\n" +
                    "<u>Note:</u> This defaults to \"-replacify\"";
            }
            string watermark = "-replacify";

            var dialog = new PopupWindows.SetSuffixInputResetWindow(window, title, body, watermark);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true || dialog.BtnReset.IsChecked == true)
            {
                viewModel.SetSuffix(dialog.InputText);
            }
        }
    }
}
