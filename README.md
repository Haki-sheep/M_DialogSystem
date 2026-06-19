# M Dialog System

Unity 节点式对话系统，带可视化 GraphView 编辑器，支持分支条件、图参数与 JSON 导入导出。

- **Unity 版本**：2022.3.53f1c1
- **命名空间**：`Miemie.DialogSystem`
- **仓库**：https://github.com/Haki-sheep/M_DialogSystem

---

## 依赖

| 依赖 | 用途 |
|------|------|
| [Odin Inspector](https://odininspector.com/) | 编辑器菜单树、字典序列化、`DialogueRunner` 继承 `SerializedMonoBehaviour` |
| Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`) | 对话图 JSON 导入导出 |

---

## 快速开始

### 1. 打开编辑器

菜单：**Tools → MmDialogWindow**

### 2. 创建对话图

工具栏点击 **新建对话图**，资产保存在 `Assets/DialogSystem/NodeSo/`。

### 3. 编辑对话

1. 左侧选中一张 **对话图**
2. 画布空白处右键 **创建节点**
3. 拖拽 **Out / 选项口** 连线
4. 点击连线编辑 **条件**；点击节点编辑 **台词、选项**
5. 左侧 **Parameters** 面板添加图级变量（Float / Int / Bool）
6. **Ctrl+S** 保存；工具栏右侧 `*` 表示有未保存修改

### 4. 运行时测试

1. 场景挂载 `DialogueRunner`（需 `SerializedMonoBehaviour`）
2. 指定 `dialogueGraph` 与 `variableList`
3. 进入 Play：
   - **空格**：普通节点前进
   - **数字键 1~9**：选项节点选择

编辑器 Play 模式下可用工具栏 **播放当前图**（需场景中有 `DialogueRunner`）。

### 5. JSON

- **导出 JSON**：工具栏 → 导出 JSON → `Assets/DialogSystem/Export/`
- **导入 JSON**：工具栏 → 导入 JSON

---

## 编辑器窗口架构

```
┌──────────────────────────────────────────────────────────────────────────┐
│ 工具栏  新建对话图 | 校验本图 | 导出/导入 JSON | 播放当前图          *    │
├──────────┬────────────┬────────────────────────────┬─────────────────────┤
│ 对话图   │ Parameters │        GraphView 画布       │       属性          │
│ (左栏)   │ (参数栏)   │                             │     (右栏)          │
├──────────┼────────────┼────────────────────────────┼─────────────────────┤
│ OdinMenu │ 图级变量   │ 节点方块 + 连线             │ 节点 / 连线 Inspector│
│ Tree     │ 定义       │                             │                     │
└──────────┴────────────┴────────────────────────────┴─────────────────────┘
```

### 面板 ↔ 代码对应

| 编辑器位置 | 主要类 |
|-----------|--------|
| 工具栏 | `DialogueGraphEditorWindow.Toolbar.cs` |
| 左侧对话图树 | `DialogueGraphEditorWindow.MenuTree.cs` + `DialogueGraphLeftPanel` |
| Parameters 栏 | `DialogueGraphParametersPanel` + `DialogueGraphParametersDrawer` |
| 中间画布 | `DialogueGraphView` + `DialogueNodeView` |
| 右侧属性 | `DialogueGraphInspectorPanel` + `DialogueNodeInspectorDrawer` / `DialogueTransitionInspectorDrawer` |
| 节点布局持久化 | `DialogueGraphLayoutStore` → `DialogueGraphLayouts.asset` |

### 画布元素 ↔ 数据类

| 画面上看到的东西 | 数据类 | 挂在哪 |
|-----------------|--------|--------|
| 大方块（节点） | `DialogueNode` | `DialogueGraph.nodeList` |
| 普通节点 `Out` 口 + 连线 | `DialogueTransition` | `DialogueNode.nextTransition` |
| 选项节点 `选项1/2/3` 口 + 连线 | `DialogueOptionTransition` | `DialogueNode.choiceList` |
| 连线上的条件 | `DialogueCondition` | `Transition/OptionTransition.conditionList` |
| 左侧 Parameters 一行 | `DialogueParameterDefinition` | `DialogueGraph.parameterList` |

选中 **Transition** 连线 → 右侧显示普通跳转条件。  
选中 **Option Transition** 连线 → 右侧多一个 **选项文本** 字段（运行时按钮文案）。

---

## 代码分层

```
Assets/DialogSystem/Scripts/
├── Data/                    # ScriptableObject 与 JSON 模型
│   ├── DialogueGraph.cs
│   ├── DialogueNode.cs
│   ├── DialogueParameter.cs
│   └── DialogueJsonModels.cs
├── Rumtime/
│   ├── DialogueRunner.cs    # 运行时驱动（Debug 键盘版）
│   ├── Condition/
│   │   ├── DialogueCondition.cs
│   │   ├── DialogueConditionTypes.cs   # ECondition 枚举
│   │   └── DialogueVariables.cs        # 运行时变量（Odin 字典）
│   ├── Transition/
│   │   ├── DialogueTransition.cs       # 普通单出口
│   │   └── DialogueOptionTransition.cs # 选项多出口
│   └── SperakTypes/
│       └── DialogueSpeakEnums.cs
└── Editor/
    ├── Window/              # 主窗口 partial
    ├── Panels/              # 左/参数/属性面板
    ├── GraphView/           # 画布与节点视图
    ├── Inspectors/          # IMGUI 属性绘制
    └── Utils/               # JSON、布局、校验、Editor 专用条件工具
```

### Data / Runtime / Editor 职责

| 层 | 职责 |
|----|------|
| **Data** | 资产结构定义，可被 SO 序列化与 JSON 互转 |
| **Runtime** | `MeetCondition`、`CanPass`、`GoTo` 等纯运行逻辑 |
| **Editor** | 可视化编辑、标签显示、`GetDisplayLabel` 等仅编辑器方法 |

---

## 条件与变量

### 图参数（声明）

在 **Parameters** 面板定义变量名、类型、默认值。  
对应 `DialogueParameterDefinition`，对话开始时由 `DialogueVariables.ApplyDefaults` 灌入运行时。

### 连线条件（规则）

在连线上添加 `DialogueCondition`，引用 Parameters 里的 `key`：

| 参数类型 | 可用条件 |
|---------|---------|
| Bool | True / False |
| Float | 大于 / 小于（不做相等比较） |
| Int | 大于 / 小于 / 等于 / 不等于 |

### 运行时变量（当前值）

挂在 `DialogueRunner.variableList`，Play 后可在 Inspector 查看。  
游戏中用 `SetBool` / `SetInt` / `SetFloat` 修改，条件判断时读取。

```
Parameters（策划定义） → DialogueVariables（运行时值） → Condition（判断能否走线）
```

---

## 运行时流程

```
StartDialog()
  → ApplyDefaults(图参数)
  → GoTo(startNode)
       ↓
  普通节点: Advance() → NextTransition.CanPass → GoTo
  选项节点: RefreshAvailableChoices() → 筛条件 → SelectOption → GoTo
```

`availableChoiceList` 是 Runner 内部缓存：**当前真正能选的选项出口**，不是节点上配置的全部 `choiceList`。

---

## 资产路径

| 路径 | 内容 |
|------|------|
| `Assets/DialogSystem/NodeSo/` | 对话图、节点 SO |
| `Assets/DialogSystem/NodeSo/DialogueGraphLayouts.asset` | 画布节点坐标 |
| `Assets/DialogSystem/Export/` | 导出的 JSON |
| `Assets/DialogSystem/Scene/` | 示例场景 |

---

## 快捷键

| 操作 | 按键 |
|------|------|
| 保存当前修改 | Ctrl+S |
| 普通节点前进（运行时 Debug） | 空格 |
| 选择选项（运行时 Debug） | 1 ~ 9 |

---

## 设计参考

整体思路类似 **Animator 状态机**：

- `DialogueNode` ≈ State
- `DialogueTransition` / `DialogueOptionTransition` ≈ Transition（带条件）
- `DialogueCondition` ≈ Condition 单条
- `DialogueParameterDefinition` ≈ Animator Parameters
- `DialogueVariables` ≈ 运行时 Parameter 当前值

---

## 后续可扩展

- [ ] `DialogueRunner` 事件驱动 UI（OnNodeEntered / OnChoicesReady）
- [ ] 台词库 lineId + DialogueBank
- [ ] 节点 OnEnter / OnExit 回调
- [ ] Runtime / Editor 程序集拆分（asmdef）

---

## License

本项目为学习与个人项目用途，第三方插件（Odin、DOTween 等）请遵循各自授权协议。
