# MVC Movie Project

## Overview

This project is a Movie Management application built using ASP.NET Core MVC. It allows users to view, manage, and interact with movie records, including adding new movies, editing existing ones, and deleting records. The application is structured using the Model-View-Controller (MVC) design pattern. Additionally, it can call an external movie API to list movies and save them to your database.

## Features

- **Movie Listings:** View a list of movies with details such as title, release date, and genre.
- **Add New Movies:** Add new movies to the database through a user-friendly form.
- **Edit Movies:** Update details of existing movies.
- **Delete Movies:** Remove movies from the database.
- **Data Storage:** Utilizes Entity Framework Core for data access and management.
- **API Integration:** Call an external movie API to list movies and save them to your database.

## Technologies

- **ASP.NET Core MVC**: Framework used to build the web application.
- **Entity Framework Core**: ORM for database operations.
- **SQL Server**: Database server (can be replaced with any other database if needed).
- **Bootstrap**: For styling and responsive design.

## Installation

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/kwincie-005/Movies-api-call.git
Navigate to the Project Directory:

bash

cd
 Movies-api-call
Restore Dependencies:

dotnet restore
Update Connection Strings:

Open appsettings.json and update the connection string to match your SQL Server configuration.

Run Migrations:

dotnet ef database update
Run the Application:

dotnet run
Access the Application:

Open a web browser and navigate to http://localhost:5000 to access the application.

API Integration
To utilize the external movie API, update the API key and endpoint in the configuration files as needed. The application includes functionality to fetch and display movie data from the API.

Contributing
Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug reports.

License
This project is licensed under the MIT License. See the LICENSE file for details.

 
