using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;  // DoTween.
using System.Collections;

public class BlinkController : MonoBehaviour {
    [SerializeField]
    public RectTransform _topImage;
    [SerializeField]
    public RectTransform _bottomImage;
    [SerializeField]
    public Image _materialPanel;    // 画面エフェクト用.

    private bool isAnime = false;

    public Blink _blink { get; set; }

    public class Blink {
        public float openTime;
        public float closeTime;
        public float moveTopY;
        public float moveBottomY;
        public Material material;
        public float matrialValue;
        public Blink(float oTime, float cTime, float topY, float bottomY, Material mat, float value) {
            openTime = oTime;
            closeTime = cTime;
            moveTopY = topY;
            moveBottomY = bottomY;
            material = mat;
            matrialValue = value;
        }
    }

    // Start is called before the first frame update
    public  void Setup(Blink blink)
    {
        _blink = blink;
        _materialPanel.material = _blink.material;
        _materialPanel.material.SetFloat("_Blur", 0);
    }

    private void Start() {
        isAnime = false;
    }

    private void Update() {
        if (Input.GetKey(KeyCode.Space) && !isAnime) {
            PlayBlinkAnimation();
        }
    }

    public void PlayBlinkAnimation()
    {
        isAnime = true;
        StartCoroutine(blinkAnimation());
    }

    private IEnumerator blinkAnimation() {
        float blur = 0;
        Sequence seq = DOTween.Sequence();
        Vector3 topPos, bottomPos, moveTopPos, moveBottomPos;

        moveTopPos = new Vector3(0, _blink.moveTopY, 1);
        moveBottomPos = new Vector3(0, _blink.moveBottomY, 1);

        topPos = _topImage.localPosition;
        bottomPos = _bottomImage.localPosition;

        if (_blink.closeTime >= 0) {
            _topImage.DOLocalMove(moveTopPos, _blink.closeTime);
            _bottomImage.DOLocalMove(moveBottomPos, _blink.closeTime);
            _materialPanel.material.DOFloat(_blink.matrialValue, "_Blur", _blink.closeTime);
            yield return new WaitForSeconds(_blink.closeTime);
        }

        if (_blink.openTime >= 0) {
            _topImage.DOLocalMove(topPos, _blink.openTime);
            _bottomImage.DOLocalMove(bottomPos, _blink.openTime);
            _materialPanel.material.DOFloat(0, "_Blur", _blink.openTime);
            yield return new WaitForSeconds(_blink.openTime);
        }

        isAnime = false;
    }
}
