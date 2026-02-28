namespace ElectronApp.Security;

/// <summary>
/// Configures security headers using NetEscapades.AspNetCore.SecurityHeaders
/// </summary>
public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseAppSecurityHeaders(this IApplicationBuilder app) =>
        app.UseSecurityHeaders(policies =>
            policies
                .AddContentTypeOptionsNoSniff()
                .AddFrameOptionsDeny()
                .AddReferrerPolicyNoReferrer()
                .AddPermissionsPolicyWithDefaultSecureDirectives()
                .AddContentSecurityPolicy(builder =>
                {
                    builder.AddDefaultSrc().Self();
                    builder.AddBaseUri().Self();
                    builder.AddFormAction().Self();
                    builder.AddFrameAncestors().None();
                    builder.AddObjectSrc().None();
                    builder.AddImgSrc().Self();
                    builder.AddFontSrc().Self();
                    builder.AddStyleSrc().Self();
                    builder.AddScriptSrc().Self().WithNonce();
                    builder.AddConnectSrc().Self();
                    builder.AddUpgradeInsecureRequests();
                })
        );
}
