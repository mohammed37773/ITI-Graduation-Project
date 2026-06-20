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
  // 2. عمل inject للـ Router والـ AuthService وعملهم public عشان الـ HTML يشوفهم
  public authService = inject(AuthService);
  public router = inject(Router); // السطر ده هو اللي هيحل المشكلة فوراً! 🚀

  onLogout() {
    this.authService.logout();
    
    // 3. التوجيه لصفحة الـ login الحقيقية بناءً على الـ routes بتاعتك
    this.router.navigate(['/auth/login']); 
  }
}