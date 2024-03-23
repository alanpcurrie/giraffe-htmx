# F# Giraffe HTMX App

![Logo](logo.WEBP)

This repository contains an F# Giraffe application with HTMX for frontend interactivity, and open props for styling

Currently the app runs on Azure app service Web apps, connected to neon db serverless postgres service.

[The demo is live at https://giraffe-htmx.azurewebsites.net/](https://giraffe-htmx.azurewebsites.net/)

## Getting Started

These instructions will get your copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

Before you begin, install:

- [.NET 5.0 SDK or later](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/en/) (which includes npm)

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/alanpcurrie/giraffe-htmx.git
   cd giraffe-htmx
   ```

2. **Install Browser-Sync globally**

   Browser-Sync enables you to rapidly iterate on your front-end code with live reloading of your web app.

   ```bash
   npm install -g browser-sync
   ```

3. **Start the development environment**

   You will use `dotnet watch run` for the .NET core part, which watches for file changes and automatically recompiles and restarts the server. Concurrently, use Browser-Sync to proxy your local development server and watch CSS file changes in the `WebRoot` directory for live reloading.

   - Start the .NET Core application with watch mode enabled:

     ```bash
     dotnet watch run
     ```

   - Open a new terminal tab/window, then start Browser-Sync to watch CSS file changes:

     ```bash
     browser-sync start --proxy "localhost:5000" --files "WebRoot/*/*.css"
     ```

   This setup provides a development environment where changes to both your F# and CSS files cause the application and styles to update and Css changes reload live in the browser.

## Usage

After starting the development servers, visit `http://localhost:3000` (Browser-Sync) to see your application in action. Any changes to the CSS files under `WebRoot` automatically refresh your browser. F# files will need a manual refresh

## Contributing

We welcome contributions! Please read our [CONTRIBUTING.md](CONTRIBUTING.md) for details on how to submit pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
