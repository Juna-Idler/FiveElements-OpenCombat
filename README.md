# FiveElementsOpenCombat

Unityの習作としてOpenCombatを組んでみた。
五行説を元にした属性相性を取り入れたが、うーんどうだろう？


Font:しっぽりアンチック
SIL Open Font License Version 1.1

## 謎の設定

命を懸けた戦いではなく、紳士的な対戦競技。
対戦者は互いに相手の攻撃を一回無効にできるバリアを張っている。
攻撃を受けた後はカードを一枚使ってバリアを再展開。
ゆえに手札＋デッキがライフ。
同時に手札がなくなったときはバリアの有無で判定。おなじなら引き分け。


直前に使った自分の攻撃の残り香？が次の攻撃に影響を与える。
相手の攻撃との相性も当然影響する。


実力差があっても二回連続攻撃で直接攻撃してはいけません。
自分のバリアがない状態で攻撃するのも違反です。


あとは、なんで手札枚数に制限があるのかと、なんで相手に手札を見せているのか、を説明したいところ。


攻撃に使用するカードは起動状態しておく必要があり、その状態を維持するのに数に制限がある。
また起動状態のカードは互いに感知できるので相手の手の内が分かる。


もう一つ、どちらも後出しを狙わず、同時に攻撃をする理由が欲しいか？



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


