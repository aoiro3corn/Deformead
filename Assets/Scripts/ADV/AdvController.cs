using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;  // DoTween.

public class AdvController : MonoBehaviour {
    [SerializeField]
    public Text _uiText; // uiTextへの参照を保つ
    [SerializeField]
    public Text _uiNameText;

    [SerializeField]
    public Button _btnScript;
    [SerializeField]
    public Button _btnAuto;

    [SerializeField]
    public GameObject _nameSpace;
    [SerializeField]
    public GameObject _panelChara;
    [SerializeField]
    public GameObject _preChara;
    [SerializeField]
    public GameObject _panelBg;

    [SerializeField]
    public ChoiceController _choiceController;  // 選択肢制御コントローラー
    [SerializeField]
    public InputFieldController _inputController;   // 名前入力制御コントローラー
    [SerializeField]
    public BlinkController _blinkController;
    [SerializeField]
    [Range(0.001f, 0.3f)]
    float _intervalForCharacterDisplay = 0.05f;  // 1文字の表示にかかる時間

    // メニュー画面.
    /*
    [SerializeField]
    public Button _btnOption;
    [SerializeField]
    public GameObject _panelOption;
    [SerializeField]
    public GameObject _panelOptionContent;
    [SerializeField]
    public GameObject _prfSaveData;
    [SerializeField]
    public GameObject _panelMenuBg;
    */
    private bool _pauseFlag;

    public Dictionary<string, object> _scriptParams;
    public ExploreController exploreController;

    private string[] _scenarios; // シナリオを格納する
    private string _currentText = string.Empty;  // 現在の文字列
    private string _loadKey = "loadText";       // ロードテキスト
    private string _countKey = "countText";     // カウントテキスト

    private float _timeUntilDisplay = 0;     // 表示にかかる時間
    private float _timeElapsed = 1;          // 文字列の表示を開始した時間

    private int _currentLine = 0;             // 現在の行番号
    private int _lastUpdateCharacter = -1;		// 表示中の文字数

    // private int _saveFileCount = 10;             // 保持セーブファイル数.
    // private int _moveLineTypeCnt = 0;               // スクリプトのラインタイプのときの表示カウント.

    private Color[] _colors; // シナリオの再生色を格納する
    private Color _defaultColors = new Color(0.0f, 0.0f, 0.0f, 1.0f);

    private bool _isAuto = false;
    private bool _once = false;
    private bool _isLoadScript = true;
    private bool isAdv = false;
    private bool isEffect = false;

    public enum MoveType {
        ALL = 0,
        TYPING = 1,
    }

    // iniファイル用.
    private string _path;
    private MoveType _moveType = MoveType.ALL;

    public void Setup (string path, string moveType = "") {
        // パスと動作タイプの設定.
        _path = path;
        if (string.IsNullOrEmpty(moveType) == false)
            _moveType = (MoveType)Enum.Parse(typeof(MoveType), moveType);

        // moveTypeに応じてテキストの表示を修正
        if (_moveType == MoveType.ALL) {
            _uiText.alignment = TextAnchor.MiddleCenter;
        } else if (_moveType == MoveType.TYPING) {
            var pos = _uiText.transform.localPosition;
            _uiText.transform.localPosition = new Vector3(pos.x, pos.y-20, pos.z);
            _uiText.alignment = TextAnchor.UpperLeft;
        }

        // 変数の初期化.
        _currentLine = 0;
        _lastUpdateCharacter = -1;
        _pauseFlag = false;
    }

    public void playScript() {
        // スクリプトのコンバート
        ScriptSetup(_path);
        // 画面表示.
        gameObject.SetActive(true);
    }

