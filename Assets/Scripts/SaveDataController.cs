using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Collections.Generic;

public class SaveDataController : MonoBehaviour {
    //　Saveボタン
    public Button _btnSave;
    // Loadボタン
    public Button _btnLoad;
    // テキストボックス.
    public Text _txtScript;
    // テキストボックス.
    private AdvController _advController;

    //　ファイルストリーム
    private FileStream fileStream;
    //　バイナリフォーマッター
    private BinaryFormatter bf;
    // ローカルデータ管理
    private Data _data;
    private string _loadKey = "loadText";       // ロードテキスト
    private string _countKey = "countText";     // カウントテキスト

    public void Setup(AdvController advController, int saveFileCount) {
        _data = new Data();
        _advController = advController;
        Load(saveFileCount, true);
        // ボタン設定.
        _btnSave.onClick.RemoveAllListeners();
        _btnSave.onClick.AddListener(() => {
            Save(_advController._scriptParams, saveFileCount);
        });
        _btnLoad.onClick.RemoveAllListeners();
        _btnLoad.onClick.AddListener(() => {
            Load(saveFileCount);
        });
    }

    public void Save(Dictionary<string, object> scriptParams, int saveFileCount) {
        Debug.Log("data save");
        bf = new BinaryFormatter();
        fileStream = null;

        try {
            //　ゲームフォルダにfiledata.datファイルを作成
            fileStream = File.Create(Application.dataPath + "/filedata" + saveFileCount + ".dat");

            //　クラスの作成
            string dataText = "";
            if (scriptParams.ContainsKey(_loadKey)) {
                dataText = scriptParams[_loadKey].ToString();
            }
            Data data = new Data();
            _txtScript.text = dataText;
            data.scriptParams = scriptParams;

            //　ファイルにクラスを保存
            bf.Serialize(fileStream, data);
        } catch (IOException e1) {
            Debug.Log("ファイルオープンエラー : " + e1.ToString());
        } finally {
            if (fileStream != null) {
                fileStream.Close();
            }
        }
    }

    public void Load(int saveFileCount, bool isFirst = false) {
        Debug.Log("data load");
        bf = new BinaryFormatter();
        fileStream = null;

        try {
            //　ファイルを読み込む
            fileStream = File.Open(Application.dataPath + "/filedata" + saveFileCount + ".dat", FileMode.Open);
            //　読み込んだデータをデシリアライズ
            Data data = bf.Deserialize(fileStream) as Data;
            var param = data.scriptParams;
            string dataText = "セーブデータはありません";
            if (param.ContainsKey(_countKey)) {
                for (int i = 0; i < (int)param[_countKey]; i++) {
                    _advController.SetNextLine();
                }
            }
            if (param.ContainsKey(_loadKey)) {
                dataText = param[_loadKey].ToString();
            }
            _txtScript.text = dataText;
            // 初回でないならデータを差し替える.
            if (!isFirst) {
                _advController._scriptParams = param;
            }
        } catch (FileNotFoundException e1) {
            Debug.Log("ファイルがありません : " + e1.ToString());
        } catch (IOException e2) {
            Debug.Log("ファイルオープンエラー : " + e2.ToString());
        } finally {
            if (fileStream != null) {
                fileStream.Close();
            }
        }

    }

    //　保存するデータクラス
    [Serializable]
    public class Data {
        // 表示テキスト
        public string dataText;
        // 進捗パラメータ
        public Dictionary<string, object> scriptParams;
    }
}

