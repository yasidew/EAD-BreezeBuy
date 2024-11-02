# BreezeBuy eCommerce Platform

**BreezeBuy** is a comprehensive **eCommerce web application** designed for seamless online shopping experiences. Developed with a **client-server architecture**, the backend is powered by **.NET Core**, while the frontend is built using **React**. The platform features robust capabilities for order management, inventory tracking, user management, and payment processing, all supported by **MongoDB** as the database solution.

## Features
- **Order Management**: Create, track, and manage customer orders efficiently.
- **Inventory Management**: Maintain real-time stock levels, add new products, and receive low-stock alerts.
- **Product Management**: Comprehensive catalog management, including product details and categorization.
- **User Management**: Secure authentication and role-based user profiles.
- **Payment Integration**: Secure payment processing and transaction tracking.
- **Data Visualization Dashboard**: Real-time insights using **MongoDB Charts** for key metrics and analytics.
- **Advanced Search**: Quick and easy search functionality for products and orders.

## Tech Stack
### Backend
- **.NET Core (ASP.NET Web API)**: Framework for developing RESTful APIs.
- **MongoDB**: NoSQL database for efficient data management.
- **JWT (JSON Web Tokens)**: Authentication and authorization management.
- **Docker** (optional): For containerized deployment and scaling.

### Frontend
- **React**: Framework for building a dynamic and responsive user interface.
- **Axios**: For seamless HTTP requests to backend APIs.
- **React Router**: Facilitates efficient routing within the application.
- **Bootstrap/CSS**: Styling for a user-friendly interface.
- **React Toastify**: Notifications and alerts for improved user interaction.

## How to Run

1. **Clone the repository**:
   ```bash
   git clone https://github.com/yasidew/EAD-BreezeBuy.git
   ```

2. **Backend Setup**:
   - Ensure **MongoDB** is running locally or remotely.
   - Navigate to the backend directory and build the project:
     ```bash
     dotnet build
     dotnet run
     ```

3. **Frontend Setup**:
   - Install the required dependencies and run the development server:
     ```bash
     git clone https://github.com/yasidew/BreezeBuy-Frontend.git
     npm install
     npm start
     ```
---

