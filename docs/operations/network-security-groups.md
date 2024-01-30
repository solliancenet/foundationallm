# Network Security Group Configurations

FoundationaLLM uses Azure Virtual Networks for network segmentation.  The Standard Deployment uses Network Security Groups (NSGs) to control inbound and outbound traffic.  The following table lists the NSGs used in the Standard Deployment and the ports that are open by default.

## NSG Rules

### Application Gateway

| Rule Name                    | Access | DestinationAddressPrefix | DestinationPortRange | Direction | Priority | Protocol | ProvisioningState | SourceAddressPrefix | SourcePortRange | Notes                                                       |
| ---------------------------- | ------ | ------------------------ | -------------------- | --------- | -------- | -------- | ----------------- | ------------------- | --------------- | ----------------------------------------------------------- |
| allow-internet-http-inbound  | Allow  | VirtualNetwork           | 80                   | Inbound   | 128      | Tcp      | Succeeded         | Internet            | *               | Customers may restrict inbound connectivity as desired.     |
| allow-internet-https-inbound | Allow  | VirtualNetwork           | 443                  | Inbound   | 132      | Tcp      | Succeeded         | Internet            | *               | Customers may restrict inbound connectivity as desired.     |
| allow-gatewaymanager-inbound | Allow  | *                        | 65200-65535          | Inbound   | 148      | Tcp      | Succeeded         | GatewayManager      | *               | This rule is required by Azure and cannot be changed.[1][1] |
| allow-loadbalancer-inbound   | Allow  | *                        | *                    | Inbound   | 164      | *        | Succeeded         | AzureLoadBalancer   | *               | This rule is required by Azure and cannot be changed.[1][1] |
| deny-all-inbound             | Deny   | *                        | *                    | Inbound   | 4096     | *        | Succeeded         | *                   | *               | Customers may modify this rule if needed (not reccomended)            |

1: For further information regarding required NSG rules for Application Gateway, please see [this article][1].

[1]: https://learn.microsoft.com/en-us/azure/application-gateway/configuration-infrastructure#network-security-groups