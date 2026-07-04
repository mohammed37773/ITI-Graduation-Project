import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BookingsService } from '../../../../core/services/bookings';
import { Booking } from '../../../../core/models/booking.model';
// import { RouterLink } from '@angular/router';

interface StatCard {
  label: string;
  value: string;
  icon: string;
  trend: string;
  trendUp: boolean;
  iconBg: string;
  iconColor: string;
}

interface MonthlyBookingPoint {
  label: string;
  successful: number;
  cancelled: number;
}

interface BookingStatusSegment {
  label: string;
  value: number;
  color: string;
}

interface ActivityItem {
  user: string;
  action: string;
  target: string;
  timeAgo: string;
  avatar: string;
}

interface PendingNursery {
  name: string;
  submitted: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})

export class Dashboard implements OnInit, AfterViewInit {
  @ViewChild('registrationChart') registrationChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('bookingChart') bookingChartRef!: ElementRef<HTMLCanvasElement>;

  private bookingsService = inject(BookingsService);

  selectedPeriod = signal('30days');

  // بيانات حقيقية قادمة من /api/admin/bookings
  bookings = signal<Booking[]>([]);
  isLoadingBookings = signal(true);
  loadError = signal(false);
  private viewReady = false;

  private readonly arabicMonths = [
    'يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو',
    'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر',
  ];

  private normalizeStatus(status: Booking['status']): string {
    return String(status);
  }

  // 🔥 توزيع الحجوزات الشهري (آخر 6 شهور) — ناجحة (Confirmed/Completed) مقابل ملغاة (Cancelled)
  monthlyTrend = computed<MonthlyBookingPoint[]>(() => {
    const now = new Date();
    const buckets: MonthlyBookingPoint[] = [];
    for (let i = 5; i >= 0; i--) {
      const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
      buckets.push({ label: this.arabicMonths[d.getMonth()], successful: 0, cancelled: 0 });
    }

    this.bookings().forEach((b) => {
      const d = new Date(b.startDate);
      if (isNaN(d.getTime())) return;
      const monthsAgo =
        (now.getFullYear() - d.getFullYear()) * 12 + (now.getMonth() - d.getMonth());
      if (monthsAgo < 0 || monthsAgo > 5) return;
      const bucket = buckets[5 - monthsAgo];
      const status = this.normalizeStatus(b.status);
      if (status === 'Cancelled') bucket.cancelled++;
      else if (status === 'Confirmed' || status === 'Completed') bucket.successful++;
    });

    return buckets;
  });

  // 🔥 توزيع الحجوزات حسب الحالة (بيانات حقيقية 100%)
  bookingStatusSegments = computed<BookingStatusSegment[]>(() => {
    const counts: Record<string, number> = { Completed: 0, Confirmed: 0, Pending: 0, Cancelled: 0 };
    this.bookings().forEach((b) => {
      const status = this.normalizeStatus(b.status);
      if (status in counts) counts[status]++;
    });

    return [
      { label: 'مكتملة', value: counts['Completed'], color: '#2ecc71' },
      { label: 'مؤكدة', value: counts['Confirmed'], color: '#9b59b6' },
      { label: 'قيد الانتظار', value: counts['Pending'], color: '#3498db' },
      { label: 'ملغاة', value: counts['Cancelled'], color: '#e74c3c' },
    ].filter((s) => s.value > 0);
  });

  stats: StatCard[] = [
    { label: 'إجمالي المستخدمين', value: '12,482', icon: 'bi-people', trend: '+12.5%', trendUp: true, iconBg: 'rgba(52, 152, 219, 0.1)', iconColor: '#3498db' },
    { label: 'الحضانات النشطة', value: '842', icon: 'bi-building', trend: '+4.2%', trendUp: true, iconBg: 'rgba(26, 122, 74, 0.1)', iconColor: '#1a7a4a' },
    { label: 'إجمالي المراجعات', value: '45,210', icon: 'bi-star', trend: '-2.1%', trendUp: false, iconBg: 'rgba(243, 156, 18, 0.1)', iconColor: '#f39c12' },
    { label: 'حجوزات جديدة', value: '1,205', icon: 'bi-calendar-check', trend: '+18.7%', trendUp: true, iconBg: 'rgba(155, 89, 182, 0.1)', iconColor: '#9b59b6' }
  ];

