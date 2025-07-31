# NOL Car Rental - Admin API Documentation for Angular 20

## 🎯 Overview

This document provides comprehensive API documentation for the NOL Car Rental admin system, specifically designed for integration with Angular 20 applications. The API follows RESTful principles and uses JWT-based authentication with role-based authorization.

## 🔐 Authentication & Authorization

### Base URL
```
https://your-api-domain.com/api
```

### Authentication
All admin endpoints require JWT authentication with appropriate roles.

#### Roles
- **SuperAdmin**: Full system access
- **Admin**: General administrative access
- **BranchManager**: Branch-specific management
- **Employee**: Limited access

#### Headers Required
```typescript
{
  'Authorization': 'Bearer <jwt-token>',
  'Content-Type': 'application/json',
  'Accept': 'application/json'
}
```

### Login Endpoint
```typescript
POST /auth/login
```

**Request Body:**
```typescript
interface LoginDto {
  email: string;
  password: string;
}
```

**Response:**
```typescript
interface AuthResponseDto {
  token: string;
  refreshToken: string;
  user: {
    id: string;
    email: string;
    fullName: string;
    role: string;
    preferredLanguage: 'Arabic' | 'English';
  };
  expiresAt: string;
}
```

## 📊 Dashboard API

### Get Dashboard Statistics
```typescript
GET /admin/dashboard/stats
```

**Query Parameters:**
```typescript
interface DashboardFilterDto {
  startDate?: string; // ISO date
  endDate?: string;   // ISO date
  branchId?: number;
}
```

**Response:**
```typescript
interface DashboardStatsDto {
  overallStats: {
    totalBookings: number;
    totalRevenue: number;
    activeCars: number;
    totalCustomers: number;
    pendingBookings: number;
    completedBookings: number;
    averageBookingValue: number;
    carUtilizationRate: number;
  };
  revenueStats: {
    todayRevenue: number;
    weekRevenue: number;
    monthRevenue: number;
    yearRevenue: number;
    revenueGrowthRate: number;
    monthlyRevenue: Array<{
      year: number;
      month: number;
      monthName: string;
      revenue: number;
      bookingCount: number;
    }>;
  };
  bookingStats: {
    todayBookings: number;
    weekBookings: number;
    monthBookings: number;
    yearBookings: number;
    cancelledBookings: number;
    cancellationRate: number;
    bookingsByStatus: Array<{
      status: string;
      count: number;
      percentage: number;
    }>;
  };
  carStats: {
    totalCars: number;
    availableCars: number;
    rentedCars: number;
    maintenanceCars: number;
    outOfServiceCars: number;
    utilizationRate: number;
  };
  customerStats: {
    totalCustomers: number;
    newCustomersThisMonth: number;
    activeCustomers: number;
    customerRetentionRate: number;
    averageLoyaltyPoints: number;
  };
}
```

### Get Real-time Data
```typescript
GET /admin/dashboard/realtime
```

**Response:** Real-time dashboard data for auto-refresh functionality.

### Export Dashboard Report
```typescript
POST /admin/dashboard/export
```

**Request Body:**
```typescript
interface ExportRequestDto {
  format: 'excel' | 'pdf';
  startDate?: string;
  endDate?: string;
  includeCharts?: boolean;
}
```

**Response:** File download (blob)

## 🚗 Car Management API

### Get Cars (Paginated)
```typescript
GET /admin/cars
```

**Query Parameters:**
```typescript
interface CarFilterDto {
  status?: 'Available' | 'Rented' | 'Maintenance' | 'OutOfService';
  categoryId?: number;
  branchId?: number;
  brand?: string;
  model?: string;
  yearFrom?: number;
  yearTo?: number;
  dailyRateFrom?: number;
  dailyRateTo?: number;
  transmissionType?: 'Manual' | 'Automatic';
  fuelType?: 'Petrol' | 'Diesel' | 'Electric' | 'Hybrid';
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}
```

