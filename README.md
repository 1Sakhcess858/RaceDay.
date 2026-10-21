RaceDay - Event Management System



Overview

RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community.



System Roles



Organiser

\- Create, edit, and delete events

\- Manage event categories

\- Capture participant results

\- View all enrolments



Participant

\- Create an account and manage profile

\- Browse upcoming events

\- Enter events by selecting a category

\- View personal enrolment history

\- Track personal race results



Technology Stack

\- Backend: C# with ASP.NET Core Web API (.NET 8)

\- Database: SQL Server with Entity Framework Core

\- Frontend: ASP.NET Core MVC

\- Cloud Storage: Azure Blob Storage (Part 3)

\- Containerization: Docker

\- Version Control: GitHub

\- CI/CD: GitHub Actions



Project Structure

RaceDay./

&#x20; .github/workflows/ci.yml

&#x20; docs/

&#x20;   ERD\_RaceDay.png

&#x20;   API\_Endpoint\_Plan.md

&#x20;   RaceDay\_Database.sql

&#x20;   ci-build-success.png

&#x20; src/

&#x20;   RaceDay.API/

&#x20;   RaceDay.API.Tests/

&#x20;   RaceDay.Web/

&#x20; README.md

&#x20; .gitignore

&#x20; LICENSE.txt



Setup Instructions



1\. Clone the repository

2\. Open the solution in Visual Studio 2022

3\. Update the connection string in appsettings.json (RaceDay.API)

4\. Run Update-Database in the Package Manager Console

5\. Press F5 to run both the API and the MVC applications



CI/CD Pipeline

GitHub Actions workflow validates:

\- /docs folder exists

\- ERD image is present

\- Endpoint plan is present

\- SQL script is present

\- API builds and tests pass



Video Presentation

(Add your YouTube link here)



Docker Instructions

To run the MVC application in a Docker container:



1\. Ensure Docker Desktop is running.

2\. Navigate to the MVC project folder: cd src/RaceDay.Web

3\. Build the Docker image: docker build -t raceday-web .

4\. Run the container: docker run -d -p 8080:8080 --name raceday-container raceday-web

5\. Access the application at: http://localhost:8080

