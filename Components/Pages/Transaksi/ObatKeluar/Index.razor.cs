using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Transaksi.ObatKeluar
{
    public partial class Index : ComponentBase
    {
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] IDialogService DialogService { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        List<Models.ObatKeluar> ObatKeluarList = new();
        bool Loading = true;

        decimal totalHariIni = 0;
        int transaksiHariIni = 0;
        decimal totalBulanIni = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        async Task LoadData()
        {
            Loading = true;

            ObatKeluarList = await DbContext.ObatKeluars
                .Include(ok => ok.Obat)
                .OrderByDescending(ok => ok.TglKeluar)
                .ToListAsync();

            // Calculate summary
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            totalHariIni = ObatKeluarList
                .Where(ok => ok.TglKeluar.Date == today)
                .Sum(ok => ok.TotalHarga);

            transaksiHariIni = ObatKeluarList
                .Count(ok => ok.TglKeluar.Date == today);

            totalBulanIni = ObatKeluarList
                .Where(ok => ok.TglKeluar >= startOfMonth)
                .Sum(ok => ok.TotalHarga);

            Loading = false;
            StateHasChanged();
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

            var dialog = DialogService.Show<Create>("Tambah Penjualan Obat", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
                Snackbar.Add("Penjualan obat berhasil ditambahkan!", Severity.Success);
            }
        }

        async Task OpenEditDialog(Models.ObatKeluar obatKeluar)
        {
            var parameters = new DialogParameters();
            parameters.Add("ObatKeluar", obatKeluar);
            var options = new DialogOptions()
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            var dialog = DialogService.Show<Edit>("Edit Penjualan Obat", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
                Snackbar.Add("Penjualan obat berhasil diupdate!", Severity.Success);
            }
        }

        async Task OpenDeleteDialog(Models.ObatKeluar obatKeluar)
        {
            var parameters = new DialogParameters();
            parameters.Add("ObatKeluar", obatKeluar);
            var options = new DialogOptions()
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Small
            };

            var dialog = DialogService.Show<Delete>("Hapus Penjualan Obat", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
                Snackbar.Add("Penjualan obat berhasil dihapus!", Severity.Success);
            }
        }
    }
}