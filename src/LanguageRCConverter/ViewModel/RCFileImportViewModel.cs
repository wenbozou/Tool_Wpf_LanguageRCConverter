using LanguageRCConverter.Command;
using LanguageRCConverter.Core;
using LanguageRCConverter.Helper;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LanguageRCConverter.ViewModel
{
    public class RCFileImportViewModel: PropertyChangedBase
    {


        #region
        private string _LanRCFilePath = string.Empty;
        public string LanRCFilePath
        {
            get { return _LanRCFilePath; }
            set { _LanRCFilePath = value; RaisePropertyChangedEventImmediately("LanRCFilePath"); }
        }

        private string _LanTextFilePath = string.Empty;
        public string LanTextFilePath
        {
            get { return _LanTextFilePath; }
            set { _LanTextFilePath = value; RaisePropertyChangedEventImmediately("LanTextFilePath"); }
        }

        private string _OutputFolder = string.Empty;
        public string OutputFolder
        {
            get { return _OutputFolder; }
            set { _OutputFolder = value; RaisePropertyChangedEventImmediately("OutputFolder"); }
        }

        private bool _ConvertEnabled = true;
        public bool ConvertEnabled
        {
            get { return _ConvertEnabled; }
            set { _ConvertEnabled = value; RaisePropertyChangedEventImmediately("ConvertEnabled"); }
        }
        #endregion

        public RCFileImportViewModel()
        {
        }

        public RelayCommand XamlFileSelecteCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    string filter = "xaml files (*.xaml)|*.xaml";
                    string filePath = FileHelper.SelectFiles(filter);
                    LanRCFilePath = filePath;
                });
            }
        }

        public RelayCommand ExcelFileSelecteCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    string filter = "xlsx files (*.xlsx)|*.xlsx|xls files (*.xls)|*.xls";
                    string filePath = FileHelper.SelectFiles(filter);
                    LanTextFilePath = filePath;
                });
            }
        }

        public RelayCommand OutputFolderCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    string folderPath = FileHelper.SelectFolder("");
                    OutputFolder = folderPath;
                });
            }
        }

        public RelayCommand ConvertFileCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    ConvertEnabled = false;
                    if (string.IsNullOrEmpty(LanRCFilePath) || !File.Exists(LanRCFilePath)) return;
                    if (string.IsNullOrEmpty(LanTextFilePath) || !File.Exists(LanTextFilePath)) return;
                    if (string.IsNullOrEmpty(OutputFolder)) return;
                    FileInfo fi = new FileInfo(LanRCFilePath);
                    string fileName = fi.Name;

                    Task.Factory.StartNew(() => {
                        LanguageRCConvert.LanguageRCConverter obj = new LanguageRCConvert.LanguageRCConverter();
                        if (0 == obj.StartConverter(LanRCFilePath, LanTextFilePath)) {
                            obj.OutputLanRC(OutputFolder, fileName);
                        }
                        ConvertEnabled = true;
                    });
                });
            }
        }
        
    }
}
