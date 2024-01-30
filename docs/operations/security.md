# Platform security features & best practices

Maintaining the security of the Azure platform is crucial for protecting sensitive data and ensuring the integrity of your infrastructure.

1. **Identity and Access Management (IAM)**
   - Ensure that only authorized users have access to your Azure resources.
   - FoundationaLLM uses Azure Entra ID for centralized identity management.
   - Enable multi-factor authentication (MFA) for all user accounts.

2. **Network Security**
   - The standard deployment:
     - Uses Azure Virtual Networks for network segmentation.
     - Implements Network Security Groups (NSGs) to control inbound and outbound traffic.
     - [Network Security Rule Details](./network-security-groups.md)

3. **Data Encryption**
   - Wherever possible the Standard Deployment uses encryption at rest with system-managed keys.
   - Customer managed keys can be enabled at your discretion.

4. **Threat Detection and Monitoring**
   - Wherever supported the Standard Deployment enables Azure Diagnostics on the resources it deploys.  These logs are sent to a Log Analytics workspace. as part of the standard resources.
   - Customers are encouraged to enable Azure Defender for additional threat detection and monitoring.
   - Customers may use Azure Sentinel for advanced security information and event management (SIEM). Customers already using Azure Sentinel may redirect the logs from the Standard Deployment to their existing Azure Sentinel instance.

5. **Patch Management**
   - Regularly check the FoundationaLLM github repository for new image releases and update your deployment accordingly.
