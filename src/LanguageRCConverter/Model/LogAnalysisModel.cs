using LanguageRCConverter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageRCConverter.Model
{
    public enum LogItemType
    {
        FunctionName,
        LanguageName,
        UserName,
        CreateDate
    }
    public class LogNameModel
    {
        public string FunctionName { get; set; }
        public string LanguageName { get; set; }
        public string UserName { get; set; }
        public string Date { get; set; }
    }
    public class LogAnalysisModel: PropertyChangedBase
    {

        #region
        private Dictionary<LogItemType, List<string>> _DicKeyLogName = new Dictionary<LogItemType, List<string>>();
        public Dictionary<LogItemType, List<string>> DicKeyLogName
        {
            get { return _DicKeyLogName; }
            set { _DicKeyLogName = value; RaisePropertyChangedEventImmediately("DicKeyLogName"); }
        }

        #endregion
        public LogAnalysisModel() { }

        public void Start(List<string> _lstFiles)
        {
            AnalysisFileNames(_lstFiles);
        }


        private void AnalysisFileNames(List<string> _lstFiles)
        {
            if (null == _lstFiles) return;

            DicKeyLogName.Clear();
            foreach (string item in _lstFiles) {
                LogNameModel logNameModel = AnalysisFileName(item);
                if (null == logNameModel) continue;

                if (!DicKeyLogName.ContainsKey(LogItemType.FunctionName))
                    DicKeyLogName.Add(LogItemType.FunctionName, new List<string>() { logNameModel.FunctionName });
                else
                {
                    if (!DicKeyLogName[LogItemType.FunctionName].Contains(logNameModel.FunctionName))
                        DicKeyLogName[LogItemType.FunctionName].Add(logNameModel.FunctionName);
                }

                if (!DicKeyLogName.ContainsKey(LogItemType.LanguageName))
                    DicKeyLogName.Add(LogItemType.LanguageName, new List<string>() { logNameModel.LanguageName });
                else
                {
                    if (!DicKeyLogName[LogItemType.LanguageName].Contains(logNameModel.LanguageName))
                        DicKeyLogName[LogItemType.LanguageName].Add(logNameModel.LanguageName);
                }

                if (!DicKeyLogName.ContainsKey(LogItemType.UserName))
                    DicKeyLogName.Add(LogItemType.UserName, new List<string>() { logNameModel.UserName });
                else
                {
                    if (!DicKeyLogName[LogItemType.LanguageName].Contains(logNameModel.UserName))
                        DicKeyLogName[LogItemType.LanguageName].Add(logNameModel.UserName);
                }

            }

        }

        private LogNameModel AnalysisFileName(string fileName) {
            LogNameModel logNameModel = null;
            if (string.IsNullOrEmpty(fileName)) return null;

            char flag = '_';
            int indexFlag = fileName.IndexOf(flag, 0);
            if (-1 == indexFlag) return null;

            logNameModel = new LogNameModel();
            logNameModel.FunctionName = fileName.Substring(0, indexFlag - 1 - 0 + 1);

            int indexFlag2 = fileName.IndexOf(flag, indexFlag + 1);
            logNameModel.LanguageName = fileName.Substring(indexFlag + 1, indexFlag2 - 1 - (indexFlag + 1) + 1);

            int indexFlag3 = fileName.IndexOf(flag, indexFlag2 + 1);
            logNameModel.UserName = fileName.Substring(indexFlag2 + 1, indexFlag3 - 1 - (indexFlag2 + 1) + 1);



            return logNameModel;
        }

    }
}
