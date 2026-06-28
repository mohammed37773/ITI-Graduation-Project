

import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../../../core/services/users';

interface User {
  id: string;
  fullName: string;
  email: string;
  emailConfirmed: boolean;
  lockoutEnabled: boolean;
  roles: string[];
}

@Component({
  selector: 'app-manage-users',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-users.html',
  styleUrl: './manage-users.css',
})
export class ManageUsers implements OnInit {
  searchQuery = signal('');
  selectedRole = signal('all');
  selectedUser = signal<User | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  users: User[] = [];
  statsData = { totalUsers: 0, parents: 0, nurseryAdmins: 0, banned: 0 };

  constructor(private usersService: UsersService) {}

  ngOnInit() {
    this.loadUsers();
    this.loadStats();
  }

  loadUsers() {
    this.loading.set(true);
    this.error.set(null);
    this.usersService.getAll().subscribe({
      next: (data: any) => {
        this.users = Array.isArray(data) ? data : (data?.data ?? data?.users ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading users:', err);
        this.error.set('تعذر تحميل المستخدمين');
        this.loading.set(false);
      }
    });
  }

  loadStats() {
    this.usersService.getStats().subscribe({
      next: (data: any) => {
        this.statsData = {
          totalUsers: data?.totalUsers ?? 0,
          parents: data?.parents ?? 0,
          nurseryAdmins: data?.nurseryAdmins ?? 0,
          banned: data?.banned ?? 0
        };
      },
      error: (err) => console.error('Error loading user stats:', err)
    });
  }

  get filteredUsers(): User[] {
    return this.users.filter(u => {
      const q = this.searchQuery().toLowerCase();
      const matchesSearch = u.fullName?.toLowerCase().includes(q) || u.email?.toLowerCase().includes(q);
      const matchesRole = this.selectedRole() === 'all' ||
        (this.selectedRole() === 'parent' && u.roles?.includes('Parent')) ||
        (this.selectedRole() === 'admin' && u.roles?.includes('Admin')) ||
        (this.selectedRole() === 'nurseryAdmin' && u.roles?.includes('NurseryAdmin'));
      return matchesSearch && matchesRole;
    });
  }

  get stats() {
    return {
      total: this.statsData.totalUsers || this.users.length,
      parents: this.statsData.parents,
      nurseryAdmins: this.statsData.nurseryAdmins,
      banned: this.statsData.banned || this.users.filter(u => u.lockoutEnabled).length
    };
  }

  selectUser(user: User) { this.selectedUser.set(user); }

  banUser(id: string) {
    this.usersService.ban(id).subscribe({
      next: () => {
        const u = this.users.find(x => x.id === id);
        if (u) u.lockoutEnabled = true;
        if (this.selectedUser()?.id === id) {
          this.selectedUser.set({ ...this.selectedUser()!, lockoutEnabled: true });
        }
      },
      error: (err) => console.error('Error banning user:', err)
    });
  }

  unbanUser(id: string) {
    this.usersService.unban(id).subscribe({
      next: () => {
        const u = this.users.find(x => x.id === id);
        if (u) u.lockoutEnabled = false;
        if (this.selectedUser()?.id === id) {
          this.selectedUser.set({ ...this.selectedUser()!, lockoutEnabled: false });
        }
      },
      error: (err) => console.error('Error unbanning user:', err)
    });
  }

  getRoleBadgeClass(roles: string[]): string {
    if (!roles?.length) return 'badge-neutral';
    if (roles.includes('Admin')) return 'badge-error';
    if (roles.includes('NurseryAdmin')) return 'badge-warning';
    return 'badge-neutral';
  }

  getRoleLabel(roles: string[]): string {
    if (!roles?.length) return 'مستخدم';
    if (roles.includes('Admin')) return 'مسؤول';
    if (roles.includes('NurseryAdmin')) return 'مشرف حضانة';
    if (roles.includes('Parent')) return 'ولي أمر';
    return roles[0];
  }

  setRole(role: string) { this.selectedRole.set(role); }
}

