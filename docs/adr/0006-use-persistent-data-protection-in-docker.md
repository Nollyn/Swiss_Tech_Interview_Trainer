# ADR 0006: Persistence of Data Protection and Antiforgery in Containerized Environments

## Status
Accepted

## Context
When running the `SwissTechTrainer.Web` application inside Docker containers, the Blazor Server interactive circuit (SignalR) would intermittently become corrupted during the site's initial load.

System logs reported the following exception:
`Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException: The antiforgery token could not be decrypted. The key was not found in the key ring.`

### Root Cause
By default, ASP.NET Core generates Data Protection cryptographic keys in the container's ephemeral memory. Whenever the container is rebuilt or restarted, these keys are destroyed and new ones are generated. If the user's browser retains a cookie from a previous session, decryption of the Antiforgery token fails; this causes the Blazor (SignalR) circuit to disconnect immediately and freezes UI interactivity upon the first click.

## Decision
We have decided to externalize and persist the ASP.NET Core cryptographic key ring storage outside the Docker container lifecycle through two actions:

1. **Code Configuration (`Program.cs`):** Force the use of a fixed physical path (`dp-keys`) within the application runtime environment and define a static application name to prevent decryption collisions.
2. **Infrastructure (`docker-compose.yml`):** Map the internal directory `/app/dp-keys` to a persistent volume on the host machine.

## Consequences

### Positive
* **Circuit Stability:** Users will no longer experience button unresponsiveness or the need to reload the page (F5) after container deployment or restarts. * **Horizontal Scalability:** By defining a constant `ApplicationName` and persisting the keys, the solution is prepared to share the same keyring if the web service is scaled out to multiple replicas behind a load balancer in the future.

### Drawbacks / Risks
* **Host Security:** Cryptographic keys are now physically written to the host's disk. Access to the volume folder in production must be restricted exclusively to the user running the Docker daemon.