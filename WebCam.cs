using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

// RealSenseがない環境でWebカメラに切り替えるプログラム
public class WebCam : MonoBehaviour
{
  [SerializeField] private int _width;
  [SerializeField] private int _height;
  [SerializeField] private int _fps;
  [SerializeField] private int cameraNum;
  private int selectCamera = 0;
  public int numPhoto = 0;

  // UI
  public WebCamTexture _webCamTexture;


  IEnumerator Init()
  {
    while (true)
    {
      if (_webCamTexture.width > 16 && _webCamTexture.height > 16) {
        break;
      }
      yield return null;
    }
  }

  // スタート時に呼ばれる
  void Start () 
  {
    if (WebCamTexture.devices.Length == 0) throw new System.Exception("Web Camera devices are not found");
    else if (WebCamTexture.devices.Length <= cameraNum) throw new System.Exception("specified camera " + cameraNum + " is not found");
    
    foreach (var device in WebCamTexture.devices)
    {
      Debug.Log(device.name);
    }

    // Webカメラの開始
    var webCamDevice = WebCamTexture.devices[cameraNum];
    this._webCamTexture = new WebCamTexture(webCamDevice.name, _width, _height, _fps);
    this._webCamTexture.Play();

    StartCoroutine(Init());
  }
  
  void Update()
  {
    // if (ft != null && _webCamTexture.isPlaying) {
    //   _webCamTexture.Pause();
    // }
    // else if (!_webCamTexture.isPlaying) {
    //   _webCamTexture.Play();
    // }
  }

  public void ChangeCamera()
  {
    WebCamDevice[] webCamDevice = WebCamTexture.devices;

    // カメラが1個の時は無処理
    if (webCamDevice.Length <= 2) return;

    this._webCamTexture.Stop();
    // カメラの切り替え
    while (true) {
      selectCamera++;
      if (selectCamera >= webCamDevice.Length) selectCamera = 0;
      this._webCamTexture = new WebCamTexture(webCamDevice[selectCamera].name, _width, _height, _fps);
      if(!this._webCamTexture.isPlaying) break;
    }
    this._webCamTexture.Play();
  }

  // カメラ画像をjpgで保存
  public void TakeShot()
  {
    // Texture2Dを新規作成
    Texture2D texture = new Texture2D(_webCamTexture.width, _webCamTexture.height, TextureFormat.ARGB32, false);
    // カメラのピクセルデータを設定
    texture.SetPixels(_webCamTexture.GetPixels());
    // TextureをApply
    texture.Apply();

    // Encode
    byte[] bin = texture.EncodeToJPG();
    // Encodeが終わったら削除
    Object.Destroy(texture);

    // 書き出し
    File.WriteAllBytes(Application.dataPath + "/" + numPhoto.ToString() + ".jpg", bin);

    numPhoto++;
  }
}