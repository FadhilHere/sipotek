using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;
using SIPOTEK.Services;

namespace SIPOTEK.Components.Pages.Obat
{
    public partial class Create : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] IFileUploadService FileUploadService { get; set; } = default!;

        MudForm form = default!;
        bool isValid = false;
        bool isUploading = false;
        Models.Obat obat = new();
        IBrowserFile? selectedFile;
        string previewImageUrl = "";

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
            obat.Harga = 0;
            obat.Stok = 0;
            obat.StokMinimum = 10; // Default stok minimum
        }

        void ClearFile()
        {
            selectedFile = null;
            previewImageUrl = "";
            StateHasChanged();
        }

        // FUNCTION INI YANG KURANG - SAMA SEPERTI DI EDIT
        void RemoveCurrentImage()
        {
            selectedFile = null;
            previewImageUrl = "";
            StateHasChanged();
        }

        async Task OnFileChanged(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null) return;

            // Validasi file
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                Snackbar.Add("Hanya file gambar (JPEG, PNG, GIF) yang diperbolehkan!", Severity.Error);
                return;
            }

            if (file.Size > 5 * 1024 * 1024) // 5MB
            {
                Snackbar.Add("Ukuran file tidak boleh lebih dari 5MB!", Severity.Error);
                return;
            }

            selectedFile = file;

            try
            {
                // Create high quality preview
                using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                var imageBytes = memoryStream.ToArray();
                var base64String = Convert.ToBase64String(imageBytes);
                previewImageUrl = $"data:{file.ContentType};base64,{base64String}";

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading preview: {ex.Message}", Severity.Error);
                selectedFile = null;
                previewImageUrl = "";
            }
        }

        async Task Submit()
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(obat.NamaObat))
            {
                Snackbar.Add("Nama obat harus diisi!", Severity.Warning);
                return;
            }

            if (obat.Harga <= 0)
            {
                Snackbar.Add("Harga harus lebih dari 0!", Severity.Warning);
                return;
            }

            if (obat.Stok < 0)
            {
                Snackbar.Add("Stok tidak boleh negatif!", Severity.Warning);
                return;
            }

            if (obat.StokMinimum < 0)
            {
                Snackbar.Add("Stok minimum tidak boleh negatif!", Severity.Warning);
                return;
            }

            if (obat.TglKadaluarsa <= DateTime.Today)
            {
                Snackbar.Add("Tanggal kadaluarsa harus di masa depan!", Severity.Warning);
                return;
            }

            isUploading = true;
            StateHasChanged();

            try
            {
                // Upload gambar jika ada
                if (selectedFile != null)
                {
                    var fileName = await FileUploadService.UploadImageAsync(selectedFile);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        obat.GambarFileName = fileName;
                        obat.GambarUrl = FileUploadService.GetImageUrl(fileName);
                    }
                }

                DbContext.Obats.Add(obat);
                await DbContext.SaveChangesAsync();

                Snackbar.Add("Obat berhasil ditambahkan!", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
            finally
            {
                isUploading = false;
                StateHasChanged();
            }
        }

        void Cancel()
        {
            MudDialog.Cancel(); // Fix: pakai Cancel() bukan Close()
        }
    }
}