﻿using System;
using System.Linq;
using Bit.Core.Contracts;
using Bit.Core.Models;
using Bit.IdentityServer.Contracts;
using Bit.Owin.Contracts;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using Owin;

namespace Bit.IdentityServer.Middlewares
{
    public class IdentityServerMiddlewareConfiguration : IOwinMiddlewareConfiguration
    {
        private readonly IAppEnvironmentProvider _appEnvironmentProvider;
        private readonly ICertificateProvider _certificateProvider;
        private readonly IDependencyManager _dependencyManager;
        private readonly IScopesProvider _scopesProvider;
        private readonly IRedirectUriValidator _redirectUriValidator;

        public IdentityServerMiddlewareConfiguration(IAppEnvironmentProvider appEnvironmentProvider,
            IScopesProvider scopesProvider, ICertificateProvider certificateProvider, IDependencyManager dependencyManager, IRedirectUriValidator redirectUriValidator)
        {
            if (appEnvironmentProvider == null)
                throw new ArgumentNullException(nameof(appEnvironmentProvider));

            if (scopesProvider == null)
                throw new ArgumentNullException(nameof(scopesProvider));

            if (certificateProvider == null)
                throw new ArgumentNullException(nameof(certificateProvider));

            if (dependencyManager == null)
                throw new ArgumentNullException(nameof(dependencyManager));

            if (redirectUriValidator == null)
                throw new ArgumentNullException(nameof(redirectUriValidator));

            _appEnvironmentProvider = appEnvironmentProvider;
            _scopesProvider = scopesProvider;
            _certificateProvider = certificateProvider;
            _dependencyManager = dependencyManager;
            _redirectUriValidator = redirectUriValidator;
        }

        protected IdentityServerMiddlewareConfiguration()
        {

        }

        public virtual void Configure(IAppBuilder owinApp)
        {
            if (owinApp == null)
                throw new ArgumentNullException(nameof(owinApp));

            owinApp.Map("/core", coreApp =>
            {
                LogProvider.SetCurrentLogProvider(_dependencyManager.Resolve<ILogProvider>());

                AppEnvironment activeAppEnvironment = _appEnvironmentProvider.GetActiveAppEnvironment();

                IdentityServerServiceFactory factory = new IdentityServerServiceFactory()
                    .UseInMemoryClients(_dependencyManager.Resolve<IClientProvider>().GetClients().ToArray())
                    .UseInMemoryScopes(_scopesProvider.GetScopes());

                factory.UserService =
                    new Registration<IUserService>(_dependencyManager.Resolve<IUserService>());

                factory.ViewService = new Registration<IViewService>(_dependencyManager.Resolve<IViewService>());

                factory.RedirectUriValidator = new Registration<IRedirectUriValidator>(_redirectUriValidator);

                bool requireSslConfigValue = activeAppEnvironment.GetConfig("RequireSsl", defaultValueOnNotFound: false);

                string identityServerSiteName = activeAppEnvironment.GetConfig("IdentityServerSiteName", $"{activeAppEnvironment.AppInfo.Name} Identity Server");

                IdentityServerOptions identityServerOptions = new IdentityServerOptions
                {
                    SiteName = identityServerSiteName,
                    SigningCertificate = _certificateProvider.GetSingleSignOnCertificate(),
                    Factory = factory,
                    RequireSsl = requireSslConfigValue,
                    EnableWelcomePage = activeAppEnvironment.DebugMode == true,
                    IssuerUri = activeAppEnvironment.GetSsoUrl(),
                    CspOptions = new CspOptions
                    {
                        // Content security policy
                        Enabled = false
                    },
                    Endpoints = new EndpointOptions
                    {
                        EnableAccessTokenValidationEndpoint = true,
                        EnableAuthorizeEndpoint = true,
                        EnableCheckSessionEndpoint = true,
                        EnableClientPermissionsEndpoint = true,
                        EnableCspReportEndpoint = true,
                        EnableDiscoveryEndpoint = true,
                        EnableEndSessionEndpoint = true,
                        EnableIdentityTokenValidationEndpoint = true,
                        EnableIntrospectionEndpoint = true,
                        EnableTokenEndpoint = true,
                        EnableTokenRevocationEndpoint = true,
                        EnableUserInfoEndpoint = true
                    }
                };

                coreApp.UseIdentityServer(identityServerOptions);
            });
        }
    }
}