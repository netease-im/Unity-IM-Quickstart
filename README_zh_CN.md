_Other Languages: [English](README.md)_

## 简介

这个开源示例项目演示了不同场景下，`NIM SDK` 的基本集成逻辑。 项目中每个示例的`MainScene`都是一个独立的场景，可以独立运行。


## 目录结构

```

├─ API-Examples // NIM API Examples，包括基本的登录、消息收发、聊天室等
│  ├─ Examples  // 所有的示例
│  │  ├─ Basic                 // 演示 基本功能的示例代码
│  │  │  ├─ Chatroom           // 演示 NIM 聊天室初始化、登录和消息收发的示例代码
│  │  │  ├─ Login              // 演示 NIM 初始化、登录、退出、SDK清理的示例代码
│  │  │  ├─ TextMessage        // 演示 NIM 收发文本消息的示例代码
│  │  ├─ Utils                 // 工具类
│  │  │
├─ ├─ nim       // 导入SDK包之后出现,SDK代码文件夹
│  ├─ Plugins   // 导入SDK包之后出现,SDK插件文件夹
│  │  ├─ Android               // 导入SDK包之后出现,Android平台插件文件夹
│  │  ├─ iOS                   // 导入SDK包之后出现,iOS平台插件文件夹
│  │  ├─ x86                   // 导入SDK包之后出现,Windows 32位平台插件文件夹
│  │  ├─ x86_64                // 导入SDK包之后出现,Windows 64位平台插件文件夹

```


## 如何运行示例程序

### 开发环境要求

在开始运行示例项目之前，请确保开发环境满足以下要求：

| 环境要求 | 说明 |
|--------|--------|
| Unity Editor 版本 | 2019.4.30f1及以上版本 |

### 前提条件
- [已创建应用并获取`App Key`](https://doc.yunxin.163.com/nertc/docs/DE3NDM0NTI?platform=unity) 
- 已联系网易云信工作人员开通相关能力，并注册自己的IM 账号
- [已下载Unity IM SDK](https://doc.yunxin.163.com/messaging/sdk-download?platform=unity) 

### 运行示例项目


1. 打开`API-Examples`项目，双击SDK包运行并导入到项目中。

2. 选择想要运行的场景，点击`Canvas`，给场景绑定的脚本组件填入`APP KEY`、账号、密码以及其他必要的信息之后，然后运行程序。

3. 一切就绪之后，你可以参考示例代码的实现，体验`SDK`功能。


## 联系我们
- [网易云信文档中心](https://doc.yunxin.163.com/messaging/docs/home-page?platform=unity)
- [API参考](https://doc.yunxin.163.com/all/api-refer)
- [知识库](https://faq.yunxin.163.com/kb/main/#/)
- [提交工单](https://app.yunxin.163.com/index#/issue/submit)      

