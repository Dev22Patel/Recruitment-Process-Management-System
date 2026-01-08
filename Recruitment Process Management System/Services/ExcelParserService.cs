using OfficeOpenXml;
using Recruitment_Process_Management_System.Models.DTOs;
using System.Text.RegularExpressions;

namespace Recruitment_Process_Management_System.Services
{
    public class ExcelParserService
    {
        private readonly ILogger<ExcelParserService> _logger;

        public ExcelParserService(ILogger<ExcelParserService> logger)
        {
            _logger = logger;
            // Set EPPlus license context (NonCommercial or Commercial)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Parses candidate data from Excel file
        /// </summary>
        public async Task<List<CandidateExcelRow>> ParseCandidateExcelAsync(Stream fileStream)
        {
            var candidates = new List<CandidateExcelRow>();

            try
            {
                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // First sheet
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

                    if (rowCount <= 1)
                    {
                        _logger.LogWarning("Excel file is empty or has no data rows");
                        return candidates;
                    }

                    // Start from row 2 (row 1 is header)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var candidate = new CandidateExcelRow
                        {
                            RowNumber = row,
                            FirstName = GetCellValue(worksheet, row, 1),
                            LastName = GetCellValue(worksheet, row, 2),
                            Email = GetCellValue(worksheet, row, 3).ToLower().Trim(),
                            PhoneNumber = GetCellValue(worksheet, row, 4),
                            CurrentLocation = GetCellValue(worksheet, row, 5),
                            CollegeName = GetCellValue(worksheet, row, 6),
                            Degree = GetCellValue(worksheet, row, 7),
                            GraduationYear = ParseIntOrNull(GetCellValue(worksheet, row, 8)),
                            Skills = GetCellValue(worksheet, row, 9),
                            TotalExperience = ParseDecimalOrNull(GetCellValue(worksheet, row, 10)),
                            CurrentCompany = GetCellValue(worksheet, row, 11)
                        };

                        // Validate the row
                        ValidateRow(candidate);
                        candidates.Add(candidate);
                    }
                }

                _logger.LogInformation($"Successfully parsed {candidates.Count} rows from Excel");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing Excel file: {ex.Message}");
                throw new Exception($"Failed to parse Excel file: {ex.Message}", ex);
            }

            return candidates;
        }

