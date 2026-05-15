# 美术资源接入清单(给美术 + Codex / GPT-5.5)

> **本文档用法**
> - **美术**:从头到尾通读一遍,跟着 §3 的清单准备资源,然后按 §4 在 Unity 里挂。
> - **Codex**:本文档可直接整篇贴给 Codex。它能帮你:回答字段名/路径/规格问题,生成命名建议,解释代码字段含义。**但 Unity Inspector 拖资源的物理操作只能美术在 Unity 里做** — Codex 看不到 Unity 编辑器。
>
> 阅读顺序:§1 项目背景 → §2 任务总览 → §3 资源清单 → §4 操作步骤 → §5 验收 → §6 FAQ

---

## 1. 项目背景(Codex 必读)

**项目名**:碰碰球 (PongPongPong) — Unity 2D 期中作业
**类型**:本地 2~3 人 PvP 物理推挤 + PvE 合作生存
**项目根目录**:`D:\Unity Projects\PongPongPong`
**Unity 版本**:Unity 6+(脚本用 `FindFirstObjectByType`)
**UI 系统**:Canvas Screen-Space Overlay,Reference Resolution 1920×1080
**文字系统**:TextMeshPro,中文字体已配置(`Assets/Fonts/MSYH SDF.asset`)

**当前状态**(2026-05-14):
- 所有 C# 代码已写完,共 35 个脚本在 `Assets/LogicScripts/`
- 3 个 Editor 工具在 `Assets/Editor/`,菜单 `Tools/PongPongPong/...`
- MainMenu 场景 / SampleScene 接线已用工具自动完成
- **唯一缺的是美术资源**:背景图、按钮底图、AudioClip、地图预览图、障碍物 Prefab、各种 SFX

**文档矩阵**:
- `Docs/碰碰球大纲.md` — 玩法设计意图(看完知道"这游戏在玩什么")
- `Docs/主菜单与地图系统.md` — 主菜单 + 地图系统接线(已有 Editor 工具替代手搭)
- `Docs/系统扩展-第二批.md` — AI 难度/COOP/音效/Buff/视觉/演示模式 所有新系统
- **本文档** — 美术接入清单(你正在看)

---

## 2. 任务总览

美术总共要补的东西分 4 类:

| 类别 | 数量 | 优先级 | 备注 |
|---|---|---|---|
| **音频资源** (AudioClip) | 2 BGM + 8 SFX | ⭐⭐⭐⭐⭐ | 没音乐就完全没声音,影响最大 |
| **UI 美化** (Sprite) | 6~8 张 | ⭐⭐⭐⭐ | 占位有色块能用,美化拉档次 |
| **地图资源** (Sprite + Prefab) | 3~4 张地图 + 障碍 Prefab | ⭐⭐⭐ | 地图选择列表显示用 |
| **角色 / 球 视觉** (Sprite) | 4~5 张 | ⭐⭐ | 现在用纯色圆,换 Sprite 提升 |

如果时间紧只做一类,**优先音频** — 静音游戏答辩很尴尬。

---

## 3. 资源清单(详细规格)

### 3.1 音频(`Assets/Audio/` 下,新建文件夹)

> 所有音频格式建议 `.ogg`(Unity 友好,体积小)。Unity 导入时把 Load Type 设 **Decompress on Load**(短 SFX)或 **Streaming**(长 BGM)。

