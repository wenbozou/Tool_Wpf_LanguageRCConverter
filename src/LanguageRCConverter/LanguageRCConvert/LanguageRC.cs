using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageRCConverter.LanguageRCConvert
{
    public class RCItemInfo
    {
        public int LineNum { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }

        private string _LineValue;
        public string LineValue {
            get { return _LineValue; }
            set { _LineValue = value; }
        }

        public RCItemInfo(int lineNum, string value, int startIndex, int length)
        {
            LineNum = lineNum;
            _LineValue = value;
            StartIndex = startIndex;
            Length = length;
        }

    }
    public class LanguageRC
    {
        #region 字段
        /// <summary>
        /// 文件中存在的每一行的内容
        /// </summary>
        private List<string> _lstLines = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, RCItemInfo> _DicLanguageRC = null;//new Dictionary<string, RCItemInfo>();

        public Dictionary<string, RCItemInfo> DicLanguageRC {
            get { return _DicLanguageRC; }
            set { _DicLanguageRC = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private string _itemStringRCFlag = "sys:String";
        private string _itemKeyFlag = "x:Key=";
        #endregion
        public LanguageRC() { }

        public int StartParse(string filePath)
        {
            int iResult = -1;

            iResult = ReadingFile(filePath);

            return iResult;
        }

        public int SaveFile(string filePath)
        {
            return WrittingFile(filePath);
        }


        private int ReadingFile(string filePath)
        {
            int iResult = -1;

            if (string.IsNullOrEmpty(filePath)
                || !File.Exists(filePath))
                return iResult;

            if (null == _lstLines) return -2;
            _lstLines.Clear();

            if (null == DicLanguageRC) DicLanguageRC = new Dictionary<string, RCItemInfo>();
            DicLanguageRC.Clear();

            try {
                using (StreamReader sr = new StreamReader(filePath)){
                    string line = string.Empty;
                    while ((line = sr.ReadLine()) != null) _lstLines.Add(line);
                }

                iResult = 0;
            } catch (Exception e) { }

            string key = string.Empty;
            string value = string.Empty;
            int startIndex = -1;
            int length = 0;
            int nCount = _lstLines.Count;
            for (int i = 0; i < nCount; i++){
                string item = _lstLines[i];
                if (0 == ParseLine(item, ref key, ref value, ref startIndex, ref length))
                    DicLanguageRC.Add(key, new RCItemInfo(i+1, value, startIndex, length));
                else Console.WriteLine("line number:{0}, line content:{1}", i, item);
            }
            
            return iResult;
        }

        private int ParseLine(string line, ref string key, ref string value, ref int startIndex, ref int length)
        {
            int iResult = 0;
            if (string.IsNullOrEmpty(line)) return -1;

            //"</sys:String>"
            int indexItemSubffix = line.IndexOf(string.Format("</{0}>", _itemStringRCFlag));
            if (-1 == indexItemSubffix) return -2;

            int indexKeyFlag = line.IndexOf(_itemKeyFlag);
            if (-1 == indexKeyFlag) return -3;

            int indexItemPrefixEnd = line.IndexOf('>', indexKeyFlag);
            if (-1 == indexItemPrefixEnd) return -4;
            if (indexItemPrefixEnd >= indexItemSubffix) return -5;

            string itemKey = string.Empty;
            string itemValue = string.Empty;

            string tempKey = line.Substring(indexKeyFlag, indexItemPrefixEnd - indexKeyFlag);
            if (string.IsNullOrEmpty(tempKey)) return -6;

            int indexLeftQuotes = tempKey.IndexOf('\"', 0);
            if (tempKey.Length <= (indexLeftQuotes + 1)) return -7;

            int indexRightQuotes = tempKey.IndexOf('\"', indexLeftQuotes + 1);
            if (-1 == indexRightQuotes) return -8;

            itemKey = tempKey.Substring(indexLeftQuotes, indexRightQuotes - indexLeftQuotes).Trim('\"');
            itemValue  = line.Substring(indexItemPrefixEnd + 1, indexItemSubffix - (indexItemPrefixEnd + 1));

            if (string.IsNullOrEmpty(itemKey)) return -9;
            key = itemKey;
            value = itemValue;
            startIndex = indexItemPrefixEnd + 1;
            length = indexItemSubffix - (indexItemPrefixEnd + 1);
            return iResult;
        }


        private int WrittingFile(string filePath)
        {
            int iResult = -1;
            if (string.IsNullOrEmpty(filePath)) return iResult;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite)){
                using (StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8)) {
                    if (null == DicLanguageRC) return -2;

                    string line = string.Empty;

                    int i = 0;
                    int j = 0;
                    RCItemInfo curRCItemInfo = null;
                    RCItemInfo preRCItemInfo = null;
                    foreach (var item in DicLanguageRC) {
                        curRCItemInfo = item.Value;
                        if (null == curRCItemInfo) continue;

                        line = string.Empty;
                        if (curRCItemInfo.LineNum > 0) line = _lstLines.ElementAt(curRCItemInfo.LineNum - 1);
                        if (-1 != curRCItemInfo.StartIndex && 0 != curRCItemInfo.Length) {
                            string oldLineValue = line.Substring(curRCItemInfo.StartIndex, curRCItemInfo.Length);
                            line = line.Replace(oldLineValue, curRCItemInfo.LineValue);
                        }

                        // 遍历中间未连续的行号
                        i = null != preRCItemInfo ? preRCItemInfo.LineNum + 1: 1;
                        j = curRCItemInfo.LineNum;
                        for (; i < j; i++){
                            string unMatchedLine = _lstLines.ElementAt(i - 1);
                            sw.WriteLine(unMatchedLine);
                        }

                        // 当前行号
                        sw.WriteLine(line);

                        preRCItemInfo = curRCItemInfo;
                    }

                    // 遍历尾部未连续的行号
                    i = null != preRCItemInfo ? preRCItemInfo.LineNum + 1 : 1;
                    j = _lstLines.Count + 1;
                    for (; i < j; i++) {
                        string unMatchedLine = _lstLines.ElementAt(i - 1);
                        sw.WriteLine(unMatchedLine);
                    }
                }
            }

            return iResult;
        }
    }
}