  activityItems: ActivityItem[] = [
    { user: 'سارة ميلر', action: 'أضافت قائمة حضانة جديدة', target: 'روضة أوكس الصغيرة', timeAgo: 'منذ دقيقتين', avatar: 'https://images.pexels.com/photos/1065084/pexels-photo-1065084.jpeg?auto=compress&cs=tinysrgb&w=80&h=80&dpr=1' },
    { user: 'جيمس ويلسون', action: 'أبلغ عن مراجعة لـ', target: 'أكاديمية الشمس', timeAgo: 'منذ 15 دقيقة', avatar: 'https://images.pexels.com/photos/220453/pexels-photo-220453.jpeg?auto=compress&cs=tinysrgb&w=80&h=80&dpr=1' },
    { user: 'إيلينا رودريغيز', action: 'تحققت من حساب مزوّد لـ', target: 'حضانة بامبلبي هايتس', timeAgo: 'منذ ساعة', avatar: 'https://images.pexels.com/photos/1239291/pexels-photo-1239291.jpeg?auto=compress&cs=tinysrgb&w=80&h=80&dpr=1' },
    { user: 'روبرت تشن', action: 'حدّث إعدادات النظام لـ', target: 'وحدة الفوترة', timeAgo: 'منذ 3 ساعات', avatar: 'https://images.pexels.com/photos/614810/pexels-photo-614810.jpeg?auto=compress&cs=tinysrgb&w=80&h=80&dpr=1' },
    { user: 'إيميلي ديفيس', action: 'حذفت مستخدماً غير نشط', target: 'user_9921', timeAgo: 'منذ 5 ساعات', avatar: 'https://images.pexels.com/photos/1542085/pexels-photo-1542085.jpeg?auto=compress&cs=tinysrgb&w=80&h=80&dpr=1' }
  ];

  pendingNurseries: PendingNursery[] = [
    { name: 'البدايات المشرقة', submitted: '2024-06-12' },
    { name: 'مونتيسوري المسارات السعيدة', submitted: '2024-06-11' },
    { name: 'مركز أطفال قوس قزح', submitted: '2024-06-10' },
    { name: 'روضة الخطوات الصغيرة', submitted: '2024-06-10' },
    { name: 'حضانة قادة المستقبل', submitted: '2024-06-09' }
  ];

  quickActions = [
    { label: 'إدارة المستخدمين', icon: 'bi-people', desc: 'مراجعة صلاحيات المستخدمين ونشاطهم', route: '/users', color: '#3498db' },
    { label: 'قائمة انتظار المراجعات', icon: 'bi-star', desc: 'الإشراف على التقييمات والملاحظات', route: '/reviews', color: '#f39c12' },
    { label: 'تقارير النظام', icon: 'bi-bar-chart-line', desc: 'تنزيل تحليلات المنصة الشهرية', route: '/reports', color: '#9b59b6' }
  ];

  ngOnInit() {
    this.fetchBookings();
  }

  ngAfterViewInit() {
    this.viewReady = true;
    this.redrawCharts();
  }

  // جلب الحجوزات الحقيقية من الباك اند ورسم الشارتات بناءً عليها
  fetchBookings() {
    this.isLoadingBookings.set(true);
    this.loadError.set(false);
    this.bookingsService.getAll().subscribe({
      next: (data: Booking[]) => {
        this.bookings.set(data || []);
        this.isLoadingBookings.set(false);
        this.redrawCharts();
      },
      error: (err) => {
        console.error('خطأ أثناء جلب حجوزات لوحة تحكم الأدمن:', err);
        this.bookings.set([]);
        this.isLoadingBookings.set(false);
        this.loadError.set(true);
        this.redrawCharts();
      },
    });
  }

  private redrawCharts() {
    if (!this.viewReady) return;
    this.drawRegistrationChart();
    this.drawBookingChart();
  }

