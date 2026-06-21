import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-manage-nursery',
  imports: [CommonModule , ReactiveFormsModule],
  templateUrl: './manage-nursery.html',
  styleUrl: './manage-nursery.css',
})
export class ManageNursery implements OnInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5104/api/Nurseries';

  nurseryForm!: FormGroup;
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  nurseryId = signal<number | null>(null);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  ngOnInit() { this.initForm(); this.checkIfNurseryExists(); }

  private initForm() {
    this.nurseryForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.minLength(20)]],
      dailyPrice: [0, [Validators.required]],
      ageRangeMin: [0, [Validators.required]],
      ageRangeMax: [0, [Validators.required]],
      capacity: [0, [Validators.required]],
      address: ['', [Validators.required]],
      city: ['', [Validators.required]],
      district: ['', [Validators.required]],
      latitude: [30.0444],
      longitude: [31.2357]
    });
  }

  checkIfNurseryExists() {
    this.isLoading.set(true);
    this.http.get<any[]>(`${this.apiUrl}/nearby?radius=5000`).subscribe({
      next: (res) => {
        if (res && res.length > 0) {
          this.isEditMode.set(true);
          this.nurseryId.set(res[0].id);
          this.nurseryForm.patchValue(res[0]);
        }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onSubmit() {
    if (this.nurseryForm.invalid) return;
    this.isSubmitting.set(true);
    const payload = this.nurseryForm.value;

    if (this.isEditMode()) {
      this.http.put(`${this.apiUrl}/${this.nurseryId()}`, payload).subscribe({
        next: () => { this.successMessage.set('تم التعديل بنجاح!'); this.isSubmitting.set(false); },
        error: () => this.isSubmitting.set(false)
      });
    } else {
      this.http.post(this.apiUrl, payload).subscribe({
        next: (res: any) => { this.successMessage.set('تم التسجيل!'); this.isEditMode.set(true); this.isSubmitting.set(false); },
        error: () => this.isSubmitting.set(false)
      });
    }
  }}
