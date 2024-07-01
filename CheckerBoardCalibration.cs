// チェッカーボードとカメラの位置関係を取得するプログラム

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;

namespace OpenCvSharp
{
  public class CheckerBoardCalibration : MonoBehaviour
  {
    // チェッカーボードの情報
    private const int PAT_HEIGHT = 7; // パターンの行数
    private const int PAT_WIDTH = 10; // パターンの列数
    public float CHESS_SIZE;// パターン1マスの1辺サイズ[mm]

    // 画像のサイズ
    public Size matSize = new Size(640, 360);  


    // WEBカメラ
    [SerializeField] private WebCam _webCam;

    // カメラキャリブレーションに用いる画像
    [SerializeField] private Texture2D[] images; 


    // カメラ特性
    private double[,] cameraMatrix = new double[3,3]; // 内部パラメータ
    private double[] distCoeffs = new double[8];      // 外部パラメータ
    public Vector3 pos, rot;

    private Size pattern_size = new Size(PAT_WIDTH, PAT_HEIGHT); // チェッカーボードの縦×横
    private Point2f[] corners;                                          // 画像上のチェッカーボードのコーナーの位置
    private List<List<Point3f>> obj_points = new List<List<Point3f>>(); // チェッカーボードのコーナーの位置
    private RawImage _rawImage;
    private Texture2D tex;
    public bool found = false;

   
    void Start()
    { 
      // Webカメラをテクスチャに設定
      _rawImage = GetComponent<RawImage>();
      StartCoroutine(Init());
      _rawImage.texture = _webCam._webCamTexture;

      // カメラキャリブレーション
      CalibrateCamera();
    }


    void Update()
    {
      GetDistance();
    }

     // WEBカメラの準備ができるまで待機する関数
    IEnumerator Init()
    {
      while(true) {
        if (_webCam == null) break;
        yield return null;
      }
      while (true) {
        if (_webCam._webCamTexture.width > 16 && _webCam._webCamTexture.height > 16) break;
        yield return null;
      }
    }

    void GetDistance()
    {
      // カメラ映像を読み込み
      var mat = OpenCvSharp.Unity.TextureToMat(_webCam._webCamTexture);  // webカメラの映像をOpenCVで加工可能な形式に変換
      if (mat.Empty()) throw new System.Exception("fail to convert WebCamTexture to Mat");
      Cv2.Resize(mat, mat, matSize);  // 画像サイズを統一

      // チェッカーボードを検出
      found = Cv2.FindChessboardCorners(mat, pattern_size, out corners, ChessboardFlags.FastCheck);

      Destroy(tex); // テクスチャのメモリ開放
      // チェッカーボードが見つかった場合
      if (found) {
        // サブピクセルを用いてより正確な値を検出
        // Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);        // 画像をモノトーンに変換
        // Cv2.Find4QuadCornerSubpix(mat, corners, new Size(3, 3));		  // サブピクセルでもう一度コーナーを探す

        // 検出結果をUnityの画面上に表示
        Cv2.DrawChessboardCorners(mat, pattern_size, corners, found);	// チェッカーボードのコーナーを描画
        tex = OpenCvSharp.Unity.MatToTexture(mat);
        _rawImage.texture = tex;

        // 外部パラメータを取得（rvec, tvecがカメラとチェッカーボードの位置関係）
        var rvec = new double[] {0, 0, 0};
        var tvec = new double[] {0, 0, 0};
        Cv2.SolvePnP(obj_points[0], corners, cameraMatrix, distCoeffs, out rvec, out tvec);
        convertPosCV2Unity(rvec, tvec, out rot, out pos);
      }
      else _rawImage.texture = _webCam._webCamTexture;  // チェッカーボードが見つからない場合はそのままカメラ映像を出力      

      mat.Dispose();
    }

    // カメラキャリブレーション
    void CalibrateCamera()
    {
      // チェッカーボードのコーナーの位置を記録
      var img_points = new List<Point2f[]>();   // 写真上のチェッカーボードの位置を記録する配列

      // 各画像でチェッカーボードの位置を取得
      for (int i = 1; i <= images.Length; i++) {
        // 画像を読み込み
        var mat = OpenCvSharp.Unity.TextureToMat(images[i - 1]);
        if (mat.Empty()) throw new System.Exception("fail to read file: " + i.ToString() + ".jpg");
        Cv2.Resize(mat, mat, matSize);    // 画像サイズを統一

        // チェッカーボードのコーナーの検出
        var found = Cv2.FindChessboardCorners(mat, pattern_size, out corners);

        if (found) {
          // より正確に検出
          Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);    // グレースケールに変換
          Cv2.Find4QuadCornerSubpix(mat,  corners, new Size(3, 3));		// サブピクセルでもう一度コーナーを探す

          // 検出したコーナーを記録
          img_points.Add(corners);
        }
        else print("Chessboard is not found in " + i.ToString());

        mat.Dispose();
      }
      Vec3d[] rvecs, tvecs; // 回転ベクトル, 並進ベクトル

      obj_points = setObjPoints(img_points.Count); // チェッカーボードの大きさやサイズを配列に記録
      // カメラキャリブレーション
      Cv2.CalibrateCamera(obj_points, img_points, matSize, cameraMatrix, distCoeffs, out rvecs, out tvecs);
    }

    // チェッカーボードのコーナーの位置を表す配列を作る関数
    private List<List<Point3f>> setObjPoints(int size)
    {
      List<List<Point3f>> objPoints = new List<List<Point3f>>();

      var temp = new List<Point3f>();
      for (int h = 0; h < PAT_HEIGHT; h++) {
        for (int w = 0; w < PAT_WIDTH; w++) {
          temp.Add(new Point3f(w * CHESS_SIZE, h * CHESS_SIZE, 0));
        }
      }

      for (int i = 0; i < size; i++) {
        objPoints.Add(temp);
      }
      return objPoints;
    }

    // OpenCVのrvec, tvecをそれぞれUnityで使えるQuaternion, Vector3型に変換
    void convertPosCV2Unity(double[] rvec, double[] tvec, out Vector3 rot, out Vector3 pos) {
      // 回転ベクトルrvecを、クォータニオンに変換
      var temp = new Vector3((float)-rvec[0], (float)-rvec[1], (float)-rvec[2]);
      var qua = Quaternion.AngleAxis(temp.magnitude * 180f / (float)Math.PI, temp);
      rot = qua.eulerAngles;

      // 並進ベクトルtvecをUnityで使える形に変換
      var xDir = qua * Vector3.right;
      var yDir = qua * Vector3.up;
      var zDir = qua * Vector3.forward;
      pos =  xDir * (float)tvec[0];
      pos += yDir * (float)tvec[1];
      pos += zDir * (float)tvec[2];
      pos *= 0.001f;
    }
  }
}