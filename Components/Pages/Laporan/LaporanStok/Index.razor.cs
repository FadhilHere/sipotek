using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;
using System.Text;

namespace SIPOTEK.Components.Pages.Laporan.LaporanStok
{
    public partial class Index : ComponentBase
    {
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] IJSRuntime JSRuntime { get; set; } = default!;

        List<Models.Obat> obatList = new();
        List<Models.Obat> filteredObatList = new();
        List<string> jenisObatList = new();
        bool Loading = true;
        bool isExporting = false;

        // Filter properties
        string? filterJenisObat = null;
        StokStatus filterStatus = StokStatus.Semua;
        KadaluarsaStatus filterKadaluarsa = KadaluarsaStatus.Semua;
        string searchString = "";

        // Summary data
        StokSummary summary = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        async Task LoadData()
        {
            Loading = true;

            obatList = await DbContext.Obats
                .OrderBy(o => o.NamaObat)
                .ToListAsync();

            jenisObatList = obatList
                .Where(o => !string.IsNullOrEmpty(o.JenisObat))
                .Select(o => o.JenisObat!)
                .Distinct()
                .OrderBy(j => j)
                .ToList();

            CalculateSummary();
            ApplyFilter();

            Loading = false;
            StateHasChanged();
        }

        void CalculateSummary()
        {
            summary = new StokSummary
            {
                TotalJenisObat = obatList.Count,
                TotalUnit = obatList.Sum(o => o.Stok),
                NilaiTotalStok = obatList.Sum(o => o.Stok * o.Harga),
                ObatStokRendah = obatList.Count(o => o.Stok <= o.StokMinimum) // Updated to use individual StokMinimum
            };
        }

