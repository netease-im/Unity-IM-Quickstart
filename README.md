_Other Languages: [简体中文](README_zh_CN.md)_

## Overview

The open source project shows the basic integration logic of `NIM SDK` in different scenes. Each `MainScene` in the example is independent.


## Folder Structure

```

├─ API-Examples // NIM API Examples，basic login, messaging, chat room, etc.
│  ├─ Examples  // Examples
│  │  ├─ Basic                 // NIM basic features
│  │  │  ├─ Chatroom           // Sample code for NIM Chat room initialization, login and messaging
│  │  │  ├─ Login              // Sample code for NIM initialization, login, logout, SDK cleanup
│  │  │  ├─ TextMessage        // Sample code for text messaging
│  │  ├─ Utils                 // Utilities
│  │  │
├─ ├─ nim       // Code folder created after the SDK package is imported.
│  ├─ Plugins   // Plugin folder created after the SDK package is imported
│  │  ├─ Android               // Android plugin
│  │  ├─ iOS                   // iOS plugin
│  │  ├─ x86                   // Windows 32-bit plugin
│  │  ├─ x86_64                // Windows 64-bit plugin

```

## Run the demo project

### Development environment requirements

Before starting the demo project, make sure your development environment meets the following requirements:


| Environment | Description |
|--------|--------|
| Unity Editor | 2019.4.30f1 or later |

### Prerequisites

- Create a project and get `App Key`
- You have contacted CommsEase technical support, activated required services and signed up your IM account.
- [NIM SDK for Unity downloaded](https://doc.commsease.com/en/messaging/sdk-download) 

### Run the demo app


1. Open `API-Examples` project, double-click the SDK package and import the SDK into your project。
2. Select the scene you want to run and click `Canvas`. Specify `APP KEY`, account and password, and other required information in the script bound to the scene, then run the app.
3. To build specific features, you can refer to the sample code in the SDK.

	
## Contact us
- [CommsEase Documentation](https://doc.commsease.com/en/messaging/docs/home-page?platform=unity)
- [API Reference](https://doc.commsease.com/docs/interface/NIM_SDK/en/Latest/Unity/index.html)
