using Microsoft.AspNetCore.Components;
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
                        obat.Stok -= existingObatMasuk.JumlahMasuk;

                        // Pastikan stok tidak negatif
                        if (obat.Stok < 0)
                        {
                            Snackbar.Add("Tidak dapat menghapus transaksi karena akan membuat stok menjadi negatif!", Severity.Error);
                            await transaction.RollbackAsync();
                            return;
                        }
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
            }
        }

        void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}