import { CurrencyPipe, NgClass } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-after-payment',
  imports: [CurrencyPipe, NgClass],
  templateUrl: './after-payment.html',
  styleUrl: './after-payment.css',
})
export class AfterPayment implements OnInit {
  isSuccess: boolean = false;
  amount: number = 0;
  transactionId: string = '';

  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      // تحويل القيم من الـ URL
      this.isSuccess = params['success'] === 'true';
      this.transactionId = params['id'] || '';
      
      // تحويل من قروش إلى جنيه
      const cents = parseFloat(params['amount_cents']) || 0;
      this.amount = cents / 100; 
    });
  }

  goToMyBookings() {
    this.router.navigate(['/parent/bookings']); // غير المسار ده حسب سيستم الـ Routing عندك
  }
}