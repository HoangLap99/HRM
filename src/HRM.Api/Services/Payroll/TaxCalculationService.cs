#nullable disable
using HRM.Api.Features.Payroll;

namespace HRM.Api.Services.Payroll;

/// <summary>
/// Triển khai biểu thuế TNCN lũy tiến từng phần từ cấu hình HRM_dmCauHinhThueThuNhapCaNhan.
/// </summary>
public sealed class TaxCalculationService : ITaxCalculationService
{
    public double CalculatePersonalIncomeTax(
        double taxableIncome,
        IEnumerable<HRM_dmCauHinhThueThuNhapCaNhan> taxBrackets)
    {
        if (taxableIncome <= 0 || taxBrackets == null)
        {
            return 0;
        }

        double tax = 0;
        foreach (var bracket in taxBrackets.OrderByDescending(a => a.STT))
        {
            if (bracket.MucLuong < taxableIncome)
            {
                tax = taxableIncome * bracket.TyLe / 100.0 - bracket.SoTien;
                break;
            }
        }

        return tax < 0 ? 0 : tax;
    }
}