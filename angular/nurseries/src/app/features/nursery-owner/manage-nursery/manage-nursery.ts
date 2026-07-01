import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import * as L from 'leaflet';
import { environment } from '../../../../environments/environment';
import { AuthResponseDto } from '../../../core/models/authModel';
import { User } from '../../../core/models/user.model';
import { Nursery } from '../../../core/services/nursery';

@Component({
  selector: 'app-manage-nursery',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule
  ],
  templateUrl: './manage-nursery.html',
  styleUrl: './manage-nursery.css'
})
export class ManageNursery implements OnInit, AfterViewInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private ns = inject(Nursery)

  private apiUrl = environment.backUrl + '/api/nurseries';
  private user: AuthResponseDto = JSON.parse(localStorage.getItem("user_session")!);

  nurseryForm!: FormGroup;
  currentNurseryId: number | null = null;
  status = '';

  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);
  isEditMode = signal<boolean>(false);

  selectedFiles: File[] = [];
  imagePreviews: string[] = [];

  @ViewChild('mapContainer', { static: false }) mapContainer!: ElementRef<HTMLDivElement>;
  map!: L.Map;
  marker!: L.Marker;

  // إحداثيات افتراضية (القاهرة كمثال)
  private defaultLat = 30.0444;
  private defaultLng = 31.2357;

  constructor() {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadNurseryData();
  }

  ngAfterViewInit(): void {
    

    // مراقبة التغيرات في المدخلات لتحديث الخريطة فوراً عند كتابة إحداثيات يدوياً
    this.nurseryForm.get('latitude')?.valueChanges.subscribe(() => this.updateMarkerFromForm());
    this.nurseryForm.get('longitude')?.valueChanges.subscribe(() => this.updateMarkerFromForm());
  }

  private initForm(): void {
    this.nurseryForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      dailyPrice: [0, [Validators.required, Validators.min(1)]],
      ageRangeMin: [2, [Validators.required, Validators.min(0)]],
      ageRangeMax: [6, [Validators.required, Validators.min(1)]],
      capacity: [0, [Validators.required, Validators.min(1)]],
      address: ['', Validators.required],
      city: ['', Validators.required],
      district: ['', Validators.required],
      latitude: [this.defaultLat, Validators.required],
      longitude: [this.defaultLng, Validators.required]
    });
  }

  // تهيئة الخريطة وإضافة أحداث الضغط (Click) لتحديد الموقع
  private initMap(): void {
  if (!this.mapContainer) return;

  // إنشاء الخريطة
  this.map = L.map(this.mapContainer.nativeElement).setView([this.defaultLat, this.defaultLng], 13);

  // إضافة طبقة الخرائط
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors',
    maxZoom: 19
  }).addTo(this.map);

  const defaultIcon = L.icon({
    iconUrl: '/marker-icon.png',
    shadowUrl: '/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41]
  });

  this.marker = L.marker([this.defaultLat, this.defaultLng], { 
    icon: defaultIcon,
    draggable: true 
  }).addTo(this.map);

  this.marker.on('dragend', (e: any) => {
    const position = e.target.getLatLng();
    this.updateFormCoordinates(position.lat, position.lng);
  });

  this.map.on('click', (e: L.LeafletMouseEvent) => {
    this.updateLocation(e.latlng.lat, e.latlng.lng);
    this.updateFormCoordinates(e.latlng.lat, e.latlng.lng);
  });

  // 🔥 الحل السحري: إجبار الخريطة على تحديث أبعادها وتحميل المربعات الرمادية فوراً
  setTimeout(() => {
    if (this.map) {
      this.map.invalidateSize();
    }
  }, 500); 
}

  // تحديث حقول خطوط الطول والعرض في الـ Reactive Form
  private updateFormCoordinates(lat: number, lng: number): void {
    this.nurseryForm.patchValue({
      latitude: Number(lat.toFixed(6)),
      longitude: Number(lng.toFixed(6))
    }, { emitEvent: false }); // emitEvent: false تمنع الـ Infinite Loop
  }

  // تحديث مكان المؤشر جغرافياً (يُستدعى عند جلب البيانات أو الضغط)
  private updateLocation(lat: number, lng: number): void {
    if (this.marker) {
      this.marker.setLatLng([lat, lng]);
    }
  }

  // تحديث المؤشر فوراً إذا قام المستخدم بتعديل الأرقام داخل الـ Input نفسه
  private updateMarkerFromForm(): void {
    const lat = Number(this.nurseryForm.get('latitude')?.value);
    const lng = Number(this.nurseryForm.get('longitude')?.value);

    if (!isNaN(lat) && !isNaN(lng) && this.map) {
      this.updateLocation(lat, lng);
      this.map.panTo([lat, lng]);
    }
  }

  loadNurseryData(): void {
  this.isLoading.set(true);
  this.ns.getNurseryByOwnerId(this.user.id).subscribe({
    next: (nursery) => {
      if (nursery) {
        this.isEditMode.set(true);
        this.currentNurseryId = nursery.id;
        this.nurseryForm.patchValue(nursery);
      }
      this.isLoading.set(false);
      
      // 🎯 تهيئة الخريطة هنا بعد أن يختفي الـ Loading ويظهر الـ HTML في الـ DOM
      setTimeout(() => this.initMap(), 100);
    },
    error: () => {
      console.warn('لا توجد حضانة مسجلة حالياً لهذا الحساب.');
      this.isEditMode.set(false);
      this.currentNurseryId = null;
      this.isLoading.set(false);
      
      // 🎯 تهيئة الخريطة هنا أيضاً في حالة التسجيل الجديد
      setTimeout(() => this.initMap(), 100);
    }
  });
}

  onFileChange(event: any): void {
    const files = event.target.files;
    if (!files) return;

    for (let i = 0; i < files.length; i++) {
      this.selectedFiles.push(files[i]);
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreviews.push(e.target.result);
      };
      reader.readAsDataURL(files[i]);
    }
  }

  onSubmit(): void {
    if (this.nurseryForm.invalid) {
      this.nurseryForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const form = this.nurseryForm.value;

    const requestBody = {
      name: form.name,
      description: form.description,
      dailyPrice: Number(form.dailyPrice),
      ageRangeMin: Number(form.ageRangeMin),
      ageRangeMax: Number(form.ageRangeMax),
      capacity: Number(form.capacity),
      address: form.address,
      city: form.city,
      district: form.district,
      latitude: Number(form.latitude),
      longitude: Number(form.longitude)
    };

    const request$ = this.isEditMode() && this.currentNurseryId
      ? this.http.put(`${this.apiUrl}/${this.currentNurseryId}`, requestBody)
      : this.http.post<any>(this.apiUrl, requestBody);

    request$.subscribe({
      next: (response) => {
        if (!this.isEditMode() && response?.id && this.selectedFiles.length > 0) {
          this.uploadNurseryImages(response.id);
        } else {
          this.isSubmitting.set(false);
          alert(this.isEditMode() ? 'تم تحديث بيانات الحضانة بنجاح.' : 'تم إنشاء الحضانة بنجاح.');
          this.loadNurseryData();
        }
      },
      error: (err) => {
        console.error(err);
        this.isSubmitting.set(false);
        
        if (err.status === 401) {
          alert('غير مصرح لك بالقيام بهذا الإجراء (Unauthorized)');
          return;
        }

        let message = 'حدث خطأ أثناء حفظ البيانات.';
        if (err.error?.errors) {
          message = Object.values(err.error.errors).flat().join('\n');
        } else if (err.error?.message) {
          message = err.error.message;
        }
        alert(message);
      }
    });
  }

  private uploadNurseryImages(nurseryId: number): void {
    let uploaded = 0;

    this.selectedFiles.forEach(file => {
      const formData = new FormData();
      formData.append('image', file, file.name);

      this.http.post(`${this.apiUrl}/${nurseryId}/images`, formData).subscribe({
        next: () => {
          uploaded++;
          if (uploaded === this.selectedFiles.length) {
            this.selectedFiles = [];
            this.isSubmitting.set(false);
            alert('تم رفع الصور بنجاح.');
            this.loadNurseryData();
          }
        },
        error: (err) => {
          console.error('خطأ أثناء رفع الصورة:', err);
        }
      });
    });
  }
}