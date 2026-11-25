namespace HRM.Api.Features.Payroll;

/// <summary>
/// Service tính thuế Thu nhập cá nhân theo biểu thuế lũy tiến từng phần (HRM_dmCauHinhThueThuNhapCaNhan).
/// </summary>
public interface ITaxCalculationService
{
    double CalculatePersonalIncomeTax(
        double taxableIncome,
        IEnumerable<HRM_dmCauHinhThueThuNhapCaNhan> taxBrackets);
}