import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

import { AttendanceService } from 'src/app/proxy/application-services';
import {
  AttendanceDto, CreateAttendanceDto, UpdateAttendanceDto,
  GetAllAttendancesInput,
} from 'src/app/proxy/dtos/models';
import { AttendanceStatus, AttendanceSource, AttendanceExplainStatus } from 'src/app/proxy/enums';

import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { SkeletonModule } from 'primeng/skeleton';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { MessageService, ConfirmationService } from 'primeng/api';

// ---- Types ----
interface EmployeeRow {
  employeeId: number;
  employeeName: string;
  employeeCode: string;
  days: (AttendanceDto | null)[];   // index = ngày - 1
  totalPresent: number;
}

@Component({
  selector: 'app-attendance',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, FormsModule,
    ButtonModule, SelectModule, InputTextModule, TextareaModule,
    DialogModule, TagModule, ToastModule, TooltipModule,
    ConfirmDialogModule, IconFieldModule, InputIconModule,
    SkeletonModule, InputNumberModule, DatePickerModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './attendance.html',
  styleUrl: './attendance.scss',
})
export class AttendanceComponent implements OnInit {
  private attendanceService = inject(AttendanceService);
  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);
  private confirmService = inject(ConfirmationService);

  // ---- Date navigation ----
  today = new Date();
  currentYear = signal(this.today.getFullYear());
  currentMonth = signal(this.today.getMonth() + 1);

  daysInMonth = computed(() =>
    new Date(this.currentYear(), this.currentMonth(), 0).getDate()
  );

  dayHeaders = computed(() =>
    Array.from({ length: this.daysInMonth() }, (_, i) => {
      const d = new Date(this.currentYear(), this.currentMonth() - 1, i + 1);
      return { day: i + 1, weekday: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'][d.getDay()], isWeekend: d.getDay() === 0 || d.getDay() === 6 };
    })
  );

  yearOptions = Array.from({ length: 5 }, (_, i) => ({ label: String(this.today.getFullYear() - 1 + i), value: this.today.getFullYear() - 1 + i }));
  monthOptions = Array.from({ length: 12 }, (_, i) => ({ label: `Tháng ${i + 1}`, value: i + 1 }));

  // ---- State ----
  rows = signal<EmployeeRow[]>([]);
  isLoading = signal(false);
  raw = signal<AttendanceDto[]>([]);

  // ---- Check-in/out widget ----
  checkinRecord = signal<AttendanceDto | null>(null);
  isCheckingIn = signal(false);
  isCheckingOut = signal(false);
  clockDisplay = signal('');
  private clockTimer: any;

  // ---- Edit cell dialog ----
  showEditDialog = signal(false);
  editingRecord = signal<AttendanceDto | null>(null);
  editingCell = signal<{ empId: number; day: number } | null>(null);
  isEditSubmitting = signal(false);

  editForm: FormGroup = this.fb.group({
    status: [null, Validators.required],
    checkInAt: [null],
    checkOutAt: [null],
    lateMinutes: [null, [Validators.min(0), Validators.max(1440)]],
    earlyLeaveMinutes: [null, [Validators.min(0), Validators.max(1440)]],
    otMinutes: [null, [Validators.min(0), Validators.max(1440)]],
    note: [''],
  });

  // ---- Create dialog (khi cell chưa có record) ----
  showCreateDialog = signal(false);
  isCreateSubmitting = signal(false);

  createForm: FormGroup = this.fb.group({
    employeeId: [null, Validators.required],
    workDate: [null, Validators.required],
    scheduleId: [1, Validators.required],   // default schedule
    status: [null, Validators.required],
    checkInAt: [null],
    checkOutAt: [null],
    otMinutes: [null, [Validators.min(0)]],
    note: [''],
    source: [AttendanceSource.Manual],
  });

  // ---- Options ----
  readonly AttendanceStatus = AttendanceStatus;

  statusOptions = [
    { label: 'Đi làm', value: AttendanceStatus.Present },
    { label: 'Đi muộn', value: AttendanceStatus.Late },
    { label: 'Về sớm', value: AttendanceStatus.Early },
    { label: 'Vắng mặt', value: AttendanceStatus.Absent },
    { label: 'Nửa ngày', value: AttendanceStatus.HalfDay },
    { label: 'Nghỉ phép', value: AttendanceStatus.OnLeave },
  ];

  private readonly errMsgs: Record<string, Record<string, string>> = {
    status: { required: 'Vui lòng chọn trạng thái' },
    employeeId: { required: 'Vui lòng chọn nhân viên' },
    workDate: { required: 'Vui lòng chọn ngày' },
    lateMinutes: { min: 'Không hợp lệ', max: 'Không quá 1440 phút' },
    earlyLeaveMinutes: { min: 'Không hợp lệ', max: 'Không quá 1440 phút' },
    otMinutes: { min: 'Không hợp lệ' },
  };

  // ---- Filter ----
  filterForm: FormGroup = this.fb.group({ keyword: [''] });

  filteredRows = computed(() => {
    const kw = this.filterForm.value.keyword?.toLowerCase() ?? '';
    return kw
      ? this.rows().filter(r =>
        r.employeeName.toLowerCase().includes(kw) ||
        r.employeeCode.toLowerCase().includes(kw))
      : this.rows();
  });

  // ---- Lifecycle ----
  ngOnInit(): void {
    this.startClock();
    this.loadGrid();
    this.loadTodayRecord();
  }

  ngOnDestroy(): void {
    clearInterval(this.clockTimer);
  }

  // ---- Clock ----
  private startClock(): void {
    const tick = () => {
      const now = new Date();
      this.clockDisplay.set(
        now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
      );
    };
    tick();
    this.clockTimer = setInterval(tick, 1000);
  }

  // ---- Load today's own record (for widget) ----
  loadTodayRecord(): void {
    const today = this.toDateStr(new Date());
    this.attendanceService.getList({
      workDateFrom: today, workDateTo: today,
      maxResultCount: 1, skipCount: 0,
    }).subscribe({ next: res => this.checkinRecord.set(res.items![0] ?? null) });
  }

  // ---- Check-in / Check-out ----
  doCheckIn(): void {
    this.isCheckingIn.set(true);
    this.attendanceService.checkIn().subscribe({
      next: (rec) => {
        this.checkinRecord.set(rec);
        this.isCheckingIn.set(false);
        this.toast('success', `Check-in lúc ${this.formatTime(rec.checkInAt)}`);
      },
      error: () => { this.toastError('Check-in thất bại'); this.isCheckingIn.set(false); },
    });
  }

  doCheckOut(): void {
    this.isCheckingOut.set(true);
    this.attendanceService.checkOut().subscribe({
      next: (rec) => {
        this.checkinRecord.set(rec);
        this.isCheckingOut.set(false);
        this.toast('success', `Check-out lúc ${this.formatTime(rec.checkOutAt)}`);
      },
      error: () => { this.toastError('Check-out thất bại'); this.isCheckingOut.set(false); },
    });
  }

  // ---- Navigation ----
  prevMonth(): void {
    if (this.currentMonth() === 1) {
      this.currentMonth.set(12);
      this.currentYear.update(y => y - 1);
    } else {
      this.currentMonth.update(m => m - 1);
    }
    this.loadGrid();
  }

  nextMonth(): void {
    if (this.currentMonth() === 12) {
      this.currentMonth.set(1);
      this.currentYear.update(y => y + 1);
    } else {
      this.currentMonth.update(m => m + 1);
    }
    this.loadGrid();
  }

  goToMonth(): void { this.loadGrid(); }

  // ---- Load grid ----
  loadGrid(): void {
    this.isLoading.set(true);
    const input: GetAllAttendancesInput = {
      month: this.currentMonth(),
      year: this.currentYear(),
      maxResultCount: 1000,
      skipCount: 0,
    };
    this.attendanceService.getList(input).subscribe({
      next: (res) => {
        this.raw.set(res.items!);
        this.rows.set(this.buildRows(res.items!));
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  private buildRows(records: AttendanceDto[]): EmployeeRow[] {
    const empMap = new Map<number, EmployeeRow>();
    const dim = this.daysInMonth();

    for (const r of records) {
      const empId = r.employeeId!;
      if (!empMap.has(empId)) {
        empMap.set(empId, {
          employeeId: empId,
          employeeName: r.employeeName ?? '',
          employeeCode: r.employeeCode ?? '',
          days: Array(dim).fill(null),
          totalPresent: 0,
        });
      }
      const day = new Date(r.workDate!).getDate();
      if (day >= 1 && day <= dim) {
        empMap.get(empId)!.days[day - 1] = r;
      }
    }

    for (const row of empMap.values()) {
      row.totalPresent = row.days.filter(d =>
        d && (d.status === AttendanceStatus.Present || d.status === AttendanceStatus.Late || d.status === AttendanceStatus.Early)
      ).length;
    }

    return [...empMap.values()].sort((a, b) => a.employeeName.localeCompare(b.employeeName));
  }


  parseTime = (timeStr: string | null | undefined) => {
    if (!timeStr) return null;
    // timeStr có thể là "08:30:00" hoặc "2024-05-20T08:30:00"
    const parts = timeStr.includes('T') ? timeStr.split('T')[1].split(':') : timeStr.split(':');
    const d = new Date();
    d.setHours(+parts[0], +parts[1], +parts[2] || 0, 0);
    return d;
  };

  // ---- Click cell ----
  onCellClick(row: EmployeeRow, dayIndex: number): void {
    const record = row.days[dayIndex];
    const day = dayIndex + 1;
    const dateObj = new Date(this.currentYear(), this.currentMonth() - 1, day);
    const isLocked = record?.isLocked;

    if (isLocked) {
      this.toast('warn', 'Bản ghi đã bị khoá, không thể sửa');
      return;
    }

    if (record) {
      this.editingRecord.set(record);
      this.editingCell.set({ empId: row.employeeId, day });

      this.editForm.patchValue({
        status: record.status ?? null,
        // Dùng hàm bọc an toàn để parse Date
        checkInAt: this.parseTime(record.checkInAt),
        checkOutAt: this.parseTime(record.checkOutAt),
        lateMinutes: record.lateMinutes ?? null,
        earlyLeaveMinutes: record.earlyLeaveMinutes ?? null,
        otMinutes: record.otMinutes ?? null,
        note: record.note ?? '',
      });
      this.showEditDialog.set(true);
    } else {
      // Create new
      this.editingCell.set({ empId: row.employeeId, day });
      this.createForm.patchValue({
        employeeId: row.employeeId,
        workDate: dateObj,
        status: null,
        checkInAt: null,
        checkOutAt: null,
      });
      this.showCreateDialog.set(true);
    }
  }

  // ---- Submit edit ----
  submitEdit(): void {
    if (this.editForm.invalid) { this.editForm.markAllAsTouched(); return; }
    const v = this.editForm.value;
    const rec = this.editingRecord()!;

    const payload: UpdateAttendanceDto = {
      status: v.status,
      checkInAt: v.checkInAt ? this.timeObjToIso(v.checkInAt, rec.workDate!) : null,
      checkOutAt: v.checkOutAt ? this.timeObjToIso(v.checkOutAt, rec.workDate!) : null,
      lateMinutes: v.lateMinutes ?? null,
      earlyLeaveMinutes: v.earlyLeaveMinutes ?? null,
      otMinutes: v.otMinutes ?? 0,
      note: v.note ?? '',
    };

    this.isEditSubmitting.set(true);
    this.attendanceService.update(rec.id!, payload).subscribe({
      next: (updated) => {
        this.patchRaw(updated);
        this.isEditSubmitting.set(false);
        this.showEditDialog.set(false);
        this.toast('success', 'Đã cập nhật chấm công');
      },
      error: () => { this.toastError(); this.isEditSubmitting.set(false); },
    });
  }

  // ---- Submit create ----
  submitCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    const v = this.createForm.value;

    const workDate = this.toDateStr(v.workDate);
    const payload: CreateAttendanceDto = {
      employeeId: v.employeeId,
      workDate,
      scheduleId: v.scheduleId,
      status: v.status,
      checkInAt: v.checkInAt ? this.timeObjToIso(v.checkInAt, workDate) : null,
      checkOutAt: v.checkOutAt ? this.timeObjToIso(v.checkOutAt, workDate) : null,
      otMinutes: v.otMinutes ?? 0,
      note: v.note ?? '',
      source: AttendanceSource.Manual,
    };

    this.isCreateSubmitting.set(true);
    this.attendanceService.create(payload).subscribe({
      next: (created) => {
        this.patchRaw(created);
        this.isCreateSubmitting.set(false);
        this.showCreateDialog.set(false);
        this.toast('success', 'Đã tạo bản ghi chấm công');
      },
      error: () => { this.toastError(); this.isCreateSubmitting.set(false); },
    });
  }

  // ---- Update signal sau khi save ----
  private patchRaw(updated: AttendanceDto): void {
    const existing = this.raw().find(r => r.id === updated.id);
    if (existing) {
      this.raw.update(list => list.map(r => r.id === updated.id ? updated : r));
    } else {
      this.raw.update(list => [...list, updated]);
    }
    this.rows.set(this.buildRows(this.raw()));
  }

  // ---- Helpers ----
  getCellSymbol(record: AttendanceDto | null): string {
    if (!record) return '';
    switch (record.status) {
      case AttendanceStatus.Present: return 'X';
      case AttendanceStatus.Late: return 'M';
      case AttendanceStatus.Early: return 'VS';
      case AttendanceStatus.Absent: return 'KP';
      case AttendanceStatus.HalfDay: return '½';
      case AttendanceStatus.OnLeave: return 'P';
      default: return '?';
    }
  }

  getCellClass(record: AttendanceDto | null, isWeekend: boolean): string {
    if (isWeekend && !record) return 'cell cell--weekend';
    if (!record) return 'cell cell--empty';
    const base = 'cell';
    const locked = record.isLocked ? ' cell--locked' : '';
    switch (record.status) {
      case AttendanceStatus.Present: return `${base} cell--present${locked}`;
      case AttendanceStatus.Late: return `${base} cell--late${locked}`;
      case AttendanceStatus.Early: return `${base} cell--early${locked}`;
      case AttendanceStatus.Absent: return `${base} cell--absent${locked}`;
      case AttendanceStatus.HalfDay: return `${base} cell--halfday${locked}`;
      case AttendanceStatus.OnLeave: return `${base} cell--onleave${locked}`;
      default: return base;
    }
  }

  getStatusLabel(s: AttendanceStatus | undefined): string {
    return this.statusOptions.find(o => o.value === s)?.label ?? '—';
  }

  getStatusSeverity(s: AttendanceStatus | undefined): string {
    switch (s) {
      case AttendanceStatus.Present: return 'success';
      case AttendanceStatus.Late: return 'warn';
      case AttendanceStatus.Early: return 'warn';
      case AttendanceStatus.Absent: return 'danger';
      case AttendanceStatus.HalfDay: return 'info';
      case AttendanceStatus.OnLeave: return 'info';
      default: return 'secondary';
    }
  }

  isFormInvalid(form: FormGroup, field: string): boolean {
    const c = form.get(field);
    return !!(c?.invalid && (c.touched || c.dirty));
  }

  getFormError(form: FormGroup, field: string): string {
    const c = form.get(field);
    if (!c?.errors) return '';
    const key = Object.keys(c.errors)[0];
    return this.errMsgs[field]?.[key] ?? 'Không hợp lệ';
  }

  formatTime(timeStr: string | null | undefined): string {
    const date = this.parseTime(timeStr);

    if (!date) return '—';

    return date.toLocaleTimeString('vi-VN', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  private toDateStr(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  private timeObjToIso(timeDate: Date, workDate: string): string {
    const h = String(timeDate.getHours()).padStart(2, '0');
    const m = String(timeDate.getMinutes()).padStart(2, '0');
    return `${workDate}T${h}:${m}:00`;
  }

  private toast(severity: string, detail: string): void {
    this.messageService.add({ severity, summary: severity === 'success' ? 'Thành công' : 'Cảnh báo', detail, life: 3000 });
  }

  private toastError(msg = 'Thao tác thất bại'): void {
    this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: msg, life: 3000 });
  }

  get monthLabel(): string {
    return `Tháng ${String(this.currentMonth()).padStart(2, '0')}/${this.currentYear()}`;
  }

  get hasCheckedIn(): boolean { return !!this.checkinRecord()?.checkInAt; }
  get hasCheckedOut(): boolean { return !!this.checkinRecord()?.checkOutAt; }
}