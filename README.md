# WSL Forward

Windows 11 上で WSL2 の SSH サーバーへ外部から接続できるようにするためのポート転送ツール。

WSL2 の IP アドレスは再起動のたびに変わるため、Windows のポート転送設定を定期的に差分更新し、Firewall の受信許可ルールとタスクスケジューラの登録もまとめて管理する。

## 前提

- Windows 11
- WSL2 インストール済み、systemd 有効
- WSL2 側で sshd が起動済み (デフォルト: TCP 22)
- .NET 8 SDK (ビルド時のみ)

## ビルド

[Task](https://taskfile.dev/) を使用:

```
task publish
```

`publish/WslForward.exe` が生成される。

## セットアップ

管理者権限のターミナルで:

```
WslForward.exe install
```

これにより以下が実行される:

1. Firewall の受信許可ルール作成
2. タスクスケジューラへのタスク登録 (ログイン時 + 10 分間隔)
3. WSL 起動 + ポート転送の初回設定

## コマンド一覧

| コマンド | 説明 | 管理者権限 |
|---|---|---|
| `init-config` | デフォルト値の config.json を作成する | |
| `install` | Firewall + タスク登録 + ポート転送を一括セットアップする | 必要 |
| `install-firewall` | Firewall の受信許可ルールを作成する | 必要 |
| `install-tasks` | タスクスケジューラにタスクを登録する | 必要 |
| `show-firewall` | Firewall ルールを表示する | |
| `show-tasks` | タスクスケジューラの登録状況を表示する | |
| `show-forward` | ポート転送設定を表示する | |
| `show-wsl` | WSL ディストリビューションの状態を表示する | |
| `start-wsl` | WSL ディストリビューションを起動する | |
| `update` | WSL を起動し、ポート転送を差分更新する | 必要 |
| `uninstall-firewall` | Firewall ルールを削除する | 必要 |
| `uninstall-tasks` | タスクスケジューラのタスクを削除する | |
| `uninstall-forward` | ポート転送設定を削除する | 必要 |
| `uninstall` | 全設定を一括削除する | 必要 |
| `help` | ヘルプを表示する | |

## 設定

exe と同じディレクトリに `config.json` を配置する。`init-config` コマンドでデフォルト値のファイルを生成できる。

```json
{
  "distroName": "Ubuntu",
  "listenAddress": "0.0.0.0",
  "listenPort": 22,
  "wslPort": 22,
  "taskFolder": "WSL Forward"
}
```

| キー | 説明 | デフォルト |
|---|---|---|
| `distroName` | 対象 WSL ディストリビューション名 | `Ubuntu` |
| `listenAddress` | ポート転送の待受アドレス | `0.0.0.0` |
| `listenPort` | ポート転送の待受ポート | `22` |
| `wslPort` | WSL 側の接続先ポート | `22` |
| `taskFolder` | タスクスケジューラのフォルダ名。空文字でフォルダなし | `WSL Forward` |

config.json がない場合はすべてデフォルト値で動作する。

## アンインストール

管理者権限のターミナルで:

```
WslForward.exe uninstall
```

ポート転送設定、Firewall ルール、タスクスケジューラのタスクがすべて削除される。

## 仕組み

- タスクスケジューラが `WslForward.exe update` をログイン時と 10 分おきに実行する
- `update` は WSL を起動し、現在の IPv4 アドレスを取得してポート転送を差分更新する
- exe は WinExe (GUI サブシステム) としてビルドされるため、タスクスケジューラからの実行時にウィンドウが表示されない
- Firewall は COM interop (`HNetCfg.FwPolicy2`) で直接操作する
- タスクスケジューラは COM interop (`Schedule.Service`) で直接操作する
- ポート転送は `netsh interface portproxy` 経由で操作する
- WSL は `wsl.exe` 経由で操作する
