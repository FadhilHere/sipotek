using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SIPOTEK.Data;
using SIPOTEK.Models;
using SIPOTEK.Services;

namespace SIPOTEK.Components.Pages.Obat
{
    public partial class Edit : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public Models.Obat Obat { get; set; } = default!;
        [Inject] SipotekDbContext DbContext { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] IFileUploadService FileUploadService { get; set; } = default!;

        bool isUploading = false;
        Models.Obat obat = new();
        IBrowserFile? selectedFile;
        string previewImageUrl = "";
        string? originalGambarFileName;

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
            // Copy data from parameter
            obat = new Models.Obat
            {
                Id = Obat.Id,
                NamaObat = Obat.NamaObat,
                JenisObat = Obat.JenisObat,
                BentukObat = Obat.BentukObat,
                Harga = Obat.Harga,
                Stok = Obat.Stok,
                TglKadaluarsa = Obat.TglKadaluarsa,
                GambarFileName = Obat.GambarFileName,
                GambarUrl = Obat.GambarUrl,
                Deskripsi = Obat.Deskripsi,
                Produsen = Obat.Produsen
            };

            originalGambarFileName = Obat.GambarFileName;
        }

        string GetCurrentImageUrl()
        {
            if (!string.IsNullOrEmpty(obat.GambarFileName))
            {
                return FileUploadService.GetImageUrl(obat.GambarFileName);
            }
            return "/images/no-image.png";
        }

        void ClearFile()
        {
            selectedFile = null;
            previewImageUrl = "";
            StateHasChanged();
        }

        void RemoveCurrentImage()
        {
            obat.GambarFileName = null;
            obat.GambarUrl = null;
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

            isUploading = true;
            StateHasChanged();

            try
            {
                var existingObat = await DbContext.Obats.FindAsync(obat.Id);
                if (existingObat != null)
                {
                    // Handle image upload/removal
                    if (selectedFile != null)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(originalGambarFileName))
                        {
                            await FileUploadService.DeleteImageAsync(originalGambarFileName);
                        }

                        // Upload new image
                        var fileName = await FileUploadService.UploadImageAsync(selectedFile);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            obat.GambarFileName = fileName;
                            obat.GambarUrl = FileUploadService.GetImageUrl(fileName);
                        }
                    }
                    else if (string.IsNullOrEmpty(obat.GambarFileName) && !string.IsNullOrEmpty(originalGambarFileName))
                    {
                        // Image was removed, delete the file
                        await FileUploadService.DeleteImageAsync(originalGambarFileName);
                    }

                    // Update entity
                    existingObat.NamaObat = obat.NamaObat;
                    existingObat.JenisObat = obat.JenisObat;
                    existingObat.BentukObat = obat.BentukObat;
                    existingObat.Harga = obat.Harga;
                    existingObat.Stok = obat.Stok;
                    existingObat.TglKadaluarsa = obat.TglKadaluarsa;
                    existingObat.GambarFileName = obat.GambarFileName;
                    existingObat.GambarUrl = obat.GambarUrl;
                    existingObat.Deskripsi = obat.Deskripsi;
                    existingObat.Produsen = obat.Produsen;

                    await DbContext.SaveChangesAsync();

                    Snackbar.Add("Obat berhasil diupdate!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
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
            MudDialog.Cancel();
        }
    }
}