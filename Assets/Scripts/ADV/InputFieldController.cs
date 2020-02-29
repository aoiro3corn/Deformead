using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InputFieldController : MonoBehaviour {
    [SerializeField]
    InputField _inputField;
    [SerializeField]
    Text _txtCheck;
    [SerializeField]
    GameObject _panelCheckText;
    [SerializeField]
    Button _btnSubmit;

    /// <summary>
    /// Startメソッド
    /// InputFieldコンポーネントの取得および初期化メソッドの実行
    /// </summary>
    public void Setup(string key, GameObject advObj) {
        gameObject.SetActive(true);
        InitInputField();
        var controller = advObj.GetComponent<AdvController>();
        _btnSubmit.onClick.RemoveAllListeners();
        _btnSubmit.onClick.AddListener(() => {
            // ADVコントローラーの選択移動を呼び出し.TODO
            if (!_panelCheckText.gameObject.activeInHierarchy) {
                InputLogger();
                _inputField.gameObject.SetActive(false);
                _panelCheckText.gameObject.SetActive(true);
            } else {
                controller.SetupDictParams("set", key, _inputField.text);
                _panelCheckText.gameObject.SetActive(false);
                _inputField.gameObject.SetActive(true);
                gameObject.SetActive(false);
                controller.SetNextLine();
            }
        });
    }

    /// <summary>
    /// Log出力用メソッド
    /// 入力値を取得してLogに出力し、初期化
    /// </summary>
    public void InputLogger() {
        string inputValue = _inputField.text;
        _txtCheck.text = inputValue;
    }

    /// <summary>
    /// InputFieldの初期化用メソッド
    /// 入力値をリセットして、フィールドにフォーカスする
    /// </summary>
    private void InitInputField() {
        // 値をリセット
        _inputField.text = "";
        // フォーカス
        _inputField.ActivateInputField();
    }
}