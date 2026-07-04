

import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NurseriesService } from '../../../../core/services/nurseries';

interface Nursery {
  id: number;
  name: string;
  description: string;
  dailyPrice: number;
  ageRangeMin: number;
  ageRangeMax: number;
  capacity: number;
  avgRating: number;
  isVerified: boolean;
  createdAt: string;
  city: string;
  address: string;
  imagesCount: number;
  reviewsCount: number;
}

@Component({
  selector: 'app-manage-nurseries',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-nurseries.html',
  styleUrl: './manage-nurseries.css',
})
export class ManageNurseries implements OnInit {
  showAddModal = signal(false);
  searchQuery = signal('');
  selectedFilter = signal('all');
  loading = signal(true);
  error = signal<string | null>(null);

  nurseries = signal<Nursery[]>([]);
  statsData = { total: 0, verified: 0, pending: 0, avgPrice: 0, avgRating: 0 };

  newNursery = {
    name: '',
    location: '',
    description: '',
    ageRange: '0-5',
    capacity: '',
    price: '',
    facilities: { outdoor: false, cctv: false, meals: false, eyfs: false }
  };

  constructor(private nurseriesService: NurseriesService) {}

  ngOnInit() {
    this.loadNurseries();
    this.loadStats();
  }

  loadNurseries() {
    this.loading.set(true);
    this.error.set(null);
    this.nurseriesService.getAll().subscribe({
      next: (data: any) => {
        this.nurseries.set(Array.isArray(data) ? data : (data?.data ?? data?.nurseries ?? []));
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading nurseries:', err);
        this.error.set('تعذر تحميل الحضانات');
        this.loading.set(false);
      }
    });
  }

  loadStats() {
    this.nurseriesService.getStats().subscribe({
      next: (data: any) => {
        this.statsData = {
          total: data?.total ?? 0,
          verified: data?.verified ?? 0,
          pending: data?.pending ?? 0,
          avgPrice: data?.avgPrice ?? 0,
          avgRating: data?.avgRating ?? 0
        };
      },
      error: (err) => console.error('Error loading nursery stats:', err)
    });
  }

  get filteredNurseries(): Nursery[] {
    return this.nurseries().filter(n => {
      const q = this.searchQuery().toLowerCase();
      const matchesSearch = n.name?.toLowerCase().includes(q) ||
        n.city?.toLowerCase().includes(q) ||
        n.address?.toLowerCase().includes(q);
      const matchesFilter = this.selectedFilter() === 'all' ||
        (this.selectedFilter() === 'verified' && n.isVerified) ||
        (this.selectedFilter() === 'pending' && !n.isVerified);
      return matchesSearch && matchesFilter;
    });
  }

  get stats() {
    return {
      total: this.statsData.total || this.nurseries().length,
      verified: this.statsData.verified || this.nurseries() .filter(n => n.isVerified).length,
      pending: this.statsData.pending || this.nurseries().filter(n => !n.isVerified).length,
      avgPrice: this.statsData.avgPrice,
      avgRating: this.statsData.avgRating
    };
  }

  openAddModal() { this.showAddModal.set(true); }
  closeAddModal() { this.showAddModal.set(false); }

  verifyNursery(id: number) {
    this.nurseriesService.verify(id).subscribe({
      next: () => {
        this.nurseries.update(list => list.map(n => n.id === id ? { ...n, isVerified: true } : n));
      },
      error: (err) => console.error('Error verifying nursery:', err)
    });
  }

  deleteNursery(id: number) {
    this.nurseriesService.delete(id).subscribe({
      next: () => this.nurseries.set(this.nurseries().filter(n => n.id !== id)),
      error: (err) => console.error('Error deleting nursery:', err)
    });
  }

  submitNursery() {
    this.closeAddModal();
    this.newNursery = { name: '', location: '', description: '', ageRange: '0-5', capacity: '', price: '', facilities: { outdoor: false, cctv: false, meals: false, eyfs: false } };
  }

  getStars(): number[] { return [1, 2, 3, 4, 5]; }
  setFilter(f: string) { this.selectedFilter.set(f); }

  formatDate(date: string): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('ar-EG', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
