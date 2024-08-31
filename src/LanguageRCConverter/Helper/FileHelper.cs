using System;
using System.Windows.Forms;

namespace LanguageRCConverter.Helper
{
    public class FileHelper
    {
        static public string SelectFiles(string filter = "All files(*.*)|*.*")
        {
            string filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = filter;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                }
            }
            return filePath;
        }

        static public string SelectFolder(string description = "")
        {
            string folderPath = string.Empty;

            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = description;
                folderBrowserDialog.ShowNewFolderButton = true;
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    folderPath = folderBrowserDialog.SelectedPath;
                }

            }

            return folderPath;
        }
    }
}
