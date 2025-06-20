using Microsoft.AspNetCore.Components;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Obat
{
    public partial class Edit : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public Models.Obat Obat { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        MudForm form = default!;
        bool isValid;
        Models.Obat obat = new();

        DateTime? tglKadaluarsa
        {
            get => obat.TglKadaluarsa;
            set => obat.TglKadaluarsa = value ?? DateTime.Today.AddYears(1);
        }

        string[] jenisObatOptions = {
            "Analgesik", "Antibiotik", "Antihistamin", "Antasida",
            "Ekspektoran", "Antiseptik", "Vitamin", "Suplemen"
        };

        string[] bentukObatOptions = {
            "Tablet", "Kapsul", "Sirup", "Salep", "Krim",
            "Injeksi", "Tetes", "Inhaler"
        };

        protected override void OnInitialized()
        {
            obat = new Models.Obat
            {
                Id = Obat.Id,
                NamaObat = Obat.NamaObat,
                JenisObat = Obat.JenisObat,
                BentukObat = Obat.BentukObat,
                Harga = Obat.Harga,
                Stok = Obat.Stok,
                TglKadaluarsa = Obat.TglKadaluarsa
            };
        }

        async Task Submit()
        {
            await form.Validate();

            if (!isValid)
                return;

            try
            {
                var existingObat = await DbContext.Obats.FindAsync(obat.Id);
                if (existingObat != null)
                {
                    existingObat.NamaObat = obat.NamaObat;
                    existingObat.JenisObat = obat.JenisObat;
                    existingObat.BentukObat = obat.BentukObat;
                    existingObat.Harga = obat.Harga;
                    existingObat.Stok = obat.Stok;
                    existingObat.TglKadaluarsa = obat.TglKadaluarsa;

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