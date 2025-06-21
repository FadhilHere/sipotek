using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Components.Pages.Obat;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Obat
{
	public partial class Index : ComponentBase
	{
		[Inject] SipotekDbContext DbContext { get; set; } = default!;
		[Inject] IDialogService DialogService { get; set; } = default!;
		[Inject] ISnackbar Snackbar { get; set; } = default!;

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

		Color GetStockColor(int stok)
		{
			return stok switch
			{
				<= 10 => Color.Error,
				<= 30 => Color.Warning,
				_ => Color.Success
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

		async Task OpenCreateDialog()
		{
			var parameters = new DialogParameters();
			var options = new DialogOptions() { CloseOnEscapeKey = true };

			var dialog = DialogService.Show<Create>("Tambah Obat Baru", parameters, options);
			var result = await dialog.Result;

			if (!result.Canceled)
			{
				await LoadData();
				Snackbar.Add("Obat berhasil ditambahkan!", Severity.Success);
			}
		}

		async Task OpenEditDialog(Models.Obat obat)
		{
			var parameters = new DialogParameters();
			parameters.Add("Obat", obat);
			var options = new DialogOptions() { CloseOnEscapeKey = true };

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