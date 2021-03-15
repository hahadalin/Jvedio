﻿
[中文](README_CHS.md) [English](README.md) [日本語](README_JP.md)


<h1 align="center">Jvedio</h1>





<h3 align="center">本地视频管理</h3>




---






&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Jvedio 是本地视频管理软件，支持扫描本地视频并导入软件，建立视频库，
提取出视频的 **唯一识别码**，自动分类视频，
添加标签管理视频，使用人工智能识别演员，支持翻译信息，
基于 FFmpeg 截取视频图片，Window 桌面端流畅美观的应用软件


官方网址：[Jvedio](https://hitchao.github.io/JvedioWebPage/) | 下载地址：[最新版本](https://github.com/hitchao/Jvedio/releases)






[![点击查看软件效果图](https://s3.ax1x.com/2021/03/06/6u8UJA.png)](https://s3.ax1x.com/2021/03/06/6u8UJA.png)
[![点击查看软件效果图](https://s3.ax1x.com/2021/03/06/6u8oeU.png)](https://s3.ax1x.com/2021/03/06/6u8oeU.png)


---

## 1 背景
Jvedio 是一款视频管理软件，集本地视频识别扫描、图片展示、筛选、刮削一体。目前，GitHub 上类似的管理软件有：[jellyfin](https://github.com/jellyfin/jellyfin)、[Emby](https://github.com/MediaBrowser/Emby)，市面上存在的软件有：极影派、KODI、PLEX等等

|软件|主要编写语言|支持平台|
|--|--|--|
|Jvedio|C#|win7,win8,win10|
|jellyfin|C#|全平台|
|Emby|C#|全平台|
|极影派|?|Window|
|KODI|?|-|
|PLEX|?|-|



### 1.1 开发初衷

随着电脑上存储的影片越来越多，对一款好的视频管理软件的渴望就越强烈，在使用过 emby、PLEX，发现一些 **问题**：

- 启动方式：先启动一个服务器，然后再使用 WEB 来管理软件
- 信息存储方式：将图片和元数据信息一起放在影片同目录下，导致转移信息非常麻烦
- 界面：WEB写界面高效简单，但我非常希望窗体的边距、阴影、缩放行为、最大化最小化等按钮可以完全自定义
- 功能：数据筛选不够丰富
- 情怀：缺少一款中国人开发的视频管理软件

因此，我开发了 Jvedio ，为了便于加深对 Jvedio 以及视频管理的理解，很多功能与模块都是重头写起，包括界面、功能、爬虫、数据库管理等。


### 1.2 关于代码

我并没有系统的学习过 C# ，但是我听过 C# 的大名，它可以做任何事情，而且很方便很简单，因此我写的代码可能会看起来很蹩脚，但没关系，我知道我会不断成长，Jvedio 也会吸引更多的人进来不断优化完善，我可以看到美好的未来。

在 Jvedio 的代码里，并没有很多晦涩难懂的东西，我尽量用最简单的方式实现一种功能。








## 2 安装

Jvedio 目前没有安装包，软件以压缩文件形式发布和存储，在更新版本后，如无特殊说明，解压后覆盖源文件即可。




## 3 使用

### 3.1 开发者
请安装并打开 Visual Studio （此为 2019），选择 **克隆存储库**，存储库位置填入

`https://github.com/hitchao/Jvedio.git`

或者使用 git

`git clone https://github.com/hitchao/Jvedio.git`


### 3.2 Window 用户

教程请看：[Jvedio 使用说明](https://github.com/hitchao/Jvedio/wiki)


## 4 相关项目


|||
|--|--|
|Jvedio 官方网页|[JvedioWebPage](https://github.com/hitchao/JvedioWebPage)|
|Chrome（360极速浏览器） 插件|[Jvedio-Chrome-Extensions](https://github.com/hitchao/Jvedio-Chrome-Extensions)|
|Jvedio 升级的服务器源|[jvedioupdate](https://github.com/hitchao/jvedioupdate)|
|Gif 控件修改于|[WpfAnimatedGif](https://github.com/hitchao/WpfAnimatedGif)|


## 5 参与贡献的方式

- 无需赞助，欢迎各位提交 [Pull Request](https://github.com/hitchao/Jvedio/pulls)
- 欢迎推广 Jvedio ，让更多人用上

## 6 功能列表

### 6.1 支持的服务器源
- 具体网址自行检索

|服务器名称|支持 |备注|是否需要代理|
|--|--|--|--|
|BUS|√||部分需要|
|BUSEUROPE|√|网址固定|部分需要|
|DB|√|需要登录后的Cookie|部分需要|
|*.CONTENTS.FC2|√|所有 FC2 影片的来源|是|
|LIBRARY|√|该网址无【系列】|部分需要|
|FANZA(DMM)|√|所有骑兵信息的来源|是|
|321|√|该网址无【类别】、但是包含【摘要】|是|
|MOO|√||是|



### 6.2 界面：支持三种皮肤（黑、白、蓝）

[![点击查看](https://s3.ax1x.com/2021/03/06/6uGSeO.png)](https://s3.ax1x.com/2021/03/06/6uGSeO.png)

[![点击查看](https://s3.ax1x.com/2021/03/06/6uGPFH.png)](https://s3.ax1x.com/2021/03/06/6uGPFH.png)


### 6.3 语言：支持中文、英语、日语

[![点击查看](https://s3.ax1x.com/2021/03/06/6uJaKP.png)](https://s3.ax1x.com/2021/03/06/6uJaKP.png)

[![点击查看](https://s3.ax1x.com/2021/03/06/6uJfbT.png)](https://s3.ax1x.com/2021/03/06/6uJfbT.png)


### 6.4 多影视库管理


[![点击查看](https://s3.ax1x.com/2021/03/06/6uJLKx.png)](https://s3.ax1x.com/2021/03/06/6uJLKx.png)


### 6.5 其他功能

- 图片展示模式：缩略图、海报图、预览图、GIF（可能是唯一一个支持 GIF 的视频管理软件）

[![点击查看](https://s3.ax1x.com/2021/03/06/6uYFMt.png)](https://s3.ax1x.com/2021/03/06/6uYFMt.png)

- 丰富的筛选功能：有图/无图、高清/中文/流出、仅显示可播放、仅显示分段视频、视频类型选择

[![点击查看](https://s3.ax1x.com/2021/03/06/6uYlMq.png)](https://s3.ax1x.com/2021/03/06/6uYlMq.png)


- 丰富的右键功能

[![点击查看](https://s3.ax1x.com/2021/03/06/6uY3LV.png)](https://s3.ax1x.com/2021/03/06/6uY3LV.png)


- 快速导航功能

[![点击查看](https://s3.ax1x.com/2021/03/06/6uYJdU.png)](https://s3.ax1x.com/2021/03/06/6uYJdU.png)

- 智能分类

[![点击查看](https://s3.ax1x.com/2021/03/06/6uYLWj.png)](https://s3.ax1x.com/2021/03/06/6uYLWj.png)


- 自定义度较高的设置



[![点击查看](https://s3.ax1x.com/2021/03/06/6utx9H.png)](https://s3.ax1x.com/2021/03/06/6utx9H.png)


- 丰富的工具库


[![点击查看](https://s3.ax1x.com/2021/03/06/6ut3pd.png)](https://s3.ax1x.com/2021/03/06/6ut3pd.png)


- 批量处理功能



[![点击查看](https://s3.ax1x.com/2021/03/06/6utJXt.png)](https://s3.ax1x.com/2021/03/06/6utJXt.png)


- 影视库管理功能



[![点击查看](https://s3.ax1x.com/2021/03/06/6utscn.png)](https://s3.ax1x.com/2021/03/06/6utscn.png)

- 升级


[![点击查看](https://s3.ax1x.com/2021/03/06/6ut0hQ.png)](https://s3.ax1x.com/2021/03/06/6ut0hQ.png)

## 7 鸣谢

**感谢以下网友在 Jvedio 4.0 开发中的贡献**，希望在大家的支持下， `Jvedio` 发展的越来越好！


板块|网友
:--:|:--:
UI|青萍之末, Engine, Erdon, Erik
技术支持|[JavGO](https://github.com/javgo-2020/JavGo), [JavSDT](https://github.com/junerain123/javsdt)
调试|Sheldon, SHAWN, dddsG, EEE, Jion 等人
赞助支持|小猪培根 等众多网友