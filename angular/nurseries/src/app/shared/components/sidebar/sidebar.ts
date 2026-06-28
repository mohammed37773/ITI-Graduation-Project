import { Component, Input, Output, EventEmitter } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  badge?: number;
}

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  @Input() collapsed = false;
  @Output() toggleSidebar = new EventEmitter<void>();

  navItems: NavItem[] = [
  
    { label: 'لوحة التحكم', icon: 'bi-grid-1x2', route: '/dashboard' },
    { label: 'إدارة الحضانات', icon: 'bi-building', route: '/manage-nurseries' },
    { label: 'إدارة المستخدمين', icon: 'bi-people', route: '/users' },
    { label: 'إدارة الحجوزات', icon: 'bi-calendar-check', route: '/bookings' },
    { label: 'إدارة المدفوعات', icon: 'bi-credit-card', route: '/payments' },
    { label: 'التقارير والتحليلات', icon: 'bi-bar-chart-line', route: '/reports' },
  ];

  onToggle() {
    this.toggleSidebar.emit();
  }
}
