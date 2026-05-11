# UI 搭建指南（交给 UI 搭建人员）

> 项目代码已经写完，**UI 是唯一还没搭的部分**。本指南把每个步骤都写到了"具体点哪里、填什么值"的颗粒度，按顺序做即可。完成后游戏即可跑通。
>
> 预计耗时：30~60 分钟（不含找美术素材；美术素材另见 `素材搜集清单.md`）。

---

## 0. 你需要知道的背景

游戏画面是一个 1920×1080 的横屏 2D 游戏。两颗球在矩形场地内对撞，被挤出界即淘汰。UI 需要承载三件事：

1. **顶部计时器 HUD**：永远显示，俩倒计时数字
2. **胜负结算面板**：游戏结束时弹出（默认不显示，靠脚本激活）
3. **暂停菜单**：按 ESC 弹出（默认不显示）

脚本端引用名一一对应在 §6 的拖拽表里。**搭好后只要把对应 UI 节点拖到脚本 Inspector 字段，全部完成。**

---

## 1. 准备工作

### 1.1 打开正确的场景

打开 `Assets/Scenes/SampleScene.unity`。

### 1.2 (可选) 决定文字方案

脚本目前默认使用 **Legacy Text**（无需额外安装）。

如果你想用 **TextMeshPro**（视觉更好但要先 import + 给中文生成 Font Asset），请告诉程序，让他把脚本里 `using UnityEngine.UI;` → `using TMPro;`，`Text` → `TMP_Text`，再继续按本文档操作（创建文字时选 TMP 版本而非 Legacy）。

**本指南默认 Legacy。**

---

## 2. 创建 Canvas

1. Hierarchy 空白处右键 → **UI → Canvas**
   - Unity 会同时自动创建 `EventSystem`（必需，按钮才能响应）
2. 选中刚创建的 `Canvas`，在 Inspector 设置：

| 组件 | 字段 | 值 |
|---|---|---|
| Canvas | Render Mode | Screen Space - Overlay |
| Canvas Scaler | UI Scale Mode | **Scale With Screen Size** |
| Canvas Scaler | Reference Resolution | X = 1920，Y = 1080 |
| Canvas Scaler | Screen Match Mode | Match Width Or Height |
| Canvas Scaler | Match | 0.5 |

---

## 3. 搭建 HUD（顶部计时器）

### 3.1 创建 HUD 容器

1. 选中 `Canvas` → 右键 → **Create Empty**
2. 重命名为 `HUD`

### 3.2 创建 StateLabel（大字状态倒计时）

1. 右键 `HUD` → **UI → Legacy → Text**，重命名为 `StateLabel`
2. Inspector 设置：

| 组件 / 字段 | 值 |
|---|---|
| **RectTransform** Anchor Preset | top-center（按住 Shift+Alt 点 anchor 图标会同时锚定+对齐） |
| Pos X | 0 |
| Pos Y | -80 |
| Width | 800 |
| Height | 100 |
| **Text** Text 内容 | `5 秒后切换状态`（占位，会被脚本覆盖） |
| Font | 你选的中文字体（拖入 Font 字段） |
| Font Size | 48 |
| Alignment | 水平居中 + 垂直居中 |
| Horizontal Overflow | Overflow |
| Vertical Overflow | Overflow |
| Color | 白色 |

### 3.3 创建 AILabel（小字 AI 倒计时）

1. 右键 `HUD` → **UI → Legacy → Text**，重命名为 `AILabel`
2. Inspector 设置：

| 字段 | 值 |
|---|---|
| Anchor Preset | top-center |
| Pos X | 0 |
| Pos Y | -160 |
| Width | 600 |
| Height | 50 |
| Text 内容 | `60 秒后 AI` |
| Font Size | 24 |
| Alignment | 居中 |
| Color | 白色 |

### 3.4 HUD 完成后层级应该是：

```
Canvas
└── HUD
    ├── StateLabel
    └── AILabel
```

---

## 4. 搭建胜负面板（EndScreenPanel）

### 4.1 创建面板

1. 右键 `Canvas` → **UI → Panel**，重命名为 `EndScreenPanel`
2. Panel 默认会铺满 Canvas，自带一个半透明灰色背景图——这就是我们要的
3. 调整 Image 颜色：选中 `EndScreenPanel`，在 Inspector 的 Image 组件里把 Color 改为 `(0, 0, 0, 180)`（接近黑色、半透明）