**Response:**
```typescript
interface AdminCarDto {
  id: number;
  brand: string;
  model: string;
  year: number;
  plateNumber: string;
  seatingCapacity: number;
  transmissionType: string;
  fuelType: string;
  dailyRate: number;
  weeklyRate: number;
  monthlyRate: number;
  status: string;
  imageUrl?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  mileage: number;
  features?: string;
  categoryId: number;
  branchId: number;
  createdAt: string;
  updatedAt: string;
  totalBookings: number;
  totalRevenue: number;
  utilizationRate: number;
  lastBookingDate?: string;
  nextMaintenanceDate?: string;
}
```

### Create Car
```typescript
POST /admin/cars
```

**Request Body:**
```typescript
interface AdminCreateCarDto {
  brand: string;
  model: string;
  year: number;
  plateNumber: string;
  seatingCapacity: number;
  transmissionType: 'Manual' | 'Automatic';
  fuelType: 'Petrol' | 'Diesel' | 'Electric' | 'Hybrid';
  dailyRate: number;
  weeklyRate: number;
  monthlyRate: number;
  status?: 'Available' | 'Rented' | 'Maintenance' | 'OutOfService';
  imageUrl?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  mileage?: number;
  features?: string;
  categoryId: number;
  branchId: number;
}
```

### Update Car
```typescript
PUT /admin/cars/{id}
```

**Request Body:** `AdminUpdateCarDto` (same as create but all fields optional)

### Delete Car
```typescript
DELETE /admin/cars/{id}
```

### Update Car Status
```typescript
PUT /admin/cars/{id}/status
```

**Request Body:**
```typescript
interface UpdateCarStatusDto {
  status: 'Available' | 'Rented' | 'Maintenance' | 'OutOfService';
  notes?: string;
}
```

### Bulk Operations
```typescript
POST /admin/cars/bulk/operation
```

**Request Body:**
```typescript
interface BulkCarOperationDto {
  carIds: number[];
  operation: 'delete' | 'updateStatus' | 'updateBranch' | 'updateCategory';
  newStatus?: string;
  newBranchId?: number;
  newCategoryId?: number;
}
```

### Car Analytics
```typescript
GET /admin/cars/{id}/analytics
```

**Query Parameters:**
```typescript
{
  startDate?: string;
  endDate?: string;
}
```

### Import Cars
```typescript
POST /admin/cars/import
```

**Request Body:** FormData with Excel/CSV file

### Export Cars
```typescript
GET /admin/cars/export
```

**Query Parameters:** Same as car filter + format parameter

## 📅 Booking Management API

### Get Bookings (Paginated)
```typescript
GET /admin/bookings
```

**Query Parameters:**
```typescript
interface BookingFilterDto {
  status?: 'Pending' | 'Confirmed' | 'Active' | 'Completed' | 'Cancelled';
  customerId?: string;
  carId?: number;
  branchId?: number;
  startDateFrom?: string;
  startDateTo?: string;
  endDateFrom?: string;
  endDateTo?: string;
  totalAmountFrom?: number;
  totalAmountTo?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}
```

**Response:**
```typescript
interface AdminBookingDto {
  id: number;
  userId: string;
  carId: number;
  startDate: string;
  endDate: string;
  totalAmount: number;
  status: string;
  receivingBranchId: number;
  deliveryBranchId: number;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  carBrand: string;
  carModel: string;
  carPlateNumber: string;
  receivingBranchName: string;
  deliveryBranchName: string;
  extrasDetails: Array<{
    extraTypeName: string;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
  }>;
  paymentDetails: Array<{
    amount: number;
    paymentMethod: string;
    transactionId?: string;
    paymentDate: string;
    status: string;
  }>;
  createdAt: string;
  updatedAt: string;
}
```

### Create Booking
```typescript
POST /admin/bookings
```

### Update Booking Status
```typescript
PUT /admin/bookings/{id}/status
```

### Confirm Booking
```typescript
POST /admin/bookings/{id}/confirm
```

### Cancel Booking
```typescript
POST /admin/bookings/{id}/cancel
```

**Request Body:**
```typescript
interface CancelBookingDto {
  cancellationReason: string;
  refundAmount?: number;
}
```

### Booking Analytics
```typescript
GET /admin/bookings/analytics
```

## 👥 Customer Management API

### Get Customers (Paginated)
```typescript
GET /admin/customers
```

