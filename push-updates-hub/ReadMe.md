# Prerequisites
- [Docker Compose](https://docs.docker.com/compose/)
- [Powershell](https://github.com/PowerShell/PowerShell/releases)
- Disable *Powershell*'s required script signature restriction.\
Run *Powershell* command with admin's privileges `Set-ExecutionPolicy Unrestricted`
- Pack and push **IntegrationMessages** nuget package to **../nupkgs/** directory.\
Run *Powershell* script `./scripts/contracts-pack-push.ps1`

# Start
`docker compose up`\
Default environment is `Development`. This enables *PushUpdatesHub*'s debug controller and *swagger*.\
This spins up such services:
- [Traefik](http://ui.traefik.pushupdateshub.localhost/)
- [PushUpdatesHub](http://pushupdateshub.localhost/swagger/)
- [RabbitMQ](http://ui.rabbitmq.pushupdateshub.localhost/) Default credentials: user pass
- [Keycloak](http://keycloak.pushupdateshub.localhost/) Default admin credentials: admin pass

# Tests
`dotnet test ./PushUpdatesHub.sln`\
Under the hood of test `ShouldRetrievePushUpdateViaWebsocketWhenIntegrationMessageIsPublished` *RabbitMQ* and *Keycloak* are started (as *Docker containers*) and *Keycloak realm* is imported.