﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using AdminWebApp.Models;
using Twilio;
using System.Diagnostics;
using System.Net.Mail;
using System.Net.Mime;
using System.Web.Configuration;

namespace AdminWebApp
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            string text = message.Body;
            string html = message.Body;
            var From = WebConfigurationManager.AppSettings["From"];
            var FromName = WebConfigurationManager.AppSettings["FromName"];
            var Password = WebConfigurationManager.AppSettings["Password"];
            var SMTPServer = WebConfigurationManager.AppSettings["SMTPServer"];
            var Port = WebConfigurationManager.AppSettings["Port"];
            try
            {
                //do whatever you want to the message  
                MailAddress from = new MailAddress(From, FromName);
                MailAddress to = new MailAddress(message.Destination);
                MailMessage msg = new MailMessage(from, to);
                //MailAddress bcc = new MailAddress("manager1@contoso.com");
                //message.Bcc.Add(bcc);
                msg.Subject = message.Subject;
                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

                SmtpClient smtpClient = new SmtpClient(SMTPServer, Convert.ToInt32(Port));
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(From, Password);
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = credentials;
                smtpClient.Send(msg);

                return Task.FromResult(0);
            }
            catch (Exception)
            {
                return Task.FromResult(0);
            }


        }
    }
    public static class Keys
    {
        public static string SMSAccountIdentification = "CKVAMKVW3EC0";
        public static string SMSAccountPassword = "uWkFvEVSzHyZO31lwrgnPip5";
        public static string SMSAccountFrom = "+15555551234";
    }
    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            // Twilio Begin
            var Twilio = new TwilioRestClient(
              System.Configuration.ConfigurationManager.AppSettings["SMSAccountIdentification"],
              System.Configuration.ConfigurationManager.AppSettings["SMSAccountPassword"]);
            var result = Twilio.SendMessage(
              System.Configuration.ConfigurationManager.AppSettings["SMSAccountFrom"],
              message.Destination, message.Body
            );
            // Status is one of Queued, Sending, Sent, Failed or null if the number is not valid
            Trace.TraceInformation(result.Status);
            // Twilio doesn't currently have an async API, so return success.
            return Task.FromResult(0);
            // Twilio End

            // ASPSMS Begin 
            //var soapSms = new AdminWebApp.ASPSMSX2.ASPSMSX2SoapClient("ASPSMSX2Soap");
            //soapSms.SendSimpleTextSMS(
            //  Keys.SMSAccountIdentification,
            //  Keys.SMSAccountPassword,
            //  message.Destination,
            //  Keys.SMSAccountFrom,
            //  message.Body);
            //soapSms.Close();
            //return Task.FromResult(0);
            // ASPSMS End
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                //Before
                //manager.UserTokenProvider =
                //    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
                //After
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"))
                {
                    TokenLifespan = TimeSpan.FromHours(3)
                };
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
