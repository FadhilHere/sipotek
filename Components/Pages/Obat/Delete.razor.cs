using Microsoft.AspNetCore.Components;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Obat
{
    public partial class Delete : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public Models.Obat Obat { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        async Task Submit()
        {
            try
            {
                var existingObat = await DbContext.Obats.FindAsync(Obat.Id);
                if (existingObat != null)
                {
                    DbContext.Obats.Remove(existingObat);
                    await DbContext.SaveChangesAsync();
                    MudDialog.Close(DialogResult.Ok(true));
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
        }

        void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}