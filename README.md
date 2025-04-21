# Azure Weather Logging System

## Overview

This project is a **ongoing** Azure Function-based system that periodically fetches weather data from OpenWeatherMap and logs the data into Azure Storage. 

**This was a test from an interview process that needed to be done within one day only.**

The system consists of:

- **Azure Timer Trigger Function**: Runs every minute to fetch weather data.
- **Azure Table Storage**: Stores logs of success/failure attempts.
- **Azure Blob Storage**: Stores full weather payloads.
- **Azure HTTP Trigger Functions**:
  - Fetch logs within a specific time range.
  - Retrieve a weather payload from Blob Storage.
- **Unit Testing**: Ensures functionality using Moq and xUnit.
- **Polly Resilience**: Implements retry and fallback policies.

## Technologies Used

- **.NET 8**
- **Azure Functions** (Timer Trigger, HTTP Trigger)
- **Azure Table Storage**
- **Azure Blob Storage**
- **Polly** (Resilience & Retry Policies)
- **Moq & xUnit** (Unit Testing)

## Setup Instructions

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools
- Azure Storage Emulator / Azure Storage Account
- OpenWeatherMap API Key ([Sign up here](https://home.openweathermap.org/users/sign_up))

### Configuration

1. Clone the repository:
   ```bash
   git clone https://github.com/Diogo-Pires/WeatherAPITest.git
   cd WeatherAPITest
   ```
   
1. Set up your local.settings.json:
   ```bash
   {
   "IsEncrypted": false,
   "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "OpenWeatherApiKey": "YOUR_API_KEY"}}
   ```

 ### Running Locally
1. Start the Azure Storage Emulator (if using local storage).

1. Run the Azure Functions project:
  ```bash
  func start
  ```

## Usage

 - The Timer Trigger function will execute every minute, fetching and storing weather data.
 - Use the provided HTTP endpoints to query logs and retrieve specific weather data payloads.

## Testing
Run the unit tests using the following command:​
```bash
dotnet test
  ```

## License
This project is licensed under the MIT License. See the LICENSE file for details.​

