using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Transaksi.ObatKeluar
{
    public partial class Edit : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public Models.ObatKeluar ObatKeluar { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        MudForm form = default!;
        bool isValid;
        Models.ObatKeluar obatKeluar = new();
        List<Models.Obat> ObatList = new();
        Models.Obat? selectedObat = null;

        decimal hargaSatuan = 0;
        int originalJumlahKeluar = 0;

        DateTime? tglKeluar
        {
            get => obatKeluar.TglKeluar;
            set => obatKeluar.TglKeluar = value ?? DateTime.Now;
        }

        protected override async Task OnInitializedAsync()
        {
            ObatList = await DbContext.Obats
                .OrderBy(o => o.NamaObat)
                .ToListAsync();

            // Copy data from parameter
            originalJumlahKeluar = ObatKeluar.JumlahKeluar;
            obatKeluar = new Models.ObatKeluar
            {
                Id = ObatKeluar.Id,
                ObatId = ObatKeluar.ObatId,
                JumlahKeluar = ObatKeluar.JumlahKeluar,
                TotalHarga = ObatKeluar.TotalHarga,
                TglKeluar = ObatKeluar.TglKeluar,
                Pelanggan = ObatKeluar.Pelanggan,
                NoTransaksi = ObatKeluar.NoTransaksi
            };

            selectedObat = ObatList.FirstOrDefault(o => o.Id == obatKeluar.ObatId);

            // Calculate harga satuan
            if (obatKeluar.JumlahKeluar > 0)
            {
                hargaSatuan = obatKeluar.TotalHarga / obatKeluar.JumlahKeluar;
            }
        }

        int GetAvailableStock(Models.Obat obat)
        {
            // Jika ini adalah obat yang sedang diedit, tambahkan jumlah keluar original ke stok
            if (obat.Id == obatKeluar.ObatId)
            {
                return obat.Stok + originalJumlahKeluar;
            }
            return obat.Stok;
        }

        int GetMaxQuantity()
        {
            if (selectedObat != null)
            {
                return GetAvailableStock(selectedObat);
            }
            return 0;
        }

        void OnObatChanged(int obatId)
        {
            obatKeluar.ObatId = obatId;
            selectedObat = ObatList.FirstOrDefault(o => o.Id == obatId);

            if (selectedObat != null)
            {
                hargaSatuan = selectedObat.Harga;
                CalculateTotal();
            }

            StateHasChanged();
        }

        void CalculateTotal()
        {
            obatKeluar.TotalHarga = obatKeluar.JumlahKeluar * hargaSatuan;
            StateHasChanged();
        }

        async Task Submit()
        {
            await form.Validate();

            if (!isValid)
                return;

            // Validasi stok
            if (selectedObat != null && obatKeluar.JumlahKeluar > GetAvailableStock(selectedObat))
            {
                Snackbar.Add("Jumlah keluar melebihi stok yang tersedia!", Severity.Error);
                return;
            }

            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                var existingObatKeluar = await DbContext.ObatKeluars.FindAsync(obatKeluar.Id);
                if (existingObatKeluar != null)
                {
                    // Update obat keluar
                    existingObatKeluar.ObatId = obatKeluar.ObatId;
                    existingObatKeluar.JumlahKeluar = obatKeluar.JumlahKeluar;
                    existingObatKeluar.TotalHarga = obatKeluar.TotalHarga;
                    existingObatKeluar.TglKeluar = obatKeluar.TglKeluar;
                    existingObatKeluar.Pelanggan = obatKeluar.Pelanggan;

                    // Update stok obat (kembalikan stok lama, kurangi stok baru)
                    var obat = await DbContext.Obats.FindAsync(obatKeluar.ObatId);
                    if (obat != null)
                    {
                        obat.Stok = obat.Stok + originalJumlahKeluar - obatKeluar.JumlahKeluar;
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
            }
        }

        void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}