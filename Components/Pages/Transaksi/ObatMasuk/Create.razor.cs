using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Transaksi.ObatMasuk
{
    public partial class Create : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        MudForm form = default!;
        bool isValid;
        Models.ObatMasuk obatMasuk = new();
        List<Models.Obat> ObatList = new();

        decimal hargaSatuan = 0;

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
            obatMasuk.TglMasuk = DateTime.Today;
            obatMasuk.TglKadaluarsaM = DateTime.Today.AddYears(1);
            obatMasuk.JumlahMasuk = 0;
            obatMasuk.TotalHarga = 0;

            ObatList = await DbContext.Obats
                .OrderBy(o => o.NamaObat)
                .ToListAsync();
        }

        void OnJumlahChanged(int value)
        {
            obatMasuk.JumlahMasuk = value;
            CalculateTotal();
        }

        void OnHargaChanged(decimal value)
        {
            hargaSatuan = value;
            CalculateTotal();
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

            if (obatMasuk.JumlahMasuk <= 0)
            {
                Snackbar.Add("Jumlah masuk harus lebih dari 0!", Severity.Error);
                return;
            }

            if (hargaSatuan <= 0)
            {
                Snackbar.Add("Harga satuan harus lebih dari 0!", Severity.Error);
                return;
            }

            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                // Tambah obat masuk
                DbContext.ObatMasuks.Add(obatMasuk);
                await DbContext.SaveChangesAsync();

                // Update stok obat
                var obat = await DbContext.Obats.FindAsync(obatMasuk.ObatId);
                if (obat != null)
                {
                    obat.Stok += obatMasuk.JumlahMasuk;
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
            MudDialog.Close();
        }
    }
}