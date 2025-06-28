using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Transaksi.ObatMasuk
{
    public partial class Edit : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public Models.ObatMasuk ObatMasuk { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        MudForm form = default!;
        bool isValid;
        Models.ObatMasuk obatMasuk = new();
        List<Models.Obat> ObatList = new();

        decimal hargaSatuan = 0;
        int originalJumlahMasuk = 0;
        DateTime originalTglKadaluarsaM;

        DateTime? tglMasuk
        {
            get => obatMasuk.TglMasuk;
            set => obatMasuk.TglMasuk = value ?? DateTime.Today;
        }

        DateTime? tglKadaluarsaM
        {
            get => obatMasuk.TglKadaluarsaM;
            set => obatMasuk.TglKadaluarsaM = value ?? DateTime.Today.AddYears(1);
        }

        protected override async Task OnInitializedAsync()
        {
            ObatList = await DbContext.Obats
                .OrderBy(o => o.NamaObat)
                .ToListAsync();

            // Copy data from parameter
            originalJumlahMasuk = ObatMasuk.JumlahMasuk;
            originalTglKadaluarsaM = ObatMasuk.TglKadaluarsaM;

            obatMasuk = new Models.ObatMasuk
            {
                Id = ObatMasuk.Id,
                ObatId = ObatMasuk.ObatId,
                JumlahMasuk = ObatMasuk.JumlahMasuk,
                TotalHarga = ObatMasuk.TotalHarga,
                TglMasuk = ObatMasuk.TglMasuk,
                TglKadaluarsaM = ObatMasuk.TglKadaluarsaM,
                Supplier = ObatMasuk.Supplier
            };

            // Calculate harga satuan
            if (obatMasuk.JumlahMasuk > 0)
            {
                hargaSatuan = obatMasuk.TotalHarga / obatMasuk.JumlahMasuk;
            }
        }

        void CalculateTotal()
        {
            obatMasuk.TotalHarga = obatMasuk.JumlahMasuk * hargaSatuan;
        }

        async Task Submit()
        {
            await form.Validate();

            if (!isValid)
                return;

            if (obatMasuk.JumlahMasuk <= 0)
            {
                Snackbar.Add("Jumlah masuk harus lebih dari 0!", Severity.Error);
                return;
            }

            if (hargaSatuan <= 0)
            {
                Snackbar.Add("Harga satuan harus lebih dari 0!", Severity.Error);
                return;
            }

            // Validasi tanggal kadaluarsa
            if (obatMasuk.TglKadaluarsaM <= DateTime.Today)
            {
                Snackbar.Add("Tanggal kadaluarsa harus di masa depan!", Severity.Error);
                return;
            }

            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                var existingObatMasuk = await DbContext.ObatMasuks.FindAsync(obatMasuk.Id);
                if (existingObatMasuk != null)
                {
                    // Update obat masuk
                    existingObatMasuk.ObatId = obatMasuk.ObatId;
                    existingObatMasuk.JumlahMasuk = obatMasuk.JumlahMasuk;
                    existingObatMasuk.TotalHarga = obatMasuk.TotalHarga;
                    existingObatMasuk.TglMasuk = obatMasuk.TglMasuk;
                    existingObatMasuk.TglKadaluarsaM = obatMasuk.TglKadaluarsaM;
                    existingObatMasuk.Supplier = obatMasuk.Supplier;

                    // Update stok obat dengan FIFO Logic
                    var obat = await DbContext.Obats.FindAsync(obatMasuk.ObatId);
                    if (obat != null)
                    {
                        // Rollback stok lama, apply stok baru
                        var oldStok = obat.Stok;
                        obat.Stok = obat.Stok - originalJumlahMasuk + obatMasuk.JumlahMasuk;

                        // FIFO Logic untuk tanggal kadaluarsa
                        await UpdateExpiryDateWithFIFO(obat, originalTglKadaluarsaM, obatMasuk.TglKadaluarsaM);

                        // Success message
                        var successMessage = $"Edit berhasil! Stok {obat.NamaObat}: {oldStok} → {obat.Stok} " +
                                           $"(perubahan: {obatMasuk.JumlahMasuk - originalJumlahMasuk:+#;-#;0})";

                        Snackbar.Add(successMessage, Severity.Success);
                    }

                    await DbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                MudDialog.Close(DialogResult.Ok(true));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
                Console.WriteLine($"Error in ObatMasuk Edit: {ex.Message}");
            }
        }

        async Task UpdateExpiryDateWithFIFO(Models.Obat obat, DateTime oldExpiry, DateTime newExpiry)
        {
            // Cek semua batch obat masuk untuk obat ini (kecuali yang sedang diedit)
            var allBatches = await DbContext.ObatMasuks
                .Where(om => om.ObatId == obat.Id && om.Id != obatMasuk.Id)
                .Select(om => om.TglKadaluarsaM)
                .ToListAsync();

            // Tambahkan batch baru
            allBatches.Add(newExpiry);

            // Cari tanggal kadaluarsa paling cepat (FIFO)
            var earliestExpiry = allBatches.Min();

            // Update jika ada perubahan
            if (obat.TglKadaluarsa != earliestExpiry)
            {
                var oldObatExpiry = obat.TglKadaluarsa;
                obat.TglKadaluarsa = earliestExpiry;

                Console.WriteLine($"FIFO Update Edit: {obat.NamaObat} - Expire date updated from {oldObatExpiry:dd/MM/yyyy} to {earliestExpiry:dd/MM/yyyy}");

                Snackbar.Add($"Tanggal kadaluarsa {obat.NamaObat} diupdate ke {earliestExpiry:dd/MM/yyyy} (FIFO)", Severity.Info);
            }
        }

        void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}