using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Transaksi.ObatMasuk
{
    public partial class Index : ComponentBase
    {
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] IDialogService DialogService { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        List<Models.ObatMasuk> ObatMasukList = new();
        bool Loading = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        async Task LoadData()
        {
            Loading = true;
            ObatMasukList = await DbContext.ObatMasuks
                .Include(om => om.Obat)
                .OrderByDescending(om => om.TglMasuk)
                .ToListAsync();
            Loading = false;
            StateHasChanged();
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

        async Task OpenCreateDialog()
        {
            var parameters = new DialogParameters();
            var options = new DialogOptions()
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            var dialog = DialogService.Show<Create>("Tambah Obat Masuk", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
                Snackbar.Add("Obat masuk berhasil ditambahkan!", Severity.Success);
            }
        }

        async Task OpenEditDialog(Models.ObatMasuk obatMasuk)
        {
            var parameters = new DialogParameters();
            parameters.Add("ObatMasuk", obatMasuk);
            var options = new DialogOptions()
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            var dialog = DialogService.Show<Edit>("Edit Obat Masuk", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
                Snackbar.Add("Obat masuk berhasil diupdate!", Severity.Success);
            }
        }

        async Task OpenDeleteDialog(Models.ObatMasuk obatMasuk)
        {
            var parameters = new DialogParameters();
            parameters.Add("ObatMasuk", obatMasuk);
            var options = new DialogOptions()
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Small
            };

            var dialog = DialogService.Show<Delete>("Hapus Obat Masuk", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
                Snackbar.Add("Obat masuk berhasil dihapus!", Severity.Success);
            }
        }
    }
}