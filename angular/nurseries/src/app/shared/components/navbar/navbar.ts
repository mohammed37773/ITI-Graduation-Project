import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from "@angular/router";
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink , RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  authService = inject(AuthService);
  private router = inject(Router);

  // دالة الـ Logout اللي بتنفذ الـ TODO بتاعك وتمسح الـ Token
  onLogout() {
    this.authService.logout(); // هتمسح الـ token وتخلي الـ Role بـ null
    this.router.navigate(['/auth/login']); // توجيهه لصفحة تسجيل الدخول
  }
}
