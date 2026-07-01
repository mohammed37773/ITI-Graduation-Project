import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router'; // 1. تأكد من عمل import لـ Router هنا
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {
  public authService = inject(AuthService);
  public router = inject(Router);

  onLogout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']); 
  }
}