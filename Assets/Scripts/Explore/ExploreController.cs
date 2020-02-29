using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class ExploreController : MonoBehaviour {
    [SerializeField]
    public Button _btnBG;
    [SerializeField]
    public GameObject _preExploreBtn;
    [SerializeField]
    public GameObject _parentBtn;

    public AdvController advController;
    public AdvController onAdvController;

    private List<ExploreButton> _btnList = new List<ExploreButton>();
    private int _checkCount;        // チェック回数.
    private int _endCount;          // 終了チェック回数.
    private int _currentLine;             // 現在の行番号
    private int _colorParam;
    private string _endCommand;
    private string _endPath;
    private bool _isPlayADV;

    public void Setup(string path) {
        // 画面の表示.
        gameObject.SetActive(true);

        //変数の初期化.
        _checkCount = 0;
        _endCount = 0;
        _currentLine = 0;
        _colorParam = 255;
        _endCommand = "";
        _endPath = "";
        _isPlayADV = false;

        // ボタンの初期化.
        _btnList.ForEach(btn => Destroy(btn.gameObject));
        _btnBG.onClick.RemoveAllListeners();
        _btnBG.onClick.AddListener(() => {
            resetState();
        });

        // スクリプトロード.
        // path = "Explore/Scripts/dummy_explore";
        ScriptSetup(path);
    }

    public void Update() {
        checkCount();
    }

    public void ScriptSetup(string script) {
        //テキストファイルのデータを取得するインスタンスを作成
        TextAsset textasset = new TextAsset();
        //Resourcesフォルダから対象テキストを取得
        textasset = Resources.Load(script, typeof(TextAsset)) as TextAsset;
        //テキスト全体をstring型で入れる変数を用意して入れる
        string load_script = textasset.text;//@TODO
        // 背景画像の登録.
        string[] scripts = ConvertScript(load_script);
        Dictionary<string, string> param = new Dictionary<string, string>();
        int cnt = 0;
        while (cnt <scripts.Count()) {
            CheckBG(scripts[cnt]);
            CheckButton(scripts[cnt]);
            CheckEmptyButton(scripts[cnt]);
            ChecEnd(scripts[cnt]);
            cnt++;
        }
    }

    /*
     * @bref: スクリプト変換関数
     *  　　　１クリックベースで分割する。色の変換等を行う
     */
    private string[] ConvertScript(string script) {
        string split = "@";
        string[] scripts = script.Split(new string[] { split }, StringSplitOptions.RemoveEmptyEntries);
        int cnt = 0;
        foreach (string str in scripts) {
            string new_text = "";
            // コメントアウト処理
            new_text = Regex.Replace(str, "#.*#", "");
            new_text = new_text.Replace("\n", "");
            scripts[cnt] = new_text;
            cnt++;
        }
        return scripts;
    }

    private void CheckBG(string script) {
        // 背景設定.
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[bgpath .*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                string[] split_text;
                split_text = match.Value.Split(' ');
                string imgPath = split_text[1];
                float posX = float.Parse(split_text[2]);
                float posY = float.Parse(split_text[3]);
                float alpha = (float)int.Parse(split_text[4].Replace("]", "")) / _colorParam;
                // 画像の差し替え.
                Texture2D texture = Resources.Load(imgPath, typeof(Texture2D)) as Texture2D;
                _btnBG.image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector3(posX, posY));
                _btnBG.image.color = new Color(_btnBG.image.color.r, _btnBG.image.color.g, _btnBG.image.color.b, alpha);
                _btnBG.image.SetNativeSize();
                // 座標の設定.
                _btnBG.transform.localPosition = new Vector3(posX, posY);
            }
        }
    }

    // ボタン画像を設定しないタイプ.
    private void CheckEmptyButton(string script) {
        // ボタン画像設定.
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[path .*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                string[] split_text;
                split_text = match.Value.Split(' ');
                float posX = float.Parse(split_text[1]);
                float posY = float.Parse(split_text[2]);
                float btnW = float.Parse(split_text[3]);
                float btnH = float.Parse(split_text[4]);
                float alpha = (float)int.Parse(split_text[5]) / _colorParam;
                string scriptPath = split_text[6].Replace("]", "");

                // prefabの複製.
                GameObject obj = Instantiate(_preExploreBtn, _parentBtn.transform);
                // ボタンサイズの設定
                ExploreButton btn = obj.GetComponent<ExploreButton>();
                btn.image.rectTransform.sizeDelta = new Vector2(btnW, btnH);
                btn.image.color = new Color(btn.image.color.r, btn.image.color.g, btn.image.color.b, alpha);
                // 座標の設定.
                btn.transform.localPosition = new Vector3(posX, posY);
                // 次に読むスクリプトの登録.
                btn.Setup(resetState, onAdvController, scriptPath);
                btn.onClick.AddListener(() => {
                    if (btn.state == ExploreButton.State.Select) {
                        _checkCount++;
                        btn.state = ExploreButton.State.None;
                    }
                });
                _btnList.Add(btn);
            }
        }
    }


    private void CheckButton(string script) {
        // ボタン画像設定.
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[pathImg .*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                string[] split_text;
                split_text = match.Value.Split(' ');
                string imgPath = split_text[1];
                float posX = float.Parse(split_text[2]);
                float posY = float.Parse(split_text[3]);
                float alpha = (float)int.Parse(split_text[4]) / _colorParam;
                string scriptPath = split_text[5].Replace("]", "");

                // prefabの複製.
                GameObject obj = Instantiate(_preExploreBtn, _parentBtn.transform);
                // 画像の差し替え.
                Texture2D texture = Resources.Load(imgPath, typeof(Texture2D)) as Texture2D;
                ExploreButton btn = obj.GetComponent<ExploreButton>();
                btn.image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector3(posX, posY));
                btn.image.color = new Color(btn.image.color.r, btn.image.color.g, btn.image.color.b, alpha);
                btn.image.SetNativeSize();
                // 座標の設定.
                btn.transform.localPosition = new Vector3(posX, posY);
                // 次に読むスクリプトの登録.
                btn.Setup(resetState, onAdvController, scriptPath);
                btn.onClick.AddListener(() => {
                    if (btn.state == ExploreButton.State.Select) {
                        _checkCount++;
                        btn.state = ExploreButton.State.None;
                    }
                });
                _btnList.Add(btn);
            }
        }
    }

    private void ChecEnd (string script) {
        MatchCollection match_text;
        // ボタン画像設定.
        match_text = Regex.Matches(script, @"\[end .*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                string[] split_text;
                split_text = match.Value.Split(' ');
                int count= int.Parse(split_text[1]);
                _endPath = split_text[2];
                _endCount = count;
            }
        }
        // コマンド取得.
        MatchCollection match_text_command;
        match_text_command = Regex.Matches(script, @"\{.*?\}");
        foreach (Match match in match_text_command) {
            if (match.Success) {
                string command;
                command = match.Value;
                command = command.Replace("{", "[");
                command = command.Replace("}", "]");
                _endCommand = command;
            }
        }
    }

    private void checkCount() {
        if (_checkCount >= _endCount && !onAdvController.isActiveAndEnabled) {
            advController.ScriptSetup(_endPath, _endCommand);
            advController.playScript();

            gameObject.SetActive(false);
        }
    }

    private void resetState() {
        _btnList.ToList().ForEach(btn => {
            btn.resetState();
        });
    }
}