| 文件名 | 类型 | 时长建议 | 风格建议 | 接到哪 |
|---|---|---|---|---|
| `bgm_menu.ogg` | BGM | 30s~2min 循环 | 轻快、电子、点击感 | AudioManager.menuBgm |
| `bgm_game.ogg` | BGM | 30s~2min 循环 | 紧张、节奏感、对抗 | AudioManager.gameBgm |
| `sfx_click_ui.ogg` | SFX | < 0.3s | 清脆"咔嗒"声 | AudioManager.sfxClickUI |
| `sfx_impact_soft.ogg` | SFX | < 0.5s | 闷响、低速碰撞 | AudioManager.sfxImpactSoft |
| `sfx_impact_hard.ogg` | SFX | < 0.5s | 重击、高速碰撞 | AudioManager.sfxImpactHard |
| `sfx_death.ogg` | SFX | < 1s | 玻璃碎 / "啪"一声 | AudioManager.sfxDeath |
| `sfx_pickup.ogg` | SFX | < 0.5s | 升调铃声、收集感 | AudioManager.sfxBuffPickup |
| `sfx_state_switch.ogg` | SFX | < 0.5s | 切换提示音、低调 | AudioManager.sfxStateSwitch |
| `sfx_ai_spawn.ogg` | SFX | < 0.5s | 警示、危险感 | AudioManager.sfxAISpawn |
| `sfx_win.ogg` | SFX | < 2s | 胜利号、欢呼 | AudioManager.sfxWin |

**挂载位置**:Unity 打开 MainMenu 场景 → Hierarchy 找 `_MainMenuRoot` → Inspector 找 AudioManager 组件 → 把对应 AudioClip 拖到对应字段。**只挂一次** — AudioManager 是 `DontDestroyOnLoad` 单例,SampleScene 自动跟着用。

**Codex 可回答的问题**:
- "为什么撞击有两个 SFX?" → 看 `Assets/LogicScripts/AudioManager.cs` 的 `PlayImpact()` — 按 relativeVelocity 阈值(默认 6)分软/硬两档
- "音量怎么控制?" → AudioManager 读 GameSession.BgmVolume / SfxVolume,在 Settings 面板里滑条改

### 3.2 UI 美化(`Assets/Art/UI/` 下,新建文件夹)

| 文件名 | 用途 | 规格建议 | 9-slice? |
|---|---|---|---|
| `bg_main_menu.png` | 主菜单背景 | 1920×1080,深色调 | 否 |
| `bg_panel.png` | 通用面板底 | 64×64,中间纯色,边角圆 | 是,Border 12px |
| `btn_normal.png` | 按钮普通态 | 256×80,圆角矩形 | 是,Border 16px |
| `btn_selected.png` | 按钮选中态(可选) | 256×80,带高亮 | 是 |
| `icon_player1.png` | P1 图标(可选) | 64×64,蓝色调 | 否 |
| `icon_player2.png` | P2 图标(可选) | 64×64,橙色调 | 否 |
| `icon_player3.png` | P3 图标(可选) | 64×64,绿色调 | 否 |
| `logo_title.png` | 标题图(替代文字,可选) | 1200×300,透明背景 | 否 |

**9-slice 设置**(必须):
1. 选中 Sprite → Inspector → Sprite Editor
2. Border 上下左右拖 12~16px(取决于按钮圆角大小)
3. Apply

**挂载位置**:Unity Hierarchy 找对应节点,Inspector 的 Image 组件 → Source Image 字段拖 sprite。具体节点:
- `Canvas/Background` → bg_main_menu.png
- `Canvas/MainPanel/LeftColumn/*` 各 Panel section → bg_panel.png
- 各 Button GameObject (StartButton/HowToPlayButton/...) 的 Image → btn_normal.png + Image Type 设 Sliced

### 3.3 地图资源(`Assets/Art/Maps/`)

每张地图需要 **1 张预览图** + **可选 1 张场地背景** + **0~N 个障碍物 Prefab**。

| 文件 | 用途 | 规格 |
|---|---|---|
| `map_default_preview.png` | 主菜单"地图选择"列表中显示 | 360×240,2D 俯视图 |
| `map_default_bg.png` | 进游戏后场地背景 | 1920×1080 或可平铺 |
| `map_narrow_preview.png` | (同上,窄长地图) | 360×240 |
| `map_square_preview.png` | (同上,方形地图) | 360×240 |
| `map_pillar_preview.png` | (同上,带障碍地图) | 360×240 |

**接入流程**(策划做,美术只提供资源):
1. Project 视图右键 → Create → PongPongPong → Arena Preset
2. 命名 `Arena_Default.asset` 放在 `Assets/Resources/ArenaPresets/`(**必须这个路径**)
3. Inspector 填:
   - Display Name = "标准"
   - Preview Sprite = 拖 map_default_preview.png
   - Bounds Min/Max = (-8, -4.5) / (8, 4.5)
   - Background Sprite = 拖 map_default_bg.png(可选)
   - Border Color = 红色半透 (1, 0.3, 0.3, 0.55)

