# FiveElementsOpenCombat

Unityの習作としてOpenCombatを組んでみた。
五行説を元にした属性相性を取り入れたが、うーんどうだろう？


Font:しっぽりアンチック
SIL Open Font License Version 1.1


## 開発日記
ClientWebSocket.ReceiveAsyncを無限ループで回すことにしたが、こいつの終了判定が判明するまでめちゃくちゃ迷走しまくった。
最初はキャンセルしたり例外をcatchしてみたり、なんとなくは動くけどエラーが消えなかった。
いやまさかWebSocketReceiveResultで判定するとはな。
CloseStatusだけあって「なんで？」とおもったら
>MessageType：現在のメッセージが UTF-8 メッセージであるか、バイナリ メッセージであるかを示します。
噓書くなや、三状態じゃねーかよ
>Close	2：終了メッセージを受信したため受信が完了しました。


WebSocketの変遷

1. 自前処理がなんとかできた
WebGLに対応したい
2. https://github.com/jirihybek/unity-websocket-webgl にたどり着く
文字で送信されてると無視されて読めないのを修正する。websocket-sharpが要求される（まあ別にいいっちゃ良いんだが）
3. https://github.com/endel/NativeWebSocket なんてのもあった
WebGL以外は適当なメインスレッドでDispatchMessageQueueを呼ばないとOnMessageを飛ばさないという一貫性のない実装
とりあえずNativeWebSocketのReceiveAsyncループ内でsynchronizationContext.Postを使ってDispatchMessageQueueすることに

4. なんとなくPUN2を用いたマスタークライアントで実装してみたが、めちゃくちゃ疲れた。
カスタムプロパティとルームコールバックでいろいろやってみたが、更新タイミングが意味不明でいろいろめんどくさくなったので
結局、IOnEventCallback.OnEventとPhotonNetwork.RaiseEventでゴリ押すことに。流すデータのシリアライズもJSONでゴリ押し。
サーバとクライアントがくっ付いてる構造がややこしいし、デバッグも面倒だしでほんと疲れた。

