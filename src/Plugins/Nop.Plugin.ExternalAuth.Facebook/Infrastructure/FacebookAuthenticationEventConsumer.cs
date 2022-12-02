using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Authentication.External;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Media;

namespace Nop.Plugin.ExternalAuth.Facebook.Infrastructure
{
    /// <summary>
    /// Facebook authentication event consumer (used for saving customer fields on registration)
    /// </summary>
    public partial class FacebookAuthenticationEventConsumer : IConsumer<CustomerAutoRegisteredByExternalMethodEvent>
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly HttpClient _httpClient;
        private readonly IPictureService _pictureService;
        private IWorkContext _workContext;

        #endregion

        #region Ctor

        public FacebookAuthenticationEventConsumer(IGenericAttributeService genericAttributeService, HttpClient _httpClient, IPictureService pictureService, IWorkContext workContext)
        {
            _genericAttributeService = genericAttributeService;
            this._httpClient = _httpClient;
            _pictureService = pictureService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(CustomerAutoRegisteredByExternalMethodEvent eventMessage)
        {
            if (eventMessage?.Customer == null || eventMessage.AuthenticationParameters == null)
                return;

            //handle event only for this authentication method
            if (!eventMessage.AuthenticationParameters.ProviderSystemName.Equals(FacebookAuthenticationDefaults.SystemName))
                return;

            //store some of the customer fields
            var firstName = eventMessage.AuthenticationParameters.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
            if (!string.IsNullOrEmpty(firstName))
                await _genericAttributeService.SaveAttributeAsync(eventMessage.Customer, NopCustomerDefaults.FirstNameAttribute, firstName);

            var lastName = eventMessage.AuthenticationParameters.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.Surname)?.Value;
            if (!string.IsNullOrEmpty(lastName))
                await _genericAttributeService.SaveAttributeAsync(eventMessage.Customer, NopCustomerDefaults.LastNameAttribute, lastName);
            
            var picture = eventMessage.AuthenticationParameters.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrEmpty(picture))
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var result = await _httpClient.GetByteArrayAsync(picture);
                var pictureRes = await _pictureService.InsertPictureAsync(result, "image/jpeg", null);
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute, pictureRes.Id);
                
                //Console.Out.Write(result);
            }
        }

        #endregion
    }
}