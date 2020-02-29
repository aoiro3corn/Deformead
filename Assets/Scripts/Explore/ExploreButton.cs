using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;

public class ExploreButton : Button {
    public GameObject objectCheck;
    public AdvController controller;

    public enum State {
        None = 0,
        Check,
        Select,
    };
    public State state = State.None;
    public string script = "ADV/Scripts/sample_explore_script_01";

    public void Setup(UnityAction action, AdvController advController,  string fileName) {
        script = fileName;
        controller = advController;
        onClick.RemoveAllListeners();
        onClick.AddListener(() => {
            changeState(action);
        });
    }

    private void changeState(UnityAction action) {
        objectCheck.SetActive(false);
        if (state == State.Check) {
            state = State.Select;
            controller.Setup(script);
            controller.playScript();
        } else if (state == State.None) {
            action();
            state = State.Check;
            objectCheck.SetActive(true);
        }
    }
    
    public void playADV() {
        if (state == State.Select) {
            controller.Setup(script);
            controller.playScript();

            state = State.None;
        }
    }

    public void resetState() {
        state = State.None;
        objectCheck.SetActive(false);
    }
}