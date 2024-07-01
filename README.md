# 前置き
ポートフォリオとして利用しているリポジトリです。
そのため、一般的な利用はあまり想定しておりません。

# 概要
Unityのwebカメラと、カメラに写った7×10のチェッカーボードとでチェッカーボードキャリブレーションを行うことで、カメラとチェッカーボードの位置、角度の関係を求めるプログラムです。
OpenCV plus Unityの導入を前提としています。

Unity 2021.3.20f1, OpenCV plus Unity Version 1.7.1で動作確認済み。

# 仕様技術
Unity, C#, OpenCV

# 利用方法
1. OpenCV plus Unityを導入する
2. Project Settings → Player → Allow 'unsafe' Codeに☑を入れる
3. 当スクリプトをインポートする
4. CheckerBoardCalibration.cs, WebCam.csをそれぞれ別の空のオブジェクトに割り当てる
5. コンポーネントCheckerBoardCalibration の 変数Web Cam に コンポーネントWebCam を割り当てたオブジェクトを割り当てる
6. コンポーネントCheckerBoardCalibration の 変数CEHSS_SIZE に使用するチェッカーボードの1マスの長さ(mm)を入れる
7. コンポーネントCheckerBoardCalibration の 変数Mat Size の縦横比を、カメラの比率と等しくする
8. コンポーネントCheckerBoardCalibration の 変数Images に、利用するウェブカメラで撮影したチェッカーボードの画像を複数枚（最低10枚ほど）追加する
9. ビルドする
10. Webカメラでチェッカーボードを撮影する

この際、コンポーネントCheckerBoardCalibration の 変数Pos, Rot に表示されるのが、それぞれ位置、回転です。