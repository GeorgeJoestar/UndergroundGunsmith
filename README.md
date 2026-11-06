# 地下枪匠许可证与定制订单

English: [README.en.md](README.en.md)

为游戏新增3种地下枪匠许可证，逻辑与原版许可证一致，但取消了治安部的低声望吊销，与不会将武器中的违禁品tag转换为管制品tag。
注意此mod的许可证没有更改销售违禁品的逻辑，也就是说你还是会因走私被治安部调查。

因此你可以获得：与原版许可证一致的枪械定制客户、退休枪匠商人销售的高级物品。

需要游戏版本 **0.46D**（Harmony 补丁可能随大版本更新失效）。

## 从源码构建

```bash
dotnet build -c Release
# 产物落点：<游戏目录>/Mods/UndergroundGunsmith/
```

默认假设仓库位于 `<游戏目录>/Modding/UndergroundGunsmith/`；游戏装在别处时：

```bash
dotnet build -c Release -p:GameDir="D:\Steam\steamapps\common\Probably Stolen\"
```

## 兼容性

- 顾客、订单池、商品池、结算全部复用原版实现，其它模组对这些内容的修改同样生效。
- 若另有模组同样改写「展示柜顾客预测」或「人脉图标解析」，两者会叠加执行。
- 不写入 `ActivePermits`，因此与原版武器许可证、化学品许可证类模组互不干扰。

## 许可

[MIT](LICENSE)
