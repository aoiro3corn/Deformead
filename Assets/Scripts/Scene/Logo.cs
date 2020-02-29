using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logo : MonoBehaviour
{
    public string playSe = "se_logo";
    private void Start() {
        FadeManager.FadeIn();
        SoundManager.Instance.PlaySe(playSe);
    }

    private void Update() {
        if (!SoundManager.Instance.IsSePlaying(playSe))
            FadeManager.FadeOut("Title");
    }
}
