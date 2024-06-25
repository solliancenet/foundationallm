# Troubleshooting & issue reporting guide

There are three common issues that may arise when using the FoundationaLLM platform. This guide provides a structured approach to troubleshooting these issues.

## Troubleshooting Azure App Registration misconfiguration

### 1. **Symptoms:**

- Users unable to authenticate or access Azure services using the app.
- Error messages related to authentication failures.

### 2. **Troubleshooting Steps:**

a. **Verify App Registration Configuration:**
- Check the Azure Portal for the App Registration settings.
- Ensure the correct redirect URIs, client secrets, and authentication settings are configured by reviewing (the setup guide)[../deployment/authentication/core-authentication-setup-entra.md#update-app-configuration-settings].

b. **Error Logs Examination:**
- Review logs for any authentication-related errors.
- Check for error details and correlate them with misconfigured settings.

## Troubleshooting missing Azure App Registration details in Azure App Configuration

### 1. **Symptoms:**

- Application unable to retrieve configuration settings.
- Errors related to missing or invalid configuration values.
- Login problems similar to those described in the previous section.

### 2. **Troubleshooting Steps:**

a. **Check Azure App Configuration:**
- Verify that the App Registration details are correctly stored in Azure App Configuration using [the setup guide](../deployment/authentication/index.md).
- Ensure that keys, secrets, and connection strings are accurate.

c. **Azure App Configuration Logs:**
- Inspect Azure App Configuration logs for any errors related to configuration retrieval.
- Look for issues such as key not found or invalid values.

d. **Azure Key Vault Integration:**
- FoundationaLLM Azure Key Vault for sensitive configuration, verify the correct values are in Key Vault using [the setup guide][1].
- Ensure the Azure App Configuration managed identity has the necessary permissions to access Key Vault secrets.

## Troubleshooting container crashing

### 1. **Symptoms:**

- Containers restarting frequently or failing to start.
- Application unavailability due to container issues.

### 2. **Troubleshooting Steps:**

a. **Container Logs Examination:**
- Access container logs in Log Analytics to identify error messages or issues during startup.
- Look for any crashes, exceptions, or resource constraints.

b. **Resource Utilization:**
- Check resource utilization metrics (CPU, memory) for the container.
- Ensure that the container has adequate resources allocated.

c. **Dependency Check:**
- Examine dependencies within the containerized application.
- Verify that required services or components are accessible.

d. **Container Health Checks:**
- Identify and address health check failures impacting container stability.

e. **Container Image Update:**
- Review the container image version, update to the latest version to receive bug fixes and new features.

## Additional support and issue reporting

If you encounter an issue that is not addressed by the troubleshooting steps outlined in this document, we encourage you to open a GitHub issue. This ensures that our team can provide tailored assistance and continuously improve our troubleshooting resources.

### Steps to open a GitHub issue

1. **Navigate to our GitHub Repository:**
   - Visit our GitHub repository at [https://github.com/solliancenet/foundationallm](https://github.com/solliancenet/foundationallm).

2. **Check Existing Issues:**
   - Before creating a new issue, check the existing issues to see if the problem has already been reported or discussed.

3. **Create a New Issue:**
   - Click on the "Issues" tab in the repository.
   - Select "New Issue" to open a new issue template.

4. **Provide Detailed Information:**
   - Clearly describe the issue, including symptoms, error messages, and steps to reproduce.
   - Attach relevant logs or screenshots that can assist in understanding the problem.

5. **Tag the Issue Appropriately:**
   - Tag the issue with relevant labels, such as "bug," "enhancement," or "question," to categorize it correctly.

6. **Monitor for Updates:**
   - After creating the issue, monitor it for updates and respond promptly to any requests for additional information.

By opening a GitHub issue at [https://github.com/solliancenet/foundationallm](https://github.com/solliancenet/foundationallm), you contribute to our collaborative effort in maintaining a robust and well-supported system. Our team values your feedback, and addressing issues through GitHub allows for a transparent and efficient resolution process.

Thank you for your collaboration, and we look forward to assisting you with any challenges you may encounter. Your input is instrumental in enhancing the overall reliability and functionality of our system.