    public void ScriptSetup(string path, string command="") {
        //テキストファイルのデータを取得するインスタンスを作成
        TextAsset textasset = new TextAsset();
        //Resourcesフォルダから対象テキストを取得
        textasset = Resources.Load(path, typeof(TextAsset)) as TextAsset;
        //テキスト全体をstring型で入れる変数を用意して入れる
        string load_script = textasset.text;
        ConvertScript(load_script);

        // スクリプトパラメータのセット.
        _scriptParams = new Dictionary<string, object>();

        // セーブデータ仮実装
        // @note 19.05.12 現状セーブ機能が不要のため優先度下げ
        // _panelOption.SetActive(true);
        /*
        for (int i = 0; i < _saveFileCount; i++) {
            var saveData = Instantiate(_prfSaveData, _panelOptionContent.transform);
            var saveController = saveData.GetComponent<SaveDataController>();
            saveController.Setup(this, i);
        }
        */

        // ボタン系セットアップ.
        // @note 19.05.12 現状オート、スキップ機能が不要のため優先度下げ
        /*
        _btnAuto.onClick.RemoveAllListeners();
        _btnAuto.onClick.AddListener(() => {
            _isAuto = !_isAuto;
        });
        _btnOption.onClick.RemoveAllListeners();
        _btnOption.onClick.AddListener(() => {
            OnClickOption();
        });
        */


        _btnScript.onClick.RemoveAllListeners();
        _btnScript.onClick.AddListener(() => {
            if (!isEffect)
                SetNextLine();
        });

        // sound
        /*
        _playSound.Setup("test", "ADV/Sounds/under_waltz");
        _playSound._audioDict["test"].Play();
        */
        // コマンドが来ていたらカウント初期化.
        if (!string.IsNullOrEmpty(command)) 
            _currentLine = 0;
        // ADVセット
        SetNextLine(command);
        isAdv = true;
    }

    private void Update() {
        OpenMenu();
        PlayAdv();
    }

    private void OpenMenu() {
        if (Input.GetKey(KeyCode.Space)) {
            _pauseFlag = !_pauseFlag;
        }
        if (_pauseFlag) {
            // _panelMenuBg.SetActive(true);
            isAdv = false;
        } else {
            // _panelMenuBg.SetActive(false);
            isAdv = true;
        }
    }

