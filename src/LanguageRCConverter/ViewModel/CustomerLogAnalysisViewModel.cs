using LanguageRCConverter.Command;
using LanguageRCConverter.Core;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageRCConverter.Model;

namespace LanguageRCConverter.ViewModel
{
    
    public class CustomerLogAnalysisViewModel: PropertyChangedBase
    {
        #region
        private string _LogFolderPath = string.Empty;
        public string LogFolderPath
        {
            get { return _LogFolderPath; }
            set { _LogFolderPath = value; RaisePropertyChangedEventImmediately("LogFolderPath"); }
        }

        private List<string> _LstFiles = new List<string>();
        public List<string> LstFiles
        {
            get { return _LstFiles; }
            set { _LstFiles = value; RaisePropertyChangedEventImmediately("LstFiles"); }
        }

        private bool _AnalysisEnabled = true;
        public bool AnalysisEnabled
        {
            get { return _AnalysisEnabled; }
            set { _AnalysisEnabled = value; RaisePropertyChangedEventImmediately("AnalysisEnabled"); }
        }

        private Dictionary<string, int> _DicFunctionCount = new Dictionary<string, int>();
        public Dictionary<string, int> DicFunctionCount
        {
            get { return _DicFunctionCount; }
            set { _DicFunctionCount = value; RaisePropertyChangedEventImmediately("DicFunctionCount"); }
        }
        #endregion
        public CustomerLogAnalysisViewModel() { }


        public RelayCommand LogFolderSelecteCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    LogFolderPath = Helper.FileHelper.SelectFolder();
                });
            }
        }

        public RelayCommand AnalysisCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    if (string.IsNullOrEmpty(LogFolderPath) || !Directory.Exists(LogFolderPath)) return;
                    AnalysisEnabled = false;
                    Task.Factory.StartNew(() => {
                        LstFiles.Clear();

                        DirectoryInfo di = new DirectoryInfo(LogFolderPath);
                        foreach (FileInfo fItem in di.GetFiles()) {
                            if (!File.Exists(fItem.FullName)) continue;
                            LstFiles.Add(fItem.Name);
                        }

                        LogAnalysisModel logAnalysisModel = new LogAnalysisModel();
                        logAnalysisModel.Start(LstFiles);

                        if (null != logAnalysisModel.DicKeyLogName && null != logAnalysisModel.DicKeyLogName[LogItemType.FunctionName]) {
                            DicFunctionCount.Clear();
                            var DicKeyLogNameValue = logAnalysisModel.DicKeyLogName[LogItemType.FunctionName];
                            foreach (string item in DicKeyLogNameValue) {
                                foreach(string nameItem in LstFiles) {
                                    if (nameItem.Contains(item)){
                                        if (!DicFunctionCount.ContainsKey(item))
                                            DicFunctionCount.Add(item, 1);
                                        else
                                            DicFunctionCount[item] += 1;
                                    }
                                }
                            }
                        }

                        int total = 0;
                        foreach (var item in DicFunctionCount){
                            total += item.Value;
                        }
                        System.Console.WriteLine("Total:{0}", total);

                        foreach (var item in DicFunctionCount){
                            System.Console.WriteLine("{0}:{1}, Percentage:{2:F2}%", item.Key, item.Value, 100.0*item.Value/total);
                        }
                        AnalysisEnabled = true;
                    });

                });
            }
        }
    }
}
