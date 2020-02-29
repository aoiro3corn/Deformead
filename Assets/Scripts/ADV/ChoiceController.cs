using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class ChoiceController : MonoBehaviour {
    [SerializeField]
    public ChoiceButton _btnChoice;
    [SerializeField]
    public Transform _trfChoiceArea;
    [SerializeField]
    public Text _topText;
    [SerializeField]
    public Text _bottomText;

    private List<ChoiceButton> _btnList = new List<ChoiceButton>();

    // Start is called before the first frame update
    public void Setup(string txt_choice, GameObject advObj) {
        gameObject.SetActive(true);
        ChoiceButton obj = Instantiate(_btnChoice);
        obj.transform.SetParent(_trfChoiceArea);
        obj.Setup(resetState, txt_choice, _trfChoiceArea.childCount, advObj);
        _btnList.Add(obj);
    }

    public void SetTopText(string text) {
        _topText.text = text;
    }

    public void SetBottomText(string text) {
        _bottomText.text = text;
    }

    private void resetState() {
        _btnList.ToList().ForEach(btn => {
            btn.resetState();
        });
    }

    public void ResetButton() {
        _btnList.ForEach(obj => { Destroy(obj); });
    }
}
