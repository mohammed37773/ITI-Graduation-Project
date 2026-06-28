import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
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

  selectedPeriod = signal('30days');

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

  ngOnInit() {}

  ngAfterViewInit() {
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

    const paddingLeft = 48;
    const paddingRight = 20;
    const paddingTop = 20;
    const paddingBottom = 50;
    const chartW = w - paddingLeft - paddingRight;
    const chartH = h - paddingTop - paddingBottom;

    const labels = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو'];
    const users = [280, 420, 380, 620, 890, 1240];
    const nurseries = [40, 65, 55, 90, 130, 180];

    const maxVal = Math.max(...users) * 1.1;
    const minVal = 20;

    const xStep = chartW / (labels.length - 1);
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
    [1360, 1025, 690, 355, 20].forEach((v, i) => {
      ctx.fillText(String(v), paddingLeft - 6, paddingTop + (i / 4) * chartH + 4);
    });

    const gradU = ctx.createLinearGradient(0, paddingTop, 0, paddingTop + chartH);
    gradU.addColorStop(0, 'rgba(46, 204, 113, 0.25)');
    gradU.addColorStop(1, 'rgba(46, 204, 113, 0)');

    ctx.beginPath();
    users.forEach((v, i) => i === 0 ? ctx.moveTo(toX(i), toY(v)) : ctx.lineTo(toX(i), toY(v)));
    ctx.lineTo(toX(users.length - 1), paddingTop + chartH);
    ctx.lineTo(toX(0), paddingTop + chartH);
    ctx.closePath();
    ctx.fillStyle = gradU;
    ctx.fill();

    ctx.beginPath();
    ctx.strokeStyle = '#2ecc71';
    ctx.lineWidth = 2.5;
    ctx.lineJoin = 'round';
    users.forEach((v, i) => i === 0 ? ctx.moveTo(toX(i), toY(v)) : ctx.lineTo(toX(i), toY(v)));
    ctx.stroke();

    users.forEach((v, i) => {
      ctx.beginPath();
      ctx.arc(toX(i), toY(v), 4, 0, Math.PI * 2);
      ctx.fillStyle = '#2ecc71';
      ctx.fill();
      ctx.strokeStyle = '#fff';
      ctx.lineWidth = 2;
      ctx.stroke();
    });

    ctx.beginPath();
    ctx.strokeStyle = '#3498db';
    ctx.lineWidth = 2.5;
    ctx.lineJoin = 'round';
    nurseries.forEach((v, i) => i === 0 ? ctx.moveTo(toX(i), toY(v)) : ctx.lineTo(toX(i), toY(v)));
    ctx.stroke();

    nurseries.forEach((v, i) => {
      ctx.beginPath();
      ctx.arc(toX(i), toY(v), 4, 0, Math.PI * 2);
      ctx.fillStyle = '#3498db';
      ctx.fill();
      ctx.strokeStyle = '#fff';
      ctx.lineWidth = 2;
      ctx.stroke();
    });

    ctx.fillStyle = '#adb5bd';
    ctx.font = '11px Cairo, sans-serif';
    ctx.textAlign = 'center';
    labels.forEach((l, i) => ctx.fillText(l, toX(i), h - 10));
  }

  drawBookingChart() {
    const canvas = this.bookingChartRef?.nativeElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const size = Math.min(canvas.offsetWidth || 200, 220);
    canvas.width = size;
    canvas.height = size;

    const cx = size / 2;
    const cy = size / 2;
    const r = size * 0.38;
    const innerR = size * 0.24;

    const segments = [
      { value: 720, color: '#2ecc71' },
      { value: 350, color: '#3498db' },
      { value: 135, color: '#e74c3c' }
    ];
    const total = segments.reduce((s, x) => s + x.value, 0);

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
