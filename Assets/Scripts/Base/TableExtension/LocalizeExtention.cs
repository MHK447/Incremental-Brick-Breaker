using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanpoFri
{
    public static class LocalizeExtention
    {
        public static string GetString(this BanpoFri.Localize tableData, string key, bool IsEmptyReturn = false)
        {
            var data = tableData.GetData(key);
            if (data == null && !IsEmptyReturn)
            {
                return $"empty!!{key}";
            }
            else if (data == null && IsEmptyReturn)
            {
                return "";
            }
            switch (GameRoot.Instance.UserData.Language)
            {
                case Language.en:
                    return data.en;
                case Language.ko:
                    return data.ko;
                case Language.ja:
                    return data.ja;
                case Language.es:
                    return data.es;
                case Language.de:
                    return data.de;
                case Language.tw:
                    return data.tw;
                case Language.ru:
                    return data.ru;
                case Language.fr:
                    return data.fr;
                case Language.vi:
                    return data.vi;
                default:
                    return data.en;
            }
        }

        public static string GetFormat(this BanpoFri.Localize tableData, string key, params object[] args)
        {
            var str = GetString(tableData, key);
            return string.Format(str, args);
        }
    }
}