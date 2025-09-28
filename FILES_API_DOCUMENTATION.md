# Files API - Frontend Documentation

## Overview
File upload/download endpoints for handling images, documents, and other files. Requires authentication for all operations.

Base route: `/api/files`

## Authentication
All endpoints require: `Authorization: Bearer {jwt_token}`

## File Constraints
- **Max file size**: 10MB
- **Allowed extensions**: `.jpg`, `.jpeg`, `.png`, `.gif`, `.pdf`, `.doc`, `.docx`, `.txt`
- **Storage location**: `wwwroot/uploads/{folder}/`

---

## Endpoints

### POST /api/files/upload
Upload a single file

**Headers:**
- `Authorization: Bearer {token}`
- `Content-Type: multipart/form-data`

**Parameters:**
- `file` (form-data): File to upload (required)
- `folder` (query): Optional folder name (e.g., "cars", "advertisements", "users")

**Response:** `ApiResponse<FileUploadResultDto>`
```json
{
  "succeeded": true,
  "message": "File uploaded successfully",
  "data": {
    "fileName": "a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
    "originalFileName": "car-image.jpg",
    "fileSize": 1024000,
    "fileUrl": "/uploads/cars/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
    "uploadedAt": "2024-01-15T10:30:00Z",
    "uploadedBy": "user123"
  },
  "errors": [],
  "statusCode": "Success",
  "statusCodeValue": 200
}
```

### POST /api/files/upload-multiple
Upload multiple files

**Headers:**
- `Authorization: Bearer {token}`
- `Content-Type: multipart/form-data`

**Parameters:**
- `files` (form-data): Array of files to upload (required)
- `folder` (query): Optional folder name

**Response:** `ApiResponse<List<FileUploadResultDto>>`
```json
{
  "succeeded": true,
  "message": "Files uploaded successfully",
  "data": [
    {
      "fileName": "file1.jpg",
      "originalFileName": "image1.jpg",
      "fileSize": 1024000,
      "fileUrl": "/uploads/cars/file1.jpg",
      "uploadedAt": "2024-01-15T10:30:00Z",
      "uploadedBy": "user123"
    }
  ],
  "errors": [],
  "statusCode": "Success",
  "statusCodeValue": 200
}
```

### DELETE /api/files/{fileName}
Delete a file

**Headers:**
- `Authorization: Bearer {token}`

**Parameters:**
- `fileName` (path): Name of the file to delete
- `folder` (query): Optional folder name

**Response:** `ApiResponse`
```json
{
  "succeeded": true,
  "message": "File deleted successfully",
  "errors": [],
  "statusCode": "Success",
  "statusCodeValue": 200
}
```

### GET /api/files/{fileName}
Get file information

**Headers:**
- `Authorization: Bearer {token}`

**Parameters:**
- `fileName` (path): Name of the file
- `folder` (query): Optional folder name

**Response:** `ApiResponse<FileInfoDto>`
```json
{
  "succeeded": true,
  "message": "File information retrieved successfully",
  "data": {
    "fileName": "a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
    "fileSize": 1024000,
    "fileUrl": "/uploads/cars/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
    "createdAt": "2024-01-15T10:30:00Z",
    "modifiedAt": "2024-01-15T10:30:00Z",
    "extension": ".jpg"
  },
  "errors": [],
  "statusCode": "Success",
  "statusCodeValue": 200
}
```

---

## Frontend Usage Examples

### JavaScript/TypeScript
```javascript
// Upload single file
async function uploadFile(file, folder = null) {
  const formData = new FormData();
  formData.append('file', file);
  
  const url = folder ? `/api/files/upload?folder=${folder}` : '/api/files/upload';
  
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });
  
  const result = await response.json();
  if (!result.succeeded) throw new Error(result.message);
  return result.data;
}

// Upload multiple files
async function uploadMultipleFiles(files, folder = null) {
  const formData = new FormData();
  files.forEach(file => formData.append('files', file));
  
  const url = folder ? `/api/files/upload-multiple?folder=${folder}` : '/api/files/upload-multiple';
  
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });
  
  const result = await response.json();
  if (!result.succeeded) throw new Error(result.message);
  return result.data;
}

// Delete file
async function deleteFile(fileName, folder = null) {
  const url = folder ? `/api/files/${fileName}?folder=${folder}` : `/api/files/${fileName}`;
  
  const response = await fetch(url, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  const result = await response.json();
  if (!result.succeeded) throw new Error(result.message);
  return result;
}

// Get file info
async function getFileInfo(fileName, folder = null) {
  const url = folder ? `/api/files/${fileName}?folder=${folder}` : `/api/files/${fileName}`;
  
  const response = await fetch(url, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  const result = await response.json();
  if (!result.succeeded) throw new Error(result.message);
  return result.data;
}
```

