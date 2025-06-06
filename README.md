[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=AndrexAce_ADAtickets&token=5e4556b655d4ea59dadb70371521d437de829163)](https://sonarcloud.io/summary/overall?id=AndrexAce_ADAtickets&branch=master)

![Static Badge](https://img.shields.io/badge/Framework-.NET%209.0.5%20%28STS%29-512BD4)
![Static Badge](https://img.shields.io/badge/Language-C%2313-8A2BE2)
![Static Badge](https://img.shields.io/badge/API-ASP.NET%20MVC%209-00A4EF)
![Static Badge](https://img.shields.io/badge/Web%20app-ASP.NET%20Blazor%209-244FFF)
![Static Badge](https://img.shields.io/badge/Database-PostgreSQL-CC2927)
![Static Badge](https://img.shields.io/badge/License-GPL%20v3-FF0000)
![Static Badge](https://img.shields.io/badge/Status-Active-32CD32)

# 🎫 ADAtickets

## ❓ What is ADAtickets
ADAtickets is a simple, lightweight, open source ticketing system interacting with your enterprise's repositories on Azure DevOps with a two-way synchronization.

It is based on .NET and other Microsoft technologies.

It was thought to be executed on Docker Compose environments and is designed to be used in conjunction with Microsoft Entra ID (formerly Azure Active Directory) for authentication and authorization, as well as Azure DevOps.

## ⚙️ Configuring ADAtickets

### Important knowledge

If you are not familiar with Azure and OpenID Connect terminology, I hereby present some terms that will be used throughout this documentation in order for you to better understand what I'm saying:

- **Azure**: A cloud computing platform and service created by Microsoft for building, testing, deploying, and managing applications and services through Microsoft-managed data centers.
- **Resource group**: A resource group is a logical container in Azure that holds related resources, such as databases, tenants, virtual machines, and other services.
- **Microsoft Entra ID**: Formerly known as Azure Active Directory (Azure AD), it is a cloud-based identity and access management service. It helps your employees sign in and access resources.
- **Microsoft Entra External ID**: A service that allows external users to access your organization's resources securely. It enables collaboration with partners, suppliers, and customers while maintaining control over your data.
- **Tenant**: A tenant is a dedicated instance of the Microsoft Entra ID service that your organization receives when it signs up for a Microsoft cloud service such as Azure, Microsoft 365, or Dynamics 365.
- **Directory**: A directory is the part of the tenant that contains users, groups, permissions, and other resources.
- **Azure DevOps**: A set of development tools and services provided by Microsoft for software development, including version control, project management, and continuous integration/continuous deployment (CI/CD) pipelines.
- **Application role**: An application role is a role that can be assigned to users or applications interacting with your application. It establishes the identity and permission level of the user or application (who they are and what they can access).
- **Scope**: An OpenID Connect scope is a permission requested by an application to the API. It establishes specific actions that can be performed by the user or application on the API resources (what they can do).

> [!TIP]
> Often the terms **tenant** and **directory** are used interchangeably since every tenant is linked to one and one directory only, but they are not the same thing.

### Prerequisites

In order to follow the configuration steps below, you will need to have the following prerequisites:

- You must have an Azure account with permissions to create resource groups with related resources and to manage Microsoft Entra ID settings.

> [!NOTE]
> I won't cover the steps to gain the adequate permissions here, but you can find more information in the [Microsoft documentation](https://learn.microsoft.com/en-us/azure).

- You must have Docker installed on your machine. You can download it from the [Docker website](https://www.docker.com/products/docker-desktop) or install it via CLI.

> [!NOTE]
> If you are using Windows Home, make sure to enable WSL 2 before installing Docker. This is because Windows Professional supports Hyper-V to run Docker while Home doesn't.

> [!WARNING]
> You must install a Docker Desktop version that is at least **4.4.2** and/or a Docker CLI version that is at least **20.10.13** as Docker Compose is needed. Otherwise, the installation will fail.

- You must have a valid SSL origin certificate and SSL client certificate. The SSL certificate will be used by Kestrel (the ASP.NET built-in web server) to secure the connection with HTTPS, while the SSL client certificate is used by the application to authenticate users securely against Microsoft Entra ID.

### Azure setup

#### Premise

ADAtickets is designed for two kind of accesses:

- **Internal access** is meant for developers, the people who will receive tickets and work on them. Since the app is meant to be synced with Azure DevOps, it is expected that these users have access to the Azure DevOps organization where the tickets will be created and managed.
- **External access** is meant for users outside your organization, such as partners or customers who use your application and thus have no access to Azure DevOps.

These two require thus two different tenants.

#### Tenant setup

I will assume you are already part or have already created a Microsoft Entra ID tenant. If you don't know how to do it, you can follow the [Microsoft documentation](https://learn.microsoft.com/en-us/entra/identity/quickstart-create-tenant).

##### API app registration

1. Sign in to the [Entra admin center](https://entra.microsoft.com/).
2. Navigate to **Identity** > **Applications** > **App registrations**.
3. Click on **New registration** to create a new application registration. Here you can create applications, that is, virtual representation of your application.
4. Insert a name for your application, such as **ADAticketsAPI**.
5. Since we want to enable access to this tenant only to people in your organization, select the option **Accounts in this organizational directory only (Single tenant)**.
6. Register the application by clicking on the **Register** button. You will be redirected to your application page.

##### Expose APIs and add scopes

1. Go to **Expose an API** in the left sidebar. This section is meant to be used to define the scopes and which of them can be requested by the application.
2. Next to **Application ID URI**, click on **Add** and accept the default identifier value.
3. Click on **Add a scope** to create a new scope for your API.
4. Add the name **Read**.
5. In the **Who can consent** section, select **Admins only**.
6. In the **Admin consent display name** field, insert a name such as **Read ADAtickets data**.
7. In the **Admin consent description** field, insert a description such as **Allows the application to read ADAtickets data**.
8. Finalize with **Add scope**.
9. Add a new scope called **ReadWrite**.
10. In the **Who can consent** section, select **Admins only**.
11. In the **Admin consent display name** field, insert a name such as **ReadWrite ADAtickets data**.
12. In the **Admin consent description** field, insert a description such as **Allows the application to read and write ADAtickets data**.
13. Finalize with **Add scope**.

##### Add application roles

1. Go to **App roles** in the left sidebar. This section is meant to be used to define the roles that can be assigned to users or applications.
2. Click on **Create app role** to define a new role for your application.
3. Insert a name for your role, such as **User**.
4. In the **Allowed member types** section, select **User/Groups**.
5. In the **Value** field, insert **User**.
6. In the **Description** field, insert a description of the role, such as **Users can perform limited actions.**.
7. Click on **Add** to finalize.
8. Click on **Create app role** to define another role for your application.
9. Insert a name for your role, such as **Operator**.
10. In the **Allowed member types** section, select **User/Groups**.
11. In the **Value** field, insert **Operator**.
12. In the **Description** field, insert a description of the role, such as **Operators can perform almost all actions.**.
13. Click on **Add** to finalize.
14. Click on **Create app role** to define the last role for your application.
15. Insert a name for your role, such as **Admin**.
16. In the **Allowed member types** section, select **User/Groups**.
17. In the **Value** field, insert **Admin**.
18. In the **Description** field, insert a description of the role, such as **Admins can perform all actions.**.
19. Click on **Add** to finalize.

> [!NOTE]
> You can personalize your application info in the **Branding & properties** section, such as adding a logo or changing the color theme.

###### Web app registration

About the web app registration, you can follow the same steps as above, but with the following differences:

1. Choose another name, such as **ADAticketsWeb**.
2. In the **Redirect URI** section, select **Web** as the platform and insert the URLs `http://localhost:[port]/signin-oidc` for local development and `https://[yourdomain]/signin-oidc` for production. These represents which redirects are valid when the user has completed the authentication flow.

##### Configure authentication

1. Go to **Authentication** in the left sidebar. This section is meant to be used to define the authentication details for your application.
2. In the **Redirect URIs** section, ensure to add all necessary callback URLs to successfully handle the authentication response.
3. In the **Front-channel logout URL** section, add the URLs `http://localhost:[port]/signin-oidc` for local development and `https://[yourdomain]/signin-oidc` for production to allow users to log out of the application.
4. In the **Implicit grant and hybrid flows** section, check the box for **ID tokens** to enable the application to receive ID tokens during the authentication flow.
5. Click on **Save** to finalize.

##### Add delegated permissions

1. Go to **API permissions** in the left sidebar. This section is meant to be used to define the permissions (scopes) that the application needs.
2. In the **Configured permissions** section, click **Add a permission**.
3. Select **My APIs**.
4. Select your API application.
5. Select the **Read** and **ReadWrite** permissions created earlier.
6. Click on **Add permissions** to finalize.
7. Click on **Grant admin consent for ADAtickets** so that all the permissions can be used without the need of asking to grant them on login.

##### Add SSL client certificate

1. Go to **Certificates & secrets** in the left sidebar. This section is meant to be used to define the certificates and/or secrets that can be used to authenticate the application when interacting with the authentication service.
2. Click on **Certificates**.
3. Click on **Upload certificate** to upload your SSL client certificate.
4. Click on **Add** to finalize.

> [!WARNING]
> A certificate is made up of a public key and a private key. The public key is safe to be shared, while the private key must be kept secret. Thus, you must upload your public key (it should have a `.cer`, `.pem`, or `.crt` extension). The private key will be used during the application setup on the server.

##### Add client as trusted to API

1. Go back to your API application.
2. Go to **Expose an API** in the left sidebar.
3. In the **Authorized client applications** section, click on **Add a client application**. This section is meant to be used to define which applications can the APIs without asking consents.
4. Insert the **Application (client) ID** of your web application registration. You can find it in the **Overview** section of your web application registration.
5. In the **Authorized scopes** section, select the **Read** and **ReadWrite** scopes created earlier.
6. Click on **Add application** to finalize.

#### External tenant setup

I will assume you have already created a Microsoft Entra External ID tenant. If you don't know how to do it, you can follow the [Microsoft documentation](https://learn.microsoft.com/en-us/entra/external-id/quickstart-create-tenant).

The steps to configure the external tenant are exactly the same as the ones above.

There are just a couple more things to configure, with the instructions in the following section.

##### Add user authentication flow

The user authentication flow represents the process that external users will follow to authenticate themselves in your application.

By default, the external tenant will have an already setup flow. You can edit it or create a new one.

It is important for you to select in the **User attributes** section (which represent the data the flow will ask the user to provide in order to sign up), the **Given name** and **Surname** attributes. The **Email address** attribute is already selected by default, so you don't need to worry about it.

> [!NOTE]
> You can configure other identity providers such as Google, Facebook, or Microsoft accounts in the **All identity providers** section. This way, external users can sign up using their existing accounts. It is not mandatory though.

##### Optional configuration

Since the application authentication is almost fully managed by Entra, related services like multi factor authentication (MFA), self-service password reset and other authentication methods can be enabled and managed only in the Entra admin center.

You don't need to manually enable the MFA, since it is enforced by the **Security defaults** setting in the tenant.

> [!TIP]
> It is not mandatory but really recommended to enable MFA for users, as it adds an extra layer of security to the authentication process.

If you want to enable self-service password reset, you can follow these steps:

1. Navigate to **Protection** > **Password reset**.
2. Click on the **Properties** tab in the left sidebar.
3. In the **Self-service password reset enabled** section, select **All**.

> [!TIP]
> It is not mandatory but really recommended to enable self-service password reset for users, as it allows them to reset their password without needing to contact the support team.

> [!NOTE]
> Self-service password reset is enabled by default for administrators.

If you want to enable other authentication methods, you can follow these steps:

1. Navigate to **Protection** > **Authentication methods**.
2. Click on the **Policies** tab.
3. In the **Authentication method policies** section, many options are available. Here you can configure the methods you fancy the most.

## 🗺️ Roadmap

- Automatic deployment of needed resources during setup with Azure SDK.

## 📃 License
This project is licensed under the GPL v3 License — see the [LICENSE](https://github.com/AndrexAce/ADAtickets/blob/master/LICENSE.txt) file for details.