using Microsoft.AspNetCore.Components;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Transaksi.ObatKeluar
{
    public partial class Delete : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public Models.ObatKeluar ObatKeluar { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        async Task Submit()
        {
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                var existingObatKeluar = await DbContext.ObatKeluars.FindAsync(ObatKeluar.Id);
                if (existingObatKeluar != null)
                {
                    // Update stok obat (tambah kembali stok)
                    var obat = await DbContext.Obats.FindAsync(existingObatKeluar.ObatId);
                    if (obat != null)
                    {
                        obat.Stok += existingObatKeluar.JumlahKeluar;
                    }

                    // Hapus transaksi
                    DbContext.ObatKeluars.Remove(existingObatKeluar);
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