# ContactUs API - Complete Implementation

## Overview

This document describes the implementation of a comprehensive ContactUs system that allows administrators to manage contact information and provides public access to active contact details.

## 🎯 Key Features

### ✅ **ContactUs Entity**
- **Email**: Contact email address
- **Phone**: Contact phone number  
- **WhatsApp**: WhatsApp contact number
- **Facebook**: Facebook page URL
- **Instagram**: Instagram profile URL
- **X**: X (formerly Twitter) profile URL
- **TikTok**: TikTok profile URL
- **Active Status**: Only one ContactUs entry can be active at a time
- **Timestamps**: Created and updated timestamps

### ✅ **Public & Admin APIs**
- **Public Endpoint**: Get active contact information (no authentication)
- **Admin Endpoints**: Full CRUD operations (admin authentication required)
- **Active Management**: Set specific ContactUs entry as active

## 🚀 API Endpoints

### **Public Endpoints**

#### **GET /api/contactus/active**
Get the currently active contact information (public access)

**Response**:
```json
{
  "succeeded": true,
  "message": "Active contact us retrieved",
  "data": {
    "email": "contact@nolcarrental.com",
    "phone": "+966123456789",
    "whatsApp": "+966123456789",
    "facebook": "https://facebook.com/nolcarrental",
    "instagram": "https://instagram.com/nolcarrental",
    "x": "https://x.com/nolcarrental",
    "tikTok": "https://tiktok.com/@nolcarrental"
  },
  "errors": [],
  "statusCode": 200
}
```

### **Admin Endpoints** (Require Admin Authentication)

#### **GET /api/contactus**
Get all contact us entries

**Headers**: `Authorization: Bearer {admin_jwt_token}`

**Response**:
```json
{
  "succeeded": true,
  "message": "Contact us list retrieved",
  "data": [
    {
      "id": 1,
      "email": "contact@nolcarrental.com",
      "phone": "+966123456789",
      "whatsApp": "+966123456789",
      "facebook": "https://facebook.com/nolcarrental",
      "instagram": "https://instagram.com/nolcarrental",
      "x": "https://x.com/nolcarrental",
      "tikTok": "https://tiktok.com/@nolcarrental",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z",
      "isActive": true
    }
  ],
  "errors": [],
  "statusCode": 200
}
```

#### **GET /api/contactus/{id}**
Get specific contact us entry by ID

**Headers**: `Authorization: Bearer {admin_jwt_token}`

#### **POST /api/contactus**
Create new contact us entry

**Headers**: 
- `Authorization: Bearer {admin_jwt_token}`
- `Content-Type: application/json`

**Request Body**:
```json
{
  "email": "contact@nolcarrental.com",
  "phone": "+966123456789",
  "whatsApp": "+966123456789",
  "facebook": "https://facebook.com/nolcarrental",
  "instagram": "https://instagram.com/nolcarrental",
  "x": "https://x.com/nolcarrental",
  "tikTok": "https://tiktok.com/@nolcarrental"
}
```

#### **PUT /api/contactus/{id}**
Update existing contact us entry

**Headers**: 
- `Authorization: Bearer {admin_jwt_token}`
- `Content-Type: application/json`

**Request Body** (all fields optional):
```json
{
  "email": "newemail@nolcarrental.com",
  "phone": "+966987654321",
  "whatsApp": "+966987654321",
  "facebook": "https://facebook.com/nolcarrental-new",
  "instagram": "https://instagram.com/nolcarrental-new",
  "x": "https://x.com/nolcarrental-new",
  "tikTok": "https://tiktok.com/@nolcarrental-new",
  "isActive": true
}
```

#### **DELETE /api/contactus/{id}**
Delete contact us entry

**Headers**: `Authorization: Bearer {admin_jwt_token}`

#### **POST /api/contactus/{id}/set-active**
Set specific contact us entry as active (deactivates all others)

**Headers**: `Authorization: Bearer {admin_jwt_token}`

#### **GET /api/contactus/count**
Get total count of contact us entries

**Headers**: `Authorization: Bearer {admin_jwt_token}`

## 🔧 Implementation Details