        /// <summary>
        /// Generates Excel template for download
        /// </summary>
        public async Task<MemoryStream> GenerateExcelTemplateAsync()
        {
            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Candidates");

                // Define headers
                var headers = new[]
                {
                    "FirstName*",
                    "LastName*",
                    "Email*",
                    "PhoneNumber*",
                    "CurrentLocation",
                    "CollegeName",
                    "Degree",
                    "GraduationYear",
                    "Skills (comma-separated)",
                    "TotalExperience (years)",
                    "CurrentCompany"
                };

                // Set headers
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Add sample data
                worksheet.Cells[2, 1].Value = "John";
                worksheet.Cells[2, 2].Value = "Doe";
                worksheet.Cells[2, 3].Value = "john.doe@example.com";
                worksheet.Cells[2, 4].Value = "9876543210";
                worksheet.Cells[2, 5].Value = "Mumbai";
                worksheet.Cells[2, 6].Value = "IIT Bombay";
                worksheet.Cells[2, 7].Value = "B.Tech Computer Science";
                worksheet.Cells[2, 8].Value = 2024;
                worksheet.Cells[2, 9].Value = "C#, React, SQL, Azure";
                worksheet.Cells[2, 10].Value = 0;
                worksheet.Cells[2, 11].Value = "";

                // Add another sample
                worksheet.Cells[3, 1].Value = "Jane";
                worksheet.Cells[3, 2].Value = "Smith";
                worksheet.Cells[3, 3].Value = "jane.smith@example.com";
                worksheet.Cells[3, 4].Value = "9876543211";
                worksheet.Cells[3, 5].Value = "Bangalore";
                worksheet.Cells[3, 6].Value = "NIT Karnataka";
                worksheet.Cells[3, 7].Value = "MCA";
                worksheet.Cells[3, 8].Value = 2023;
                worksheet.Cells[3, 9].Value = "Python, Django, PostgreSQL";
                worksheet.Cells[3, 10].Value = 1;
                worksheet.Cells[3, 11].Value = "Tech Corp";

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Add instructions sheet
                var instructionsSheet = package.Workbook.Worksheets.Add("Instructions");
                instructionsSheet.Cells[1, 1].Value = "Instructions for Bulk Upload";
                instructionsSheet.Cells[1, 1].Style.Font.Bold = true;
                instructionsSheet.Cells[1, 1].Style.Font.Size = 14;

                instructionsSheet.Cells[3, 1].Value = "Required Fields (marked with *):";
                instructionsSheet.Cells[3, 1].Style.Font.Bold = true;
                instructionsSheet.Cells[4, 1].Value = "- FirstName, LastName, Email, PhoneNumber";

                instructionsSheet.Cells[6, 1].Value = "Optional Fields:";
                instructionsSheet.Cells[6, 1].Style.Font.Bold = true;
                instructionsSheet.Cells[7, 1].Value = "- All other fields";

                instructionsSheet.Cells[9, 1].Value = "Format Guidelines:";
                instructionsSheet.Cells[9, 1].Style.Font.Bold = true;
                instructionsSheet.Cells[10, 1].Value = "- Email must be valid format";
                instructionsSheet.Cells[11, 1].Value = "- Phone number: 10 digits";
                instructionsSheet.Cells[12, 1].Value = "- Skills: Comma-separated (e.g., C#, React, SQL)";
                instructionsSheet.Cells[13, 1].Value = "- GraduationYear: 4-digit year (e.g., 2024)";
                instructionsSheet.Cells[14, 1].Value = "- TotalExperience: Decimal number (e.g., 2.5)";

                instructionsSheet.Cells[instructionsSheet.Dimension.Address].AutoFitColumns();

                await package.SaveAsync();
            }

            stream.Position = 0;
            return stream;
        }

        #region Helper Methods

        private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            return worksheet.Cells[row, col].Text?.Trim() ?? string.Empty;
        }

        private int? ParseIntOrNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (int.TryParse(value, out int result))
                return result;

            return null;
        }

        private decimal? ParseDecimalOrNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value, out decimal result))
                return result;

            return null;
        }

        private void ValidateRow(CandidateExcelRow candidate)
        {
            // Required fields
            if (string.IsNullOrWhiteSpace(candidate.FirstName))
                candidate.Errors.Add("FirstName is required");

            if (string.IsNullOrWhiteSpace(candidate.LastName))
                candidate.Errors.Add("LastName is required");

            if (string.IsNullOrWhiteSpace(candidate.Email))
                candidate.Errors.Add("Email is required");
            else if (!IsValidEmail(candidate.Email))
                candidate.Errors.Add("Invalid email format");

            if (string.IsNullOrWhiteSpace(candidate.PhoneNumber))
                candidate.Errors.Add("PhoneNumber is required");
            else if (!IsValidPhoneNumber(candidate.PhoneNumber))
                candidate.Errors.Add("Phone number must be 10 digits");

            // Optional validations
            if (candidate.GraduationYear.HasValue)
            {
                var currentYear = DateTime.Now.Year;
                if (candidate.GraduationYear < 1950 || candidate.GraduationYear > currentYear + 5)
                    candidate.Errors.Add($"Invalid graduation year (must be between 1950 and {currentYear + 5})");
            }

            if (candidate.TotalExperience.HasValue && candidate.TotalExperience < 0)
                candidate.Errors.Add("Total experience cannot be negative");
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove common separators
            phone = phone.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");

            // Check if it's 10 digits
            return phone.Length == 10 && phone.All(char.IsDigit);
        }

        #endregion
    }
}