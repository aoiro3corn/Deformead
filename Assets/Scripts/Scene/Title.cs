using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour {
    // Start is called before the first frame update
    private void Start() {
        // FadeManager
        FadeManager.FadeIn();

        // BGMの再生.
        SoundManager.Instance.PlayBgm("bgm_title");
    }

    // Update is called once per frame
    private void Update() {
        
    }

    public void onClickStart() {
        // SEの再生.
        SoundManager.Instance.PlaySe("se_door_open");
        FadeManager.FadeOut("Main");
    }
}
