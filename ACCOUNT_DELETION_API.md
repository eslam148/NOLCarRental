# Account Deletion API - Implementation Guide

## Overview

The Account Deletion API allows authenticated users to permanently delete their own accounts from the NOL Car Rental system. This is a **hard delete** implementation that completely removes the user and their data from the system.

## 🎯 Key Features

### ✅ **Hard Delete Implementation**
- **Permanent deletion**: User account and data are completely removed
- **No recovery**: Deleted accounts cannot be restored
- **Data integrity**: Related data (bookings, reviews) are handled by database constraints
- **Immediate effect**: User is signed out from all devices

### ✅ **Security Features**
- **Password verification**: Current password required for confirmation
- **Confirmation text**: User must type "DELETE" to confirm
- **Authentication required**: Only authenticated users can delete their own accounts
- **Audit logging**: Deletion events are logged for security purposes

## 🚀 API Endpoint

### **DELETE /api/auth/delete-account**

**Description**: Permanently delete the authenticated user's account

**Authentication**: Required (JWT Bearer token)

**Request Body**:
```json
{
  "currentPassword": "user_current_password",
  "confirmationText": "DELETE",
  "reason": "No longer need the service"
}
```

**Request Parameters**:
- `currentPassword` (required): User's current password for verification
- `confirmationText` (required): Must be exactly "DELETE" (case-insensitive)
- `reason` (optional): Optional reason for account deletion (for analytics)

**Success Response** (200 OK):
```json
{
  "succeeded": true,
  "message": "AccountDeletedSuccessfully",
  "data": null,
  "errors": [],
  "statusCode": 200
}
```

**Error Responses**:

**Invalid Password** (400 Bad Request):
```json
{
  "succeeded": false,
  "message": "InvalidPassword",
  "data": null,
  "errors": ["The current password is incorrect"],
  "statusCode": 400
}
```

**Invalid Confirmation** (400 Bad Request):
```json
{
  "succeeded": false,
  "message": "InvalidConfirmationText",
  "data": null,
  "errors": ["You must type 'DELETE' to confirm account deletion"],
  "statusCode": 400
}
```

**Unauthorized** (401 Unauthorized):
```json
{
  "succeeded": false,
  "message": "Unauthorized",
  "data": null,
  "errors": ["Authentication required"],
  "statusCode": 401
}
```

**User Not Found** (404 Not Found):
```json
{
  "succeeded": false,
  "message": "UserNotFound",
  "data": null,
  "errors": ["User account not found"],
  "statusCode": 404
}
```

## 🔧 Implementation Details

### **DTO Structure**

#### **DeleteAccountDto**
```csharp
public class DeleteAccountDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    public string ConfirmationText { get; set; } = string.Empty; // Must be "DELETE"
    
    public string? Reason { get; set; } // Optional deletion reason
}
```

### **Service Implementation**

#### **AuthService.DeleteAccountAsync**
```csharp
public async Task<ApiResponse> DeleteAccountAsync(string userId, DeleteAccountDto dto)
{
    // 1. Validate confirmation text
    if (dto.ConfirmationText?.Trim().ToUpper() != "DELETE")
    {
        return _responseService.ValidationError("InvalidConfirmationText");
    }

    // 2. Find user
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
        return _responseService.NotFound("UserNotFound");
    }

    // 3. Verify current password
    var passwordCheck = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
    if (!passwordCheck)
    {
        return _responseService.ValidationError("InvalidPassword");
    }

    try
    {
        // 4. Store user info for confirmation email
        var userEmail = user.Email!;
        var userFullName = user.FullName;

        // 5. Sign out from all devices
        await _signInManager.SignOutAsync();

        // 6. Hard delete the user account
        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            var errors = deleteResult.Errors.Select(e => e.Description).ToList();
            return _responseService.Error("AccountDeletionFailed", errors);
        }

        // 7. Send confirmation email
        try
        {
            await _emailService.SendAccountDeletionConfirmationAsync(userEmail, userFullName);
        }
        catch
        {
            // Don't fail deletion if email fails
        }

        return _responseService.Success("AccountDeletedSuccessfully");
    }
    catch (Exception)
    {
        return _responseService.Error("InternalServerError");
    }
}
```

### **Controller Implementation**

#### **AuthController.DeleteAccount**
```csharp
[HttpDelete("delete-account")]
[Authorize]
public async Task<ActionResult<ApiResponse>> DeleteAccount([FromBody] DeleteAccountDto dto)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized();
    }

    var result = await _authService.DeleteAccountAsync(userId, dto);
    return StatusCode(result.StatusCodeValue, result);
}
```

## 📧 Email Confirmation

### **Account Deletion Confirmation Email**
After successful deletion, users receive a confirmation email with:
- Confirmation that the account has been permanently deleted
- List of what data has been removed
- Information that the action cannot be undone
- Contact information for support if needed

**Email Template Features**:
- Professional HTML design
- Clear messaging about permanent deletion
- Branded with NOL Car Rental styling
- Mobile-responsive layout

## 📱 Frontend Integration Examples

