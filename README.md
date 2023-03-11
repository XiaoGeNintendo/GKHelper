# GKHelper
 Desktop Widget For NFLS 2023 Gaokao Class

**注意：本程序的开源工作仅供NFLS相关同学参考修改和个人归档，不作为对外发布二进制文件的载体。本程序作者没有任何意愿公开发布该程序的二进制文件**

在big分支下有大屏版本的代码

（理论上）程序应该运行在 **Windows 10, x64, .NET 4** 系统上。

- 附属工具：[GKHelperBand](https://github.com/XiaoGeNintendo/GKHelperBand)

# 一些说明
## Config文件格式
```
2023/06/07 -- 高考时间
擦黑板|倒垃圾 -- 值日岗位名称
1.0 -- 默认缩放
100 100 -- 程序启动时的窗口位置。设置成负数可以使用Windows默认设置。
testConfig.txt -- 程序启动时默认样式文件。设置成null可禁用。
# 从下一行开始是Tag，格式为：DoSomething或NoSomething
DoExpand -- 是否默认展开公告栏
DoToast -- 是否默认启用吐司
DoWordpad -- 是否用写字板打开anno文件
NoBorder -- 是否启动后关闭外框
```
## 公告文件模板
- 详见`default.rtf`
## 吐司
吐司的图片放在`hero/`下，基本来自于[pixabay](https://pixabay.com/)

其中default生成自AI作图网站。
