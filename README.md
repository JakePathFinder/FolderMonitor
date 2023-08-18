# FolderMonitor : Micro-Services Question

Folder monitor encorporates 2 Micro-Services:
* **FileListener** (A.K.A _Service A_)- Enables monitoring a folder for files change events (using **FileSystemWatcher**)
  - Exposes APIs to enable monitoring: **AddFolder**, **RemoveFolder** + **GetAllFolders** (Fetch monitored folders)
* **EventManager** (A.K.A _Service B_)- 
  - Persists the emitted FileEvents to memory cache
  - Exposes Query(Print) APIs for the last x events: **LastEventsByFolder**, **LastEventsByEventType** & **LastEvents** (All recent x events)

The solution Utilizes: 
* **Redis** as a high performant, thread safe cached memory
* **RabbitMq** for message queueing of events emitted by **FileListener** to be processed by **EventManager** (A.K.A Pub-Sub)
    
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
Please follow these steps to clone the repository to your local machine:
1. Open your favorite IDE (Preferrably **VisualStudio 2022**)
2. Clone the repository [Here](https://github.com/JakePathFinder/FolderMonitor.git) (Main Brach is ok)
Some IDEs allow you to browse GitHub\AzureDevops:
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/194544a4-bcfb-40b5-b7ec-5f081bf4144f)


## Issuing a security certificate
* Since the services communicate securely using **https**, we need to self-issue a security certificate.
  <br>Developers can issue a **dev certificate** for these exact purposes.
  - For your convenience, i've prepared 2 scripts that handles creating & removing dev certificates for you, using _dotnet_ command.
    <br> The scripts are located under the _certificates_ folder
    <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/a949d56b-04c5-4301-a029-6eeae3a6e1e4)<br>
  - Upon running, an _output_ folder is created with a varonisdevcert.pfx inside
   <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/f6da2982-7682-421b-b94b-b50027534f57)<br>

   For more information about issueing dev certificates See [dotnet-dev-certs](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs)
   > :memo: **Note:** This certificate is copied to the containers upon running.
   > <br>Please verify the _CERTIFICATE_FOLDER_ .env variable is correct
   <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/d21d3f8e-4857-4318-aeb2-978f58fc391c)<br>
   > :warning: **Warning:** Since Dev certificates are not secure, please clear them when you are done.
   > This can be done using _clear_dev_certificates.bat_
   > These certificates are not intended to be used in production.

## Configuring the solution
To make it easy, the solution arrives with the config. files and Environment variables already set up.
> :memo: **Note:** In a real production scenario, these settings should be ommitted from the repo and ought to be populated by the pipeline using a keyvault.

Following are the main configuration files:
* \<Project Folder\>\\appsettings.json:
  <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/633c9a4e-ef1d-4325-b95e-8fab5940116a)<br>
  Contains General configurations such as Logging, Log4Net
  <br>As well as custome configurations for the Common project (**CommonConfig**) and the current project (**AppConfig**)
  <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/c5c6d7b7-23e8-42e7-b449-142487b7c17b)<br>
* .env File
  <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/71985f60-6153-4d5b-873a-eb3048171537)<br>
  Contains environment variables used by docker-compose and the containerized services  
* docker-compose.yml
  <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/a3c0c471-f9ea-45be-aec3-b562ecd4c448)<br>
  Main settings for the container services and container orcestration.
  <br>Provides settings to backend container services, such as name, ports, enviroment variables, volumes and dependencies.

## 1st Time Run
### Set Debug\Run Configuration
* In order to enable the 1-Click setup, we need to set docker-compose as the run configuration.
  <br> We can do so by right clicking on the solution -> Properties -> Startup Project -> select _docker-compose_ as a single startup project.
  <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/fb9e2a7e-bc85-4222-a205-5c04e0741ace)<br>
  After which, we'll be able to run the project using that configuration:
  <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/b43cf668-65ea-4998-8312-7b391ac7ed74)<br>
### Running the project
Running the solution triggers the following flow:
1. docker-compose.yml configuration is being set up, running "docker compose up" behind the scenes. Following which:
   - Docker images are automatically being pulled (Unless the already exist)
   - Docker containers are created and launched for each of the following service:
     + FileListener Service
     + EventHandler Service
     + RabbitMq (varonisrmq)
     + Redis
    The running containers can bee seen at DockerDesktop -> Containers, under docker-compose (a random name suffix is added to allow other containers on the same host)
    <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/e1976c37-8553-40cc-899f-dbfa096f9027)<br>
