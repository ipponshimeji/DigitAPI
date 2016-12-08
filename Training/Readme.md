# Digit APIで利用している学習モデル（NMIST.model）を生成するためのスクリプト

## このフォルダの内容は

Digit APIの実装で用いているCNTKの学習モデル（NMIST.model）を生成するための資産です。
`Service/DigitAPI/MNIST.model` が生成したバイナリになります。

基本的に、
[Microsoft Cognitive Toolkit](https://www.microsoft.com/en-us/research/product/cognitive-toolkit/)
のチュートリアル
[GettingStarted](https://github.com/Microsoft/CNTK/tree/master/Examples/Image/GettingStarted)
に記述されている `03_OneConvDropout.cntk` を使った学習方法と同じです。
ただし、少し変更を行っており、その変更点について後述します。
フォルダに格納してある `03_OneConvDropout.cntk` は変更後のものです。


## スクリプトの変更点

変更点の詳細は、オリジナルのものとDiffを取ってみてください。

*   最終出力にSoftmaxをかけています。これは出力をクラス分けではなく、各ノードの「確率」値とするためです。旧: `outputNodes = (ol)`  → 新: `outputNodes = (Softmax(ol))`
*   入力データフォルダを `../DataSets/MNIST` から `./DataSets/` に変更しています。
*   出力ファイル（学習済みモデル）を `./Models/03_OneConvDropout` から `./Models/MNIST.model` に変更しています。


## 学習のさせ方

のチュートリアル
[GettingStarted](https://github.com/Microsoft/CNTK/tree/master/Examples/Image/GettingStarted)
に記述されている `03_OneConvDropout.cntk` を使った学習方法と同じです。

ただし、入力データファイル（上のページの説明にあるように、スクリプト 'install_mnist.py' によって取ってきたもの）の置き場所を
同一フォルダの下に変更してある点に注意してください。

