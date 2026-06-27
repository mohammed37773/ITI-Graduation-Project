import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { GoogleMapsModule } from '@angular/google-maps';
@Component({
  selector: 'app-manage-nursery',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], 
  templateUrl: './manage-nursery.html',
  styleUrl: './manage-nursery.css',
})
export class ManageNursery implements OnInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5104/api/nurseries';
  nurseryForm!: FormGroup;
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);
  isEditMode = signal<boolean>(false); 
  
  currentNurseryId: number | null = null;
  selectedFiles: File[] = [];
  imagePreviews: string[] = [];

  // 🗺️ متغيرات إعدادات الخريطة والمؤشر
  mapCenter: google.maps.LatLngLiteral = { lat: 30.0444, lng: 31.2357 }; // القاهرة افتراضياً
  mapZoom = 13;
  markerPosition: google.maps.LatLngLiteral = { lat: 30.0444, lng: 31.2357 };

  constructor() {
    this.nurseryForm = this.fb.group({
      name: ['', [Validators.required]],
      description: ['', [Validators.required]],
      dailyPrice: [0, [Validators.required, Validators.min(1)]],
      ageRangeMin: [2, [Validators.required, Validators.min(0)]],
      ageRangeMax: [6, [Validators.required, Validators.min(1)]],
      capacity: [0, [Validators.required, Validators.min(1)]],
      address: ['', [Validators.required]],
      city: ['', [Validators.required]],
      district: ['', [Validators.required]],
      latitude: [30.0444, [Validators.required]],
      longitude: [31.2357, [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadNurseryData();
  }

  loadNurseryData() {
    this.isLoading.set(true);
    this.http.get<any>(`${this.apiUrl}/my`).subscribe({
      next: (nursery) => {
        if (nursery) {
          this.isEditMode.set(true);
          this.currentNurseryId = nursery.id;
          this.nurseryForm.patchValue(nursery);

          // 🗺️ تحديث موقع الخريطة والمؤشر بناءً على الإحداثيات الراجعة من الباك إند
          if (nursery.latitude && nursery.longitude) {
            const latLng = { lat: Number(nursery.latitude), lng: Number(nursery.longitude) };
            this.mapCenter = latLng;
            this.markerPosition = latLng;
            this.mapZoom = 16; // عمل زووم أقرب على مكان الحضانة المسجلة
          }

          if (nursery.images) {
            this.imagePreviews = nursery.images.map((img: any) => img.imageUrl);
          }
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.warn('تحذير: لا توجد حضانة مسجلة، تفعيل وضع الإضافة.');
        this.isEditMode.set(false);
        this.currentNurseryId = null;
        this.isLoading.set(false);
      }
    });
  }

  // 🗺️ 🎯 الدالة السحرية: لقط المكان المختار من الخريطة وتحديث الفورم فوراً
  onMapClick(event: google.maps.MapMouseEvent) {
    if (event.latLng) {
      const lat = event.latLng.lat();
      const lng = event.latLng.lng();

      // تحديث قيمة المؤشر بصرياً على الخريطة
      this.markerPosition = { lat, lng };

      // تحديث قيم الـ Inputs داخل الـ FormGroup أوتوماتيكياً
      this.nurseryForm.patchValue({
        latitude: lat,
        longitude: lng
      });
    }
  }

  onFileChange(event: any) {
    const files = event.target.files;
    if (files) {
      for (let i = 0; i < files.length; i++) {
        this.selectedFiles.push(files[i]);
        
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.imagePreviews.push(e.target.result);
        };
        reader.readAsDataURL(files[i]);
      }
    }
  }

  onSubmit() {
    if (this.nurseryForm.invalid) {
      this.nurseryForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    const formValues = this.nurseryForm.value;
    const requestBody = {
      name: formValues.name,
      description: formValues.description,
      dailyPrice: Number(formValues.dailyPrice),
      ageRangeMin: Number(formValues.ageRangeMin),
      ageRangeMax: Number(formValues.ageRangeMax),
      capacity: Number(formValues.capacity),
      address: formValues.address,
      city: formValues.city,
      district: formValues.district,
      latitude: Number(formValues.latitude),
      longitude: Number(formValues.longitude)
    };

    const request$ = this.isEditMode() && this.currentNurseryId
      ? this.http.put(`${this.apiUrl}/${this.currentNurseryId}`, requestBody)
      : this.http.post<any>(`${this.apiUrl}`, requestBody);

    request$.subscribe({
      next: (response) => {
        if (!this.isEditMode() && response && response.id && this.selectedFiles.length > 0) {
          this.uploadNurseryImages(response.id);
        } else {
          this.isSubmitting.set(false);
          alert(this.isEditMode() ? 'تم تحديث بيانات حضانتك بنجاح! ✨' : 'تم تسجيل بيانات الحضانة بنجاح! 🎉');
          this.loadNurseryData();
        }
      },
      error: (err) => {
        console.error('❌ خطأ تفصيلي من السيرفر:', err);
        this.isSubmitting.set(false);
        
        if (err.status === 401) {
          alert('جلسة العمل انتهت أو غير مصرح لك (401 Unauthorized).\nيرجى عمل تسجيل دخول (Login) جديد لتحديث التوكن فريش في المتصفح!');
          return;
        }

        let errorMessage = 'حدث خطأ أثناء معالجة البيانات، يرجى مراجعة المدخلات.';
        if (err.error && err.error.errors) {
          errorMessage = Object.values(err.error.errors).flat().join('\n');
        } else if (err.error && err.error.message) {
          errorMessage = err.error.message;
        }
        alert(errorMessage);
      }
    });
  }

  private uploadNurseryImages(nurseryId: number) {
    let uploadCount = 0;
    this.selectedFiles.forEach(file => {
      const imgFormData = new FormData();
      imgFormData.append('image', file, file.name);

      this.http.post(`${this.apiUrl}/${nurseryId}/images`, imgFormData).subscribe({
        next: () => {
          uploadCount++;
          if (uploadCount === this.selectedFiles.length) {
            this.isSubmitting.set(false);
            alert('تم تسجيل بيانات الحضانة ورفع كافة الصور بنجاح! 🎉');
            this.selectedFiles = [];
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