    private void PlayAdv() {
        if (isAdv) {
            switch (_moveType) {
                case MoveType.ALL:
                    _uiText.text = _currentText;
                    break;
                case MoveType.TYPING:
                    // クリックから経過した時間が想定表示時間の何%か確認し、表示文字数を出す
                    int displayCharacterCount = 0;
                    if (_currentText.Length > 0) {
                        displayCharacterCount = (int) (Mathf.Clamp01((Time.time - _timeElapsed) / _timeUntilDisplay) * _currentText.Length);
                        try {
                            // 表示文字数が前回の表示文字数と異なるならテキストを更新する
                            if (displayCharacterCount != _lastUpdateCharacter) {
                                _uiText.text = _currentText.Substring(0, displayCharacterCount);
                                _lastUpdateCharacter = displayCharacterCount;
                                if (_isAuto) {
                                    _once = true;
                                }
                            }
                        } catch {
                            Debug.LogError("[ERROR]表示できるスクリプトがありません\n" +
                                "[re]以外のコマンドは表示テキストの前に書くようにしてください。");
                        }
                    } else {
                        // 文字列が何もない場合は、テキストボックスの中身だけリセットしておく.
                        _uiText.text = "";
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /*
     * @bref: オプション開閉
     */
    public void OnClickOption() {
        Debug.Log("option");
        // var active = _panelOption.activeInHierarchy;
        // _panelOption.SetActive(!active);
    }

    /*
     * @bref: スクリプト変換関数
     *  　　　１クリックベースで分割する。色の変換等を行う
     */
    private void ConvertScript(string script) {
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

    // 次行の表示
    public void SetNextLine(string command = "") {
        if (string.IsNullOrEmpty(command)) {
            _currentText = _scenarios[_currentLine];
        } else {
            _currentText = command;
        }
        // 次のスクリプトへの移動.
        LoatNextScript(_currentText);
        // スキップ処理
        SkipScript(_currentText);
        // ロード処理.
        SetNameSpace();
        // 画像読み込み～表示
        SetBgImage(_currentText);
        SetCharaImage(_currentText);
        // サウンド再生.
        Playsound(_currentText);
        // 動作関係
        PlayBgImage(_currentText);
        PlayCharaImage(_currentText);
        PlayBlinkAnimation(_currentText);
        // 画像破棄処理
        DropCharaImage(_currentText);
        // 選択肢
        SetChoice(_currentText);
        // インプットテキスト.
        SetInput(_currentText);
        // 探索パート読み込み.
        SetExplore(_currentText);
        // パラメータ変動
        SetParams(_currentText);
        // テキストボックスの色を変える
        // _uiText.color = _colors[_currentLine];
        // 改行処理
        _currentText = _currentText.Replace("\n", "");
        _currentText = _currentText.Replace("[br]", "\n");
        // ループ処理.
        int next = 0;
        bool isLoop = false;
        Match match_text;
        match_text = Regex.Match(_currentText, @"\[loop .*?\]");
        if (match_text.Success) {
            string[] split_text;
            split_text = match_text.Value.Split(' ');
            next = int.Parse(split_text[1].Replace("]", ""));
            isLoop = true;
        }
        // コマンド系のテキストの消去
        _currentText = Regex.Replace(_currentText, @"\[.*\]", "");
        //パラメータの差し替え.
        match_text = Regex.Match(_currentText, @"\@.*?\@");
        string txtReplace = "";
        if (match_text.Success) {
            string key = match_text.Value.Replace("@", "");
            txtReplace = Convert.ToString(_scriptParams[key]);
        }
        _currentText = Regex.Replace(_currentText, @"\@.*?\@", txtReplace);

        // 現状のデータをロード.
        DataLoad();

        _currentLine++;
        // ループ処理.
        if (isLoop) {
            _currentLine = next;
        }
        // 次の行番号が再生行番号を超えていたら非表示.
        if (_currentLine >= _scenarios.Count()) {
            gameObject.SetActive(false);
        }
        // 想定表示時間と現在の時刻をキャッシュ
        _timeUntilDisplay = _currentText.Length * _intervalForCharacterDisplay;
        _timeElapsed = Time.time;
        // 文字カウントを初期化
        _lastUpdateCharacter = -1;
    }

    private void DataLoad() {
        // ロード用テキスト準備
        if (_scriptParams.ContainsKey(_loadKey)) {
            _scriptParams[_loadKey] = _currentText.Substring(0, Math.Min(_currentText.Length, 20));
        } else {
            _scriptParams.Add(_loadKey, _currentText.Substring(0, Math.Min(_currentText.Length, 20)));
        }
        // ロード用カウント準備
        if (_scriptParams.ContainsKey(_countKey)) {
            _scriptParams[_countKey] = (int)_scriptParams[_countKey];
        } else {
            _scriptParams.Add(_countKey, 0);
        }
    }

    private void SetCharaImage(string script) {
        Dictionary<string, string> param = new Dictionary<string, string>();
        string[] pos = { "0", "0", "0" };
        string[] scale = { "1.0f", "1.0f", "1.0f" };
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[loadC.*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                param = ToDictList(match.Value);
                if (param.ContainsKey("pos")) {
                    pos = param["pos"].Split(',');
                }
                if (param.ContainsKey("scale")) {
                    scale = param["scale"].Split(',');
                }
                Transform trf = _panelChara.transform.Find(param["name"]);
                if (trf == null) {
                    Texture2D texture = Resources.Load(param["src"], typeof(Texture2D)) as Texture2D;
                    var obj = (GameObject)Instantiate(_preChara);
                    obj.name = param["name"];
                    var img = obj.GetComponent<Image>();
                    obj.transform.SetParent(_panelChara.transform);
                    img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    img.SetNativeSize();
                    obj.transform.localPosition = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
                    obj.transform.localScale = new Vector3(float.Parse(scale[0]), float.Parse(scale[1]), float.Parse(scale[2]));
                }
            }
        }
    }

    // キャラ画像をいろいろ動かす.
    private void PlayCharaImage(string script) {
        Dictionary<string, string> param = new Dictionary<string, string>();
        string[] to = { "1.0f", "1.0f", "1.0f" };
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[chara.*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                param = ToDictList(match.Value);
                if (param.ContainsKey("to")) {
                    to = param["to"].Split(',');
                }
                RectTransform trf;
                Vector3 toPos;
                float delay, target;
                Color changeColor;
                Image img;
                isEffect = true;
                switch (param["cmd"]) {
                    case "move":
                        trf = _panelChara.transform.Find(param["name"]).GetComponent<RectTransform>();
                        toPos = new Vector3(float.Parse(to[0]), float.Parse(to[1]), float.Parse(to[2]));
                        delay = float.Parse(param["delay"]);
                        trf.DOLocalMove(toPos, delay, true).OnComplete(() => {
                            isEffect = false;
                        });
                        break;
                    case "shake":
                        trf = _panelChara.transform.Find(param["name"]).GetComponent<RectTransform>();
                        delay = float.Parse(param["delay"]);
                        target = float.Parse(param["target"]);
                        trf.DOShakePosition(delay, target).OnComplete(() => {
                            isEffect = false;
                        });
                        break;
                    case "jump":
                        trf = _panelChara.transform.Find(param["name"]).GetComponent<RectTransform>();
                        toPos = trf.localPosition;// new Vector3(float.Parse(to[0]), float.Parse(to[1]), float.Parse(to[2]));
                        delay = float.Parse(param["delay"]);
                        trf.DOLocalJump(toPos, 80.0f, 1, delay, true).OnComplete(() => {
                            isEffect = false;
                        });　//.SetLoops(-1, LoopType.Yoyo);
                        break;
                    case "fade":
                        trf = _panelChara.transform.Find(param["name"]).GetComponent<RectTransform>();
                        delay = float.Parse(param["delay"]);
                        target = float.Parse(param["target"]);
                        img = trf.GetComponent<Image>();
                        img.color = new Color(img.color.r, img.color.g, img.color.b, (float)(1 - target));
                        DOTween.ToAlpha(
                            () => img.color,
                            color => img.color = color,
                            target, // 目標値
                            delay // 所要時間
                        ).OnComplete(() => {
                            isEffect = false;
                        }).SetLoops(1);
                        break;
                    case "blink":
                        trf = _panelChara.transform.Find(param["name"]).GetComponent<RectTransform>();
                        delay = float.Parse(param["delay"]);
                        target = float.Parse(param["target"]);
                        if (param.ContainsKey("color")) {
                            changeColor = ToColor(param["color"]);
                        }
                        img = trf.GetComponent<Image>();
                        img.color = new Color(img.color.r, img.color.g, img.color.b, (float)(1 - target));
                        DOTween.ToAlpha(
                            () => img.color,
                            color => img.color = color,
                            target, // 目標値
                            delay // 所要時間
                        ).OnComplete(() => {
                            isEffect = false;
                        }).SetLoops(1, LoopType.Yoyo);
                        break;
                }
            }
        }
    }

    private void DropCharaImage(string script) {
        string name = "";
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[delC.*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                string[] split_text;
                split_text = match.Value.Split(' ');
                name = split_text[1].Replace("]", "");
                Transform trf = _panelChara.transform.Find(name);
                if (trf == null) {
                    // Debug.LogWarning("[WARNING]削除対象のオブジェクトがありません。");
                } else {
                    Destroy(trf.gameObject);
                }
            }
        }
    }

    private void SetBgImage(string script) {
        Dictionary<string, string> param = new Dictionary<string, string>();
        string[] pos = { "0", "0", "0" };
        string[] scale = { "1.0f", "1.0f", "1.0f" };
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[loadB.*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                param = ToDictList(match.Value);
                if (param.ContainsKey("pos")) {
                    pos = param["pos"].Split(',');
                }
                if (param.ContainsKey("scale")) {
                    scale = param["scale"].Split(',');
                }
                Texture2D texture = Resources.Load(param["src"], typeof(Texture2D)) as Texture2D;
                var img = _panelBg.GetComponent<Image>();
                img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                img.color = ToColor(param["color"]);
                img.SetNativeSize();
                _panelBg.transform.localPosition = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
                _panelBg.transform.localScale = new Vector3(float.Parse(scale[0]), float.Parse(scale[1]), float.Parse(scale[2]));
            }
        }
    }

    // 背景画像をいろいろ動かす.
    private void PlayBgImage(string script) {
        Dictionary<string, string> param = new Dictionary<string, string>();
        string[] to = { "1.0f", "1.0f", "1.0f" };
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[bg.*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                param = ToDictList(match.Value);
                if (param.ContainsKey("to")) {
                    to = param["to"].Split(',');
                }
                RectTransform trf;
                Vector3 toPos;
                float delay, target;
                Color changeColor;
                Image img;
                isEffect = true;
                switch (param["cmd"]) {
                    case "move":
                        trf = _panelBg.GetComponent<RectTransform>();
                        toPos = new Vector3(float.Parse(to[0]), float.Parse(to[1]), float.Parse(to[2]));
                        delay = float.Parse(param["delay"]);
                        trf.DOLocalMove(toPos, delay, true).OnComplete(() => {
                            isEffect = false;
                        });
                        break;
                    case "shake":
                        trf = _panelBg.GetComponent<RectTransform>();
                        delay = float.Parse(param["delay"]);
                        target = float.Parse(param["target"]);
                        trf.DOShakePosition(delay, 10).OnComplete(() => {
                            isEffect = false;
                        });
                        break;
                    case "fade":
                        trf = _panelBg.GetComponent<RectTransform>();
                        delay = float.Parse(param["delay"]);
                        target = float.Parse(param["target"]);
                        if (param.ContainsKey("color")) {
                            changeColor = ToColor(param["color"]);
                            // GetComponent<Image>().color = changeColor;
                        }
                        img = trf.GetComponent<Image>();
                        img.color = new Color(img.color.r, img.color.g, img.color.b, (float)(1 - target));
                        DOTween.ToAlpha(
                            () => img.color,
                            color => img.color = color,
                            target, // 目標値
                            delay // 所要時間
                        ).OnComplete(() => {
                            isEffect = false;
                        }).SetLoops(1);
                        break;
                    case "blink":
                        trf = _panelBg.GetComponent<RectTransform>();
                        delay = float.Parse(param["delay"]);
                        target = float.Parse(param["target"]);
                        if (param.ContainsKey("color")) {
                            changeColor = ToColor(param["color"]);
                            // GetComponent<Image>().color = changeColor;
                        }
                        img = trf.GetComponent<Image>();
                        img.color = new Color(img.color.r, img.color.g, img.color.b, (float)(1 - target));
                        DOTween.ToAlpha(
                            () => img.color,
                            color => img.color = color,
                            target, // 目標値
                            delay // 所要時間
                        ).OnComplete(() => {
                            isEffect = false;
                        }).SetLoops(1, LoopType.Yoyo);
                        break;
                }
            }
        }
    }