2. A Swagger page is loaded for the default project (FileListener [@https://localhost:5001/swagger/index.html](https://localhost:5001/swagger/index.html))
   <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/76f16d52-1592-4e8e-85df-400033cd0de0)<br>
   Since only 1 project is automatically launched, we'd need to open the other manually, by duplicating the tab and changing the port to 5003
   EventHandler [@https://localhost:5003/swagger/index.html](https://localhost:5003/swagger/index.html))
    <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/becf9d7d-1c6b-4e04-a4ac-c6d4a97963d3)<br>

   **Congratulations !** We now have an up & Running project !


# Solution & Project Structure
## Solution Structure
The solution has 7 projects:
* 2 micro-services projects, each with its own Test project)
* A **Common** Project (+ CommonTests) for the common resources (Implementing DRY)
* And a virtual "docker-compose" project which incorporates the container orchestration.
 <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/fd764adc-88d4-4685-a90d-c116bbcd9d34)<br>

## Micro-Service Project Structure
### Controllers, Services & Repos: The Onion Architechture
<br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/75009662-60d3-42d7-a1a3-99bb3cf4e18c)<br>
The project is built according to the Onion Architecture (See [More Info]([https://blog.allegro.tech/2023/02/onion-architecture.html](https://medium.com/expedia-group-tech/onion-architecture-deed8a554423))) as reflected by its structure.
Following an API request, the data flows from the Controller, to the service, then to the repo.
```mermaid
graph LR;
A((Controller)) --> B((Service)) --> C((Repo));
```


### DTO and Domain Model
<br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/3ba1c753-7a20-4320-92ac-c1e38bc5cc3e)<br>
Generally speaking, a _Contoller_ endpoint (API) would be recieved as simple parameters or an DTO for complex objects.<br>
It should then be mapped to a **DomainModel**, which is Processed by the _Service_, Moved on to the _Repo_.

In our simple case the controllers params. are simple primitives.<br>
However, as seen in EventHandler's Controller endpoint **LastEvents**, <br>
the List<**Model.FileEvent**> (results) is mapped to a List<**DTO.FileEvent**> prior to returning the response.<br>
<br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/299fd53b-b614-418d-81b8-7ea8dfea9560)<br>

### Program.cs
The entry point for the micro-service.<br>
Utilizing Extension Methods found at the **Common** project the code is kept clear and clean.
<br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/62a44108-0b60-49ce-bbf1-8f11293a1cf3)<br>

* **AddVaronisServices** - Configuring Swagger, Logging (Log4Net), Common Services.<br>
  It also contains the Common Configuration (E.G Redis, RabbitMq Connection strings) for services (mainly singletons) that belong to that project.
*  **UseVaronisServices** - After building the "app", Uses the common elements such as the Swagger and the Middleware

### Middleware
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/804e00a7-24d5-4335-a3e4-275d86d10229)

2 Middlewares exist:
1. **UnhandledExceptionHandlingMiddleware**: Handles exceptions at the top level of the application (outmost part of the pipeline).
   It catches the Exception and retuens an ErrorCode (Selectively 400, in production we would diffrentiate by exception type)<br>
   Utilizing the RequestResponseFactory, it encapsulats a RequestResponse which indicates a failure and the message. <br>
   ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/54b51f73-c401-4e5d-aad0-f8ca45ac70ba)
   > :memo: **Note:** In a real production scenario we would obfoscate the message and log the real exception.
2. **RequestLoggingMiddleware**: Automatically Logs every incoming and outgoing request\response.
   > :memo: **Note:** Cloud native tools such as Application Insights can automatically collect such information.
   






# Troubleshooting
* **Issue:** Resource startup
  Some services use fixed ports (E.G. RabbitMq, using :5672).<br>
  On occasion, a port may already be in use, hence it may prevent a service from starting up.
  Here's an example for such a scenario:
  ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/09637a23-24ad-4c06-aad9-b904f48fb08b)
  **Possible Solutions:**
  1. Sometimes a service takes some time to start up, the container console can be accessed via DockerDektop -> Containers -> selecting the container -> Logs
     <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/bf64b93d-28dc-4b14-9cdc-5371fd73a392)<br>
     <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/2272e10a-c977-4bd9-a09e-283b0184a539)<br>
     At this situation, all we need to do is wait a while for the service to completed initializing. Then _Run_ the solution again.
  2. Cleaning the solution may release resources occuping the port
     <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/8ac9d7ae-84e0-4b40-b82a-5c80762b2e53)<br>
     Shortly, the containers would be cleared and the port should be available.
  3. If the above did not assist, closing and reopening the IDE after a while (also any cmd windows that may have been in use) - can assist with occupying resources in use.
     






