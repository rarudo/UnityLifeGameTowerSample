# UnityLifeGameTowerSample

Unityでライフゲームタワーのサンプルプロジェクトです

[解説記事](https://technote.qualiarts.jp/article/56)

<img width="400" src="https://github.com/rarudo/UnityLifeGameTowerSample/assets/15700036/8fb9d7db-5627-4303-ae35-9f7904aaa774">
<img width="400" src="https://github.com/rarudo/UnityLifeGameTowerSample/assets/15700036/34fdd70c-50e9-4953-b04c-c27dd51c05a0">
<img width="400" src="https://github.com/rarudo/UnityLifeGameTowerSample/assets/15700036/9b80ded2-1de7-4880-a931-6974f6df4a99">
<img width="400" src="https://github.com/rarudo/UnityLifeGameTowerSample/assets/15700036/0d1b729d-3c3c-40aa-af98-57a19ed44de6">
<img width="800" src="https://github.com/rarudo/UnityLifeGameTowerSample/assets/15700036/f3840f94-6919-43b6-ab23-0c7856d9a577">


## 使い方
LifeGameTowerCubeSampleシーンを開きUnityを実行すると、タワーが生成されます
![gif_drawmeshinstancedproceduralhuge](https://github.com/rarudo/UnityLifeGameTowerSample/assets/15700036/7741c65c-7ae6-489c-8d12-b494ebad41f0)

## Life Game Behaviourのパラメーター説明
|パラメーター            | 説明                                                                   |
| ------------- | -------------------------------------------------------------------- |
| Size          | ライフゲームの生成範囲指定  (値が大きいと処理が重くなる)             |
| Render Type   | Cubeの配置法を選択 (初期設定のCombinedMeshが最も高速)                |
| Use Random     | 初期配置をランダム配置するか (チェックを外すとInitialDataが使われる) |
| Warm Up Frame | タワーの生成を開始するフレーム                                       |
| Mesh          | タワーを構成するメッシュ                                             |
| Material      | タワーを構成するマテリアル                                           |
| Initial Width | 初期配置のEditor拡張用のWidth                                        |
| Initial Data  | Use Randomのチェックをつけていない場合、ここで指定した配置が初期配置になる                                                                     |
