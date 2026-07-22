# Goldenglow Mod - 缺失资源清单

## 卡牌图片 (card_atlas)

### 完全没有图片 (无 PNG, 无 atlas) - 共 44 张

#### 攻击牌 (14)
- [x] BarberProblem - 理发师难题
- [x] ChargeRelease - 电荷释放
- [ ] ClusterTactics - 群聚战术
- [x] CrystalClearSparkle - 澄净闪耀
- [ ] CurrentAcceleration - 电流加速
- [ ] DroneStrike - 浮游打击
- [ ] FireExit - 消防通道
- [ ] HalfWaveRectifier - 半波整流
- [ ] LeydenJar - 莱顿瓶
- [ ] MillisecondPulsar - 毫秒脉冲
- [x] NewLife - 新生
- [ ] PolesRepel - 排斥
- [ ] TargetLockOn - 锁定攻击
- [x] Telepathy - 心电感应

#### 技能牌 (20)
- [ ] AidAI - 援助型AI
- [ ] AlertTactics - 警戒战术
- [ ] Capacitor - 电容器
- [ ] CurrentShield - 电流护盾
- [ ] ElectrostaticField - 静电场
- [ ] GivingRoses - 赠人玫瑰
- [ ] HairComb - 美发梳
- [ ] HairCurler - 卷发器
- [ ] LimitingComb - 限位梳
- [ ] PermanentMagnet - 永磁体
- [ ] RageOfTheBeacons - 信标的愤怒
- [ ] ScatteredTactics - 零散战术
- [ ] Selection - 选型
- [ ] ShelteringTactics - 庇护战术
- [ ] StaticCharge - 静电充能
- [ ] StorageBox - 收纳箱
- [ ] TeslaCoil - 特斯拉线圈
- [ ] Thinning - 打薄
- [ ] TransmissionChannel - 传电通道
- [ ] Wishlist - 心愿清单

#### 能力牌 (10)
- [ ] Alternator - 发电机
- [ ] BuoyRecovery - 浮标回收
- [ ] ChargeBalance - 电荷平衡
- [ ] DroneCaster - 驭械
- [ ] Excitation - 励电器
- [ ] LiquidSoap - 洗手液
- [ ] PiezoelectricEffect - 压电效应
- [ ] RadiationLamp - 放射灯
- [ ] Renovation - 翻新
- [x] ScatterSparks - 火花四溅

### 有 PNG 但未打包进 atlas (2)
- [x] DroneGroup - 浮标集群
- [x] SupportTactics - 支援战术

---

## Power 图标 (power_atlas)

### 完全缺失 (无 PNG, 无 atlas) (4)
- [ ] ElectrostaticFieldPower - 静电场
- [ ] PiezoelectricEffectPower - 压电效应
- [ ] SubharmonicResonancePower - 亚谐共振
- [ ] TargetLockOnPower - 锁定攻击

### 名字不匹配 (atlas 存在但用旧名) (3)
- [ ] ScatterSparksBlockPower - 火花护盾 (atlas 现为 `ScatterSparksPower`)
- [ ] ScatterSparksDiscardPower - 火花四溅 (atlas 现为 `ScatterSparksPower`)
- [ ] TechniquePower - 手法 (atlas 现为 `TracingTechniquePower`)

### 临时 Power (可能不需要独立图标) (2)
- [ ] CapacitorTempPower - 扩容（临时）
- [ ] DegaussingTempPower - 消磁（临时）

---

## 遗物图片 (relics) - 全部缺失 (9)
- [ ] CleaningTools - 清扫工具
- [ ] ColorSwatch - 试色卡
- [ ] FloralToner - 花香焕肤液
- [ ] HairCape - 美发围布
- [ ] InsulatingComb - 绝缘梳子
- [ ] InsulatingScissors - 绝缘剪刀
- [ ] NightSkyProjector - 夜空投影灯
- [ ] TechniqueNotes - 技巧笔记
- [ ] WindChime - 风铃

---

## 需要清理的过时文件

### Power 图标 (atlas + PNG 都有, 但类已不存在) (3)
- [ ] StableCastingPower - 已移除的 power
- [ ] TracingTechniquePower - 旧名, 现为 TechniquePower
- [ ] ScatterSparksPower - 旧名, 现拆分为 ScatterSparksBlockPower / ScatterSparksDiscardPower

### 卡牌 atlas 测试文件 (1)
- [ ] test - 测试文件, 应删除

### 卡牌 atlas 重复 (1)
- [ ] Degauss - 与 Degaussing 重复
