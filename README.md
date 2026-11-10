# IS THAT A BOT? — Unity 6

Hop into the fight bot and defend the city!

本分支包含 Unity 6 迁移后的开发版本。Unity 工程位于仓库根目录，使用 **Unity 6000.3.20f1** 打开。

## 运行与测试

- 正常流程：打开 `Assets/Scenes/Level0.unity`，输入 `555` 启动。
- 开发测试：打开 `Assets/Scenes/DevelopmentTestRange.unity`，进入 Play 后按 `P` 开始；再次按 `P` 重置位置并回收活动子弹与命中特效。
- 测试场为直接保存的独立场景，包含不同距离的靶、飞行门、坡道和障碍物。

## 操作

| 按键 | 功能 |
| --- | --- |
| 鼠标 | 调整视线 |
| W / A / S / D | 移动 |
| Space / Left Ctrl | 上升 / 下降 |
| Q / E、R / F、Z / C | 机体旋转 |
| 左键 | 双头部机枪 |
| 右键 | 光束武器 |

## 瞄准与武器

- 双圆环准星通过驾驶舱球体的 Shader Graph 显示，并保留扫描线和发光效果。
- 两种武器共同瞄准双圆环之间的画面中心；机枪另有重力补偿。
- 子弹按实际飞行路径检测命中，减少高速飞行时穿过薄目标的漏判。
- 机枪、光束及两种命中特效通过对象池复用。池管理代码集中在 `Assets/Scripts/Combat`。
- `PlayerHolder` 的 `Weapon Pool Prewarm` 可设置预创建数量：机枪 128、光束 4、机枪命中特效 96、光束命中特效 4。对象不足时按需增加。

仓库保留工程资源、包配置及项目设置；Unity 缓存、日志、生成的 IDE 文件和本机设置由 `.gitignore` 排除。
