using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChoiceButton : Button {
    public Text _txtChoice;
    public Image _imgChoice;
    public AdvController _controller;

    private string choiceColor = "#e6e6f0";
    private string freeColor = "#9595a4";

    public enum State {
        None = 0,
        Check,
        Select,
    };
    public State state = State.None;

    public void Setup(UnityAction action, string text, int choice, GameObject advObj)
    {
        _txtChoice.text = text;
        _controller = advObj.GetComponent<AdvController>();
        onClick.RemoveAllListeners();
        onClick.AddListener(() => {
            changeState(action, choice);
        });
    }

    private void changeState(UnityAction action, int choice) {
        if (state == State.Check) {
            state = State.Select;
            image.rectTransform.localScale = new Vector3(1.0f, 1.0f);
            _txtChoice.color = ToColor(freeColor);
            _txtChoice.fontSize = 26;
            _imgChoice.gameObject.SetActive(false);
            PlayADV(choice);
        } else if (state == State.None) {
            action();
            image.rectTransform.localScale = new Vector3(1.3f, 1.3f);
            _txtChoice.color = ToColor(choiceColor);
            _txtChoice.fontSize = 28;
            _imgChoice.gameObject.SetActive(true);
            state = State.Check;
        }
    }

    public void PlayADV(int choice) {
        // ADVコントローラーの選択移動を呼び出し.TODO
        _controller.SelectChoice(choice);
        var choiceParent = transform.parent;
        foreach (Transform trf in choiceParent.GetComponentInChildren<Transform>()) {
            Destroy(trf.gameObject);
        }
        var parent = choiceParent.parent.gameObject;
        parent.SetActive(false);
    }

    public void resetState() {
        state = State.None;
        image.rectTransform.localScale = new Vector3(1.0f, 1.0f);
        _txtChoice.color = ToColor(freeColor);
        _txtChoice.fontSize = 26;
        _imgChoice.gameObject.SetActive(false);
    }

    private Color ToColor(string htmlColor) {
        Color color = default(Color);
        if (!ColorUtility.TryParseHtmlString(htmlColor, out color)) {
            Debug.LogError("[ERROR][" + htmlColor + "]\n" +
                "カラーコードの指定が間違えています。\n" +
                "#から始まる6桁もしくは8桁の16進数にで指定してください。");
        }
        return color;
    }
}
