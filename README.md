# Peer-To-Peer Chat💬

This repository contains a Windows Presentation Foundation (WPF) desktop application dedicated to peer-to-peer (P2P) chat functionality. Developed in C# utilizing the .NET Framework, it leverages Windows Communication Foundation (WCF) to establish real-time, two-way communication between clients without the need for a centralized server.

## 🛠️ Tech Stack & Libraries

*   **Framework:** C# / .NET Framework 4.7.2.
*   **UI Architecture:** Windows Presentation Foundation (WPF) using the Model-View-ViewModel (MVVM) pattern.
*   **Networking:** WCF with `System.ServiceModel.NetTcp` and `System.ServiceModel.Duplex` for continuous, stateful two-way connections.
*   **Security:** Cryptographic operations managed via `Microsoft.Bcl.Cryptography`.
*   **Data Handling:** JSON serialization powered by `Newtonsoft.Json`.

## ✨ Core Features

*   **Peer-to-Peer Networking:** Custom serverless network logic handled by the `NetworkManager.cs` component.
*   **Connection Management:** Handles incoming connections dynamically with `RequestAcceptCommand` and safely terminates them via `DisconnectCommand`.
*   **Rich Chat Commands:** Includes interactive chat features such as sending a "Buzz" (nudge) via `BuzzCommand`.
*   **Message History:** Allows users to review past conversations using the `ViewHistoryCommand` and `MainWindowViewHistoryCommand`.

## 📂 Project Structure

The project is structured around the MVVM design pattern:
*   **`View/`**: Contains the XAML UI definitions for the main chat interface.
*   **`ViewModel/Command/`**: Houses encapsulated user actions like `KeyEnterCommand`, `BuzzCommand`, and connection handlers.
*   **`Model/`**: Contains the core business logic, including `NetworkManager.cs` for managing the TCP duplex streams.
*   **`Data/`**: Manages localized data storage, utilizing `Data.json` to persist chat history and configurations.

## 🚀 Getting Started

### Prerequisites
*   Visual Studio 2022 (v17) or later.
*   .NET Framework 4.7.2 SDK.

### Installation & Execution
1. Clone this repository to your local machine.
2. Open the `Demo.sln` solution file in Visual Studio.
3. Restore any missing NuGet packages (e.g., `Newtonsoft.Json`, `Microsoft.Xaml.Behaviors`).
4. Build the solution and run the `Demo.exe` application. 
5. To test the P2P functionality locally, launch two separate instances of the compiled executable and connect them via your localhost IP.
