using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] SipotekDbContext DbContext { get; set; } = default!;

        // Summary data
        int totalObat = 0;
        int totalStok = 0;
        decimal penjualanHariIni = 0;
        int transaksiHariIni = 0;
        int obatStokRendah = 0;

        // Lists
        List<Models.Obat> obatStokRendahList = new();
        List<Models.Obat> obatMendekatiKadaluarsa = new();
        List<RecentActivity> recentActivities = new();
        List<NotificationItem> notifikasi = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadDashboardData();
        }

        async Task LoadDashboardData()
        {
            var today = DateTime.Today;
            var thirtyDaysFromNow = DateTime.Today.AddDays(30);

            // Load summary data
            totalObat = await DbContext.Obats.CountAsync();
            totalStok = await DbContext.Obats.SumAsync(o => o.Stok);

            var penjualanToday = await DbContext.ObatKeluars
                .Where(ok => ok.TglKeluar.Date == today)
                .ToListAsync();

            penjualanHariIni = penjualanToday.Sum(ok => ok.TotalHarga);
            transaksiHariIni = penjualanToday.Count;

            // Load obat stok rendah (stok <= 10)
            obatStokRendahList = await DbContext.Obats
                .Where(o => o.Stok <= 10)
                .OrderBy(o => o.Stok)
                .Take(5)
                .ToListAsync();

            obatStokRendah = obatStokRendahList.Count;

            // Load obat mendekati kadaluarsa (30 hari ke depan)
            obatMendekatiKadaluarsa = await DbContext.Obats
                .Where(o => o.TglKadaluarsa <= thirtyDaysFromNow && o.TglKadaluarsa > today)
                .OrderBy(o => o.TglKadaluarsa)
                .Take(5)
                .ToListAsync();

            // Load recent activities
            await LoadRecentActivities();

            // Generate notifications
            GenerateNotifications();
        }

        async Task LoadRecentActivities()
        {
            var activities = new List<RecentActivity>();

            // Get recent obat masuk
            var recentMasuk = await DbContext.ObatMasuks
                .Include(om => om.Obat)
                .OrderByDescending(om => om.TglMasuk)
                .Take(3)
                .ToListAsync();

            foreach (var masuk in recentMasuk)
            {
                activities.Add(new RecentActivity
                {
                    Waktu = masuk.TglMasuk,
                    Jenis = "Obat Masuk",
                    Detail = $"{masuk.Obat?.NamaObat} - {masuk.JumlahMasuk} unit dari {masuk.Supplier}",
                    Color = Color.Success
                });
            }

            // Get recent obat keluar
            var recentKeluar = await DbContext.ObatKeluars
                .Include(ok => ok.Obat)
                .OrderByDescending(ok => ok.TglKeluar)
                .Take(3)
                .ToListAsync();

            foreach (var keluar in recentKeluar)
            {
                activities.Add(new RecentActivity
                {
                    Waktu = keluar.TglKeluar,
                    Jenis = "Penjualan",
                    Detail = $"{keluar.Obat?.NamaObat} - {keluar.JumlahKeluar} unit ke {keluar.Pelanggan}",
                    Color = Color.Info
                });
            }

            recentActivities = activities
                .OrderByDescending(a => a.Waktu)
                .Take(6)
                .ToList();
        }

        void GenerateNotifications()
        {
            notifikasi.Clear();

            // Notifikasi stok rendah
            if (obatStokRendah > 0)
            {
                notifikasi.Add(new NotificationItem
                {
                    Pesan = $"{obatStokRendah} obat memiliki stok rendah dan perlu di-restock",
                    Severity = Severity.Warning
                });
            }

            // Notifikasi obat mendekati kadaluarsa
            if (obatMendekatiKadaluarsa.Count > 0)
            {
                notifikasi.Add(new NotificationItem
                {
                    Pesan = $"{obatMendekatiKadaluarsa.Count} obat akan kadaluarsa dalam 30 hari ke depan",
                    Severity = Severity.Error
                });
            }

            // Notifikasi penjualan hari ini
            if (transaksiHariIni > 0)
            {
                notifikasi.Add(new NotificationItem
                {
                    Pesan = $"Hari ini telah terjadi {transaksiHariIni} transaksi penjualan",
                    Severity = Severity.Success
                });
            }
        }

        Color GetStockColor(int stok)
        {
            return stok switch
            {
                <= 5 => Color.Error,
                <= 10 => Color.Warning,
                _ => Color.Success
            };
        }

        string GetStockStatus(int stok)
        {
            return stok switch
            {
                <= 5 => "Kritis",
                <= 10 => "Rendah",
                _ => "Normal"
            };
        }

        Color GetExpiryColor(DateTime expiryDate)
        {
            var daysUntilExpiry = (expiryDate - DateTime.Now).Days;
            return daysUntilExpiry switch
            {
                <= 7 => Color.Error,
                <= 30 => Color.Warning,
                _ => Color.Success
            };
        }

        int GetDaysUntilExpiry(DateTime expiryDate)
        {
            return (expiryDate - DateTime.Now).Days;
        }

        public class RecentActivity
        {
            public DateTime Waktu { get; set; }
            public string Jenis { get; set; } = string.Empty;
            public string Detail { get; set; } = string.Empty;
            public Color Color { get; set; }
        }

        public class NotificationItem
        {
            public string Pesan { get; set; } = string.Empty;
            public Severity Severity { get; set; }
        }
    }
}