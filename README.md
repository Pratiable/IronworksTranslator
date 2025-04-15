# IronworksTranslator-Enhanced

A tool that translates FFXIV in-game chat in real-time.

This version is an improved fork of the original [IronworksTranslator](https://github.com/sappho192/IronworksTranslator), incorporating Gemini support along with several new features and quality-of-life improvements.

## New Features
- Gemini translation support
- Speaker-separated translation for improved accuracy
- Window position saving between sessions
- Fixed translation window scroll position (no auto-scrolling when reading previous messages)
- Maximum line count of 1000 to prevent performance issues
- Channel-specific color settings for better visual distinction between chat types

## System Requirements
- Windows 10 64bit or higher
- .NET 6.0 Runtime ([Download](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime))
- Final Fantasy XIV running in DirectX 11 mode

## Setup Tips
- You need to enter an Gemini API key when using the Gemini

## Building the Project

To build the project yourself, you'll need the [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed.

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/Pratiable/IronworksTranslator-Enhanced.git
    cd IronworksTranslator-Enhanced
    ```

2.  **Build for development:**
    This command compiles the code. The output will typically be in the `bin/Debug/net6.0-windows` folder.
    ```bash
    dotnet build
    ```

3.  **Publish for distribution:**
    This command creates a distributable package, gathering all necessary files. It assumes the target machine has the .NET 6.0 Desktop Runtime installed (as specified in System Requirements).
    ```bash
    # For 64-bit Windows
    dotnet publish -c Release -r win-x64 --self-contained false
    ```
    The published files will be in a folder like `bin/Release/net6.0-windows/win-x64/publish`. You can distribute the contents of this `publish` folder.

## License
This project is distributed under the MIT License.