### 4.2 在面板内放结果文字

1. 右键 `EndScreenPanel` → **UI → Legacy → Text**，重命名为 `ResultText`
2. Inspector 设置：

| 字段 | 值 |
|---|---|
| Anchor Preset | middle-center |
| Pos X | 0 |
| Pos Y | 100 |
| Width | 1000 |
| Height | 200 |
| Text 内容 | `P1 胜`（占位，会被脚本覆盖） |
| Font Size | 96 |
| Font Style | Bold |
| Alignment | 居中 |
| Color | 白色 |

### 4.3 放两个按钮

#### RestartButton（重开）

1. 右键 `EndScreenPanel` → **UI → Legacy → Button**，重命名为 `RestartButton`
2. Inspector 设置：

| 字段 | 值 |
|---|---|
| Anchor Preset | middle-center |
| Pos X | **-150** |
| Pos Y | -100 |
| Width | 200 |
| Height | 70 |

3. 展开 `RestartButton`，里面有自动生成的 `Text` 子物体。点开，把：
   - Text 内容改为 `重开`
   - Font Size 改为 32
   - Font 拖入你的中文字体

#### QuitButton（退出）

重复上面流程：
- 名字 `QuitButton`
- **Pos X = +150**（右半边）
- 子 Text 改为 `退出`

### 4.4 EndScreenPanel 完成后层级：

```
Canvas
└── EndScreenPanel  (Image)
    ├── ResultText
    ├── RestartButton
    │   └── Text
    └── QuitButton
        └── Text
```

### 4.5 ⚠️ 重要：默认禁用

**选中 `EndScreenPanel`，在 Inspector 最顶上的勾去掉**（让它默认不显示）。脚本运行时会按需开启。

---

## 5. 搭建暂停菜单（PauseMenuPanel）

### 5.1 复制 EndScreenPanel 简化

最快做法：选中刚做好的 `EndScreenPanel`，按 Ctrl+D 复制一份，重命名为 `PauseMenuPanel`。然后改成下面这样：

1. 把 `ResultText` 改名为 `TitleText`，Text 内容写死 `暂停`（这个**不会**被脚本覆盖）
2. 多加一个按钮 `ResumeButton`：
   - 右键 `PauseMenuPanel` → **UI → Legacy → Button**
   - Pos X = 0, Pos Y = 0（中间）
   - 子 Text 改为 `继续`
3. 把现有的 `RestartButton`、`QuitButton` 位置调一下，三个按钮纵向排列：

| 按钮 | Pos X | Pos Y |
|---|---|---|
| ResumeButton | 0 | 0 |
| RestartButton | 0 | -90 |
| QuitButton | 0 | -180 |

### 5.2 同样默认禁用 `PauseMenuPanel`（Inspector 顶部勾去掉）

### 5.3 完成后层级：

```
Canvas
└── PauseMenuPanel  (Image)
    ├── TitleText  (内容:"暂停")
    ├── ResumeButton
    │   └── Text
    ├── RestartButton
    │   └── Text
    └── QuitButton
        └── Text
```

---

## 6. 创建脚本宿主 GameObject + 拖拽引用

UI 节点搭好后，需要 3 个空 GameObject 来**挂载脚本**并**接收引用**。

### 6.1 创建 HudController

1. Hierarchy 右键空白处 → Create Empty，命名 `HudController`
2. 拖动 `Assets/LogicScripts/HudController.cs` 到这个 GameObject（或在 Inspector 点 Add Component → 搜 HudController）
3. **拖拽引用**：

| 脚本字段 | 从 Hierarchy 拖什么过来 |
|---|---|
| State Label | `Canvas/HUD/StateLabel` |
| Ai Label | `Canvas/HUD/AILabel` |
| Warning Threshold | 保持默认 3 |
| Normal Color | 白色 |
| Warning Color | 红色 |

### 6.2 创建 EndScreen

1. Create Empty，命名 `EndScreen`
2. Add Component → `EndScreen`
3. 拖拽引用：

| 脚本字段 | 从 Hierarchy 拖什么过来 |
|---|---|
| Panel | `Canvas/EndScreenPanel` |
| Result Text | `Canvas/EndScreenPanel/ResultText` |
| Restart Button | `Canvas/EndScreenPanel/RestartButton` |
| Quit Button | `Canvas/EndScreenPanel/QuitButton` |

