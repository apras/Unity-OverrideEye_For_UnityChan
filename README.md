# Unity-OverrideEye_For_UnityChan
This is the script and shader to render the eye in front of the hair at Unity5 for UnityChan.  
これは、UnityChan用のUnity5にて眼などを髪の毛の手前にレンダリングするスクリプト及びシェーダーです。

![Sample](https://pbs.twimg.com/media/CNIfNKgVEAIm-ds.png:large)
<https://twitter.com/apras60/status/635604810398433280/photo/1>

Demo(WebGL)  
<http://www.apras.net/contents/data_unity/Unity-OverrideEye/index.html>

## How to use
* 手前に描画したい GameObject に、OverrideEye コンポーネントを追加。
 + **Type** から描画タイプを選択
    - **OVERRIDE** 手間に描画
    - **MASK** 手前に描画する対象をマスク
    - **CLIP** 描画する領域
 + **Priority** を設定 数値の小さいものから描画されます

  
* シーン内のカメラに RenderOverrideEye コンポーネントを追加。
 + **Blend Alpha** で手前に描画される対象の透明度を設定

## Description (Assets\OverrideEye\SampleScenes\Scene01.unity)
* SD_unitychan_humanoidFaceFix
 + OverrideEye OVERRIDE
    - _whiteEye
    - _eye
    - _eyebrows
    - _eyelashes
 + OverrideEye MASK
    - _face
    - _faceCap
 + OverrideEye CLIP
    - _Fhair
    - _Fhair2