**Query Parameters:**
```typescript
interface CustomerFilterDto {
  name?: string;
  email?: string;
  phone?: string;
  userRole?: string;
  preferredLanguage?: 'Arabic' | 'English';
  isActive?: boolean;
  emailVerified?: boolean;
  createdDateFrom?: string;
  createdDateTo?: string;
  minBookings?: number;
  maxBookings?: number;
  minSpent?: number;
  maxSpent?: number;
  minLoyaltyPoints?: number;
  maxLoyaltyPoints?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}
```

**Response:**
```typescript
interface AdminCustomerDto {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  userRole: string;
  preferredLanguage: string;
  isActive: boolean;
  emailConfirmed: boolean;
  createdAt: string;
  updatedAt: string;
  lastLoginDate?: string;
  lastBookingDate?: string;
  totalBookings: number;
  totalSpent: number;
  averageBookingValue: number;
  completedBookings: number;
  cancelledBookings: number;
  cancellationRate: number;
  totalLoyaltyPoints: number;
  availableLoyaltyPoints: number;
  lifetimePointsEarned: number;
  lifetimePointsRedeemed: number;
}
```

### Award Loyalty Points
```typescript
POST /admin/customers/loyalty-points/award
```

**Request Body:**
```typescript
interface AwardLoyaltyPointsDto {
  customerId: string;
  points: number;
  reason: string;
  expiryDate?: string;
}
```

### Customer Analytics
```typescript
GET /admin/customers/analytics
```

## 👨‍💼 Admin Management API

### Get Admins (Paginated)
```typescript
GET /admin/admins
```

**Query Parameters:**
```typescript
interface AdminFilterDto {
  name?: string;
  email?: string;
  userRole?: 'Employee' | 'BranchManager' | 'Admin' | 'SuperAdmin';
  isActive?: boolean;
  branchId?: number;
  createdDateFrom?: string;
  createdDateTo?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}
```

**Response:**
```typescript
interface AdminUserDto {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  userRole: string;
  preferredLanguage: string;
  isActive: boolean;
  emailConfirmed: boolean;
  createdAt: string;
  updatedAt: string;
  lastLoginDate?: string;
  createdByAdmin: string;
  assignedBranches: number[];
  assignedBranchNames: string[];
}
```

### Create Admin (SuperAdmin only)
```typescript
POST /admin/admins
```

**Request Body:**
```typescript
interface CreateAdminUserDto {
  email: string;
  fullName: string;
  phoneNumber?: string;
  userRole: 'Employee' | 'BranchManager' | 'Admin' | 'SuperAdmin';
  preferredLanguage?: 'Arabic' | 'English';
  password: string;
  confirmPassword: string;
  assignedBranches?: number[];
  permissions?: string[];
  sendWelcomeEmail?: boolean;
}
```

### Update Admin
```typescript
PUT /admin/admins/{id}
```

### Activate/Deactivate Admin
```typescript
POST /admin/admins/{id}/activate
POST /admin/admins/{id}/deactivate
```

### Update Admin Permissions
```typescript
PUT /admin/admins/{id}/permissions
```

**Request Body:**
```typescript
interface UpdatePermissionsDto {
  permissions: string[];
}
```

### Get Admin Activity Logs
```typescript
GET /admin/admins/activity-logs
```

## 🔧 Angular 20 Integration Examples

### Service Setup
```typescript
// admin-api.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminApiService {
  private baseUrl = 'https://your-api-domain.com/api';
  
  constructor(private http: HttpClient) {}
  
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('admin_token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }
  
  // Dashboard
  getDashboardStats(filter?: DashboardFilterDto): Observable<ApiResponse<DashboardStatsDto>> {
    let params = new HttpParams();
    if (filter?.startDate) params = params.set('startDate', filter.startDate);
    if (filter?.endDate) params = params.set('endDate', filter.endDate);
    if (filter?.branchId) params = params.set('branchId', filter.branchId.toString());
    
    return this.http.get<ApiResponse<DashboardStatsDto>>(
      `${this.baseUrl}/admin/dashboard/stats`,
      { headers: this.getHeaders(), params }
    );
  }
  
  // Cars
  getCars(filter?: CarFilterDto): Observable<ApiResponse<AdminCarDto[]>> {
    let params = new HttpParams();
    Object.keys(filter || {}).forEach(key => {
      const value = (filter as any)[key];
      if (value !== undefined && value !== null) {
        params = params.set(key, value.toString());
      }
    });
    
    return this.http.get<ApiResponse<AdminCarDto[]>>(
      `${this.baseUrl}/admin/cars`,
      { headers: this.getHeaders(), params }
    );
  }
  
  createCar(car: AdminCreateCarDto): Observable<ApiResponse<AdminCarDto>> {
    return this.http.post<ApiResponse<AdminCarDto>>(
      `${this.baseUrl}/admin/cars`,
      car,
      { headers: this.getHeaders() }
    );
  }
  
  // Bookings
  getBookings(filter?: BookingFilterDto): Observable<ApiResponse<AdminBookingDto[]>> {
    // Similar implementation
  }
  
  // Customers
  getCustomers(filter?: CustomerFilterDto): Observable<ApiResponse<AdminCustomerDto[]>> {
    // Similar implementation
  }
}
```

