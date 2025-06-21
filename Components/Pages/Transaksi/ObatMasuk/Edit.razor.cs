using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Transaksi.ObatMasuk
{
    public partial class Edit : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public Models.ObatMasuk ObatMasuk { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        MudForm form = default!;
        bool isValid;
        Models.ObatMasuk obatMasuk = new();
        List<Models.Obat> ObatList = new();

        decimal hargaSatuan = 0;
        int originalJumlahMasuk = 0;

        DateTime? tglMasuk
        {
            get => obatMasuk.TglMasuk;
            set => obatMasuk.TglMasuk = value ?? DateTime.Today;
        }

        DateTime? tglKadaluarsaM
        {
            get => obatMasuk.TglKadaluarsaM;
            set => obatMasuk.TglKadaluarsaM = value ?? DateTime.Today.AddYears(1);
        }

        protected override async Task OnInitializedAsync()
        {
            ObatList = await DbContext.Obats
                .OrderBy(o => o.NamaObat)
                .ToListAsync();

            // Copy data from parameter
            originalJumlahMasuk = ObatMasuk.JumlahMasuk;
            obatMasuk = new Models.ObatMasuk
            {
                Id = ObatMasuk.Id,
                ObatId = ObatMasuk.ObatId,
                JumlahMasuk = ObatMasuk.JumlahMasuk,
                TotalHarga = ObatMasuk.TotalHarga,
                TglMasuk = ObatMasuk.TglMasuk,
                TglKadaluarsaM = ObatMasuk.TglKadaluarsaM,
                Supplier = ObatMasuk.Supplier
            };

            // Calculate harga satuan
            if (obatMasuk.JumlahMasuk > 0)
            {
                hargaSatuan = obatMasuk.TotalHarga / obatMasuk.JumlahMasuk;
            }
        }

        void OnJumlahChanged(ChangeEventArgs e)
        {
            if (e.Value != null && int.TryParse(e.Value.ToString(), out int jumlah))
            {
                obatMasuk.JumlahMasuk = jumlah;
                CalculateTotal();
            }
        }

        void OnHargaChanged(ChangeEventArgs e)
        {
            if (e.Value != null && decimal.TryParse(e.Value.ToString(), out decimal harga))
            {
                hargaSatuan = harga;
                CalculateTotal();
            }
        }

        void CalculateTotal()
        {
            obatMasuk.TotalHarga = obatMasuk.JumlahMasuk * hargaSatuan;
            StateHasChanged();
        }

        async Task Submit()
        {
            await form.Validate();

            if (!isValid)
                return;

            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                var existingObatMasuk = await DbContext.ObatMasuks.FindAsync(obatMasuk.Id);
                if (existingObatMasuk != null)
                {
                    // Update obat masuk
                    existingObatMasuk.ObatId = obatMasuk.ObatId;
                    existingObatMasuk.JumlahMasuk = obatMasuk.JumlahMasuk;
                    existingObatMasuk.TotalHarga = obatMasuk.TotalHarga;
                    existingObatMasuk.TglMasuk = obatMasuk.TglMasuk;
                    existingObatMasuk.TglKadaluarsaM = obatMasuk.TglKadaluarsaM;
                    existingObatMasuk.Supplier = obatMasuk.Supplier;

                    // Update stok obat (kembalikan stok lama, tambah stok baru)
                    var obat = await DbContext.Obats.FindAsync(obatMasuk.ObatId);
                    if (obat != null)
                    {
                        obat.Stok = obat.Stok - originalJumlahMasuk + obatMasuk.JumlahMasuk;
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