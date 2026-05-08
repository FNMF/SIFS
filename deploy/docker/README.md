# SIFS Docker Deployment

## Start

```powershell
docker compose up -d --build
```

Default gateway:

- User frontend: http://localhost/
- Admin frontend: http://localhost/admin/
- Swagger: http://localhost/swagger/

Default dev accounts are inserted by `deploy/docker/mysql/init/99-dev-test-users.sql`:

- `admin` / `Admin@123456`
- `user` / `User@123456`

Do not include `99-dev-test-users.sql` in production deployments.

## Services

- `nginx`: public reverse proxy and backend load balancer.
- `backend-1`, `backend-2`: two SIFS backend workers.
- `web`: user frontend static container.
- `web-op`: admin frontend static container.
- `mysql`: MySQL 8 database.
- `fldcf-api`: FLDCF algorithm API.
- `fecdnet-api`: FECDNet algorithm API.

The backend and algorithm containers share the `app-data` volume mounted at `/data`.

## Production Notes

Set these environment variables before starting:

```powershell
$env:MYSQL_ROOT_PASSWORD="replace-me"
$env:JWT_SECRET_KEY="replace-with-a-long-secret"
$env:PUBLIC_BASE_URL="https://your-domain.example"
$env:HTTP_PORT="80"
```

If the host does not have NVIDIA Container Toolkit installed, remove `gpus: all` from `fldcf-api` and `fecdnet-api`.