### Component Example
```typescript
// dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { AdminApiService } from './admin-api.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  dashboardStats: DashboardStatsDto | null = null;
  loading = false;
  
  constructor(private adminApi: AdminApiService) {}
  
  ngOnInit() {
    this.loadDashboardStats();
  }
  
  loadDashboardStats() {
    this.loading = true;
    this.adminApi.getDashboardStats().subscribe({
      next: (response) => {
        if (response.succeeded) {
          this.dashboardStats = response.data;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard stats:', error);
        this.loading = false;
      }
    });
  }
}
```

## 📝 Response Format

All API responses follow this standard format:

```typescript
interface ApiResponse<T> {
  data: T;
  succeeded: boolean;
  message: string;
  errors?: string[];
  statusCode: number;
  statusCodeValue: number;
}
```

## 🚨 Error Handling

Common HTTP status codes:
- **200**: Success
- **400**: Bad Request (validation errors)
- **401**: Unauthorized (invalid/missing token)
- **403**: Forbidden (insufficient permissions)
- **404**: Not Found
- **500**: Internal Server Error

## 🔄 Pagination

Paginated endpoints return:

```typescript
interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
```

## 📊 Advertisement Management API

### Get Advertisements
```typescript
GET /admin/advertisements
```

**Query Parameters:**
```typescript
interface AdvertisementFilterDto {
  title?: string;
  status?: 'Active' | 'Inactive' | 'Scheduled' | 'Expired';
  type?: 'Banner' | 'Popup' | 'Sidebar';
  startDateFrom?: string;
  startDateTo?: string;
  endDateFrom?: string;
  endDateTo?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}
```

**Response:**
```typescript
interface AdminAdvertisementDto {
  id: number;
  titleAr: string;
  titleEn: string;
  descriptionAr?: string;
  descriptionEn?: string;
  imageUrl: string;
  linkUrl?: string;
  type: string;
  status: string;
  startDate: string;
  endDate: string;
  displayOrder: number;
  targetAudience?: string;
  clickCount: number;
  impressionCount: number;
  clickThroughRate: number;
  createdAt: string;
  updatedAt: string;
}
```

### Create Advertisement
```typescript
POST /admin/advertisements
```

**Request Body:**
```typescript
interface AdminCreateAdvertisementDto {
  titleAr: string;
  titleEn: string;
  descriptionAr?: string;
  descriptionEn?: string;
  imageUrl: string;
  linkUrl?: string;
  type: 'Banner' | 'Popup' | 'Sidebar';
  status: 'Active' | 'Inactive' | 'Scheduled';
  startDate: string;
  endDate: string;
  displayOrder?: number;
  targetAudience?: string;
}
```

### Advertisement Analytics
```typescript
GET /admin/advertisements/analytics
```

### Schedule Advertisement
```typescript
POST /admin/advertisements/{id}/schedule
```

## 🏢 Branch Management API

### Get Branches
```typescript
GET /admin/branches
```

**Response:**
```typescript
interface BranchDto {
  id: number;
  nameAr: string;
  nameEn: string;
  addressAr: string;
  addressEn: string;
  phoneNumber: string;
  email?: string;
  latitude: number;
  longitude: number;
  isActive: boolean;
  workingHours: string;
  totalCars: number;
  availableCars: number;
  totalBookings: number;
  monthlyRevenue: number;
}
```