  drawRegistrationChart() {
    const canvas = this.registrationChartRef?.nativeElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const w = canvas.offsetWidth || 560;
    const h = canvas.offsetHeight || 220;
    canvas.width = w;
    canvas.height = h;
    ctx.clearRect(0, 0, w, h);

    const points = this.monthlyTrend();
    const labels = points.map((p) => p.label);
    const successful = points.map((p) => p.successful);
    const cancelled = points.map((p) => p.cancelled);

    const paddingLeft = 40;
    const paddingRight = 20;
    const paddingTop = 20;
    const paddingBottom = 50;
    const chartW = w - paddingLeft - paddingRight;
    const chartH = h - paddingTop - paddingBottom;

    const allVals = [...successful, ...cancelled];
    const rawMax = Math.max(...allVals, 0);
    // لو مفيش أي حجوزات خالص في آخر 6 شهور
    if (rawMax === 0) {
      ctx.fillStyle = '#adb5bd';
      ctx.font = '13px Cairo, sans-serif';
      ctx.textAlign = 'center';
      ctx.fillText('لا توجد بيانات حجوزات كافية لعرض الاتجاه', w / 2, h / 2);
      return;
    }

    const maxVal = Math.max(rawMax * 1.2, 4);
    const minVal = 0;

    const xStep = chartW / (labels.length - 1 || 1);
    const toX = (i: number) => paddingLeft + i * xStep;
    const toY = (v: number) => paddingTop + chartH - ((v - minVal) / (maxVal - minVal)) * chartH;

    ctx.strokeStyle = '#f0f0f0';
    ctx.lineWidth = 1;
    [0, 0.25, 0.5, 0.75, 1].forEach(t => {
      const y = paddingTop + t * chartH;
      ctx.beginPath();
      ctx.moveTo(paddingLeft, y);
      ctx.lineTo(w - paddingRight, y);
      ctx.stroke();
    });

    ctx.fillStyle = '#adb5bd';
    ctx.font = '11px Cairo, sans-serif';
    ctx.textAlign = 'right';
    [0, 0.25, 0.5, 0.75, 1].forEach((t) => {
      const v = Math.round(maxVal - t * maxVal);
      ctx.fillText(String(v), paddingLeft - 6, paddingTop + t * chartH + 4);
    });

    const drawLine = (values: number[], color: string, fillGradient: boolean) => {
      if (fillGradient) {
        const grad = ctx.createLinearGradient(0, paddingTop, 0, paddingTop + chartH);
        grad.addColorStop(0, this.hexToRgba(color, 0.25));
        grad.addColorStop(1, this.hexToRgba(color, 0));

        ctx.beginPath();
        values.forEach((v, i) => i === 0 ? ctx.moveTo(toX(i), toY(v)) : ctx.lineTo(toX(i), toY(v)));
        ctx.lineTo(toX(values.length - 1), paddingTop + chartH);
        ctx.lineTo(toX(0), paddingTop + chartH);
        ctx.closePath();
        ctx.fillStyle = grad;
        ctx.fill();
      }

      ctx.beginPath();
      ctx.strokeStyle = color;
      ctx.lineWidth = 2.5;
      ctx.lineJoin = 'round';
      values.forEach((v, i) => i === 0 ? ctx.moveTo(toX(i), toY(v)) : ctx.lineTo(toX(i), toY(v)));
      ctx.stroke();

      values.forEach((v, i) => {
        ctx.beginPath();
        ctx.arc(toX(i), toY(v), 4, 0, Math.PI * 2);
        ctx.fillStyle = color;
        ctx.fill();
        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 2;
        ctx.stroke();
      });
    };

    drawLine(successful, '#2ecc71', true);
    drawLine(cancelled, '#e74c3c', false);

    ctx.fillStyle = '#adb5bd';
    ctx.font = '11px Cairo, sans-serif';
    ctx.textAlign = 'center';
    labels.forEach((l, i) => ctx.fillText(l, toX(i), h - 10));
  }

  private hexToRgba(hex: string, alpha: number): string {
    const clean = hex.replace('#', '');
    const r = parseInt(clean.substring(0, 2), 16);
    const g = parseInt(clean.substring(2, 4), 16);
    const b = parseInt(clean.substring(4, 6), 16);
    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
  }

  drawBookingChart() {
    const canvas = this.bookingChartRef?.nativeElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const size = Math.min(canvas.offsetWidth || 200, 220);
    canvas.width = size;
    canvas.height = size;
    ctx.clearRect(0, 0, size, size);

    const cx = size / 2;
    const cy = size / 2;
    const r = size * 0.38;
    const innerR = size * 0.24;

    const segments = this.bookingStatusSegments();
    const total = segments.reduce((s, x) => s + x.value, 0);

    if (total === 0) {
      ctx.beginPath();
      ctx.arc(cx, cy, r, 0, Math.PI * 2);
      ctx.fillStyle = '#f1f3f5';
      ctx.fill();
      ctx.beginPath();
      ctx.arc(cx, cy, innerR, 0, Math.PI * 2);
      ctx.fillStyle = '#fff';
      ctx.fill();
      ctx.fillStyle = '#adb5bd';
      ctx.font = `${Math.round(size * 0.07)}px Cairo, sans-serif`;
      ctx.textAlign = 'center';
      ctx.fillText('لا توجد حجوزات', cx, cy + 4);
      return;
    }

    let startAngle = -Math.PI / 2;
    segments.forEach(seg => {
      const angle = (seg.value / total) * 2 * Math.PI;
      ctx.beginPath();
      ctx.moveTo(cx, cy);
      ctx.arc(cx, cy, r, startAngle, startAngle + angle);
      ctx.closePath();
      ctx.fillStyle = seg.color;
      ctx.fill();
      startAngle += angle;
    });

    ctx.beginPath();
    ctx.arc(cx, cy, innerR, 0, Math.PI * 2);
    ctx.fillStyle = '#fff';
    ctx.fill();

    ctx.fillStyle = '#212529';
    ctx.font = `bold ${Math.round(size * 0.1)}px Cairo, sans-serif`;
    ctx.textAlign = 'center';
    ctx.fillText(String(total), cx, cy + 2);
    ctx.fillStyle = '#adb5bd';
    ctx.font = `${Math.round(size * 0.065)}px Cairo, sans-serif`;
    ctx.fillText('الإجمالي', cx, cy + Math.round(size * 0.1) + 4);
  }

  setPeriod(period: string) {
    this.selectedPeriod.set(period);
  }
}
