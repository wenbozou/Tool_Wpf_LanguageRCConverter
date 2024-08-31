using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LanguageRCConverter.LanguageRCConvert
{
    public class LanguageRCConverter
    {
        private LanguageRC _LanguageRCObj = null;
        private ExcelRC _ExcelRCObj = null;
        public LanguageRCConverter() { }

        public int StartConverter(string xamlPath, string xlsPath)
        {
            int iResult = 0;

            _LanguageRCObj = new LanguageRCConvert.LanguageRC();
            iResult = _LanguageRCObj.StartParse(xamlPath);
            if (0 != iResult) return iResult;

            _ExcelRCObj = new LanguageRCConvert.ExcelRC();
            iResult = _ExcelRCObj.StartParse(xlsPath);
            if (0 != iResult) return iResult;

            if (null == _ExcelRCObj.dicLanRC || null == _LanguageRCObj.DicLanguageRC) return -1;

            foreach(var xlsItem in _ExcelRCObj.dicLanRC){
                for (int i = 0; i < _LanguageRCObj.DicLanguageRC.Count; i++)
                {
                    var xamlItem = _LanguageRCObj.DicLanguageRC.ElementAt(i);
                    if (xlsItem.Key == xamlItem.Value.LineValue)
                        xamlItem.Value.LineValue = xlsItem.Value;
                    else
                        ;
                }
             
            }

            return iResult;
        }


        public int OutputLanRC(string xamlFolder, string xamlFileName)
        {
            int iResult = 0;

            if (string.IsNullOrEmpty(xamlFolder) || string.IsNullOrEmpty(xamlFileName)) return -1;
            if (!Directory.Exists(xamlFolder)) Directory.CreateDirectory(xamlFolder);
            if (null == _LanguageRCObj) return -2;

            string filePath = Path.Combine(xamlFolder, xamlFileName);
            _LanguageRCObj.SaveFile(filePath);

            return iResult;
        }
    }
}