### Create Branch
```typescript
POST /admin/branches
```

### Update Branch
```typescript
PUT /admin/branches/{id}
```

## 📊 Reports & Analytics API

### Generate Custom Report
```typescript
POST /admin/reports/generate
```

**Request Body:**
```typescript
interface ReportRequestDto {
  reportType: 'Revenue' | 'Bookings' | 'Cars' | 'Customers' | 'Custom';
  startDate: string;
  endDate: string;
  filters?: {
    branchIds?: number[];
    carIds?: number[];
    customerIds?: string[];
    categories?: number[];
  };
  groupBy?: 'Day' | 'Week' | 'Month' | 'Year';
  format: 'excel' | 'pdf' | 'csv';
  includeCharts?: boolean;
}
```

### Export Data
```typescript
GET /admin/export/{entityType}
```

**Query Parameters:**
```typescript
{
  format: 'excel' | 'csv' | 'pdf';
  filters?: string; // JSON string of filter object
  columns?: string[]; // Specific columns to export
}
```

## 🔧 Advanced Angular 20 Integration

### HTTP Interceptor for Admin Authentication
```typescript
// admin-auth.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AdminAuthInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Add auth header for admin routes
    if (req.url.includes('/admin/')) {
      const token = localStorage.getItem('admin_token');
      if (token) {
        req = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`,
            'Accept-Language': localStorage.getItem('admin_language') || 'en'
          }
        });
      }
    }

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && req.url.includes('/admin/')) {
          // Redirect to admin login
          localStorage.removeItem('admin_token');
          this.router.navigate(['/admin/login']);
        }
        return throwError(() => error);
      })
    );
  }
}
```

### Admin State Management (NgRx)
```typescript
// admin.state.ts
export interface AdminState {
  currentAdmin: AdminUserDto | null;
  dashboardStats: DashboardStatsDto | null;
  cars: AdminCarDto[];
  bookings: AdminBookingDto[];
  customers: AdminCustomerDto[];
  loading: boolean;
  error: string | null;
}

// admin.actions.ts
export const loadDashboardStats = createAction(
  '[Admin] Load Dashboard Stats',
  props<{ filter?: DashboardFilterDto }>()
);

export const loadDashboardStatsSuccess = createAction(
  '[Admin] Load Dashboard Stats Success',
  props<{ stats: DashboardStatsDto }>()
);

export const loadCars = createAction(
  '[Admin] Load Cars',
  props<{ filter?: CarFilterDto }>()
);

// admin.effects.ts
@Injectable()
export class AdminEffects {
  loadDashboardStats$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadDashboardStats),
      switchMap(({ filter }) =>
        this.adminApi.getDashboardStats(filter).pipe(
          map(response => loadDashboardStatsSuccess({ stats: response.data })),
          catchError(error => of(loadDashboardStatsFailure({ error: error.message })))
        )
      )
    )
  );

  constructor(
    private actions$: Actions,
    private adminApi: AdminApiService
  ) {}
}
```

### Reactive Forms for Admin Operations
```typescript
// car-form.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdminApiService } from '../services/admin-api.service';

