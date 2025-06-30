using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages
{
    public partial class Landing : ComponentBase
    {
        [Inject] SipotekDbContext DbContext { get; set; } = default!;

        private List<Models.Obat> obatList = new();
        private List<Models.Obat> filteredObatList = new();
		private List<Models.Obat> paginatedObatList = new();
		private int itemsPerPage = 12;
		private string searchTerm = "";
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadObatData();
        }

        private async Task LoadObatData()
        {
            try
            {
                obatList = await DbContext.Obats
                    .Where(o => o.Stok > 0)
                    .OrderBy(o => o.NamaObat)
                    .ToListAsync();

                filteredObatList = obatList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading obat data: {ex.Message}");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }
		private void UpdatePagination()
		{
			ApplyPagination();
			StateHasChanged();
		}

		private void ApplyPagination()
		{
			if (itemsPerPage == 100)
			{
				paginatedObatList = filteredObatList;
			}
			else
			{
				paginatedObatList = filteredObatList.Take(itemsPerPage).ToList();
			}
		}

		private void FilterObat()
		{
			if (string.IsNullOrWhiteSpace(searchTerm))
			{
				filteredObatList = obatList;
			}
			else
			{
				filteredObatList = obatList
					.Where(o => o.NamaObat.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
							   o.JenisObat?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
					.ToList();
			}
			ApplyPagination();
			StateHasChanged();
		}

		private string GetImageUrl(Models.Obat obat)
		{
			if (!string.IsNullOrEmpty(obat.GambarFileName))
			{
				return $"/uploads/obat/{obat.GambarFileName}";
			}
			return "/images/no-image.png"; // Default image
		}

		private string GetStockStatus(int stok, int stokMinimum)
        {
            if (stok == 0) return "Habis";
            if (stok <= stokMinimum) return "Terbatas";
            return "Tersedia";
        }

        private string GetStockColor(int stok, int stokMinimum)
        {
            if (stok == 0) return "text-danger";
            if (stok <= stokMinimum) return "text-warning";
            return "text-success";
        }
    }
}