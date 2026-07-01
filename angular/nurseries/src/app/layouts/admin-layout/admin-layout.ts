import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from '../../shared/components/sidebar/sidebar';
import { Topnav } from '../../shared/components/topnav/topnav';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, Sidebar, Topnav, CommonModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout {
  sidebarCollapsed = signal(false);

  toggleSidebar() {
    this.sidebarCollapsed.update(v => !v);
  }
}
