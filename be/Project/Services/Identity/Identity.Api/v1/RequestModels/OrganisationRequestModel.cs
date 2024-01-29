using System;

namespace Identity.Api.v1.RequestModels
{
    public class OrganisationRequestModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string CommunicationLanguage { get; set; }
    }
}
