![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/ba2362e3-311b-4be0-bef0-a7a5bc67236f)# FolderMonitor : Micro-Services Question

Folder monitor encorporates 2 Micro-Services:
* **FileListener** (A.K.A _Service A_)- Enables monitoring a folder for files change events (using **FileSystemWatcher**)
  - Exposes APIs to enable monitoring: **AddFolder**, **RemoveFolder** + **GetAllFolders** (Fetch monitored folders)
* **EventManager** (A.K.A _Service B_)- 
  - Persists the emitted FileEvents to memory cache
  - Exposes Query(Print) APIs for the last x events: **LastEventsByFolder**, **LastEventsByEventType** & **LastEvents** (All recent x events)
    
## Some images
File Listener Swagger (Service A) [@https://localhost:5001/swagger/index.html](https://localhost:5001/swagger/index.html)
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/b0a13176-94ab-45da-9d55-bfa8b6184333)


Event Manager Swagger (Service B) [@https://localhost:5003/swagger/index.html](https://localhost:5003/swagger/index.html)
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/5b765a7e-e9f0-47b0-bdef-85e5e1ff2274)

Backend Services (1-Click Deployment)
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/bcbce611-b852-4439-ad6c-ecfe69ee8864)
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/91c368d8-dfc3-404c-a112-4c5d3f4c6229)

Message Queuing (RabbitMq)
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/24427024-5214-4cd7-b21c-6b1b7b5dd103)

Solution Structure
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/c082310e-f9b6-49f8-9c90-57d874c73a17)

## Prerequisites
Deploying & Running the entire infrastructure is done in 1-click thanks to _Docker-Compose_

In order to enjoy these fruits, please verify you have the following prerequisites set up:
* .Net 7 Installed [Get it here](https://dotnet.microsoft.com/en-us/download)
* Docker Desktop Installed [Get it here](https://www.docker.com/products/docker-desktop/)
* Access to [Docker Hub](https://hub.docker.com/) container images library

## Cloning the project
Please follow these steps to get your code up & running:
1. Open your favorite IDE (Preferrably **VisualStudio 2022**)
2. Clone the repository [Here](https://github.com/JakePathFinder/FolderMonitor.git) (Main Brach is ok)
Some IDEs allow you to browse GitHub\AzureDevops:
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/194544a4-bcfb-40b5-b7ec-5f081bf4144f)

## Configuring the solution
Configuration files and Environment variables are attached to the solution for an early kickoff.
Note: In a real production scenario, these settings should be ommitted from the repo and be populated by the pipeling using a keyvault.

Following are the main configuration files:
* <Project Folder>\appsettings.json:
  ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/633c9a4e-ef1d-4325-b95e-8fab5940116a)
  Contains General configurations such as Logging, Log4Net
  As well as custome configurations for the Common project (**CommonConfig**) and the current project (**AppConfig**)
  ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/c5c6d7b7-23e8-42e7-b449-142487b7c17b)
* .env File
  ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/71985f60-6153-4d5b-873a-eb3048171537)
  Contains environment variables used by docker-compose and the containerized services  
* docker-compose.yml
  ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/a3c0c471-f9ea-45be-aec3-b562ecd4c448)
  Main settings for the container services and container orcestration.
  Provides settings to backend container services, such as name, ports, enviroment variables, volumes and dependencies.


## First Time Run
  WIP





