using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Interfaces.SmartEos;
using HRM.Api.Common.Options;
using HRM.Api.Features.Attendance;
using HRM.Api.Features.DanhMuc;
using HRM.Api.Features.Payroll;
using HRM.Api.BackgroundJobs;
using HRM.Api.ExternalServices.SmartEos;
using HRM.Api.Identity;
using HRM.Api.Persistence;
using HRM.Api.Persistence.Interceptors;
using HRM.Api.Services.Attendance;
using HRM.Api.Services.Catalog;
using HRM.Api.Services.Payroll;
using HRM.Api.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HRM.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddHrmServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<HrmOptions>(configuration.GetSection(HrmOptions.SectionName));
        services.AddMemoryCache();

        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        var connectionString = configuration.GetConnectionString("HRMAPIConnection")
            ?? throw new InvalidOperationException("Connection string 'HRMAPIConnection' is missing.");

        services.AddDbContext<HrmDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(120);
            });
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddScoped<IHrmDbContext>(sp => sp.GetRequiredService<HrmDbContext>());

        var smartEosBaseUrl = configuration.GetSection(HrmOptions.SectionName)[nameof(HrmOptions.URLSmartEOS)]
            ?? "http://localhost/";

        RegisterSmartEosClient<ISmartEosAuthClient, SmartEosAuthClient>(services, smartEosBaseUrl);
        RegisterSmartEosClient<ISmartEosOrganizationClient, SmartEosOrganizationClient>(services, smartEosBaseUrl);
        RegisterSmartEosClient<ISmartEosWorkflowClient, SmartEosWorkflowClient>(services, smartEosBaseUrl);
        RegisterSmartEosClient<ISmartEosFaceClient, SmartEosFaceClient>(services, smartEosBaseUrl);

        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddScoped<IDanhMucGenericService, DanhMucGenericService>();
        services.AddScoped<IAttendancePairingService, AttendancePairingService>();
        services.AddScoped<ITaxCalculationService, TaxCalculationService>();
        services.AddScoped<IPayrollGridBuilder, DynamicPayrollGridBuilder>();
        services.AddScoped<IAttendanceSummaryProvider, AttendanceSummaryProvider>();

        // Legacy DI-enabled services
        services.AddScoped<CallWebService>();
        services.AddScoped<FuncCanhBao>();
        services.AddScoped<FuncBoPhan>();
        services.AddScoped<FuncChucVu>();

        foreach (var serviceType in typeof(FuncBase).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(FuncBase))))
        {
            services.AddScoped(serviceType);
        }

        services.AddHostedService<HrmCronJobsHostedService>();

        return services;
    }

    private static void RegisterSmartEosClient<TInterface, TImplementation>(
        IServiceCollection services,
        string baseUrl)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddHttpClient<TInterface, TImplementation>(client =>
        {
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
            {
                client.BaseAddress = uri;
            }
            client.Timeout = TimeSpan.FromMinutes(10);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseCookies = false
        });
    }
}