@Component({
  selector: 'app-car-form',
  template: `
    <form [formGroup]="carForm" (ngSubmit)="onSubmit()">
      <div class="form-group">
        <label for="brand">Brand</label>
        <input
          id="brand"
          type="text"
          formControlName="brand"
          class="form-control"
          [class.is-invalid]="carForm.get('brand')?.invalid && carForm.get('brand')?.touched">
        <div class="invalid-feedback" *ngIf="carForm.get('brand')?.invalid && carForm.get('brand')?.touched">
          Brand is required (2-50 characters)
        </div>
      </div>

      <div class="form-group">
        <label for="model">Model</label>
        <input
          id="model"
          type="text"
          formControlName="model"
          class="form-control"
          [class.is-invalid]="carForm.get('model')?.invalid && carForm.get('model')?.touched">
      </div>

      <div class="form-group">
        <label for="year">Year</label>
        <input
          id="year"
          type="number"
          formControlName="year"
          class="form-control"
          [class.is-invalid]="carForm.get('year')?.invalid && carForm.get('year')?.touched">
      </div>

      <div class="form-group">
        <label for="plateNumber">Plate Number</label>
        <input
          id="plateNumber"
          type="text"
          formControlName="plateNumber"
          class="form-control"
          [class.is-invalid]="carForm.get('plateNumber')?.invalid && carForm.get('plateNumber')?.touched">
      </div>

      <div class="form-group">
        <label for="dailyRate">Daily Rate</label>
        <input
          id="dailyRate"
          type="number"
          step="0.01"
          formControlName="dailyRate"
          class="form-control"
          [class.is-invalid]="carForm.get('dailyRate')?.invalid && carForm.get('dailyRate')?.touched">
      </div>

      <button type="submit" [disabled]="carForm.invalid || loading" class="btn btn-primary">
        {{ loading ? 'Saving...' : 'Save Car' }}
      </button>
    </form>
  `
})
export class CarFormComponent implements OnInit {
  carForm: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private adminApi: AdminApiService
  ) {
    this.carForm = this.fb.group({
      brand: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      model: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      year: ['', [Validators.required, Validators.min(1990), Validators.max(new Date().getFullYear() + 1)]],
      plateNumber: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]],
      seatingCapacity: ['', [Validators.required, Validators.min(1), Validators.max(20)]],
      transmissionType: ['', Validators.required],
      fuelType: ['', Validators.required],
      dailyRate: ['', [Validators.required, Validators.min(0.01), Validators.max(10000)]],
      weeklyRate: ['', [Validators.required, Validators.min(0.01), Validators.max(50000)]],
      monthlyRate: ['', [Validators.required, Validators.min(0.01), Validators.max(200000)]],
      categoryId: ['', Validators.required],
      branchId: ['', Validators.required]
    });
  }

  ngOnInit() {}

  onSubmit() {
    if (this.carForm.valid) {
      this.loading = true;
      const carData: AdminCreateCarDto = this.carForm.value;

      this.adminApi.createCar(carData).subscribe({
        next: (response) => {
          if (response.succeeded) {
            // Handle success
            console.log('Car created successfully:', response.data);
            this.carForm.reset();
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Error creating car:', error);
          this.loading = false;
        }
      });
    }
  }
}
```

### Data Table Component with Pagination
```typescript
// admin-data-table.component.ts
import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';

@Component({
  selector: 'app-admin-data-table',
  template: `
    <div class="table-responsive">
      <table class="table table-striped">
        <thead>
          <tr>
            <th *ngFor="let column of columns"
                [class.sortable]="column.sortable"
                (click)="onSort(column.key)">
              {{ column.label }}
              <i *ngIf="column.sortable && sortBy === column.key"
                 [class]="sortOrder === 'asc' ? 'fas fa-sort-up' : 'fas fa-sort-down'"></i>
            </th>
            <th *ngIf="actions.length > 0">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let item of data">
            <td *ngFor="let column of columns">
              <ng-container [ngSwitch]="column.type">
                <span *ngSwitchCase="'date'">{{ item[column.key] | date:'short' }}</span>
                <span *ngSwitchCase="'currency'">{{ item[column.key] | currency:'USD':'symbol':'1.2-2' }}</span>
                <span *ngSwitchCase="'status'"
                      [class]="'badge badge-' + getStatusClass(item[column.key])">
                  {{ item[column.key] }}
                </span>
                <span *ngSwitchDefault>{{ item[column.key] }}</span>
              </ng-container>
            </td>
            <td *ngIf="actions.length > 0">
              <button *ngFor="let action of actions"
                      [class]="'btn btn-sm ' + action.class"
                      (click)="onAction(action.key, item)"
                      [disabled]="action.disabled && action.disabled(item)">
                <i [class]="action.icon"></i> {{ action.label }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <nav *ngIf="totalPages > 1">
      <ul class="pagination justify-content-center">
        <li class="page-item" [class.disabled]="currentPage === 1">
          <a class="page-link" (click)="onPageChange(currentPage - 1)">Previous</a>
        </li>
        <li *ngFor="let page of getPageNumbers()"
            class="page-item"
            [class.active]="page === currentPage">
          <a class="page-link" (click)="onPageChange(page)">{{ page }}</a>
        </li>
        <li class="page-item" [class.disabled]="currentPage === totalPages">
          <a class="page-link" (click)="onPageChange(currentPage + 1)">Next</a>
        </li>
      </ul>
    </nav>
  `
})
export class AdminDataTableComponent implements OnInit {
  @Input() data: any[] = [];
  @Input() columns: TableColumn[] = [];
  @Input() actions: TableAction[] = [];
  @Input() currentPage = 1;
  @Input() totalPages = 1;
  @Input() sortBy = '';
  @Input() sortOrder: 'asc' | 'desc' = 'asc';

