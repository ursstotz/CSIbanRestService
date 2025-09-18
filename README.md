# CSIbanRestService 🚀

A simple REST service that validates and fixes IBANs using the correct mod 97 calculation.  
Additionally, the service provides a `/version` endpoint to query the running version of the app.

## Features
- `GET /iban/fix?iban=<IBAN>` → returns the IBAN with correct mod 97 check digits.
- `GET /version` → returns the app version.

## Project Structure
```
CSIbanRestService/
  ├── CSIbanRestService/           # ASP.NET Core Web API project
  ├── .github/workflows/           # GitHub Actions workflows
  │     └── ci-cd.yml              # CI/CD pipeline configuration
  ├── Dockerfile                   # Dockerfile for container build
  └── README.md
```

## Local Development
### Run Locally
```bash
dotnet build
dotnet run --project CSIbanRestService
```
The service will be available at:
- `http://localhost:5000/iban/fix?iban=<IBAN>`
- `http://localhost:5000/version`

### Example Calls
```bash
curl "http://localhost:5000/iban/fix?iban=CH9300762011623852957"
curl "http://localhost:5000/version"
```

---

## Docker Deployment
### Build Docker image
```bash
docker build -t csibanrestservice .
```

### Run Docker container
```bash
docker run -p 8080:8080 csibanrestservice
```
Service available at `http://localhost:8080`.

---

## Azure CLI Installation
To deploy with CI/CD, you need the **Azure CLI (`az`)** installed locally or in your CI environment.

### Install via MSI (Windows)
1. Download the installer: [Azure CLI MSI](https://aka.ms/installazurecliwindows)
2. Run the installer and follow the setup wizard.
3. After installation, verify:
   ```powershell
   az --version
   ```

### Install via Scoop (Windows)
If you have [Scoop](https://scoop.sh/) installed:
```powershell
scoop install azure-cli
```

### Login to Azure
After installation:
```powershell
az login
```
This opens a browser window for authentication.

---

## CI/CD with GitHub Workflows
The directory `.github/workflows/` contains GitHub Actions workflows.

### Workflow: `.github/workflows/ci-cd.yml`
This workflow builds the Docker image, pushes it to Azure Container Registry (ACR), and deploys it to Azure Web App for Containers.

### Required GitHub Secrets
- `ACR_LOGIN_SERVER` → e.g. `myregistry.azurecr.io`
- `ACR_USERNAME` → Azure Container Registry username
- `ACR_PASSWORD` → Azure Container Registry password
- `AZURE_WEBAPP_NAME` → Name of the Azure Web App
- `AZURE_WEBAPP_PUBLISH_PROFILE` → Publish Profile XML string from Azure

### How to obtain the Publish Profile
1. In Azure Portal, go to your **App Service (Web App for Containers)**.
2. In the left menu, click **Get publish profile**.
3. This downloads an XML file (e.g., `MyAppName.PublishSettings`).
4. Open the file, copy the entire XML content.
5. In GitHub → **Settings → Secrets and variables → Actions**, create a new secret:
    - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
    - Value: paste the XML content.

---

## Azure Deployment
With CI/CD configured:
1. Commit and push to the `main` branch.
2. GitHub Actions will build and push the Docker image to ACR.
3. The workflow deploys the container image to your Azure Web App.

### Verify Deployment
```bash
curl "https://<your-app-name>.azurewebsites.net/iban/fix?iban=CH9300762011623852957"
curl "https://<your-app-name>.azurewebsites.net/version"
```

---

## Azure CLI Commands for Setup
Here are the key commands to create the required resources.

### 1. Create a Resource Group
```bash
az group create --name rg-csiban-demo --location westeurope
```

### 2. Create an Azure Container Registry (ACR)
```bash
az acr create --resource-group rg-csiban-demo --name csibanacr --sku Basic
```

### 3. Create an App Service Plan
```bash
az appservice plan create --name csiban-plan --resource-group rg-csiban-demo --sku B1 --is-linux
```

### 4. Create a Web App for Containers
```bash
az webapp create \
  --resource-group rg-csiban-demo \
  --plan csiban-plan \
  --name csibanrestservice-demo \
  --deployment-container-image-name csibanacr.azurecr.io/csibanrestservice:latest
```

### 5. Configure Web App to use port 8080
```bash
az webapp config appsettings set \
  --resource-group rg-csiban-demo \
  --name csibanrestservice-demo \
  --settings WEBSITES_PORT=8080
```

---

With these commands, your Azure environment is prepared. The GitHub Actions workflow will then build and deploy the container automatically.