**障碍物 Prefab**(可选,做"中柱"地图等):
1. Hierarchy 右键 → 2D Object → Sprites → Square,加 BoxCollider2D + Rigidbody2D (Body Type=Static)
2. 给 SpriteRenderer 拖一张障碍物 sprite(或纯色)
3. Project 视图拖成 Prefab(放 `Assets/Prefabs/Obstacles/`)
4. 在 ArenaPreset 的 Obstacles 列表加一条,Prefab 字段拖这个 Prefab,Position 填位置

如果想要"移动障碍物",在 Prefab 上加 `ObstacleMover` 组件(代码已写)。

### 3.4 球的视觉(`Assets/Art/Balls/`,可选)

| 文件 | 用途 | 规格 |
|---|---|---|
| `ball_p1.png` | P1 球 sprite | 256×256,蓝色圆,透明背景 |
| `ball_p2.png` | P2 球 sprite | 256×256,橙色圆 |
| `ball_p3.png` | P3 球 sprite | 256×256,绿色圆 |
| `ball_ai.png` | AI 球 sprite | 256×256,红色圆,带"敌意"细节(如锯齿/眼睛) |
| `aura_invincible.png` | 无敌光环(可选) | 256×256,黄色环 |

**挂载位置**:Hierarchy 找 P1/P2/P3 球的 SpriteRenderer 子节点 → Sprite 字段拖对应图。AI 在 `Assets/Prefabs/AIBall.prefab` 改。

无敌光环:做一个空 GameObject 当 P1/P2/P3 球的子物体,挂 SpriteRenderer 拖 aura_invincible.png,**默认禁用**,然后在 PlayerBase 组件的 `invincibleAura` 字段拖这个子物体(脚本会自动开关)。

---

## 4. 操作步骤(Unity 内的完整流程)

> 假设你已经打开 Unity,并且项目根目录已 git pull 到最新代码。

### Step 1: 让 Unity 编译干净(2 分钟)

打开 Unity → 等编译完成 → Console 没红就 OK。

### Step 2: 跑三个一键工具(各 1 分钟)

Unity 顶部菜单依次点:

1. `Tools/PongPongPong/Build MainMenu Scene` → 弹"继续"按钮 → 生成 MainMenu.unity
2. `Tools/PongPongPong/Patch SampleScene` → 自动给 SampleScene 补缺
3. `Tools/PongPongPong/Setup Build Scenes` → 把两个场景加进 Build Settings

跑完后 Project 视图应该出现:
- `Assets/Scenes/MainMenu.unity`(新生成)
- `Assets/Prefabs/MapButton.prefab`(新生成)
- `Assets/Scenes/SampleScene.unity`(被 patch)

### Step 3: 至少建 1 个 ArenaPreset(策划做,5 分钟)

Project 视图右键 → Create → Folder,创建 `Resources/ArenaPresets/`(在 `Assets/` 下)。然后右键 → Create → PongPongPong → Arena Preset → 命名 Arena_Default,填字段(见 §3.3)。

**不创建 ArenaPreset 的话**,主菜单地图列表会是空的,点 Start 进游戏会用 fallback。

### Step 4: 美术资源逐一接入(60~120 分钟)

按 §3.1 → §3.2 → §3.3 → §3.4 顺序拖资源到对应字段。每挂一个就 Play 一下看效果(BGM 立刻能听到、按钮 sprite 立刻改观)。

### Step 5: 跑通验收(10 分钟)

按 §5 验收清单一项项过。

---

## 5. 验收清单(美术 + 策划共同确认)

按 Play 主菜单后逐项 ✅:

