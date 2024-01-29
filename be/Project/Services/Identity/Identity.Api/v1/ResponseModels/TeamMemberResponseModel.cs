using System;

namespace Identity.Api.v1.ResponseModels
{ 
    public class TeamMemberResponseModel
    {
        public Guid? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
