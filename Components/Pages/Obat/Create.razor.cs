using Microsoft.AspNetCore.Components;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;

namespace SIPOTEK.Components.Pages.Obat
{
    public partial class Create : ComponentBase
    {
		[CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
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
            obat.TglKadaluarsa = DateTime.Today.AddYears(1);
        }

        async Task Submit()
        {
            await form.Validate();

            if (!isValid)
                return;

            try
            {
                DbContext.Obats.Add(obat);
                await DbContext.SaveChangesAsync();
                MudDialog.Close(DialogResult.Ok(true));
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
        }

        void Cancel()
        {
            MudDialog.Close();
        }
    }
}