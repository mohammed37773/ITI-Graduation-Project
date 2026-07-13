import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../../../core/services/users';
import { User } from '../../../../core/models/user.model';

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

  users = signal<User[]>([]);
  statsData = { totalUsers: 0, parents: 0, nurseryAdmins: 0, banned: 0 };

  // 🔢 محرك الـ Pagination المطور عبر الـ Signals والـ Computed
  currentPage = signal(1);
  pageSize = 10; // عدد العناصر في كل صفحة

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
        this.users.set(Array.isArray(data) ? data : (data?.data ?? data?.users ?? []));
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading users:', err);
        this.error.set('تعذر تحميل بيانات المستخدمين من السيرفر.');
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

  // مصفوفة المستخدمين المفلترة (بدون قص)
  get filteredUsers(): User[] {
    return this.users().filter(u => {
      const q = this.searchQuery().toLowerCase().trim();
      const matchesSearch = !q || (u.fullName?.toLowerCase().includes(q) || u.email?.toLowerCase().includes(q));
      
      const role = this.selectedRole();
      const matchesRole = role === 'all' ||
        (role === 'parent' && u.roles?.includes('Parent')) ||
        (role === 'admin' && u.roles?.includes('Admin')) ||
        (role === 'nurseryAdmin' && u.roles?.includes('NurseryAdmin'));
        
      return matchesSearch && matchesRole;
    });
  }

  // ✂️ استخراج المجموعات المعروضة بناءً على الصفحة الحالية (Paged Content Mapping)
  get pagedUsers(): User[] {
    const startIndex = (this.currentPage() - 1) * this.pageSize;
    return this.filteredUsers.slice(startIndex, startIndex + this.pageSize);
  }

  // حساب إجمالي عدد الصفحات ديناميكياً
  totalPages = computed(() => {
    const count = this.filteredUsers.length;
    return Math.ceil(count / this.pageSize) || 1;
  });

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  // إنشاء مصفوفة أرقام الصفحات لعرضها في الـ HTML
  getPageArray(): number[] {
    const total = this.totalPages();
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  onSearchChange(value: string) {
    this.searchQuery.set(value);
    this.currentPage.set(1); // إعادة تصفير مؤشر الصفحة عند البحث الجديد
  }

  setRole(role: string) {
    this.selectedRole.set(role);
    this.currentPage.set(1); // إعادة تصفير مؤشر الصفحة عند تغيير الفلتر
  }

  get stats() {
    return {
      total: this.statsData.totalUsers || this.users().length,
      parents: this.statsData.parents,
      nurseryAdmins: this.statsData.nurseryAdmins,
      banned: this.statsData.banned || this.users().filter(u => this.isBanned(u)).length
    };
  }

  selectUser(user: User) { 
    this.selectedUser.set(user); 
  }

  banUser(id: string) {
    this.usersService.ban(id).subscribe({
      next: () => {
        this.users.update(allUsers => {
          return allUsers.map(u => u.id === id ? { ...u, lockoutEnd: new Date("9999-12-31T23:59:59Z") } : u);
        });
        const currentSelected = this.selectedUser();
        if (currentSelected?.id === id) {
          this.selectedUser.set({ ...currentSelected, lockoutEnd: new Date("9999-12-31T23:59:59Z") });
        }
      },
      error: (err) => console.error('Error banning user:', err)
    });
  }

  unbanUser(id: string) {
    this.usersService.unban(id).subscribe({
      next: () => {
        this.users.update(allUsers => {
          return allUsers.map(u => u.id === id ? { ...u, lockoutEnd: null } : u);
        });
        const currentSelected = this.selectedUser();
        if (currentSelected?.id === id) {
          this.selectedUser.set({ ...currentSelected, lockoutEnd: null });
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
    if (!roles?.length) return 'مستخدم عام';
    if (roles.includes('Admin')) return 'مسؤول النظام';
    if (roles.includes('NurseryAdmin')) return 'مشرف حضانة';
    if (roles.includes('Parent')) return 'ولي أمر';
    return roles[0];
  }

  isBanned(user: User): boolean {
    return !!user.lockoutEnd && new Date(user.lockoutEnd) > new Date();
  }

  // دالة مساعدة لاستخدام Math.min داخل قالب الـ HTML بأمان
  mathMin(a: number, b: number): number {
    return Math.min(a, b);
  }
}