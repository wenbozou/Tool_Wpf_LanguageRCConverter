using Language;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageRCConverter.LanguageRCConvert
{
    public class ExcelRC
    {
        #region
        //private Dictionary<string, string> _dicLanRC = new Dictionary<string, string>();

        public Dictionary<string, string> dicLanRC { get; set; } = null;
        #endregion 
        public ExcelRC() { }

        public int StartParse(string filePath)
        {
            int iResult = -1;

            iResult = ReadingFile(filePath);

            return iResult;
        }

        private int ReadingFile(string filePath)
        {
            int iResult = -1;

            if (string.IsNullOrEmpty(filePath)
               || !File.Exists(filePath))
                return iResult;

            if (null == dicLanRC) dicLanRC = new Dictionary<string, string>();
            dicLanRC.Clear();

            var table = NPOIExcel.ExcelToDataTable(filePath, true);

            foreach (DataRow item in table.Rows)
            {
                var key = item[0].ToString();
                var value = item[1].ToString();

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;

                key = key.Trim().Trim(new char[] { ';' }).Trim('"').Replace("\n", "").Replace("\t", "").Replace("\r", "");
                value = value.Trim().Trim(new char[] { ';' }).Trim('"').Replace("\n", "").Replace("\t", "").Replace("\r", "");

                if (!dicLanRC.ContainsKey(key)) dicLanRC.Add(key, value);
            }
            iResult = 0;
            return iResult;
        }
    }
}
