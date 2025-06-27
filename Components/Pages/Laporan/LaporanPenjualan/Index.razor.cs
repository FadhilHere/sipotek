using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;
using System.Text;

namespace SIPOTEK.Components.Pages.Laporan.LaporanPenjualan
{
    public partial class Index : ComponentBase
    {
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] IJSRuntime JSRuntime { get; set; } = default!;

        List<Models.ObatKeluar> transaksiList = new();
        List<Models.ObatKeluar> filteredTransaksiList = new();
        List<ChartData> chartData = new();
        List<TopObat> topObatList = new();
        bool Loading = true;
        bool isExporting = false;

        // Filter properties
        PeriodType periodFilter = PeriodType.Bulan;
        DateTime? startDate = DateTime.Today.AddDays(-30);
        DateTime? endDate = DateTime.Today;
        string searchString = "";

        // Summary data
        PenjualanSummary summary = new();

        protected override async Task OnInitializedAsync()
        {
            SetDefaultDates();
            await LoadData();
        }

        void SetDefaultDates()
        {
            var today = DateTime.Today;
            (startDate, endDate) = periodFilter switch
            {
                PeriodType.Hari => (today, today),
                PeriodType.Minggu => (today.AddDays(-(int)today.DayOfWeek), today),
                PeriodType.Bulan => (new DateTime(today.Year, today.Month, 1), today),
                _ => (startDate, endDate)
            };
        }

        async Task OnPeriodChanged()
        {
            SetDefaultDates();
            await LoadData();
        }

