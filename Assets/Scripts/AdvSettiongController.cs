﻿using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// INIファイルを読み書きするクラス
/// </summary>
public class AdvSettiongController {
    [DllImport("kernel32.dll")]
    private static extern int GetPrivateProfileString(
        string lpApplicationName,
        string lpKeyName,
        string lpDefault,
        StringBuilder lpReturnedstring,
        int nSize,
        string lpFileName);

    [DllImport("kernel32.dll")]
    private static extern int WritePrivateProfileString(
        string lpApplicationName,
        string lpKeyName,
        string lpstring,
        string lpFileName);

    string filePath;

    /// <summary>
    /// ファイル名を指定して初期化します。
    /// ファイルが存在しない場合は初回書き込み時に作成されます。
    /// </summary>
    public AdvSettiongController(string filePath) {
        this.filePath = filePath;
    }

    /// <summary>
    /// sectionとkeyからiniファイルの設定値を取得、設定します。 
    /// </summary>
    /// <returns>指定したsectionとkeyの組合せが無い場合は""が返ります。</returns>
    public string this[string section, string key] {
        set {
            WritePrivateProfileString(section, key, value, filePath);
        }
        get {
            StringBuilder sb = new StringBuilder(256);
            GetPrivateProfileString(section, key, string.Empty, sb, sb.Capacity, filePath);
            return sb.ToString();
        }
    }

    /// <summary>
    /// sectionとkeyからiniファイルの設定値を取得します。
    /// 指定したsectionとkeyの組合せが無い場合はdefaultvalueで指定した値が返ります。
    /// </summary>
    /// <returns>
    /// 指定したsectionとkeyの組合せが無い場合はdefaultvalueで指定した値が返ります。
    /// </returns>
    public string GetValue(string section, string key, string defaultvalue) {
        StringBuilder sb = new StringBuilder(256);
        GetPrivateProfileString(section, key, defaultvalue, sb, sb.Capacity, filePath);
        return sb.ToString();
    }
}