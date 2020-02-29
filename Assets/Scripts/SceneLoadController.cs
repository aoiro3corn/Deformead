using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadController : MonoBehaviour {
    public Scene activeScene;
    public GameObject activeObject;

    public bool CanLoadScene(string sceneName) {
        return Enumerable
            .Range(0, SceneManager.sceneCountInBuildSettings)
            .Select(c => SceneUtility.GetScenePathByBuildIndex(c))
            .Select(c => Path.GetFileNameWithoutExtension(c))
            .Any(c => c == sceneName)
        ;
    }

    public IEnumerator LoadSceneName(string sceneName) {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        Scene scene = SceneManager.GetSceneByName(sceneName);
        while (!scene.isLoaded) {
            yield return null;
        }
        if (SceneManager.SetActiveScene(scene)) {
            activeScene = scene;
        }
        activeObject = GameObject.FindGameObjectWithTag(sceneName);
        yield return activeObject;
    }
}
