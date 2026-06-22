import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth';

@Component({
  selector: 'app-nursery-owner',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './nursery-owner.html',
  styleUrl: './nursery-owner.css',
})
export class NurseryOwner {
  private authService = inject(AuthService);
  private router = inject(Router);

  userName = this.authService.currentUser()?.fullName || 'صاحب الحضانة';

  logout() {
    this.authService.logout(); // دالة مسح التوكن من الـ LocalStorage
    this.router.navigate(['/auth/login']);
}
}
