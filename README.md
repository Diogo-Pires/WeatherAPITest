# Azure Weather Logging System

## Overview

This project is an Azure Function-based system that periodically fetches weather data from OpenWeatherMap and logs the data into Azure Storage. The system consists of:

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
   ```sh
   git clone https://github.com/yourusername/azure-weather-logger.git
   cd azure-weather-logger
   
1. Set up your local.settings.json:
   ```sh
   {
   "IsEncrypted": false,
   "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "OpenWeatherApiKey": "YOUR_API_KEY"}}

 ### Running Locally
1. Start the Azure Storage Emulator (if using local storage).

1. Run the Azure Functions project:
```sh
func start

### Running Locally