- [ ] 主菜单有 BGM 在响(说明 AudioManager.menuBgm 接好)
- [ ] 标题"碰碰球"清晰显示(中文字体接好)
- [ ] 模式按钮点击有"咔嗒"音(sfxClickUI 接好,UIClickSound 组件起作用)
- [ ] 模式按钮 / 人数 / 难度切换 → 选中按钮变蓝色高亮
- [ ] 地图选择列表至少 1 张地图,缩略图显示
- [ ] 操作说明 / 设置 / 战绩 / 退出 按钮都能打开对应面板
- [ ] 设置面板拖滑条 → 右边数字实时更新(0~100)
- [ ] 设置面板拖 BGM 滑条 → 音量真的变(到 0 静音)
- [ ] 点开始 → 跳到 SampleScene
- [ ] SampleScene 里 BGM 变了(说明 gameBgm 接好)
- [ ] 控制球撞另一个球 → 听到撞击音
- [ ] 撞击有粒子效果(说明 ImpactParticle 接到 GameFeel)
- [ ] 球后面有 trail 拖尾(BallTrail 组件起作用)
- [ ] 出界 → 死亡音 + 屏幕震
- [ ] 60s 后红 AI 出现,出现时有警示音
- [ ] 拾取道具 → 拾取音 + 球颜色短暂变化
- [ ] 结算面板弹出 → 胜利音
- [ ] 主菜单 idle 30s → 自动进入演示模式(AI vs AI)

---

## 6. Codex / GPT-5.5 答疑专区

**美术可以直接问 Codex 这些问题,Codex 应该能回答**:

| 问题 | Codex 看哪个文件回答 |
|---|---|
| "AudioManager.sfxClickUI 这个字段叫什么类型" | `Assets/LogicScripts/AudioManager.cs:18` 看 `public AudioClip sfxClickUI;` |
| "我的 Sprite 应该是多大?" | 见本文档 §3 表格,直接回答 |
| "为什么 Background Sprite 看不见?" | `Assets/LogicScripts/ArenaLoader.cs:30` 检查 `backgroundRenderer` 是否接好 |
| "按钮 Image 怎么不变颜色?" | 检查 Image Type 是不是 Sliced,Sprite 的 9-slice border 有没有设 |
| "我能把背景做成动态视频吗?" | 把 Background 节点的 Image 换成 RawImage + VideoPlayer 组件 |
| "Buff 拾取颜色能改吗?" | `Assets/LogicScripts/BuffPickupSpawner.cs:76` 的 `ColorFor()` 方法 |
| "我想加一个新地图怎么做?" | §3.3 流程 |

**Codex 看不到也帮不了的**:
- Unity 编辑器内的拖引用操作(必须人手做)
- 实际声音听起来怎么样(主观判断)
- Build Profiles 里点哪个按钮(操作 UI)
- 资源导入设置(Texture Type / Sprite Mode)是否最优

---

## 7. 给 Codex 的额外提示

如果美术让 Codex 看代码改字段或脚本,Codex 应该:

1. **永远先读现有文件再改** — `Assets/LogicScripts/` 下所有脚本风格统一(SerializeField 字段、Instance 单例、简洁中文注释)
2. **不要重命名 SerializeField 字段** — 一旦改,场景里所有拖引用全断
3. **不要改 PlayerPrefs 键名** — `MatchHistory.cs` 里的 `PPP_*` 键,改了用户存档丢失
4. **遵循 `Docs/系统扩展-第二批.md` §1.1 的架构约定** — 跨场景状态走 GameSession,单局状态走 MatchManager
5. **看不懂的项目设计意图** → `Docs/碰碰球大纲.md`(BO1/完全出界/AI 不吃 buff 等核心约束)
6. **改 .unity / .prefab YAML 之前** — 必须先看清 fileID 和 m_Father 引用,否则容易破坏场景

---

## 8. 紧急联系 / 反馈渠道

- **资源命名/规格不确定** → 截图发回,我(程序)调整代码字段或文档
- **挂上去看不到效果** → 用 §5 验收清单定位是哪一步断了
- **Codex 答错了** → 把 Codex 的回答 + 实际报错都贴回来,我能判断
- **Unity 报红** → 截图整个 Console,通常是字段引用断了 / 资源路径错了
