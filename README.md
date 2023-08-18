
# FolderMonitor: Micro-Services Overview

The FolderMonitor solution comprises two micro-services:

* **FileListener** (Also known as _Service A_): 
  - Monitors specified folders for file change events using **FileSystemWatcher**.
  - Provides APIs for folder monitoring management: **AddFolder**, **RemoveFolder**, and **GetAllFolders** to retrieve currently monitored folders.

* **EventManager** (Also known as _Service B_): 
  - Stores emitted FileEvents in memory cache.
  - Offers query APIs for recent events: **LastEventsByFolder**, **LastEventsByEventType**, and **LastEvents**.

Technologies integrated:
* **Redis**: A high-performance, thread-safe memory cache.
* **RabbitMq**: Message queuing system for event transmission from **FileListener** to **EventManager** (Pub-Sub pattern).

## Snapshots

### File Listener (Service A)
[Swagger UI](https://localhost:5001/swagger/index.html)
![File Listener](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/b0a13176-94ab-45da-9d55-bfa8b6184333)

### Event Manager (Service B)
[Swagger UI](https://localhost:5003/swagger/index.html)
![Event Manager](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/5b765a7e-e9f0-47b0-bdef-85e5e1ff2274)

### Backend Services 
(One-click deployment for convenience)
![Backend Services](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/bcbce611-b852-4439-ad6c-ecfe69ee8864)

### Message Queuing (RabbitMq)
![RabbitMq](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/24427024-5214-4cd7-b21c-6b1b7b5dd103)

### Solution Structure
![Solution Structure](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/c082310e-f9b6-49f8-9c90-57d874c73a17)

## Prerequisites

To deploy and run the infrastructure seamlessly, ensure the following are installed:

- .Net 7 [Download here](https://dotnet.microsoft.com/en-us/download)
- Docker Desktop [Download here](https://www.docker.com/products/docker-desktop/)
- Access to [Docker Hub](https://hub.docker.com/) for container image libraries.

## Cloning the Project

1. Launch your preferred IDE (Recommended: **VisualStudio 2022**).
2. Clone the [repository](https://github.com/JakePathFinder/FolderMonitor.git) (Main branch will suffice).

Some IDEs provide GitHub/AzureDevOps integration:
<br>![IDE Integration](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/194544a4-bcfb-40b5-b7ec-5f081bf4144f)

## Security Certificate Handling

The services communicate securely over **https**, necessitating a self-issued security certificate. Developers can generate a **dev certificate**:

- Two scripts are provided in the _certificates_ folder to streamline the process of creating and removing these certificates.
- Executing creates an _output_ directory containing `varonisdevcert.pfx`.
  
> :warning: **Warning:** These certificates are not designed for production use. Remember to clear them when not in use using _clear_dev_certificates.bat_.

## Solution Configuration

The solution comes pre-configured for ease of use. In real-world scenarios, settings should be secured and populated through a pipeline using a key vault. Primary configurations include:

- **appsettings.json** in each project folder: Contains general and custom configurations.
- **.env** file: Houses environment variables for docker-compose and container services.
- **docker-compose.yml**: Essential settings for container services and orchestration.


## 1st Time Run

### Set Debug/Run Configuration

- Enable the 1-Click setup by setting `docker-compose` as the run configuration:
    1. Right-click on the solution.
    2. Navigate to Properties -> Startup Project.
    3. Select `docker-compose` as a single startup project.

    ![Docker Compose Configuration](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/fb9e2a7e-bc85-4222-a205-5c04e0741ace)

    Once set, you'll be ready to run the project using this configuration:

    ![Run Configuration](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/b43cf668-65ea-4998-8312-7b391ac7ed74)

### Running the project

Executing the solution initiates the following process:

1. `docker-compose.yml` configuration sets up, internally executing "docker compose up":
    - Automatically pulls Docker images (if they don't exist).
    - Creates and launches Docker containers for the services: 
        - FileListener Service
        - EventHandler Service
        - RabbitMq (varonisrmq)
        - Redis
  
    View running containers in Docker Desktop -> Containers. They are listed under `docker-compose` (with a random suffix to allow other containers on the same host).

    ![Docker Containers](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/e1976c37-8553-40cc-899f-dbfa096f9027)

2. A Swagger page for the default project, FileListener, is loaded at [https://localhost:5001/swagger/index.html](https://localhost:5001/swagger/index.html).

    ![Swagger Page](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/76f16d52-1592-4e8e-85df-400033cd0de0)

    As only one project auto-launches, you'll need to manually access the other by duplicating the tab and updating the port to 5003 for the EventHandler at [https://localhost:5003/swagger/index.html](https://localhost:5003/swagger/index.html).

    ![EventHandler Swagger](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/becf9d7d-1c6b-4e04-a4ac-c6d4a97963d3)

    **Congratulations!** Your project is now up and running.


# Solution & Project Structure

## Solution Structure
The solution comprises 7 projects:

* 2 micro-services projects (each accompanied by its own Test project)
* A **Common** Project (alongside CommonTests) containing shared resources, embracing the DRY (Don't Repeat Yourself) principle.
* Lastly, a virtual "docker-compose" project facilitates container orchestration.

![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/fd764adc-88d4-4685-a90d-c116bbcd9d34)

## Micro-Service Project Structure

### Controllers, Services & Repos: Embracing the Onion Architecture

![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/75009662-60d3-42d7-a1a3-99bb3cf4e18c)

The project is architected around the Onion Architecture ([More Info](https://medium.com/expedia-group-tech/onion-architecture-deed8a554423)). This design is evident from its structure, where, following an API request, data flows sequentially from the Controller, to the Service, and then to the Repo.

```mermaid
graph LR;
A((Controller)) --> B((Service)) --> C((Repo));
```

### DTO and Domain Model

![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/3ba1c753-7a20-4320-92ac-c1e38bc5cc3e)

Typically, a _Controller_ endpoint (API) receives either simple parameters or a DTO for more intricate objects. These are then transformed into a **DomainModel**, processed by the _Service_, and subsequently directed to the _Repo_.

In our scenario, the parameters passed to controllers are straightforward primitives. Nonetheless, as observed in the EventHandler's Controller endpoint **LastEvents**, the results in the form of List<**Model.FileEvent**> are converted to List<**DTO.FileEvent**> before dispatching the response.

![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/299fd53b-b614-418d-81b8-7ea8dfea9560)

### Program.cs

Program.cs serves as the entry point for the micro-service. By leveraging the Extension Methods found in the **Common** project, the code remains concise and organized.

![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/62a44108-0b60-49ce-bbf1-8f11293a1cf3)

* **AddVaronisServices**: This configures Swagger, sets up Logging (using Log4Net), and integrates Common Services. Furthermore, it houses the Common Configuration (e.g., Redis, RabbitMq Connection strings) for services, primarily those instantiated as singletons.
* **UseVaronisServices**: Post building the "app", this employs shared elements like Swagger and Middleware.

### Middleware

![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/804e00a7-24d5-4335-a3e4-275d86d10229)

The solution integrates two middlewares:

1. **UnhandledExceptionHandlingMiddleware**: Positioned at the topmost tier of the application (outermost segment of the pipeline), it snags Exceptions and produces an ErrorCode. Predominantly, a 400 code is returned. With the assistance of the RequestResponseFactory, it wraps the exception into a RequestResponse, indicating an error accompanied by a message.
   
   ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/54b51f73-c401-4e5d-aad0-f8ca45ac70ba)

   > :memo: **Note:** In a genuine production environment, the exact message would be obscured and the true exception logged.

2. **RequestLoggingMiddleware**: This middleware logs every inbound and outbound request/response.
   
   > :memo: **Note:** Cloud-native utilities, for instance, Application Insights, can automatically gather such data.

### AppConfig.cs (& CommonConfig.cs)

The Application Settings are sourced from the appsettings.json file. To facilitate Dependency Injection for services with **structured configuration**—as opposed to fetching configurations via path strings—a dedicated **AppConfig.cs** class is introduced and registered.

For instance, consider the setting _MaxAllowedFolders_. The journey from its configuration to class instantiation, and ultimately its utilization via DI, is depicted below:

![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/ac3b52ee-c557-48f4-b7e2-6a3d8345f87f)



# Memory-Cached Persistence 

Given the requirement to use memory cache, several options were considered:
* **ConcurrentDictionary**: While it's thread-safe, it's only suitable for single-instance services. Microservices need to be scalable.
* **Entity Framework In-Memory DB**: This is easy to use but introduces overhead and lacks scalability.
* **Redis cache**: Known for its speed, thread safety due to atomic operations, and scalability.

Ultimately, **Redis** was the chosen solution.

## Usage

`IRepo<T>` acts as a Data Abstraction Layer (DAL).
<br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/099b1a6a-0357-4708-9216-985f7ae02c57)

### Within the FileListener Service:

* Active folder monitoring (via Add/Remove folder APIs) persists to a set through the **IDistributedSetRepo** interface. 
  > Note: "Get" methods were deemed unnecessary and excluded from the IRepo. However, a "GetAll" method was introduced to the IDistributedSet interface.
  
  <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/70429a9b-e9e8-41df-8fcb-8afd2cf4e2bb)

* This interface is backed by the `DistributedSerRepoBase` abstract class.

  <br>![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/1c069010-d558-4375-9291-b298fc6d6471)

  * Other types can then inherit from this class, simplifying the process of setting the appropriate key.
    ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/8480307c-106f-4e8e-840d-1902c577e0af)
    > Note: While it's feasible to use SortedSets for indexing in the EventManager's `FileEventRepo`, a straightforward approach was taken due to time constraints.

### Within the EventHandler Service:

* The `IFileEventRepo` interface extends `_IRepo_` with a `_FileEvent_`.
  ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/73f56fb3-bcc0-4bbb-89c7-4fe5166b3f6c)

* This interface is realized by the `FileEventRepo`.
  ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/ba7aa3e9-6ee5-4b34-8a56-68ac7bd73e35)

### Retrieving Last x events (Query/Print) APIs

In the `FileEventRepo`, we need to store all events and retrieve the latest x events sorted by date. This could be for all events, or filtered by folder or event type. As always, there's a trade-off between time & space. Hence, the following approach was adopted:

1. A Hash collection for storing `FileEvent` data.
2. Utilization of SortedSets to index the `FileEvent` (by its ID) to optimize querying.
   * A general SortedSet for storing `FileEvent` IDs, ordered by datetime (ticks).
   * SortedSets for folder and `EventType`, both sorted by datetime (ticks).

From `FileEventRepo`:
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/ee3492a9-cbae-4c9c-b011-51acec41c9ad)

And from `appsettings.json`:
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/1543edeb-893e-4fe8-aa7e-4acdd0acfa72)


# Message Queue Handling

Events are emitted using the **FileListener** service's **FolderMonitoringService** (utilizing `FileSystemWatcher`). However, persistence should be executed by the **EventHandler** Service.

Due to the potential for rapid event emissions, immediate capture is essential to free up resources as quickly as possible. Moreover, since the process of persisting and managing these messages is relatively slower and not time-sensitive, asynchronous processing is preferable.

Message queues offer:

* **Buffering**: To manage surges in emitted events and cater to scenarios where event emissions surpass the system's processing capacity.
* Scalability: Allowing multiple consumers to process messages.
* Features for message retry and DLQ (Dead Letter Queues). (This feature was not utilized in this solution).
* Data encryption capabilities.
* Message preservation, useful in case of service failures.

For these reasons, **RabbitMQ** was chosen due to its simplicity and familiarity.

## Management Console

The selected RabbitMQ image comes with a management console, allowing users to view Exchanges, Queues, and manage messages. Visit [http://localhost:15672/](http://localhost:15672/) (configurable). Authentication credentials (username and password) are specified during container setup via the environment variables: `RABBITMQ_DEFAULT_USER` and `RABBITMQ_DEFAULT_PASS` (these can be found in the `.env` file).
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/232e03f7-cb14-463c-9c22-f3f3d760488a)

The queues showcase the `FileEventEmitted` Queue:
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/c121206c-08a0-4dc9-a880-9b1ddd0d05dd)

## Usage

* The **IMessageQueueService** interface provides basic Send, Subscribe, and Unsubscribe methods.
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/25a3f1bb-755e-417c-a69d-2aee0d4ffabe)

* This interface is realized by **RabbitMqService**.

> :memo: **Note:** RabbitMQ employs Exchanges & Queues with various configurations (Direct, Fanout, etc.). To maintain compatibility with potential future message queue technologies, a `SubscriptionId` (formed from `ExchangeName:QueueName`) is used.
![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/6ac6d305-40bf-463d-bd8f-20dd55ba401e)

# Main System Flow

## FileListener: Adding a folder to be monitored
```mermaid
sequenceDiagram
    participant User as User
    participant FolderController as FolderController
    participant FolderService as FolderService
    participant FolderMonitoringService as FolderMonitoringService
    participant Thread as Thread
    participant FileSystemWatcher as FileSystemWatcher

    User->>FolderController: AddFolder(foldername)
    FolderController->>FolderService: AddFolderAsync(foldername)
    FolderService->>FolderService: ValidateAddFolder(folderName)
    FolderService->>FolderMonitoringService: StartMonitoring(folderName)
    FolderMonitoringService->>Thread: MonitorFolder(folderName)
    Thread->>FolderMonitoringService: Initiate Monitoring
    Thread->>FileSystemWatcher: Initiate
```

## FileListener: File Change Event Emitted Flow
```mermaid
sequenceDiagram
    participant FileSystemWatcher as FileSystemWatcher
    participant FileSystemEventHandler as FileSystemEventHandler
    participant DataFlowActionBlock as DataFlowActionBlock
    participant RabbitMqService as RabbitMqService
    participant Queue as Queue

    FileSystemWatcher->>FileSystemEventHandler: Emit File Change Event
    FileSystemEventHandler->>DataFlowActionBlock: FileSystemEventArgs
    Note over DataFlowActionBlock: Buffered and Parallel Processing
    DataFlowActionBlock->>DataFlowActionBlock: FileEventEmittedMessage(Event, DateTime.UtcNow)
    DataFlowActionBlock->>RabbitMqService: Send(FileEventEmittedMessage)
    RabbitMqService-->>Queue: <Message>
```

## EventHandler: Handle Emitted Event
```mermaid
sequenceDiagram
    participant Queue as Queue
    participant RabbitMqService as RabbitMqService
    participant FileEventHandlerService as FileEventHandlerService
    participant FileEventRepo as FileEventRepo

    FileEventHandlerService->>RabbitMqService: Subscribe()
    RabbitMqService-->>Queue: Observe
    RabbitMqService-->>FileEventHandlerService: <Message>
    FileEventHandlerService->>FileEventHandlerService: Process Message (Check if File)
    FileEventHandlerService->>FileEventRepo: Persist Model.FileEvent
```

# Troubleshooting

## Issue: Resource Startup

Certain services operate on fixed ports (e.g., RabbitMq, which uses port :5672). Occasionally, these ports might be occupied, which could prevent the respective service from starting up. The following illustrates a situation where this problem arises:

![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/09637a23-24ad-4c06-aad9-b904f48fb08b)

### Possible Solutions:

1. **Delayed Service Initialization:** Sometimes, a service may require additional time to initialize. You can monitor the initialization progress by accessing the container's console. To do this, navigate to Docker Desktop -> Containers -> select the desired container -> Logs.

    ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/bf64b93d-28dc-4b14-9cdc-5371fd73a392)

    ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/2272e10a-c977-4bd9-a09e-283b0184a539)

    In such cases, it's advisable to patiently wait for the service to complete its initialization process. Once done, you can attempt to run the solution once more.

2. **Clean the Solution:** Cleaning the solution can help free up the ports that are currently occupied.

    ![image](https://github.com/JakePathFinder/FolderMonitor/assets/59265424/8ac9d7ae-84e0-4b40-b82a-5c80762b2e53)

    After performing this action, the containers will be cleared shortly, and the port should become available.

3. **Restart the IDE:** If the above solutions don't resolve the issue, consider shutting down the IDE and reopening it after waiting for a few moments. Additionally, ensure that any command prompt windows or other potential resource-intensive applications are closed, as they might also be occupying the needed resources.





