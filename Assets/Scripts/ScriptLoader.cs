using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ScriptLoader : MonoBehaviour {
    public string[] _scenarios; // シナリオを格納する

    public Color[] _colors; // シナリオの再生色を格納する
    private Color _defaultColors = new Color(0.0f, 0.0f, 0.0f, 1.0f);

    /*
     * @bref: スクリプト変換関数
     *  　　　１クリックベースで分割する。色の変換等を行う
     */
    public void ConvertScript(string script) {
        string split = "[re]";
        _scenarios = script.Split(new string[] { split }, StringSplitOptions.RemoveEmptyEntries);
        _colors = new Color[_scenarios.Length];
        int cnt = 0;
        foreach (string str in _scenarios) {
            string new_text = "";
            // コメントアウト処理
            new_text = Regex.Replace(str, "#.*#", "");
            // 色変え対応
            SetColor(new_text, cnt);
            _scenarios[cnt] = new_text;
            cnt++;
        }
    }

    /*
     * @bref: テキスト色変え
     */
    private void SetColor(string script, int cnt) {
        Match match_text;
        string color_text = "";
        // 初期色で設定
        _colors[cnt] = _defaultColors;
        match_text = Regex.Match(script, @"\[color.*?\]");
        if (match_text.Success) {
            string[] split_text;
            split_text = match_text.Value.Split(' ');
            color_text = split_text[1].Replace("]", "");
            _colors[cnt] = ToColor(color_text);
        }
    }

    /*
     * @bref: カラーコード変換.
     */
    private Color ToColor(string htmlColor) {
        Color color = default(Color);
        if (!ColorUtility.TryParseHtmlString(htmlColor, out color)) {
            Debug.LogError("[ERROR][" + htmlColor + "]\n" +
                "カラーコードの指定が間違えています。\n" +
                "#から始まる6桁もしくは8桁の16進数にで指定してください。");
            color = _defaultColors;
        }
        return color;
    }
}