  @Output() pageChange = new EventEmitter<number>();
  @Output() sortChange = new EventEmitter<{sortBy: string, sortOrder: 'asc' | 'desc'}>();
  @Output() actionClick = new EventEmitter<{action: string, item: any}>();

  ngOnInit() {}

  onPageChange(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.pageChange.emit(page);
    }
  }

  onSort(columnKey: string) {
    const newSortOrder = this.sortBy === columnKey && this.sortOrder === 'asc' ? 'desc' : 'asc';
    this.sortChange.emit({ sortBy: columnKey, sortOrder: newSortOrder });
  }

  onAction(actionKey: string, item: any) {
    this.actionClick.emit({ action: actionKey, item });
  }

  getStatusClass(status: string): string {
    const statusClasses: { [key: string]: string } = {
      'Active': 'success',
      'Inactive': 'secondary',
      'Pending': 'warning',
      'Confirmed': 'info',
      'Completed': 'success',
      'Cancelled': 'danger',
      'Available': 'success',
      'Rented': 'primary',
      'Maintenance': 'warning',
      'OutOfService': 'danger'
    };
    return statusClasses[status] || 'secondary';
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }
}

interface TableColumn {
  key: string;
  label: string;
  type?: 'text' | 'date' | 'currency' | 'status';
  sortable?: boolean;
}

interface TableAction {
  key: string;
  label: string;
  icon: string;
  class: string;
  disabled?: (item: any) => boolean;
}
```

## 🔒 Security Best Practices

### Token Management
```typescript
// auth-token.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthTokenService {
  private tokenSubject = new BehaviorSubject<string | null>(null);
  public token$ = this.tokenSubject.asObservable();

  constructor() {
    // Load token from storage on service initialization
    const token = localStorage.getItem('admin_token');
    if (token && !this.isTokenExpired(token)) {
      this.tokenSubject.next(token);
    } else {
      this.clearToken();
    }
  }

  setToken(token: string): void {
    localStorage.setItem('admin_token', token);
    this.tokenSubject.next(token);
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  clearToken(): void {
    localStorage.removeItem('admin_token');
    this.tokenSubject.next(null);
  }

  isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp < currentTime;
    } catch {
      return true;
    }
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return token !== null && !this.isTokenExpired(token);
  }

  getUserRole(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.role || null;
    } catch {
      return null;
    }
  }
}
```

### Route Guards
```typescript
// admin-auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthTokenService } from './auth-token.service';

@Injectable({
  providedIn: 'root'
})
export class AdminAuthGuard implements CanActivate {
  constructor(
    private authService: AuthTokenService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/admin/login']);
      return false;
    }

    const requiredRoles = route.data['roles'] as string[];
    if (requiredRoles) {
      const userRole = this.authService.getUserRole();
      if (!userRole || !requiredRoles.includes(userRole)) {
        this.router.navigate(['/admin/unauthorized']);
        return false;
      }
    }

    return true;
  }
}

// Usage in routing module
const routes: Routes = [
  {
    path: 'admin/dashboard',
    component: DashboardComponent,
    canActivate: [AdminAuthGuard],
    data: { roles: ['Admin', 'SuperAdmin', 'BranchManager'] }
  },
  {
    path: 'admin/admins',
    component: AdminManagementComponent,
    canActivate: [AdminAuthGuard],
    data: { roles: ['SuperAdmin'] }
  }
];
```

This comprehensive documentation provides everything needed to integrate the NOL Car Rental admin API with Angular 20 applications, including TypeScript interfaces, service examples, components, security implementations, and best practices.
