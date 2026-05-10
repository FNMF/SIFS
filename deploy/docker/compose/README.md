# Docker deployment modes

## Core system only

Starts MySQL, backend, user frontend, admin frontend, and Nginx.
Algorithm APIs are treated as external services and should be configured through `algo_models.api_url`.

```powershell
docker compose up -d --build
```

Default Docker seed endpoints use:

- `http://host.docker.internal:8000/detect/fldcf`
- `http://host.docker.internal:8001/detect/fecdnet`

Change them in the admin algorithm page when algorithms are deployed on another host or provided by a third party.

## Local full stack

Starts the core system plus local FLDCF and FECDNet algorithm containers.

```powershell
docker compose -f docker-compose.yml -f deploy/docker/compose/docker-compose.local-full.yml up -d --build
```

This mode exposes:

- FLDCF: `http://localhost:8000`
- FECDNet: `http://localhost:8001`
- Nginx algorithm proxy:
  - `/algo/fldcf/`
  - `/algo/fecdnet/`

## Standalone algorithm APIs

Deploy FLDCF only:

```powershell
docker compose -f deploy/docker/compose/docker-compose.fldcf.yml up -d --build
```

Deploy FECDNet only:

```powershell
docker compose -f deploy/docker/compose/docker-compose.fecdnet.yml up -d --build
```

These standalone algorithm compose files are intended for separate machines or separate lifecycle management from the core system.
