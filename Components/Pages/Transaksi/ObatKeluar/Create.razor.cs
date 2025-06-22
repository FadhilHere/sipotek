using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Transaksi.ObatKeluar
{
    public partial class Create : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        MudForm form = default!;
        bool isValid;
        Models.ObatKeluar obatKeluar = new();
        List<Models.Obat> ObatList = new();
        Models.Obat? selectedObat = null;

        decimal hargaSatuan = 0;

        DateTime? tglKeluar
        {
            get => obatKeluar.TglKeluar;
            set => obatKeluar.TglKeluar = value ?? DateTime.Now;
        }

        protected override async Task OnInitializedAsync()
        {
            obatKeluar.TglKeluar = DateTime.Now;
            obatKeluar.NoTransaksi = await GenerateNoTransaksiAsync();

            // Initialize values
            obatKeluar.JumlahKeluar = 0;
            obatKeluar.TotalHarga = 0;
            obatKeluar.ObatId = 0;

            ObatList = await DbContext.Obats
                .Where(o => o.Stok > 0)
                .OrderBy(o => o.NamaObat)
                .ToListAsync();
        }

        private async Task<string> GenerateNoTransaksiAsync()
        {
            var today = DateTime.Today;
            var prefix = $"TRX{today:yyyyMMdd}";

            var lastTransaction = await DbContext.ObatKeluars
                .Where(ok => ok.NoTransaksi!.StartsWith(prefix))
                .OrderByDescending(ok => ok.NoTransaksi)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastTransaction != null && !string.IsNullOrEmpty(lastTransaction.NoTransaksi))
            {
                var lastNumberPart = lastTransaction.NoTransaksi.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D3}";
        }

        void OnObatSelected()
        {
            Console.WriteLine($"DEBUG: ObatId selected = {obatKeluar.ObatId}");

            if (obatKeluar.ObatId > 0)
            {
                selectedObat = ObatList.FirstOrDefault(o => o.Id == obatKeluar.ObatId);

                if (selectedObat != null)
                {
                    Console.WriteLine($"DEBUG: Selected obat = {selectedObat.NamaObat}, Harga = {selectedObat.Harga}, Stok = {selectedObat.Stok}");

                    // Auto-set harga satuan dari obat
                    hargaSatuan = selectedObat.Harga;

                    // Reset jumlah keluar
                    obatKeluar.JumlahKeluar = 0;

                    // Calculate total
                    CalculateTotal();
                }
            }
            else
            {
                selectedObat = null;
                hargaSatuan = 0;
                obatKeluar.JumlahKeluar = 0;
                CalculateTotal();
            }
        }

        void CalculateTotal()
        {
            obatKeluar.TotalHarga = obatKeluar.JumlahKeluar * hargaSatuan;
            Console.WriteLine($"DEBUG: CalculateTotal - Jumlah: {obatKeluar.JumlahKeluar}, Harga: {hargaSatuan}, Total: {obatKeluar.TotalHarga}");
        }

        string GetJumlahHelperText()
        {
            if (selectedObat != null)
                return $"Max: {selectedObat.Stok} unit";
            return "Pilih obat terlebih dahulu";
        }

        string GetHargaHelperText()
        {
            if (selectedObat != null)
                return "Harga otomatis dari data obat";
            return "Pilih obat terlebih dahulu";
        }

        async Task Submit()
        {
            await form.Validate();

            if (!isValid)
                return;

            if (selectedObat == null || obatKeluar.ObatId == 0)
            {
                Snackbar.Add("Silakan pilih obat terlebih dahulu!", Severity.Error);
                return;
            }

            if (obatKeluar.JumlahKeluar <= 0)
            {
                Snackbar.Add("Jumlah keluar harus lebih dari 0!", Severity.Error);
                return;
            }

            // Validasi stok dengan data terbaru dari database
            var currentObat = await DbContext.Obats.FindAsync(obatKeluar.ObatId);
            if (currentObat == null)
            {
                Snackbar.Add("Obat tidak ditemukan!", Severity.Error);
                return;
            }

            if (obatKeluar.JumlahKeluar > currentObat.Stok)
            {
                Snackbar.Add($"Jumlah keluar ({obatKeluar.JumlahKeluar}) melebihi stok yang tersedia ({currentObat.Stok})!", Severity.Error);
                return;
            }

            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                Console.WriteLine($"DEBUG: Saving - ObatId: {obatKeluar.ObatId}, Jumlah: {obatKeluar.JumlahKeluar}, Total: {obatKeluar.TotalHarga}");

                // Tambah obat keluar
                DbContext.ObatKeluars.Add(obatKeluar);
                await DbContext.SaveChangesAsync();

                // Update stok obat
                currentObat.Stok -= obatKeluar.JumlahKeluar;
                await DbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                Snackbar.Add($"Transaksi {obatKeluar.NoTransaksi} berhasil disimpan!", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
                Console.WriteLine($"DEBUG: Error = {ex.Message}");
            }
        }

        void Cancel()
        {
            MudDialog.Close();
        }
    }
}