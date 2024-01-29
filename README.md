# GiuruIdentity

Guru Identity is the authorization server for the [Giuru](https://github.com/dawiddworak88/Giuru) application

## Installation

### Prerequisites

* **[.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0):** download and install the latest version
* **[Node 20](https://nodejs.org/en/download/):** download and install the latest Node 20 version
* **[Docker](http://hub.docker.com/):** to run ASP.NET Core web app, Node for SSR and Storybook in containers. Download and install the latest version

### Configuration file

Add the following configuration to `.env` file in the root of your project and fill with your details

```env
COMPOSE_PROJECT_NAME=GIURU
GIURU_EXTERNAL_DNS_NAME_OR_IP=host.docker.internal
SUPPORTED_CULTURES=en,de,pl
DEFAULT_CULTURE=en
ORGANISATION_ID=[PUT_YOUR_ORGANISATION_ID]
SENDER_EMAIL=[PUT_YOUR_APPLICATION_EMAIL]
SENDER_NAME=[PUT_YOUR_EMAIL_SENDER_NAME]
SENDGRID_API_KEY=[PUT_YOUR_SENDGRID_API_KEY_HERE_AND_NEVER_COMMIT]
SENDGRID_CREATE_ACCOUNT_TEMPLATE_ID=[PUT_YOUR_SENDGRID_CREATE_ACCOUNT_TEMPLATE_ID_HERE_AND_NEVER_COMMIT]
SENDGRID_RESET_PASSWORD_TEMPLATE_ID=[PUT_YOUR_SENDGRID_RESET_ACCOUNT_TEMPLATE_ID_HERE_AND_NEVER_COMMIT]
SENDGRID_TEAM_MEMBER_INVITATION_TEMPLATE_ID=[PUT_YOUR_SENDGRID_TEAM_MEMBER_INVITATION_TEMPLATE_ID_HERE_AND_NEVER_COMMIT]
```

### Quickstart

1. Clone this repository
2. Execute the following commands from the /fe folder to build fe:

    npm install --legacy-peer-deps

    npm run build-fe

3. Open Visual Studio, set the docker-compose project as a startup project and hit F5