### React Example
```tsx
import React, { useState } from 'react';

const FileUpload: React.FC = () => {
  const [uploading, setUploading] = useState(false);
  const [uploadedFiles, setUploadedFiles] = useState<FileUploadResultDto[]>([]);

  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (!files || files.length === 0) return;

    setUploading(true);
    try {
      const formData = new FormData();
      Array.from(files).forEach(file => formData.append('files', file));

      const response = await fetch('/api/files/upload-multiple?folder=cars', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      });

      const result = await response.json();
      if (result.succeeded) {
        setUploadedFiles(prev => [...prev, ...result.data]);
      } else {
        console.error('Upload failed:', result.message);
      }
    } catch (error) {
      console.error('Upload error:', error);
    } finally {
      setUploading(false);
    }
  };

  return (
    <div>
      <input
        type="file"
        multiple
        accept=".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.txt"
        onChange={handleFileUpload}
        disabled={uploading}
      />
      {uploading && <div>Uploading...</div>}
      {uploadedFiles.map(file => (
        <div key={file.fileName}>
          <img src={file.fileUrl} alt={file.originalFileName} style={{maxWidth: 200}} />
          <p>{file.originalFileName}</p>
        </div>
      ))}
    </div>
  );
};
```

### HTML Form Example
```html
<form id="uploadForm" enctype="multipart/form-data">
  <input type="file" name="file" id="fileInput" accept=".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.txt" required>
  <input type="text" name="folder" id="folderInput" placeholder="Folder (optional)">
  <button type="submit">Upload</button>
</form>

<script>
document.getElementById('uploadForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  
  const formData = new FormData();
  const fileInput = document.getElementById('fileInput');
  const folderInput = document.getElementById('folderInput');
  
  formData.append('file', fileInput.files[0]);
  
  const url = folderInput.value ? 
    `/api/files/upload?folder=${folderInput.value}` : 
    '/api/files/upload';
  
  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`
      },
      body: formData
    });
    
    const result = await response.json();
    if (result.succeeded) {
      console.log('File uploaded:', result.data.fileUrl);
    } else {
      console.error('Upload failed:', result.message);
    }
  } catch (error) {
    console.error('Upload error:', error);
  }
});
</script>
```

---

## Error Responses

### File too large
```json
{
  "succeeded": false,
  "message": "File size exceeds maximum allowed size of 10MB",
  "statusCode": "BadRequest",
  "statusCodeValue": 400
}
```

### Invalid file type
```json
{
  "succeeded": false,
  "message": "File type not allowed. Allowed types: .jpg, .jpeg, .png, .gif, .pdf, .doc, .docx, .txt",
  "statusCode": "BadRequest",
  "statusCodeValue": 400
}
```

### File not found
```json
{
  "succeeded": false,
  "message": "File not found",
  "statusCode": "NotFound",
  "statusCodeValue": 404
}
```

---

## Notes
- Files are stored in `wwwroot/uploads/{folder}/` directory
- Generated filenames use GUIDs to prevent conflicts
- Original filenames are preserved in the response
- File URLs are relative to the web root and served as static files
- All operations are logged with user information
- **Static file serving is enabled** - uploaded images can be accessed directly via browser

## Accessing Uploaded Files
Once uploaded, files can be accessed directly in the browser:
- Upload URL: `/uploads/cars/abc123.jpg`
- Full URL: `https://yourdomain.com/uploads/cars/abc123.jpg`
