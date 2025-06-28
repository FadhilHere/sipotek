using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Components.Pages.Obat;
using SIPOTEK.Data;
using SIPOTEK.Models;
using SIPOTEK.Services;

namespace SIPOTEK.Components.Pages.Obat
{
    public partial class Index : ComponentBase
    {
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] IDialogService DialogService { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] IFileUploadService FileUploadService { get; set; } = default!;

        List<Models.Obat> ObatList = new();
        bool Loading = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        async Task LoadData()
        {
            Loading = true;
            ObatList = await DbContext.Obats.ToListAsync();
            Loading = false;
            StateHasChanged();
        }

        string GetImageUrl(Models.Obat obat)
        {
            if (!string.IsNullOrEmpty(obat.GambarFileName))
            {
                return FileUploadService.GetImageUrl(obat.GambarFileName);
            }
            return "/images/no-image.png"; // Default image
        }

        Color GetStockColor(int stok, int stokMinimum)
        {
            if (stok == 0) return Color.Dark; // Habis
            if (stok <= stokMinimum) return Color.Error; // Kritis
            if (stok <= (stokMinimum * 1.5)) return Color.Warning; // Rendah
            return Color.Success; // Normal
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

            var dialog = DialogService.Show<Create>("Tambah Obat Baru", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
                Snackbar.Add("Obat berhasil ditambahkan!", Severity.Success);
            }
        }

        async Task OpenDetailDialog(Models.Obat obat)
        {
            var parameters = new DialogParameters();
            parameters.Add("Obat", obat);
            var options = new DialogOptions()
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            var dialog = DialogService.Show<Detail>("Detail Obat", parameters, options);
            await dialog.Result;
        }

        async Task OpenEditDialog(Models.Obat obat)
        {
            var parameters = new DialogParameters();
            parameters.Add("Obat", obat);
            var options = new DialogOptions()
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            var dialog = DialogService.Show<Edit>("Edit Obat", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
                Snackbar.Add("Obat berhasil diupdate!", Severity.Success);
            }
        }

        async Task OpenDeleteDialog(Models.Obat obat)
        {
            var parameters = new DialogParameters();
            parameters.Add("Obat", obat);
            var options = new DialogOptions() { CloseOnEscapeKey = true };

            var dialog = DialogService.Show<Delete>("Hapus Obat", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
                Snackbar.Add("Obat berhasil dihapus!", Severity.Success);
            }
        }
    }
}