### 6.3 创建 PauseMenu

1. Create Empty，命名 `PauseMenu`
2. Add Component → `PauseMenu`
3. 拖拽引用：

| 脚本字段 | 从 Hierarchy 拖什么过来 |
|---|---|
| Panel | `Canvas/PauseMenuPanel` |
| Resume Button | `Canvas/PauseMenuPanel/ResumeButton` |
| Restart Button | `Canvas/PauseMenuPanel/RestartButton` |
| Quit Button | `Canvas/PauseMenuPanel/QuitButton` |

---

## 7. 最终场景层级应该长这样

```
Scene Hierarchy
├── (一些已有的物体：Player1, Player2, Main Camera, ArenaBounds, MatchManager, StateRoulette, AISpawner)
├── Canvas
│   ├── HUD
│   │   ├── StateLabel
│   │   └── AILabel
│   ├── EndScreenPanel        (默认禁用)
│   │   ├── ResultText
│   │   ├── RestartButton
│   │   └── QuitButton
│   └── PauseMenuPanel         (默认禁用)
│       ├── TitleText
│       ├── ResumeButton
│       ├── RestartButton
│       └── QuitButton
├── EventSystem               (Canvas 创建时自动生成,不用动)
├── HudController             (空 GameObject + HudController 脚本)
├── EndScreen                 (空 GameObject + EndScreen 脚本)
└── PauseMenu                 (空 GameObject + PauseMenu 脚本)
```

---

## 8. 验收清单

按 Play 后逐项确认：

- [ ] 屏幕顶部中央有大字 `5 秒后切换状态`，下方小字 `60 秒后 AI`
- [ ] 数字会每秒减少（5 → 4 → 3 → ...）
- [ ] 状态倒计时到 0 → 球的颜色/大小变化（说明状态机制接通）
- [ ] AI 倒计时进入最后 3 秒（即 57s 后）时，小字数字**变红 + 闪烁**
- [ ] 第 60s 第一只红球出现
- [ ] 把对方挤出场地后 → 屏幕变暗、中央显示 `P1 胜` 或 `P2 胜`、下方两个按钮可点
- [ ] 点"重开" → 关卡重启
- [ ] 点"退出" → 编辑器停止 Play（或独立运行时退出程序）
- [ ] 游戏中按 ESC → 时间冻结、弹出暂停菜单
- [ ] 点"继续" → 关闭菜单，时间恢复
- [ ] 已经结算后再按 ESC → 不再弹暂停菜单（因为游戏已结束）

---

## 9. 常见问题

### Q: 按钮点击没反应

A: 检查场景里是否有 `EventSystem` GameObject。没有的话 Hierarchy 右键 → UI → Event System。

### Q: 文字显示乱码方块

A: Text 的 Font 字段没设中文字体。把 `Assets/Fonts/` 下的中文 TTF 拖到 Font 字段。

### Q: 面板挡住游戏画面但点不到按钮

A: 检查 Panel 的 Image 组件有没有勾上 `Raycast Target`（默认就有）。同时确认按钮自身的 Image 也勾了 `Raycast Target`。

### Q: HUD 文字在游戏运行时消失

A: 多半是 HudController 的 StateLabel / AiLabel 字段忘了拖引用，运行时脚本拿不到 Text 组件就什么都不显示。

### Q: 字号在 Editor 里看着合适但游戏窗口里太大/太小

A: 用 Game 视图测试（Editor 里 Scene 视图缩放不代表实际比例）。也确认 Canvas Scaler 用了 `Scale With Screen Size` 而非 `Constant Pixel Size`。

### Q: 我想换字体怎么办？

A: 把新字体 TTF 拖进 `Assets/Fonts/`，再分别选中 StateLabel / AILabel / ResultText / TitleText / 各按钮的 Text 子物体，把 Font 字段替换成新的。**不要修改脚本**。

### Q: 我想让胜利文字带个特效（描边/发光）

A: Legacy Text 改不了这个，需要切到 TMP。详见 §1.2 的 TMP 切换流程。

---

## 10. 完成后

把 SampleScene 保存（Ctrl+S）即可。

如果搭建过程中发现脚本字段缺失、命名不一致、报错等，**截图发回**，由程序端调整脚本。不要私自改脚本字段名，否则需要后续重新拖一遍引用。
