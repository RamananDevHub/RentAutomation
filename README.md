
![readme_page-0001](https://github.com/user-attachments/assets/c80c7aa0-1a40-48b6-aaf2-5fc364a33844)
![readme_page-0002](https://github.com/user-attachments/assets/da04fab8-9b87-445b-b57e-5cdca9fbabec)

# Tenant Management System

## Overview

The **Tenant Management System** is a web application designed to facilitate the management of tenant information, billing calculations, and historical data tracking.
This system allows property managers to efficiently handle tenant records, calculate electricity bills based on usage, generate bills, and view historical billing data.
The application is built using ASP.NET Core MVC and Entity Framework Core, ensuring a robust and scalable solution.

## Table of Contents

- [Features]
- [Technologies Used]
- [Installation]
- [Usage]
- [API Endpoints]
- [Database Schema]
- [Contributing]
- [License]
- [Contact]

## Features

- **Tenant Management**: Create, edit, and delete tenant records with essential details such as name, contact information, and billing preferences.
- **Electricity Bill Calculation**: Automatically calculate electricity bills based on current and previous month readings, including special calculations for specific tenants.
- **Bill Generation**: Generate detailed bills for tenants, including breakdowns of electricity usage, rent, and water charges.
- **Historical Data Tracking**: View and analyze historical billing data for each tenant, allowing for better financial tracking and reporting.
- **Revenue Reports**: Generate reports on total revenue collected from tenants over specified periods.
- **User -Friendly Interface**: A clean and intuitive web interface for easy navigation and management of tenant information.
- **Error Handling**: Comprehensive error handling to ensure a smooth user experience, including validation for readings and billing periods.

## Technologies Used

- **ASP.NET Core MVC**: Framework for building web applications.
- **Entity Framework Core**: ORM for database interactions.
- **SQL Server**: Database management system for storing tenant and billing data.
- **HTML/CSS/JavaScript**: Front-end technologies for building the user interface.
- **Bootstrap**: CSS framework for responsive design.

## Installation

To set up the project locally, follow these steps:

1. **Clone the repository**:
   ```bash
   git clone https://github.com/yourusername/your-repo-name.git
   cd your-repo-name
   dotnet restore
   dotnet ef database update
   dotnet run

USGAE 

Creating a Tenant
Navigate to the Create Tenant page.
Fill in the required fields, including tenant name, phone number, rent, and other relevant details.
Submit the form to create a new tenant record.
Calculating Electricity Bill
Select a tenant from the Tenant List.
Navigate to the Calculate Bill page.
Enter the current and previous month readings.
Submit the form to calculate the bill. The system will handle any necessary validations.
Viewing Bills
Go to the View Bills section.
Select a tenant to see all bills generated for that tenant, including details like billing date and total amount.
Viewing Historical Data
Access the View History page for a specific tenant.
Review all past bills and their details.
Editing Tenant Details
Navigate to the Edit page for a tenant.
Update the necessary fields and submit the form to save changes.
Deleting a Tenant
Use the Delete option to remove a tenant from the system.
Confirm the deletion to permanently remove the tenant record.
Generating PDF Reports
Navigate to the relevant section (e.g., View Bills).
Click on the Generate PDF button to download a PDF report of the selected data.
API Endpoints
Here are some of the key API endpoints available in the application:

GET /Tenant/Create: Display the form to create a new tenant.
POST /Tenant/Create : Create a new tenant record in the database.
GET /Tenant/Edit/{id}: Display the form to edit an existing tenant.
POST /Tenant/Edit/{id}: Update the tenant record with the provided details.
GET /Tenant/Delete/{id}: Display the confirmation page for deleting a tenant.
POST /Tenant/Delete/{id}: Remove the tenant record from the database.
GET /Tenant/CalculateBill/{id}: Display the bill calculation form for a specific tenant.
POST /Tenant/CalculateBill/{id}: Calculate and generate the bill based on the provided readings.
GET /Tenant/ViewBills/{id}: Retrieve and display all bills for a specific tenant.
GET /Tenant/ViewHistory/{id}: Show historical billing data for a tenant.
GET /Tenant/GeneratePdf/{id}: Generate a PDF report for the selected tenant's billing information.
Database Schema
The database schema consists of the following tables:

Tenants: Stores tenant information including name, contact details, and billing preferences.
Bills: Contains billing records associated with each tenant, including amounts, dates, and readings.
ElectricityReadings: Records the electricity usage readings for each tenant, allowing for accurate bill calculations.
Contributing
Contributions are welcome! If you would like to contribute to this project, please follow these steps:

Fork the repository.
Create a new branch for your feature or bug fix.
Make your changes and commit them with clear messages.
Push your branch to your forked repository.
Submit a pull request detailing your changes.
License
This project is licensed under the MIT License. See the LICENSE file for more details.

Contact
For any inquiries or feedback, please reach out to:

Your Name: your.email@example.com
GitHub: yourusername
Feel free to reach out if you have any questions or need further assistance with the Tenant Management System!
