using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Transaksi.ObatMasuk
{
    public partial class Delete : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public Models.ObatMasuk ObatMasuk { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        async Task Submit()
        {
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                var existingObatMasuk = await DbContext.ObatMasuks.FindAsync(ObatMasuk.Id);
                if (existingObatMasuk != null)
                {
                    // Update stok obat (kurangi stok)
                    var obat = await DbContext.Obats.FindAsync(existingObatMasuk.ObatId);
                    if (obat != null)
                    {
                        var oldStok = obat.Stok;
                        obat.Stok -= existingObatMasuk.JumlahMasuk;

                        // Pastikan stok tidak negatif
                        if (obat.Stok < 0)
                        {
                            Snackbar.Add("Tidak dapat menghapus transaksi karena akan membuat stok menjadi negatif!", Severity.Error);
                            await transaction.RollbackAsync();
                            return;
                        }

                        // FIFO Logic: Recalculate tanggal kadaluarsa setelah hapus batch
                        await RecalculateExpiryDateAfterDelete(obat, existingObatMasuk.TglKadaluarsaM);

                        // Success message
                        var successMessage = $"Batch dihapus! Stok {obat.NamaObat}: {oldStok} → {obat.Stok} (-{existingObatMasuk.JumlahMasuk})";
                        Snackbar.Add(successMessage, Severity.Success);
                    }

                    // Hapus transaksi
                    DbContext.ObatMasuks.Remove(existingObatMasuk);
                    await DbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                MudDialog.Close(DialogResult.Ok(true));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
                Console.WriteLine($"Error in ObatMasuk Delete: {ex.Message}");
            }
        }

        async Task RecalculateExpiryDateAfterDelete(Models.Obat obat, DateTime deletedExpiry)
        {
            // Cek apakah masih ada batch lain untuk obat ini
            var remainingBatches = await DbContext.ObatMasuks
                .Where(om => om.ObatId == obat.Id && om.Id != ObatMasuk.Id) // Exclude yang akan dihapus
                .Select(om => om.TglKadaluarsaM)
                .ToListAsync();

            if (remainingBatches.Any())
            {
                // Cari tanggal kadaluarsa paling cepat dari batch yang tersisa (FIFO)
                var earliestExpiry = remainingBatches.Min();

                // Update jika berbeda
                if (obat.TglKadaluarsa != earliestExpiry)
                {
                    var oldExpiry = obat.TglKadaluarsa;
                    obat.TglKadaluarsa = earliestExpiry;

                    Console.WriteLine($"FIFO Recalculate Delete: {obat.NamaObat} - Expire date updated from {oldExpiry:dd/MM/yyyy} to {earliestExpiry:dd/MM/yyyy}");

                    Snackbar.Add($"Tanggal kadaluarsa {obat.NamaObat} diupdate ke {earliestExpiry:dd/MM/yyyy} (FIFO)", Severity.Info);
                }
            }
            else
            {
                // Jika tidak ada batch tersisa, set ke default (1 tahun dari sekarang)
                var defaultExpiry = DateTime.Today.AddYears(1);

                if (obat.TglKadaluarsa != defaultExpiry)
                {
                    var oldExpiry = obat.TglKadaluarsa;
                    obat.TglKadaluarsa = defaultExpiry;

                    Console.WriteLine($"FIFO Reset Delete: {obat.NamaObat} - No batches remaining, expire date reset from {oldExpiry:dd/MM/yyyy} to {defaultExpiry:dd/MM/yyyy}");

                    Snackbar.Add($"Tidak ada batch tersisa untuk {obat.NamaObat}. Tanggal kadaluarsa direset ke {defaultExpiry:dd/MM/yyyy}", Severity.Warning);
                }
            }
        }

        void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}