        async Task LoadData()
        {
            if (startDate == null || endDate == null)
            {
                Snackbar.Add("Pilih tanggal mulai dan akhir terlebih dahulu!", Severity.Warning);
                return;
            }

            Loading = true;

            try
            {
                var start = startDate.Value.Date;
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1); // End of day

                // Debug: Log tanggal filter
                Console.WriteLine($"DEBUG: Filter dari {start:yyyy-MM-dd} sampai {end:yyyy-MM-dd}");

                // Ambil semua data dulu untuk debug
                var allTransaksi = await DbContext.ObatKeluars
                    .Include(ok => ok.Obat)
                    .OrderByDescending(ok => ok.TglKeluar)
                    .ToListAsync();

                Console.WriteLine($"DEBUG: Total semua transaksi di database: {allTransaksi.Count}");

                if (allTransaksi.Any())
                {
                    var oldestDate = allTransaksi.Min(t => t.TglKeluar);
                    var newestDate = allTransaksi.Max(t => t.TglKeluar);
                    Console.WriteLine($"DEBUG: Transaksi tertua: {oldestDate:yyyy-MM-dd}, terbaru: {newestDate:yyyy-MM-dd}");
                }

                // Filter berdasarkan tanggal
                transaksiList = allTransaksi
                    .Where(ok => ok.TglKeluar >= start && ok.TglKeluar <= end)
                    .ToList();

                Console.WriteLine($"DEBUG: Transaksi setelah filter: {transaksiList.Count}");

                CalculateSummary();
                GenerateChartData();
                GenerateTopObat();
                FilterTransactions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error = {ex.Message}");
                Snackbar.Add($"Error loading data: {ex.Message}", Severity.Error);
            }
            finally
            {
                Loading = false;
                StateHasChanged();
            }
        }

        void CalculateSummary()
        {
            summary = new PenjualanSummary
            {
                TotalPenjualan = transaksiList.Sum(t => t.TotalHarga),
                TotalTransaksi = transaksiList.Count,
                TotalUnitTerjual = transaksiList.Sum(t => t.JumlahKeluar),
                RataRataPerTransaksi = transaksiList.Any() ? transaksiList.Average(t => t.TotalHarga) : 0
            };
        }

        void GenerateChartData()
        {
            chartData.Clear();

            Console.WriteLine($"DEBUG: GenerateChartData - transaksiList.Count = {transaksiList.Count}");

            // Group by month instead of daily
            var groupedData = transaksiList
                .GroupBy(t => new { Year = t.TglKeluar.Year, Month = t.TglKeluar.Month })
                .Select(g => new ChartData
                {
                    Tanggal = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalPenjualan = g.Sum(t => t.TotalHarga),
                    TotalTransaksi = g.Count()
                })
                .OrderBy(c => c.Tanggal)
                .ToList();

            Console.WriteLine($"DEBUG: Monthly groupedData.Count = {groupedData.Count}");

            // Add all months data (take last 6 months if no data)
            if (groupedData.Any())
            {
                chartData = groupedData;
            }
            else
            {
                // If no data, show last 6 months with zero values
                var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                for (int i = 5; i >= 0; i--)
                {
                    var month = currentMonth.AddMonths(-i);
                    chartData.Add(new ChartData
                    {
                        Tanggal = month,
                        TotalPenjualan = 0,
                        TotalTransaksi = 0
                    });
                }
            }

            Console.WriteLine($"DEBUG: Final chartData.Count = {chartData.Count}");
            foreach (var data in chartData)
            {
                Console.WriteLine($"DEBUG: {data.Tanggal:yyyy-MM} - Rp {data.TotalPenjualan} ({data.TotalTransaksi} tx)");
            }
        }

        void GenerateTopObat()
        {
            topObatList = transaksiList
                .GroupBy(t => new { t.ObatId, t.Obat?.NamaObat })
                .Select(g => new TopObat
                {
                    NamaObat = g.Key.NamaObat ?? "Unknown",
                    TotalTerjual = g.Sum(t => t.JumlahKeluar),
                    TotalNilai = g.Sum(t => t.TotalHarga)
                })
                .OrderByDescending(o => o.TotalNilai)
                .Take(5)
                .ToList();
        }

        void FilterTransactions()
        {
            filteredTransaksiList = transaksiList;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                filteredTransaksiList = filteredTransaksiList
                    .Where(t => (t.NoTransaksi?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                               (t.Obat?.NamaObat?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                               (t.Pelanggan?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            StateHasChanged();
        }

        void ResetFilter()
        {
            periodFilter = PeriodType.Bulan;
            SetDefaultDates();
            searchString = "";
            LoadData();
        }

        double GetProgressValue(decimal value)
        {
            if (chartData.Count == 0) return 0;
            var maxValue = chartData.Max(c => c.TotalPenjualan);
            if (maxValue == 0) return 0;
            return (double)(value / maxValue * 100);
        }

        string GetPeriodText()
        {
            if (!startDate.HasValue || !endDate.HasValue) return "";

            if (startDate.Value.Date == endDate.Value.Date)
            {
                return startDate.Value.ToString("dd MMMM yyyy");
            }
            else
            {
                return $"{startDate.Value:dd/MM/yyyy} - {endDate.Value:dd/MM/yyyy}";
            }
        }

        async Task ExportToExcel()
        {
            isExporting = true;
            StateHasChanged();

            try
            {
                var csv = GenerateCSVContent();
                var fileName = $"Laporan_Penjualan_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                await JSRuntime.InvokeAsync<object>("downloadFile", fileName, "text/csv", csv);
                Snackbar.Add("Laporan berhasil diexport ke Excel!", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error saat export: {ex.Message}", Severity.Error);
            }
            finally
            {
                isExporting = false;
                StateHasChanged();
            }
        }

        async Task ExportToPdf()
        {
            isExporting = true;
            StateHasChanged();

            try
            {
                await JSRuntime.InvokeAsync<object>("printReport", "Laporan Penjualan");
                Snackbar.Add("Laporan siap untuk di-print sebagai PDF!", Severity.Info);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error saat export PDF: {ex.Message}", Severity.Error);
            }
            finally
            {
                isExporting = false;
                StateHasChanged();
            }
        }

        async Task PrintReport()
        {
            try
            {
                await JSRuntime.InvokeAsync<object>("printReport", "Laporan Penjualan");
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error saat print: {ex.Message}", Severity.Error);
            }
        }

        string GenerateCSVContent()
        {
            var csv = new StringBuilder();

            // Header
            csv.AppendLine("LAPORAN PENJUALAN - APOTEK CIPTA MUTIARA");
            csv.AppendLine($"Periode: {GetPeriodText()}");
            csv.AppendLine($"Tanggal Cetak: {DateTime.Now:dd/MM/yyyy HH:mm}");
            csv.AppendLine("");

            // Summary
            csv.AppendLine("RINGKASAN:");
            csv.AppendLine($"Total Penjualan,Rp {summary.TotalPenjualan:N0}");
            csv.AppendLine($"Total Transaksi,{summary.TotalTransaksi}");
            csv.AppendLine($"Total Unit Terjual,{summary.TotalUnitTerjual}");
            csv.AppendLine($"Rata-rata per Transaksi,Rp {summary.RataRataPerTransaksi:N0}");
            csv.AppendLine("");

            // Top Obat
            csv.AppendLine("TOP 5 OBAT TERLARIS:");
            csv.AppendLine("Nama Obat,Total Terjual,Total Nilai");
            foreach (var obat in topObatList)
            {
                csv.AppendLine($"\"{obat.NamaObat}\",{obat.TotalTerjual},Rp {obat.TotalNilai:N0}");
            }
            csv.AppendLine("");

            // Detail Transaksi
            csv.AppendLine("DETAIL TRANSAKSI:");
            csv.AppendLine("Tanggal,No Transaksi,Nama Obat,Bentuk,Jumlah,Total Harga,Pelanggan");

            foreach (var transaksi in filteredTransaksiList)
            {
                csv.AppendLine($"{transaksi.TglKeluar:dd/MM/yyyy HH:mm},\"{transaksi.NoTransaksi}\",\"{transaksi.Obat?.NamaObat}\",\"{transaksi.Obat?.BentukObat}\",{transaksi.JumlahKeluar},Rp {transaksi.TotalHarga:N0},\"{transaksi.Pelanggan}\"");
            }

            return csv.ToString();
        }

        public enum PeriodType
        {
            Hari,
            Minggu,
            Bulan,
            Custom
        }

        public class PenjualanSummary
        {
            public decimal TotalPenjualan { get; set; }
            public int TotalTransaksi { get; set; }
            public int TotalUnitTerjual { get; set; }
            public decimal RataRataPerTransaksi { get; set; }
        }

        public class ChartData
        {
            public DateTime Tanggal { get; set; }
            public decimal TotalPenjualan { get; set; }
            public int TotalTransaksi { get; set; }
        }

        public class TopObat
        {
            public string NamaObat { get; set; } = "";
            public int TotalTerjual { get; set; }
            public decimal TotalNilai { get; set; }
        }
    }
}