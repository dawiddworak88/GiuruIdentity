using System;

namespace Identity.Api.v1.RequestModels
{
    public class TeamMemberRequestModel
    { 
        public Guid? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ReturnUrl { get; set; }
    }
}