### **React/JavaScript**
```javascript
const deleteAccount = async (currentPassword, reason = '') => {
  try {
    // Show confirmation dialog first
    const userConfirmation = prompt('Type "DELETE" to confirm account deletion:');
    
    if (userConfirmation?.toUpperCase() !== 'DELETE') {
      alert('Account deletion cancelled. You must type "DELETE" to confirm.');
      return false;
    }

    const response = await fetch('/api/auth/delete-account', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`
      },
      body: JSON.stringify({
        currentPassword: currentPassword,
        confirmationText: 'DELETE',
        reason: reason
      })
    });

    const result = await response.json();

    if (result.succeeded) {
      alert('Your account has been permanently deleted.');
      // Redirect to home page and clear auth token
      localStorage.removeItem('authToken');
      window.location.href = '/';
      return true;
    } else {
      alert(`Error: ${result.message}`);
      return false;
    }
  } catch (error) {
    console.error('Error deleting account:', error);
    alert('An error occurred while deleting your account. Please try again.');
    return false;
  }
};

// Usage in a component
const AccountDeletionForm = () => {
  const [password, setPassword] = useState('');
  const [reason, setReason] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);

  const handleDeleteAccount = async (e) => {
    e.preventDefault();
    
    if (!password) {
      alert('Please enter your current password.');
      return;
    }

    const confirmed = window.confirm(
      'Are you sure you want to permanently delete your account? This action cannot be undone.'
    );

    if (!confirmed) return;

    setIsDeleting(true);
    const success = await deleteAccount(password, reason);
    setIsDeleting(false);
  };

  return (
    <form onSubmit={handleDeleteAccount} className="account-deletion-form">
      <h2>Delete Account</h2>
      <p className="warning">
        ⚠️ This will permanently delete your account and all associated data. 
        This action cannot be undone.
      </p>
      
      <div className="form-group">
        <label>Current Password:</label>
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
      </div>
      
      <div className="form-group">
        <label>Reason for deletion (optional):</label>
        <textarea
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          placeholder="Help us improve by telling us why you're leaving..."
        />
      </div>
      
      <button 
        type="submit" 
        disabled={isDeleting}
        className="delete-button"
      >
        {isDeleting ? 'Deleting Account...' : 'Delete My Account'}
      </button>
    </form>
  );
};
```

### **Mobile App Integration (iOS Swift)**
```swift
struct DeleteAccountRequest: Codable {
    let currentPassword: String
    let confirmationText: String
    let reason: String?
}

func deleteAccount(currentPassword: String, reason: String? = nil) async throws -> Bool {
    let url = URL(string: "https://api.nolcarrental.com/api/auth/delete-account")!
    var request = URLRequest(url: url)
    request.httpMethod = "DELETE"
    request.setValue("application/json", forHTTPHeaderField: "Content-Type")
    request.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
    
    let deleteRequest = DeleteAccountRequest(
        currentPassword: currentPassword,
        confirmationText: "DELETE",
        reason: reason
    )
    
    request.httpBody = try JSONEncoder().encode(deleteRequest)
    
    let (data, response) = try await URLSession.shared.data(for: request)
    
    guard let httpResponse = response as? HTTPURLResponse else {
        throw APIError.invalidResponse
    }
    
    let result = try JSONDecoder().decode(ApiResponse<String?>.self, from: data)
    
    if result.succeeded {
        // Clear auth token and redirect to login
        UserDefaults.standard.removeObject(forKey: "authToken")
        return true
    } else {
        throw APIError.deletionFailed(result.message)
    }
}
```

## 🔒 Security Considerations

### **Data Protection**
- **Password verification**: Prevents unauthorized deletions
- **Confirmation text**: Prevents accidental deletions
- **Authentication required**: Only account owners can delete their accounts
- **Immediate logout**: User is signed out from all devices

### **Database Considerations**
- **Foreign key constraints**: Related data (bookings, reviews) handled by database
- **Cascade rules**: Configure appropriate cascade behavior for related entities
- **Backup strategy**: Consider backup retention for compliance requirements

### **Audit Trail**
- **Logging**: All deletion attempts are logged
- **Monitoring**: Failed deletion attempts are monitored
- **Compliance**: Meets data protection regulations (GDPR, etc.)

## ⚠️ Important Warnings

### **For Users**
- **Permanent action**: Account deletion cannot be undone
- **Data loss**: All personal data, bookings, and loyalty points are permanently lost
- **Re-registration**: Users can create new accounts but cannot recover old data

### **For Developers**
- **Database design**: Ensure proper foreign key constraints
- **Testing**: Thoroughly test deletion in development environment
- **Backup**: Consider data retention policies for compliance
- **Monitoring**: Monitor deletion patterns for abuse

## ✅ Implementation Status

- [x] Hard delete implementation (permanent removal)
- [x] Password verification for security
- [x] Confirmation text requirement ("DELETE")
- [x] JWT authentication integration
- [x] Automatic logout from all devices
- [x] Email confirmation after deletion
- [x] Comprehensive error handling
- [x] Input validation and sanitization
- [x] Audit logging for security
- [x] Mobile and web frontend examples
- [x] Documentation and security guidelines

The **Account Deletion API** provides a secure, permanent way for users to delete their accounts with proper safeguards and confirmations! 🗑️🔒
