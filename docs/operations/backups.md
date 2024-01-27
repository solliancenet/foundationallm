# FoundationaLLM Backups & data resiliency

Before implementing any backup strategy, it's important to carefully plan and consider factors such as recovery time objectives (RTO), recovery point objectives (RPO), and compliance requirements. Choose the method or combination of methods that best align with your specific backup and recovery needs.

## CosmosDB

Ensuring regular backups for Azure Cosmos DB is crucial to protect data such as private agents, user profiles, and chat history. Backups play a vital role in safeguarding your data against accidental deletions, data corruption, or other unforeseen issues.  Here are some key points to consider when planning your Cosmos DB backup strategy:

1. **Backup and Restore:**

   - **Automated Backups:**
     Azure Cosmos DB includes automated backups that are taken at regular intervals. These backups are used to provide point-in-time restore capabilities.

   - **Retention Period:**
     Backups are retained for a specific retention period, allowing you to restore your data to a previous state within that time frame.  The options for retention period are 7 or 30 days.  The Standard deployment configures 30 days retention.

   - **How to Configure Backup Policy:**
     Backup policies can be configured at the Cosmos DB account level, specifying the frequency and duration of backups.

   - **Restore from Backup:**
     You can initiate a point-in-time restore using the Azure Portal, Azure PowerShell, or Azure SDKs.

2. **Data Resiliency:**

   - **Global Distribution:**
     Distributing your Cosmos DB data globally across multiple regions ensures that your data is available even in the face of regional outages. This enhances data resiliency and availability.  The Standard Deployment does not currently enable global data distribution.  Data is replicated within a single region by the Cosmos DB service.

   - **Consistency Levels:**
     Azure Cosmos DB offers various consistency levels, allowing you to balance between consistency and availability based on your application's requirements. The Standard Deployment uses Session consistency by default.

3. **Manual Backups and Data Migration:**

   - **Export and Import:**
     You can manually export your Cosmos DB data to Azure Storage or another Cosmos DB account, providing an additional layer of backup and migration capability.  The Standard Deployment does not configure this capability by default.

## Storage Accounts

Backing up the storage account where your prompts, agents, and data sources are defined is crucial to ensure the integrity and availability of your conversational data. Here are steps you can take to back up an Azure Storage account:

1. **Azure Storage Account Replication:**
   Azure Storage offers built-in redundancy options like Locally Redundant Storage (LRS), Zone-Redundant Storage (ZRS), Geo-Redundant Storage (GRS), and Read-Access Geo-Redundant Storage (RA-GRS). These options replicate your data across different locations for high availability and durability. By default, LRS is enabled for all new storage accounts in the Standard Deployment. You can change the replication option for an existing storage account by navigating to the Replication tab in the Azure portal.

2. **Azure Backup:**
   Azure Backup service allows you to create backups of your virtual machines, files, and databases, including Azure Storage accounts. You can configure backup policies and retention rules to meet your data protection requirements.  Configuring Azure Backup is not currently enabled in the Standard Deployment, but you can use the Azure portal to manually configure back up for your storage account.

3. **Azure Blob Storage Versioning:**
   Azure Blob Storage versioning is a feature that allows you to enable versioning on your storage account. When versioning is enabled, any update or deletion of a blob results in the creation of a new version of that blob. This helps you maintain a historical record of changes made to your data.  This feature is enabled in the Standard Deployment.

4. **Azure Blob Storage Soft Delete:**
   Soft delete is a feature in Azure Blob Storage that provides an extra layer of protection against accidental data deletion. When soft delete is enabled, deleted blobs are retained for a specified retention period before being permanently deleted.  In the Standard Deployment, soft delete is enabled for 30 days for blobs and containers.

## Key Vault

Azure Key Vault provides several features to help you protect and manage your keys and secrets effectively.

1. **Purge Protection:**
   Purge protection is a feature in Azure Key Vault that helps prevent the permanent deletion of a key vault. When purge protection is enabled, the key vault cannot be permanently deleted immediately after deletion. Instead, there is a retention period during which the key vault is retained, and it can be recovered. Key Vaults in the Standard Deployment have purge protection enabled by default and deleted Key Vaults are retained for 7 days.

1. **Soft Delete:**
   Soft delete is a feature that protects keys, secrets, and certificates in Azure Key Vault from immediate and irreversible deletion. When soft delete is enabled, deleted items are retained for a specified retention period before they are permanently deleted. Soft delete is enabled by default for all new key vaults in the Standard Deployment. Deleted keys, secrets, and certificates are retained for 7 days.

2. **Secret Versioning:**
   Secret versioning is a feature that allows you to store multiple versions of a secret within a key vault. Each time you create or update a secret, a new version is generated. Secret versions help you maintain a history of changes and facilitate rollbacks if needed. This feature is enabled on all Key Vaults in Azure.

1. **Backups:**
   Azure Key Vault provides a backup and restore capability, allowing you to create backups of your key vault's keys, secrets, and certificates. These backups can be used for data recovery and protection against accidental data loss. There is no way to backup the entire Key Vault or to schedule regular backups.

## App Config

Azure App Configuration provides features related to backup, versioning, and data resiliency to help you effectively manage and deploy application configuration settings.

1. **Backup in Azure App Configuration:**
   Azure App Configuration allows you to back up your configuration settings, including feature flags, connection strings, and other key-value pairs using the Import/Export feature.  Backups can be sent to another App Configuration instance, an App Service, or a local file.

2. **Versioning in Azure App Configuration:**
   Azure App Configuration automatically version-controls your configuration settings. Each change to a key-value pair creates a new version. Versioning helps track changes to configuration settings over time. You can access and roll back to previous versions of a key-value pair.

3. **Data Resiliency in Azure App Configuration:**
   Azure App Configuration is designed with built-in redundancy across multiple regions to ensure high availability and data resiliency. App Configuration supports multi-region replication, allowing you to replicate your configuration settings to different regions for additional resilience. This feature is not currently enabled in the Standard Deployment.