        void ApplyFilter()
        {
            filteredObatList = obatList;

            // Filter by search string
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                filteredObatList = filteredObatList
                    .Where(o => o.NamaObat.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                               (o.Produsen?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            // Filter by jenis obat
            if (!string.IsNullOrEmpty(filterJenisObat))
            {
                filteredObatList = filteredObatList
                    .Where(o => o.JenisObat == filterJenisObat)
                    .ToList();
            }

            // Filter by stock status using individual StokMinimum
            if (filterStatus != StokStatus.Semua)
            {
                filteredObatList = filterStatus switch
                {
                    StokStatus.Normal => filteredObatList.Where(o => o.Stok > (o.StokMinimum * 1.5)).ToList(),
                    StokStatus.Rendah => filteredObatList.Where(o => o.Stok > o.StokMinimum && o.Stok <= (o.StokMinimum * 1.5)).ToList(),
                    StokStatus.Kritis => filteredObatList.Where(o => o.Stok >= 1 && o.Stok <= o.StokMinimum).ToList(),
                    StokStatus.Habis => filteredObatList.Where(o => o.Stok == 0).ToList(),
                    _ => filteredObatList
                };
            }

            // Filter by expiry status
            if (filterKadaluarsa != KadaluarsaStatus.Semua)
            {
                var today = DateTime.Today;
                filteredObatList = filterKadaluarsa switch
                {
                    KadaluarsaStatus.Normal => filteredObatList.Where(o => (o.TglKadaluarsa - today).Days > 90).ToList(),
                    KadaluarsaStatus.Mendekati => filteredObatList.Where(o => (o.TglKadaluarsa - today).Days >= 31 && (o.TglKadaluarsa - today).Days <= 90).ToList(),
                    KadaluarsaStatus.Segera => filteredObatList.Where(o => (o.TglKadaluarsa - today).Days <= 30).ToList(),
                    _ => filteredObatList
                };
            }

            StateHasChanged();
        }

        void ResetFilter()
        {
            filterJenisObat = null;
            filterStatus = StokStatus.Semua;
            filterKadaluarsa = KadaluarsaStatus.Semua;
            searchString = "";
            ApplyFilter();
        }

        async Task ExportToExcel()
        {
            isExporting = true;
            StateHasChanged();

            try
            {
                var csv = GenerateCSVContent();
                var fileName = $"Laporan_Stok_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

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
                // Untuk implementasi PDF yang sederhana, kita bisa gunakan print dengan CSS
                await JSRuntime.InvokeAsync<object>("printReport", "Laporan Stok Obat");
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
                await JSRuntime.InvokeAsync<object>("printReport", "Laporan Stok Obat");
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
            csv.AppendLine("LAPORAN STOK OBAT - APOTEK CIPTA MUTIARA");
            csv.AppendLine($"Tanggal: {DateTime.Now:dd/MM/yyyy HH:mm}");
            csv.AppendLine("");

            // Summary
            csv.AppendLine("RINGKASAN:");
            csv.AppendLine($"Total Jenis Obat,{summary.TotalJenisObat}");
            csv.AppendLine($"Total Unit Stok,{summary.TotalUnit}");
            csv.AppendLine($"Nilai Total Stok,Rp {summary.NilaiTotalStok:N0}");
            csv.AppendLine($"Obat Stok Rendah/Kritis,{summary.ObatStokRendah}");
            csv.AppendLine("");

            // Data header
            csv.AppendLine("Nama Obat,Jenis,Bentuk,Produsen,Stok,Stok Minimum,Harga,Nilai Stok,Tanggal Kadaluarsa,Status Stok,Status Kadaluarsa");

            // Data rows
            foreach (var obat in filteredObatList)
            {
                var nilaiStok = obat.Stok * obat.Harga;
                var statusStok = GetStockStatus(obat.Stok, obat.StokMinimum);
                var statusKadaluarsa = GetExpiryStatus(obat.TglKadaluarsa);

                csv.AppendLine($"\"{obat.NamaObat}\",\"{obat.JenisObat}\",\"{obat.BentukObat}\",\"{obat.Produsen}\",{obat.Stok},{obat.StokMinimum},Rp {obat.Harga:N0},Rp {nilaiStok:N0},{obat.TglKadaluarsa:dd/MM/yyyy},\"{statusStok}\",\"{statusKadaluarsa}\"");
            }

            return csv.ToString();
        }

        Color GetStockColor(int stok, int stokMinimum)
        {
            if (stok == 0) return Color.Dark; // Habis
            if (stok <= stokMinimum) return Color.Error; // Kritis
            if (stok <= (stokMinimum * 1.5)) return Color.Warning; // Rendah
            return Color.Success; // Normal
        }

        Color GetStockColor(int stok)
        {
            // Fallback method for backward compatibility
            return stok switch
            {
                0 => Color.Dark,
                <= 10 => Color.Error,
                <= 30 => Color.Warning,
                _ => Color.Success
            };
        }

        string GetStockStatus(int stok, int stokMinimum)
        {
            if (stok == 0) return "Habis";
            if (stok <= stokMinimum) return "Kritis";
            if (stok <= (stokMinimum * 1.5)) return "Rendah";
            return "Normal";
        }

        string GetStockStatus(int stok)
        {
            // Fallback method for backward compatibility
            return stok switch
            {
                0 => "Habis",
                <= 10 => "Kritis",
                <= 30 => "Rendah",
                _ => "Normal"
            };
        }

        Color GetExpiryColor(DateTime expiryDate)
        {
            var daysUntilExpiry = (expiryDate - DateTime.Now).Days;
            return daysUntilExpiry switch
            {
                <= 30 => Color.Error,
                <= 90 => Color.Warning,
                _ => Color.Success
            };
        }

        string GetExpiryStatus(DateTime expiryDate)
        {
            var daysUntilExpiry = (expiryDate - DateTime.Now).Days;
            return daysUntilExpiry switch
            {
                <= 0 => "Kadaluarsa",
                <= 30 => "Segera",
                <= 90 => "Mendekati",
                _ => "Normal"
            };
        }

        public enum StokStatus
        {
            Semua,
            Normal,
            Rendah,
            Kritis,
            Habis
        }

        public enum KadaluarsaStatus
        {
            Semua,
            Normal,
            Mendekati,
            Segera
        }

        public class StokSummary
        {
            public int TotalJenisObat { get; set; }
            public int TotalUnit { get; set; }
            public decimal NilaiTotalStok { get; set; }
            public int ObatStokRendah { get; set; }
        }
    }
}