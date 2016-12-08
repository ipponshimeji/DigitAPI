# Digit API のソース

このリポジトリは、
[Fujitsu Advent Calendar 2016](http://qiita.com/advent-calendar/2016/fujitsu) の8日目のネタである
「[Microsoft Cognitive Servicesがうらやましいのでパチもんを作ってみた](http://ipponshimeji.cocolog-nifty.com/blog/2016/12/microsoft-cogni.html)」 で説明されているサービス（Digit API）のソースです。
要するに、Microsoft Cognitive ServicesのEmotion APIのパロディみたいなもんです。

サービスは当面の間（2016/12/25頃まで）、
デモとして
[Digit API](http://digitapiweb.azurewebsites.net) で動かしておく予定です。


## このリポジトリの内容

### `Service` フォルダ

サービスのソースです。
Visual Studio 2015のソリューションです。
このソリューションには3個のプロジェクトが含まれています

*   DigitAPI: MNIST学習結果を用いて評価を行うライブラリです。
*   DigitAPIWeb: DigitAPIをサービス化する ASP.NET MVC プロジェクトです。
*   Command: DigitAPIをコマンドラインから動かすためのコマンドをビルドします。


### `Training` フォルダ 

上記 `Service` フォルダにあるソリューションは、
Microsoft Cognitive Toolkitで学習させたモデル（`Service/DigitAPI/NMIST.model`）が含まれています。
その学習済みモデルを生成するための資産が格納されています。