    private void PlayBlinkAnimation(string script) {
        Dictionary<string, string> param = new Dictionary<string, string>();
        string[] pos = { "0", "0"};
        string[] time = { "1.0f", "1.0f"};
        string[] mat = { "1.0f", "1.0f"};
        float openTime, closeTime, topY, bottomY, matValue;
        Material material = null;
        BlinkController.Blink blink;
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[blink .*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                param = ToDictList(match.Value);
                openTime = 0;
                closeTime = 0;
                topY = 0;
                bottomY = 0;
                matValue = 0;
                if (param.ContainsKey("time")) {
                    time = param["time"].Split(',');
                    openTime = float.Parse(time[0]);
                    closeTime = float.Parse(time[1]);
                }
                if (param.ContainsKey("pos")) {
                    pos = param["pos"].Split(',');
                    topY = float.Parse(pos[0]);
                    bottomY = float.Parse(pos[1]);
                }
                if (param.ContainsKey("mat")) {
                    mat = param["mat"].Split(',');
                    material = Resources.Load(mat[0], typeof(Material)) as Material;
                    matValue = float.Parse(mat[1]);
                }
                blink = new BlinkController.Blink(openTime, closeTime, topY, bottomY, material, matValue);
                _blinkController.Setup(blink);
                _blinkController.PlayBlinkAnimation();
            }
        }
    }

    private void SetNameSpace() {
        _currentText = _scenarios[_currentLine];
        // ロード処理.
        Match match_text;
        // ネームスペース更新.
        match_text = Regex.Match(_currentText, @"\[ns .*?\]");
        if (match_text.Success) {
            string name_text;
            string[] split_text;
            split_text = match_text.Value.Split(' ');
            name_text = split_text[1].Replace("]", "");
            _nameSpace.SetActive(true);
            _uiNameText.text = name_text;
        }
        // ネームスペース非表示
        match_text = Regex.Match(_currentText, @"\[delNs\]");
        if (match_text.Success) {
            _nameSpace.SetActive(false);
        }
    }

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

    private void SetParams(string script) {
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[param .*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                string[] split_text;
                split_text = match.Value.Split(' ');
                string cmd = split_text[1];
                string key = split_text[2].Replace("]", "");
                string str_value = "";
                if (split_text.Length > 3) {
                    str_value = split_text[3].Replace("]", "");
                }
                SetupDictParams(cmd, key, str_value);
            }
        }
    }

    public void SetupDictParams(string cmd, string key, string str_value) {
        switch (cmd) {
            case "set": {
                    if (!_scriptParams.ContainsKey(key)) {
                        _scriptParams.Add(key, str_value);
                    } else {
                        // nameがついているものはstringで設定するので、スルー
                        Match match;
                        match = Regex.Match(key, "name");
                        if (!match.Success) {
                            int value = int.Parse(str_value) + (int)_scriptParams[key];
                            _scriptParams.Add(key, value);
                        }
                    }
                    Debug.Log(String.Format("set params key = {0} value = {1}", key, _scriptParams[key]));
                }
                break;
            case "check": {
                    int value = int.Parse(str_value);
                    int checkValue = Convert.ToInt32(_scriptParams[key]);
                    bool isClear = false;
                    _isLoadScript = false;
                    if (_scriptParams.ContainsKey(key)) {
                        if (checkValue >= value) {
                            LoadNextCheckParam(1);
                            isClear = true;
                        }
                    }
                    if (!isClear) {
                        LoadNextCheckParam(2);
                    };
                    _isLoadScript = true;
                    Debug.Log(String.Format("check params key = {0} value = {1} border = {2} is_clear = {3}", key, _scriptParams[key], value, isClear));
                }
                break;
            case "reset": {
                    if (_scriptParams.ContainsKey(key)) {
                        _scriptParams.Remove(key);
                    }
                    Debug.Log("reset key = " + key);
                }
                break;
        }
    }

    public void LoadNextCheckParam(int num) {
        string script = _scenarios[_currentLine];
        string txt_check = @"\[test select " + num.ToString() + @"\]";
        while (!Regex.Match(script, txt_check).Success) {
            _currentLine++;
            // 選択先まで読み込み.
            script = _scenarios[_currentLine];
        }
        SetNextLine();
        _currentLine -= 1;
    }

    private void SetInput(string script) {
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[input set .*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                string[] split_text;
                split_text = match.Value.Split(' ');
                string key = split_text[2].Replace("]", "");
                _inputController.Setup(key, gameObject);
            }
        }
    }

    private void SetExplore(string script) {
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[explore set .*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                string[] split_text;
                split_text = match.Value.Split(' ');
                string path = split_text[2].Replace("]", "");

                exploreController.Setup(path);
                gameObject.SetActive(false);
            }
        }
    }

    private void SetChoice(string script) {
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[choice .*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                string[] split_text;
                split_text = match.Value.Split(' ');
                string type = split_text[1];
                string text;
                switch (type) {
                    case "set":
                        text = split_text[2].Replace("]", "");
                        _choiceController.Setup(text, this.gameObject);
                        break;
                    case "top":
                        text = split_text[2].Replace("]", "");
                        _choiceController.SetTopText(text);
                        break;
                    case "bottom":
                        text = split_text[2].Replace("]", "");
                        _choiceController.SetBottomText(text);
                        break;
                }
            }
        }
    }

    public void SelectChoice(int num) {
        string script = _scenarios[_currentLine];
        string txt_check = @"\[choice select " + num.ToString() + @"\]";
        while (!Regex.Match(script, txt_check).Success) {
            _currentLine++;
            // 選択先まで読み込み.
            script = _scenarios[_currentLine];
        }
        // 選択肢をリセット.
        _choiceController.ResetButton();
        // リロード.
        SetNextLine();
    }

    private void SkipScript(string script) {
        Match match_text;
        match_text = Regex.Match(script, @"\[go.*?\]");
        if (match_text.Success) {
            string[] split_text;
            split_text = match_text.Value.Split(' ');
            string anchor = split_text[1].Replace("]", "");
            string txt_check = @"\[to " + anchor + @"\]";
            while (!Regex.Match(script, txt_check).Success) {
                _currentLine++;
                script = _scenarios[_currentLine];
            }
        }
    }

    private void LoatNextScript(string script) {
        Match match_text;
        string path;
        match_text = Regex.Match(script, @"\[next .*?\]");
        if (match_text.Success) {
            string[] split_text;
            split_text = match_text.Value.Split(' ');
            path = split_text[1].Split('=')[1].Replace("]", "");
            ResetStatus();
            Setup(path, _moveType.ToString());
            playScript();
            _currentLine--;
        }
    }

    private void ResetStatus() {
        if (_panelChara.transform.childCount > 0) {
            _panelChara.transform.GetComponentsInChildren<Transform>().ToList().ForEach(trf => {
                Destroy(trf.gameObject);
            });
        }
        SoundManager.Instance.StopBgm().FadeOut();
        SoundManager.Instance.StopSe();
    }

    private void Playsound(string script) {
        Dictionary<string, string> param = new Dictionary<string, string>();
        bool isLoop;
        MatchCollection match_text;
        match_text = Regex.Matches(script, @"\[sound .*?\]");
        foreach (Match match in match_text) {
            if (match.Success) {
                param = ToDictList(match.Value);
                isLoop = false;
                if (param.ContainsKey("loop")) {
                    isLoop = param["loop"] == "1";
                }
                switch (param["cmd"]) {
                    case "bgm":
                        SoundManager.Instance.PlayBgm(param["name"]);
                        break;
                    case "se":
                        SoundManager.Instance.PlaySe(param["name"]);
                        break;
                    case "stop":
                        SoundManager.Instance.StopBgm().FadeOut();
                        break;
                }
            }
        }
    }

    private Dictionary<string, string> ToDictList(string splitText) {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        string[] split_text;
        splitText = splitText.Replace("]", "");
        split_text = splitText.Split(' ');
        // 0番目のコマンド以降から変換.
        for (var i = 1; i < split_text.Length; i++) {
            string[] split = split_text[i].Split('=');
            dict.Add(split[0], split[1]);
        }
        return dict;
    }

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