### **Database Schema**
```sql
CREATE TABLE ContactUs (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Email nvarchar(255) NOT NULL,
    Phone nvarchar(50) NOT NULL,
    WhatsApp nvarchar(50) NOT NULL,
    Facebook nvarchar(255) NOT NULL,
    Instagram nvarchar(255) NOT NULL,
    X nvarchar(255) NOT NULL,
    TikTok nvarchar(255) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive bit NOT NULL DEFAULT 1
);
```

### **Key Components**

1. **ContactUs Entity** (`src/NOL.Domain/Entities/ContactUs.cs`)
   - Contains all social media and contact fields
   - Includes timestamps and active status

2. **ContactUs DTOs** (`src/NOL.Application/DTOs/ContactUsDto.cs`)
   - `ContactUsDto`: Full entity data
   - `CreateContactUsDto`: Creation request with validation
   - `UpdateContactUsDto`: Update request with optional fields
   - `PublicContactUsDto`: Public-facing data (no admin fields)

3. **ContactUs Repository** (`src/NOL.Infrastructure/Repositories/ContactUsRepository.cs`)
   - CRUD operations
   - Active status management
   - Specialized queries

4. **ContactUs Service** (`src/NOL.Application/Features/ContactUs/ContactUsService.cs`)
   - Business logic
   - Active status enforcement (only one active at a time)
   - Error handling and validation

5. **ContactUs Controller** (`src/NOL.API/Controllers/ContactUsController.cs`)
   - Public and admin endpoints
   - Authorization and validation
   - Proper HTTP status codes

### **Business Logic**

#### **Active Status Management**
- Only one ContactUs entry can be active at a time
- When creating a new entry, it automatically becomes active
- When setting an entry as active, all others become inactive
- Public API always returns the currently active entry

#### **Validation**
- Email addresses must be valid format
- Phone numbers must be valid format
- Social media URLs must be valid URLs
- All fields have appropriate length limits

## 📱 Frontend Integration Examples

### **React/JavaScript**
```javascript
// Get active contact information (public)
const getActiveContactUs = async () => {
  try {
    const response = await fetch('/api/contactus/active');
    const result = await response.json();
    
    if (result.succeeded) {
      return result.data;
    }
  } catch (error) {
    console.error('Error fetching contact info:', error);
  }
};

// Admin: Create new contact us entry
const createContactUs = async (contactData, adminToken) => {
  try {
    const response = await fetch('/api/contactus', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${adminToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(contactData)
    });
    
    const result = await response.json();
    return result;
  } catch (error) {
    console.error('Error creating contact us:', error);
  }
};
```

### **Mobile App Integration**
```swift
// iOS Swift example
struct ContactUsInfo: Codable {
    let email: String
    let phone: String
    let whatsApp: String
    let facebook: String
    let instagram: String
    let x: String
    let tikTok: String
}

func getActiveContactUs() async throws -> ContactUsInfo {
    let url = URL(string: "https://api.nolcarrental.com/api/contactus/active")!
    let (data, _) = try await URLSession.shared.data(from: url)
    let response = try JSONDecoder().decode(ApiResponse<ContactUsInfo>.self, from: data)
    return response.data
}
```

## 🔒 Security Features

- **Public Access**: Only active contact information is publicly accessible
- **Admin Protection**: All management operations require admin authentication
- **Input Validation**: Comprehensive validation for all fields
- **SQL Injection Protection**: Entity Framework provides protection
- **XSS Protection**: Proper input sanitization

## 🎨 Usage Scenarios

### **Website Footer**
Display active contact information in website footer:
```javascript
const contactInfo = await getActiveContactUs();
document.getElementById('contact-email').textContent = contactInfo.email;
document.getElementById('contact-phone').textContent = contactInfo.phone;
// Set social media links...
```

### **Mobile App Contact Screen**
Show contact options with direct links to social media and communication apps.

### **Admin Dashboard**
Manage multiple contact configurations and switch between them as needed.

## ✅ Implementation Status

- [x] ContactUs entity created with all required fields
- [x] Database migration generated
- [x] Repository layer implemented with specialized queries
- [x] Service layer with business logic and validation
- [x] Controller with public and admin endpoints
- [x] DTOs with proper validation attributes
- [x] Dependency injection registration
- [x] Active status management logic
- [x] Comprehensive error handling
- [x] API documentation complete

The ContactUs API is **production-ready** and provides a complete solution for managing contact information with both public access and administrative control! 🚀📞
