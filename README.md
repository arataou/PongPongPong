# 碰碰球 · PongPongPong

一款 Unity 2D 本地同屏物理推挤游戏。两到三名玩家共用键盘，靠速度、质量与走位把对手撞出场地，也可以合作迎战不断出现的 AI 球。

## 玩法特色

- **对战与合作**：PvP 最后存活的玩家获胜；合作模式以全队存活时间为目标，所有玩家淘汰后结算。
- **整球出界才淘汰**：场地没有围墙，球体完全越过边界才会被销毁，惯性与体型都会影响生存空间。
- **随机状态轮换**：加速、变大、变小改变玩家属性，AI 不参与轮换。
- **五种道具**：加速、变重、反向、无敌、冲击波。无敌允许穿过球体，但仍会出界淘汰。
- **递增压力与反馈**：AI 追击、场地收缩、拖尾、危险提示、碰撞形变、震屏、结算慢镜与音效。
- **本机记录**：累计对战统计；按两人／三人组保存最高难度合作生存纪录。

## 开始游戏

Windows x86_64 成品包解压后，运行 `Windows/PongPongPong.exe`，保持可执行文件、`UnityPlayer.dll` 与数据目录的完整相邻结构。

首页包含开始游戏、操作说明、设置、战绩、退出五个入口。点击开始游戏进入对局设置，选择模式、人数与 AI 难度，再开始对局。当前菜单未开放地图选择。

| 操作 | 按键 |
|---|---|
| P1 移动 | W / A / S / D |
| P2 移动 | 方向键 |
| P3 移动 | I / J / K / L |
| 暂停／继续 | Esc |
| 结算后重开 | R |
| 结算后回主菜单 | M |

详细规则见[游戏玩法介绍](Docs/游戏玩法介绍.md)。

## 打开工程

1. 使用 Unity Hub 添加仓库根目录，选择 **Unity 6000.3.23f1**，版本以 `ProjectSettings/ProjectVersion.txt` 为准。
2. 等待资源导入，以及 `Packages/manifest.json` 与锁文件中的依赖解析完成。
3. 打开 `Assets/Scenes/MainMenu.unity`，进入 Play 模式，从主菜单开始。

项目使用 C#、Unity 2D Physics、URP 17.3.0、uGUI 与 TextMeshPro。输入代码仍使用 `UnityEngine.Input`，P1 自定义轴位于 `ProjectSettings/InputManager.asset`。

构建场景依次为 `MainMenu`、`SampleScene`。Windows 构建需要对应平台模块，构建记录及工具见[发布说明](Docs/发布-20261023.md)。

## 项目结构

```text
Assets/
  LogicScripts/      玩家、AI、对局、场地、道具、UI、音频与视觉
  Editor/            菜单布局、场景接线与 Windows 构建工具
  Scenes/            MainMenu 与 SampleScene
  Prefabs/           预制体
  Resources/         运行时加载资源
  Art/ Fonts/ Audio/ 美术、字体与音频
Packages/            包依赖与锁文件
ProjectSettings/     Unity 工程配置
Docs/                玩法、进度、发布与历史设计资料
```

## 状态与文档

截至 **2026-10-26**，项目已有 Windows 交付版本，双层菜单与 Unity 6.3 构建实现 `5573953` 已通过 PR #1 合入远端 main。交付清单记录源码提交号及校验信息。本轮未重新构建或试玩，已有验收范围见项目进度。

- [架构说明](ARCHITECTURE.md)：模块职责、场景流转与数据生命周期。
- [文档索引](Docs/README.md)：当前维护入口与历史资料。
- [项目进度](Docs/项目进度报告.md)：实现状态、验证证据与交付边界。
- [当前状态与方向](TODO.md)：已有能力、候选目标与维护备注。
- [功能扩展方向](Docs/功能扩展方向.md)：基于现有实现的新增玩法候选。
- [主菜单说明](Docs/主菜单视觉调整-20261023.md)：两层导航与布局工具。

地图配置和演示模式等代码保留在工程中，当前开放范围见架构与进度。仓库内未发现项目级 LICENSE，素材也不应视为已获统一再分发授权。
