import { Component, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-topnav',
  imports: [CommonModule],
  templateUrl: './topnav.html',
  styleUrl: './topnav.css',
})
export class Topnav {
  @Output() toggleSidebar = new EventEmitter<void>();

  notificationsOpen = signal(false);
  profileOpen = signal(false);
  notifCount = signal(3);

  toggleNotifications() {
    this.notificationsOpen.update(v => !v);
    this.profileOpen.set(false);
  }

  toggleProfile() {
    this.profileOpen.update(v => !v);
    this.notificationsOpen.set(false);
  }

  onToggleSidebar() {
    this.toggleSidebar.emit();
  }
}
