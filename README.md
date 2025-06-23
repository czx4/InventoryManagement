🧾 Inventory Management System

An ASP.NET Core MVC web application for tracking product sales, managing inventory through shipments, and generating sales reports. This system is built with role-based access control and is designed for use in small to mid-sized retail or warehouse operations.
✨ Features

    User Roles & Authorization

        Admin and Manager roles with full access

        Clerk users restricted to permitted operations

    Sales Management

        Create new sales with automatic stock regulation

        Edit and delete past sales

        View sale history and sale details

    Inventory Control

        Tracks product stock levels via shipments

        Automatically deducts stock on sales and replenishes on deletion

    Reports Dashboard

        Financial summaries for selected date ranges

        Top-selling products overview

        Revenue breakdown by user

    Validation & Transactions

        Uses database transactions to ensure consistency

        Validates stock availability before confirming a sale

🧑‍💻 Tech Stack

    Backend: ASP.NET Core MVC

    ORM: Entity Framework Core

    Authentication: ASP.NET Identity

    Database: SQL Server (or any EF Core-compatible DB)

    Frontend: Razor Views with Bootstrap

📸 Screenshots
![home](https://github.com/user-attachments/assets/74bcb1c5-f430-42c0-927c-cdedd65216ec)

![admin panel](https://github.com/user-attachments/assets/70bed4a8-a7da-4f0b-a45f-dfffd7cc849e)
![edit user](https://github.com/user-attachments/assets/9ac038c4-1381-4b9a-a371-3fcce28c61da)
When editing a user's password, the MustChangePassword flag is set to ensure that the new password is temporary and the user is required to change it on their next login.
![reports](https://github.com/user-attachments/assets/b9b58b05-b55e-4dbb-9207-7c4ff98ca2e6)
![products](https://github.com/user-attachments/assets/3944974a-c918-441a-86fc-a0096d420299)
if the product's stock is low the name cell is highlighted red
![createproduct](https://github.com/user-attachments/assets/c9986a6c-5b75-4d41-9a3e-d9ff12988083)
![sales](https://github.com/user-attachments/assets/119fdfcd-a9e4-465a-b3b7-7ad2e570324c)




⚙️ Setup Instructions

    Clone the Repository

git clone https://github.com/your-username/inventorymanagement.git
cd inventorymanagement

Update the Database

dotnet ef database update

Run the Application

    dotnet run

    Access the App
    Open http://localhost:5000 in your browser.

👥 User Roles

You can seed or manually assign roles via the Admin Panel tab:

    Admin: Full access to sales, reports, and deletion (first admin is created by deafult with admin@admin.local email and Admin@123 password)

    Manager: Similar to Admin, limited user control

    Clerk: Basic usage without admin permissions

📁 Project Structure

    Controllers/ – All MVC controllers for Sales, Reports, etc.

    Models/ – Entity classes for EF Core

    ViewModels/ – View-bound data objects

    Views/ – Razor pages

    Data/ApplicationDbContext.cs – EF Core DbContext configuration

🛡️ Security & Validation
    
    User Roles and Permissions:

    - Admins and Managers have exclusive access to create, edit, and delete categories, products, and suppliers.

    - Only Admins and Managers can edit or delete sales and shipments.

    - Financial reports are visible exclusively to Admins and Managers.

    - Only Admins have the ability to create new Admin users via the admin panel.

    - Managers cannot edit or delete Admin users but are authorized to create new Clerks.

    Transactions ensure that stock levels remain consistent

    Invalid or over-quantity sales are blocked with UI feedback

