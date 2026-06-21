import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject,OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router,  } from '@angular/router';

@Component({
  selector: 'app-new-booking',
  imports: [CommonModule, ReactiveFormsModule ],
  templateUrl: './new-booking.html',
  styleUrl: './new-booking.css',
})
export class NewBooking implements OnInit  {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);

  private baseUrl = 'http://localhost:5104/api';

  nurseryId = signal<number | null>(null);
  nurseryName = signal<string>('جاري تحميل اسم الحضانة...');
  myChildren = signal<any[]>([]);
  
  isLoading = signal<boolean>(false);
  isSubmitted = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  bookingForm!: FormGroup;

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('nurseryId'));
    if (id) {
      this.nurseryId.set(id);
      this.loadNurseryDetails(id);
    }
    this.initForm();
    this.loadMyChildren();
  }

  private initForm() {
    this.bookingForm = this.fb.group({
      childId: ['', [Validators.required]],
      startDate: ['', [Validators.required]]
    });
  }

  private loadNurseryDetails(id: number) {
    this.http.get<any>(`${this.baseUrl}/Nurseries/${id}`).subscribe({
      next: (nursery) => this.nurseryName.set(nursery?.name || 'الحضانة المختارة'),
      error: () => this.nurseryName.set('حضانة مسجلة بالنظام')
    });
  }

  private loadMyChildren() {
    this.http.get<any[]>(`${this.baseUrl}/Children/my`).subscribe({
      next: (data) => this.myChildren.set(data || []),
      error: () => this.errorMessage.set('فشل في تحميل قائمة أطفالك.')
    });
  }

  onSubmitBooking() {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const payload = {
      nurseryId: this.nurseryId(),
      childId: parseInt(this.bookingForm.value.childId),
      startDate: this.bookingForm.value.startDate
    };

    this.http.post(`${this.baseUrl}/Bookings`, payload).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isSubmitted.set(true);
        setTimeout(() => this.router.navigate(['/parent/bookings']), 2000);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'حدث خطأ أثناء إرسال طلب الحجز.');
      }
    });
  }
}
