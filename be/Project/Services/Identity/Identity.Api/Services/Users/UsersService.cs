﻿using Feature.Account;
using Foundation.Extensions.Exceptions;
using Foundation.Localization;
using Foundation.Mailing.Configurations;
using Foundation.Mailing.Models;
using Foundation.Mailing.Services;
using Identity.Api.Areas.Accounts.Services.UserServices;
using Identity.Api.Configurations;
using Identity.Api.Definitions;
using Identity.Api.Infrastructure;
using Identity.Api.Infrastructure.Accounts.Entities;
using Identity.Api.ServicesModels.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Identity.Api.Services.Users
{
    public class UsersService : IUsersService
    {
        private readonly IdentityContext identityContext;
        private readonly IOptionsMonitor<MailingConfiguration> mailingOptions;
        private readonly IOptionsMonitor<AppSettings> identityOptions;
        private readonly IMailingService mailingService;
        private readonly IStringLocalizer<AccountResources> accountLocalizer;
        private readonly IUserService userService;
        private readonly LinkGenerator linkGenerator;

        public UsersService(
            IdentityContext identityContext,
            IOptionsMonitor<MailingConfiguration> mailingOptions,
            IOptionsMonitor<AppSettings> identityOptions,
            IMailingService mailingService,
            IStringLocalizer<AccountResources> accountLocalizer,
            IUserService userService,
            LinkGenerator linkGenerator)
        {
            this.identityContext = identityContext;
            this.mailingOptions = mailingOptions;
            this.identityOptions = identityOptions;
            this.mailingService = mailingService;
            this.accountLocalizer = accountLocalizer;
            this.userService = userService;
            this.linkGenerator = linkGenerator;
        }

        public async Task<UserServiceModel> CreateAsync(CreateUserServiceModel serviceModel)
        {
            var timeExpiration = DateTime.UtcNow.AddHours(IdentityConstants.VerifyTimeExpiration);

            var existingOrganisation = await this.identityContext.Organisations.FirstOrDefaultAsync(x => x.ContactEmail == serviceModel.Email && x.IsActive);
            
            if (existingOrganisation == null)
            {
                throw new CustomException(this.accountLocalizer.GetString("OrganisationNotFound"), (int)HttpStatusCode.NoContent);
            }

            Thread.CurrentThread.CurrentCulture = new CultureInfo(existingOrganisation.Language);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            var user = await this.identityContext.Accounts.FirstOrDefaultAsync(x => x.Email == serviceModel.Email);

            if (user is not null)
            {
                user.EmailConfirmed = false;
                user.VerifyExpirationDate = timeExpiration;
                user.ExpirationId = Guid.NewGuid();

                await this.identityContext.SaveChangesAsync();
                await this.mailingService.SendTemplateAsync(new TemplateEmail
                {
                    RecipientEmailAddress = user.Email,
                    RecipientName = user.UserName,
                    SenderEmailAddress = this.mailingOptions.CurrentValue.SenderEmail,
                    SenderName = this.mailingOptions.CurrentValue.SenderName,
                    TemplateId = this.identityOptions.CurrentValue.ActionSendGridResetTemplateId,
                    DynamicTemplateData = new
                    {
                        lang = existingOrganisation.Language,
                        ap_subject = this.accountLocalizer.GetString("ap_subject").Value,
                        ap_preHeader = this.accountLocalizer.GetString("ap_preHeader").Value,
                        ap_buttonLabel = this.accountLocalizer.GetString("ap_buttonLabel").Value,
                        ap_headOne = this.accountLocalizer.GetString("ap_headOne").Value,
                        ap_headTwo = this.accountLocalizer.GetString("ap_headTwo").Value,
                        ap_lineOne = this.accountLocalizer.GetString("ap_lineOne").Value,
                        resetAccountLink = this.linkGenerator.GetUriByAction("Index", "SetPassword", new { Area = "Accounts", culture = existingOrganisation.Language, Id = user.ExpirationId, ReturnUrl = string.IsNullOrWhiteSpace(serviceModel.ReturnUrl) ? null : HttpUtility.UrlEncode(serviceModel.ReturnUrl) }, serviceModel.Scheme, serviceModel.Host)
                    }
                });

                return await this.GetById(new GetUserServiceModel { Id = Guid.Parse(user.Id), Language = serviceModel.Language, Username = serviceModel.Username, OrganisationId = serviceModel.OrganisationId });
            }

            var userAccount = new ApplicationUser
            {
                UserName = serviceModel.Email,
                NormalizedUserName = serviceModel.Email,
                Email = serviceModel.Email,
                NormalizedEmail = serviceModel.Email,
                OrganisationId = existingOrganisation.Id,
                SecurityStamp = Guid.NewGuid().ToString(),
                VerifyExpirationDate = timeExpiration,
                ExpirationId = Guid.NewGuid(),
                AccessFailedCount = 0,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
            };

            this.identityContext.Accounts.Add(userAccount);
            await this.identityContext.SaveChangesAsync();
            await this.mailingService.SendTemplateAsync(new TemplateEmail
            {
                RecipientEmailAddress = userAccount.Email,
                RecipientName = userAccount.UserName,
                SenderEmailAddress = this.mailingOptions.CurrentValue.SenderEmail,
                SenderName = this.mailingOptions.CurrentValue.SenderName,
                TemplateId = this.identityOptions.CurrentValue.ActionSendGridCreateTemplateId,
                DynamicTemplateData = new
                {
                    lang = existingOrganisation.Language,
                    nc_subject = this.accountLocalizer.GetString("nc_subject").Value,
                    nc_preHeader = this.accountLocalizer.GetString("nc_preHeader").Value,
                    nc_buttonLabel = this.accountLocalizer.GetString("nc_buttonLabel").Value,
                    nc_headOne = this.accountLocalizer.GetString("nc_headOne").Value,
                    nc_headTwo = this.accountLocalizer.GetString("nc_headTwo").Value,
                    nc_lineOne = this.accountLocalizer.GetString("nc_lineOne").Value,
                    nc_lineTwo = this.accountLocalizer.GetString("nc_lineTwo").Value,
                    signAccountLink = this.linkGenerator.GetUriByAction("Index", "SetPassword", new { Area = "Accounts", culture = existingOrganisation.Language, Id = userAccount.ExpirationId, ReturnUrl = string.IsNullOrWhiteSpace(serviceModel.ReturnUrl) ? null : HttpUtility.UrlEncode(serviceModel.ReturnUrl) }, serviceModel.Scheme, serviceModel.Host)
                }
            });

            return await this.GetById(new GetUserServiceModel { Id = Guid.Parse(userAccount.Id), Language = serviceModel.Language, Username = serviceModel.Username, OrganisationId = serviceModel.OrganisationId });
        }

        public async Task<UserServiceModel> GetById(GetUserServiceModel serviceModel)
        {
            var user = await this.identityContext.Accounts.FirstOrDefaultAsync(x => x.Id == serviceModel.Id.ToString());

            return new UserServiceModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                OrganisationId = user.OrganisationId,
                TwoFactorEnabled = user.TwoFactorEnabled,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<UserServiceModel> GetByExpirationId(GetUserServiceModel serviceModel)
        {
            var user = await this.identityContext.Accounts.FirstOrDefaultAsync(x => x.ExpirationId == serviceModel.Id);

            if (user is not null)
            {
                return new UserServiceModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    OrganisationId = user.OrganisationId,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    PhoneNumber = user.PhoneNumber
                };
            }

            return default;
        }

        public async Task<UserServiceModel> SetPasswordAsync(SetUserPasswordServiceModel serviceModel)
        { 
            var existingUser = await this.identityContext.Accounts.FirstOrDefaultAsync(x => x.ExpirationId == serviceModel.ExpirationId.Value);

            if (existingUser is null)
            {
                throw new CustomException(this.accountLocalizer.GetString("UserNotFound"), (int)HttpStatusCode.NoContent);
            }

            if (existingUser.EmailConfirmed is false)
            {
                if (existingUser.VerifyExpirationDate >= DateTime.UtcNow)
                {
                    existingUser.EmailConfirmed = true;
                    existingUser.PasswordHash = this.userService.GeneratePasswordHash(existingUser, serviceModel.Password);

                    await this.identityContext.SaveChangesAsync();

                    return await this.GetById(new GetUserServiceModel { Id = Guid.Parse(existingUser.Id), Language = serviceModel.Language, Username = serviceModel.Username, OrganisationId = serviceModel.OrganisationId });
                }
            }

            return default;
        }

        public async Task<UserServiceModel> UpdateAsync(UpdateUserServiceModel serviceModel)
        {
            var existingUser = await this.identityContext.Accounts.FirstOrDefaultAsync(x => x.Email == serviceModel.Email);
            if (existingUser is null)
            {
                throw new CustomException(this.accountLocalizer.GetString("UserNotFound"), (int)HttpStatusCode.NoContent);
            }

            existingUser.FirstName = serviceModel.FirstName;
            existingUser.LastName = serviceModel.LastName;
            existingUser.TwoFactorEnabled = serviceModel.TwoFactorEnabled;
            existingUser.PhoneNumber = serviceModel.PhoneNumber;
            existingUser.LockoutEnd = serviceModel.LockoutEnd;

            var organisation = await this.identityContext.Organisations.FirstOrDefaultAsync(x => x.Id == existingUser.OrganisationId && x.IsActive);
            if (organisation is null)
            {
                throw new CustomException(this.accountLocalizer.GetString("OrganisationNotFound"), (int)HttpStatusCode.NoContent);
            }

            organisation.Name = serviceModel.Name;
            organisation.Language = serviceModel.CommunicationLanguage;

            await this.identityContext.SaveChangesAsync();

            return await this.GetById(new GetUserServiceModel { Id = Guid.Parse(existingUser.Id), Language = serviceModel.Language, Username = serviceModel.Username, OrganisationId = serviceModel.OrganisationId });
        }

        public async Task ResetPasswordAsync(ResetUserPasswordServiceModel serviceModel)
        {
            var user = await this.identityContext.Accounts.FirstOrDefaultAsync(x => x.Email == serviceModel.Email);

            if (user is not null)
            {
                var timeExpiration = DateTime.UtcNow.AddHours(IdentityConstants.VerifyTimeExpiration);

                user.EmailConfirmed = false;
                user.VerifyExpirationDate = timeExpiration;
                user.ExpirationId = Guid.NewGuid();

                var userOrganisation = await this.identityContext.Organisations.FirstOrDefaultAsync(x => x.Id == user.OrganisationId && x.IsActive);

                if (userOrganisation is null)
                {
                    throw new CustomException(this.accountLocalizer.GetString("OrganisationNotFound"), (int)HttpStatusCode.NoContent);
                }

                Thread.CurrentThread.CurrentCulture = new CultureInfo(userOrganisation.Language);
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

                await this.identityContext.SaveChangesAsync();
                await this.mailingService.SendTemplateAsync(new TemplateEmail
                {
                    RecipientEmailAddress = user.Email,
                    RecipientName = user.UserName,
                    SenderEmailAddress = this.mailingOptions.CurrentValue.SenderEmail,
                    SenderName = this.mailingOptions.CurrentValue.SenderName,
                    TemplateId = this.identityOptions.CurrentValue.ActionSendGridResetTemplateId,
                    DynamicTemplateData = new
                    {
                        lang = userOrganisation.Language,
                        ap_subject = this.accountLocalizer.GetString("ap_subject").Value,
                        ap_preHeader = this.accountLocalizer.GetString("ap_preHeader").Value,
                        ap_buttonLabel = this.accountLocalizer.GetString("ap_buttonLabel").Value,
                        ap_headOne = this.accountLocalizer.GetString("ap_headOne").Value,
                        ap_headTwo = this.accountLocalizer.GetString("ap_headTwo").Value,
                        ap_lineOne = this.accountLocalizer.GetString("ap_lineOne").Value,
                        resetAccountLink = this.linkGenerator.GetUriByAction("Index", "SetPassword", new { Area = "Accounts", culture = userOrganisation.Language, Id = user.ExpirationId, ReturnUrl = string.IsNullOrWhiteSpace(serviceModel.ReturnUrl) ? null : HttpUtility.UrlEncode(serviceModel.ReturnUrl) }, serviceModel.Scheme, serviceModel.Host)
                    }
                });
            }
        }

        public async Task<UserServiceModel> GetByEmail(GetUserByEmailServiceModel serviceModel)
        {
            var user = await this.identityContext.Accounts.FirstOrDefaultAsync(x => x.Email == serviceModel.Email);

            if (user is not null)
            {
                return new UserServiceModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    OrganisationId = user.OrganisationId,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    PhoneNumber = user.PhoneNumber
                };
            }

            return default;
        }
    }
}
