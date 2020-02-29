using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    public GameObject prefabAdv;
    public GameObject prefabExplore;
    public GameObject controllerParent;

    private AdvController advController;
    private AdvController advControllerOnExplore;   // 脱出パートに乗せるADV
    private ExploreController exploreController;

    // Start is called before the first frame update
    private void Start() {
        // FadeManager
        FadeManager.FadeIn();

        // iniファイルのロード.
#if false
        AdvSettiongController iniFile = new AdvSettiongController("adv_setting.ini");
        string path = iniFile["setting", "path"];
        string moveType = iniFile["setting", "type"];
#else
        string path = "ADV/Scripts/script_001";
        string moveType = AdvController.MoveType.ALL.ToString();
#endif
        // prefabのInstantiate.
        GameObject adv = Instantiate(prefabAdv, controllerParent.transform);
        advController = adv.GetComponent<AdvController>();
        GameObject explore = Instantiate(prefabExplore, controllerParent.transform);
        exploreController = explore.GetComponent<ExploreController>();
        GameObject advOnExplore = Instantiate(prefabAdv, controllerParent.transform);
        advControllerOnExplore = advOnExplore.GetComponent<AdvController>();

        // setup.
        advController.Setup(path, moveType);
        advController.exploreController = exploreController;
        exploreController.advController = advController;
        exploreController.onAdvController = advControllerOnExplore;

        // start.
        advController.playScript();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
