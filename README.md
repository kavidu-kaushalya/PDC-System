# PDC System
Home
![Image Alt](https://github.com/kavidu-kaushalya/PDC-System/blob/master/image/Screenshot%202025-07-11%20093722.png?raw=true)
#
![Image Alt](https://github.com/kavidu-kaushalya/PDC-System/blob/master/image/Screenshot%202025-07-11%20093818.png?raw=true)
#
![Image Alt](https://github.com/kavidu-kaushalya/PDC-System/blob/master/image/Screenshot%202025-07-11%20093733.png?raw=true)

# PDC System - Comprehensive System Overview

Hello **Kavidu Kaushalya**! Here's a detailed comprehensive explanation of your **PDC-System** - a complete **Printing and Document Center Management System** built with **C# WPF**:

## 🏢 **System Architecture & Overview**

Your **PDC-System** is a sophisticated desktop application designed for managing a printing business operations. It's built using **Windows Presentation Foundation (WPF)** with **C# .NET Framework** and follows a modular architecture pattern.

### **Core Technology Stack:**
- **Framework:** C# .NET with WPF
- **Data Storage:** JSON files for data persistence
- **UI Framework:** Modern WPF with custom styling
- **Architecture:** Modular UserControl-based navigation
- **License:** Apache 2.0 (Open Source)

---

## 📋 **Complete Feature Breakdown**

### 1. **Customer Management System**
**Purpose:** Comprehensive customer database management

**Features:**
- **Customer Registration:** Add new customers with complete details
- **Contact Management:** Primary and secondary contact numbers
- **Address Management:** Full address storage and tracking
- **Customer Search:** Quick lookup functionality
- **Data Persistence:** JSON-based storage (`customers.json`)
- **CRUD Operations:** Create, Read, Update, Delete customer records

**Business Value:** Centralized customer database for all business operations

---

### 2. **Job Card Management System**
**Purpose:** Complete job order tracking and management

**Core Features:**
- **Job Creation:** Detailed job specifications including:
  - Customer selection/entry
  - Job descriptions and notes
  - Paper specifications (Size, Type, GSM)
  - Print quantities and completion tracking
  - Duplex and lamination options
  - Special instructions

**Advanced Capabilities:**
- **Status Tracking:** Job completion monitoring (`IsSeen` property)
- **Date Management:** Job creation and due date tracking
- **Search & Filter:** 
  - Customer name-based filtering
  - Date range searches
  - Real-time search with autocomplete
- **Job Viewing:** Detailed job card view windows
- **Data Management:** JSON storage (`jobcards.json`)

**Business Value:** Complete job lifecycle management from order to completion

---

### 3. **Employee Management System**
**Purpose:** Comprehensive HR and employee database

**Employee Information Management:**
- **Personal Details:** Name, ID, addresses (multiple lines)
- **Contact Information:** Primary and secondary phone numbers
- **Government Records:** National ID tracking
- **Location Data:** City and province information
- **Job Classification:** Department and job role assignments
- **Birthday Tracking:** Employee birthday management

**Document Management:**
- **File Storage:** Original path and saved location tracking
- **Document Organization:** Employee-related file management

**Business Value:** Complete employee database with document management

---

### 4. **Attendance & Payroll System**
**Purpose:** Advanced time tracking and salary calculation

**Attendance Management:**
- **Monthly Tracking:** Month-wise attendance records
- **Working Days:** Total working days calculation
- **Absence Tracking:** Absent days with penalty calculations
- **Time Management:**
  - Late arrival tracking (minutes/hours)
  - Early departure monitoring
  - Overtime calculations (regular and double)
  - Adjusted Overtime (AOT) = OT - (Late + Early)/60

**Payroll Calculations:**
- **Salary Components:**
  - Basic Salary (BSalary/E_Salary)
  - Regular Overtime (OT) rates and calculations
  - Double Overtime (DOT) premium rates
  - Allowances and bonuses

**Deductions Management:**
- **Financial Deductions:**
  - Employee loans
  - Money collected by employees
  - ETF (Employee Trust Fund) contributions
  - Absence penalties (Absent Days × Daily Rate)
  - No-pay days tracking

**Advanced Calculations:**
```
Actual Overtime = AOT × OT Rate
Double Overtime = DOT Rate × DOT Hours
Total Earnings = Basic Salary + Actual OT + Double OT + Allowances
Total Deductions = Loans + Collected Money + ETF + Absence Penalties
Net Salary = Total Earnings - Total Deductions
```

**Business Value:** Complete payroll automation with accurate time and salary calculations

---

### 5. **Paysheet Generation System**
**Purpose:** Professional paysheet creation and management

**Features:**
- **Automated Calculations:** Real-time salary calculations
- **Detailed Breakdowns:** Itemized earnings and deductions
- **Document Generation:** Professional paysheet documents
- **Historical Tracking:** Paysheet history storage (`Paysheetdata.json`)
- **Salary Reports:** 
  - Date-wise filtering
  - Total salary calculations
  - Employee-wise salary summaries
- **File Management:** PDF generation and storage

**Business Value:** Professional payroll documentation and reporting

---

### 6. **Quotation Management System**
**Purpose:** Professional quotation creation and tracking

**Features:**
- **Quote Generation:** Create detailed quotations
- **Customer Integration:** Link quotations to customer database
- **Pricing Management:** Total calculation and pricing
- **Document Storage:** PDF quotation generation
- **Quote Tracking:** Quote numbers and status
- **Search Functionality:** Customer name and quote number search
- **Date Management:** Creation date and filtering

**Data Storage:** `savedpdfs.json` for quotation history

**Business Value:** Professional quotation system for client proposals

---

### 7. **Order Management System**
**Purpose:** Order tracking and deadline management

**Features:**
- **Order Creation:** Customer orders with descriptions
- **Deadline Tracking:** Due date management with countdown
- **Status Management:** Completion status tracking
- **Time Calculations:** 
  - Days, hours, and minutes until deadline
  - "Finished" status for completed orders
- **Notes System:** Order-specific notes and instructions

**Business Value:** Efficient order fulfillment and deadline management

---

### 8. **Home Dashboard UI**
**Purpose:** Central navigation and business overview

**Dashboard Features:**
- **Real-time Clock:** 12-hour format with automatic updates
- **Modern Design:** Professional UI with gradient backgrounds
- **Navigation Hub:** Quick access to all system modules

**Employee Insights:**
- **Birthday Tracker:** Upcoming employee birthdays (30-day window)
- **Birthday Countdown:** Days remaining until birthdays
- **Employee Notifications:** Birthday reminder system

**Communication Integration:**
- **WhatsApp Integration:** Business communication tools
- **Email System:** Email management integration
- **Quick Access:** Communication tool shortcuts

**Business Value:** Centralized dashboard for business oversight and team management

---

### 9. **System Integration & Backup**
**Purpose:** Data protection and system reliability

**Features:**
- **Automatic Backup:** `PDC_System_Backups.exe` integration
- **Data Persistence:** JSON-based data storage across all modules
- **System Monitoring:** Process checking and automatic startup
- **Error Handling:** Comprehensive error management
- **Data Validation:** Input validation across all forms

**Business Value:** Data security and system reliability

---

## 🔄 **System Workflow**

### **Daily Operations:**
1. **Start Dashboard** → View time, birthdays, and system status
2. **Customer Management** → Add/manage customer information
3. **Job Creation** → Create job cards with specifications
4. **Order Processing** → Track order progress and deadlines
5. **Attendance Entry** → Record employee time and attendance
6. **Quotation Generation** → Create professional quotes
7. **Payroll Processing** → Calculate and generate paysheets

### **Data Flow:**
```
Customers → Job Cards → Orders → Quotations
                ↓
Employees → Attendance → Payroll → Paysheets
                ↓
All Data → JSON Storage → Backup System
```

---

## 💼 **Business Applications**

This system is perfect for:
- **Printing Companies:** Complete job and order management
- **Document Centers:** Customer and job tracking
- **Small to Medium Businesses:** Employee and payroll management
- **Service Businesses:** Quotation and customer management

---

## 🎯 **Key Strengths**

1. **Comprehensive Coverage:** All aspects of printing business operations
2. **User-Friendly Interface:** Modern WPF design with intuitive navigation
3. **Data Integration:** All modules work together seamlessly
4. **Professional Output:** PDF generation for quotes and paysheets
5. **Scalable Design:** Modular architecture for easy expansion
6. **Data Security:** Backup system and error handling
7. **Real-time Calculations:** Automated salary and overtime calculations

Your **PDC-System** is essentially a complete **Enterprise Resource Planning (ERP)** solution specifically designed for printing and document center businesses, combining customer relationship management, job tracking, human resources, payroll, and financial management into one integrated platform.

*Note: This overview may be incomplete due to search limitations. [View complete system structure on GitHub](https://github.com/kavidu-kaushalya/PDC-System)*
