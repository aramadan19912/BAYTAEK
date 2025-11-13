# DDoS Protection Configuration

## Overview

This document outlines the Distributed Denial of Service (DDoS) protection strategy for the Home Service Platform, including configuration for both Azure infrastructure and Hostinger hosting environment.

## Multi-Layer DDoS Protection Strategy

```
┌─────────────────────────────────────┐
│   Layer 1: DNS/CDN (Cloudflare)    │  <- DNS-level protection
├─────────────────────────────────────┤
│   Layer 2: Hostinger WAF           │  <- Web Application Firewall
├─────────────────────────────────────┤
│   Layer 3: Azure DDoS Protection   │  <- Network-level protection
├─────────────────────────────────────┤
│   Layer 4: Application Rate Limit  │  <- Application-level protection
├─────────────────────────────────────┤
│   Layer 5: API Gateway             │  <- API-level protection
└─────────────────────────────────────┘
```

## Layer 1: DNS Protection (Cloudflare - Recommended)

### Why Cloudflare?
- Global CDN network
- Always-on DDoS protection
- Free tier available
- Works with Hostinger

### Cloudflare Setup

1. **Sign up for Cloudflare**
   ```
   https://dash.cloudflare.com/sign-up
   ```

2. **Add Your Domain**
   - Add homeservice.com to Cloudflare
   - Update nameservers at Hostinger to Cloudflare's

3. **Configure DNS Records**
   ```dns
   Type    Name        Content                     Proxy
   A       @           your-hostinger-ip           ✓ Proxied
   A       www         your-hostinger-ip           ✓ Proxied
   A       api         your-azure-ip               ✓ Proxied
   CNAME   admin       your-app.azurewebsites.net  ✓ Proxied
   ```

4. **Enable DDoS Protection**
   ```
   Security > Settings
   - Security Level: High
   - Challenge Passage: 30 minutes
   - Browser Integrity Check: On
   ```

5. **Configure Firewall Rules**
   ```javascript
   // Block suspicious traffic
   (http.request.uri.path contains "/admin" and
    ip.geoip.country notin {"SA" "EG"}) or
   (cf.threat_score gt 10)

   // Rate limiting rule
   (http.request.uri.path eq "/api/auth/login" and
    rate(1m) > 5)
   ```

6. **Enable DDoS Attack Mode**
   ```
   Security > Settings > Under Attack Mode
   - Enable automatically during attacks
   - Shows interstitial page to verify users
   ```

### Cloudflare Pricing (Optional Upgrades)

- **Free**: Basic DDoS protection, suitable for most use cases
- **Pro ($20/month)**: Advanced DDoS protection, WAF rules
- **Business ($200/month)**: Guaranteed uptime, advanced security
- **Enterprise**: Custom pricing, dedicated support

## Layer 2: Hostinger Configuration

### Hostinger DDoS Protection Features

Hostinger provides built-in DDoS protection:
- Network-level filtering
- Traffic anomaly detection
- Automatic mitigation
- 24/7 monitoring

### Hostinger Panel Configuration

1. **Access Hostinger Control Panel**
   ```
   https://hpanel.hostinger.com
   ```

2. **Enable DDoS Protection**
   ```
   Hosting > Advanced > Security
   - DDoS Protection: Enabled
   - Firewall: Enabled
   - Mod Security: Enabled
   ```

3. **Configure IP Blocking**
   ```
   Security > IP Blocker
   - Block known attack IPs
   - Add IP ranges if needed
   ```

4. **Configure .htaccess (if using Apache)**
   ```apache
   # .htaccess in web root

   # Rate limiting
   <IfModule mod_ratelimit.c>
       SetOutputFilter RATE_LIMIT
       SetEnv rate-limit 400
       SetEnv rate-initial-burst 10
   </IfModule>

   # Block suspicious user agents
   SetEnvIfNoCase User-Agent "^Wget" bad_bot
   SetEnvIfNoCase User-Agent "^curl" bad_bot
   SetEnvIfNoCase User-Agent "^libwww-perl" bad_bot
   Order Allow,Deny
   Allow from all
   Deny from env=bad_bot

   # Limit request methods
   <LimitExcept GET POST PUT DELETE>
       Order Allow,Deny
       Deny from all
   </LimitExcept>

   # Prevent multiple simultaneous connections
   <IfModule mod_limitipconn.c>
       <Location />
           MaxConnPerIP 10
       </Location>
   </IfModule>
   ```

5. **Configure ModSecurity Rules**
   ```apache
   # ModSecurity Core Rules
   SecRuleEngine On
   SecRequestBodyAccess On
   SecRule REQUEST_HEADERS:Content-Type "text/xml" \
       "id:'200000',phase:1,t:none,t:lowercase,pass,nolog,ctl:requestBodyProcessor=XML"

   # Block rapid requests
   SecAction "id:100,phase:1,nolog,pass,initcol:ip=%{REMOTE_ADDR}"
   SecRule IP:REQUEST_COUNT "@gt 100" \
       "id:101,phase:1,deny,status:429,log,msg:'Rate limit exceeded'"
   ```

### Hostinger Resource Limits

Configure resource limits in cPanel:
```
Resource Usage > Options
- Physical Memory: 2GB
- CPU Usage: 100%
- Entry Processes: 20
- Processes: 100
- I/O Usage: 1024 KB/s
```

## Layer 3: Azure DDoS Protection

### Azure DDoS Protection Standard

1. **Enable DDoS Protection Plan**
   ```bash
   # Create DDoS protection plan
   az network ddos-protection create \
       --resource-group homeservice-rg \
       --name homeservice-ddos \
       --location eastus
   ```

2. **Associate with Virtual Network**
   ```bash
   # Enable DDoS on VNet
   az network vnet update \
       --resource-group homeservice-rg \
       --name homeservice-vnet \
       --ddos-protection true \
       --ddos-protection-plan homeservice-ddos
   ```

3. **Configure via Bicep** (add to main.bicep)
   ```bicep
   resource ddosProtectionPlan 'Microsoft.Network/ddosProtectionPlans@2023-05-01' = {
     name: '${resourceNamePrefix}-ddos'
     location: location
     properties: {}
   }

   resource virtualNetwork 'Microsoft.Network/virtualNetworks@2023-05-01' = {
     name: '${resourceNamePrefix}-vnet'
     location: location
     properties: {
       addressSpace: {
         addressPrefixes: [
           '10.0.0.0/16'
         ]
       }
       enableDdosProtection: true
       ddosProtectionPlan: {
         id: ddosProtectionPlan.id
       }
     }
   }
   ```

### Azure DDoS Metrics & Alerts

```bash
# Configure DDoS attack alerts
az monitor metrics alert create \
    --name ddos-attack-alert \
    --resource-group homeservice-rg \
    --scopes /subscriptions/{sub-id}/resourceGroups/homeservice-rg/providers/Microsoft.Network/publicIPAddresses/homeservice-pip \
    --condition "avg DDoSAttackOrNot > 0" \
    --description "Alert when DDoS attack detected" \
    --evaluation-frequency 1m \
    --window-size 5m \
    --action-group-id /subscriptions/{sub-id}/resourceGroups/homeservice-rg/providers/microsoft.insights/actionGroups/ddos-response
```

### Azure Front Door (Recommended)

```bicep
resource frontDoor 'Microsoft.Cdn/profiles@2023-05-01' = {
  name: '${resourceNamePrefix}-fd'
  location: 'global'
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
  properties: {
    originResponseTimeoutSeconds: 60
  }
}

resource wafPolicy 'Microsoft.Network/FrontDoorWebApplicationFirewallPolicies@2022-05-01' = {
  name: '${resourceNamePrefix}wafpolicy'
  location: 'global'
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
  properties: {
    policySettings: {
      enabledState: 'Enabled'
      mode: 'Prevention'
      requestBodyCheck: 'Enabled'
    }
    managedRules: {
      managedRuleSets: [
        {
          ruleSetType: 'Microsoft_DefaultRuleSet'
          ruleSetVersion: '2.0'
        }
        {
          ruleSetType: 'Microsoft_BotManagerRuleSet'
          ruleSetVersion: '1.0'
        }
      ]
    }
    customRules: {
      rules: [
        {
          name: 'RateLimitRule'
          priority: 1
          ruleType: 'RateLimitRule'
          rateLimitThreshold: 100
          rateLimitDurationInMinutes: 1
          matchConditions: [
            {
              matchVariable: 'RequestUri'
              operator: 'Contains'
              matchValue: ['/api/']
            }
          ]
          action: 'Block'
        }
      ]
    }
  }
}
```

### Azure Pricing

- **DDoS Protection Standard**: ~$2,944/month
  - Covers first 100 resources
  - $30 per additional resource
- **Azure Front Door Standard**: ~$35/month + data transfer
- **Application Gateway v2 with WAF**: ~$246/month

## Layer 4: Application-Level Rate Limiting

### Rate Limiting Middleware (Already Implemented)

Our custom middleware provides:
```csharp
// Default configuration
MaxRequests: 100 per minute (production)
MaxRequests: 1000 per minute (development)

// Per-endpoint overrides
/api/auth/login: 5 per minute
/api/auth/register: 3 per hour
/api/auth/password-reset: 3 per hour
/api/files/upload: 10 per hour
```

### Redis-based Distributed Rate Limiting

For multi-instance deployments:
```csharp
// Install package
// Install-Package StackExchange.Redis.Extensions.AspNetCore

// Configure in appsettings.json
{
  "RateLimit": {
    "EnableDistributed": true,
    "RedisConnection": "your-redis-connection",
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      }
    ],
    "EndpointRules": [
      {
        "Endpoint": "POST:/api/auth/login",
        "Period": "1m",
        "Limit": 5
      }
    ]
  }
}
```

## Layer 5: API Gateway Protection

### Azure API Management (Optional)

```bicep
resource apiManagement 'Microsoft.ApiManagement/service@2023-03-01-preview' = {
  name: '${resourceNamePrefix}-apim'
  location: location
  sku: {
    name: 'Developer'
    capacity: 1
  }
  properties: {
    publisherEmail: 'admin@homeservice.com'
    publisherName: 'Home Service'
  }
}

// Rate limiting policy
resource apiPolicy 'Microsoft.ApiManagement/service/policies@2023-03-01-preview' = {
  parent: apiManagement
  name: 'policy'
  properties: {
    value: '''
      <policies>
        <inbound>
          <rate-limit calls="100" renewal-period="60" />
          <quota calls="10000" renewal-period="86400" />
          <ip-filter action="allow">
            <address-range from="0.0.0.0" to="255.255.255.255" />
          </ip-filter>
        </inbound>
      </policies>
    '''
  }
}
```

## DDoS Attack Detection

### Monitoring Metrics

1. **Network Layer**
   - Packets per second (PPS)
   - Bits per second (BPS)
   - Connection rate
   - Concurrent connections

2. **Application Layer**
   - Request rate
   - Error rate (4xx, 5xx)
   - Response time
   - CPU/Memory usage

### Alert Thresholds

```yaml
# Application Insights alert rules
alerts:
  - name: "High Request Rate"
    metric: "requests/count"
    threshold: 1000
    window: 5m
    severity: warning

  - name: "Abnormal Error Rate"
    metric: "requests/failed"
    threshold: 50%
    window: 5m
    severity: critical

  - name: "Slow Response Time"
    metric: "requests/duration"
    threshold: 5000ms
    window: 5m
    severity: warning

  - name: "High CPU Usage"
    metric: "performanceCounters/processorCpuPercentage"
    threshold: 80%
    window: 10m
    severity: warning
```

### Azure Monitor Query

```kusto
// Detect potential DDoS attack
requests
| where timestamp > ago(5m)
| summarize
    RequestCount = count(),
    UniqueIPs = dcount(client_IP),
    AvgDuration = avg(duration),
    ErrorRate = countif(success == false) * 100.0 / count()
    by bin(timestamp, 1m)
| where RequestCount > 1000 or ErrorRate > 50
| project timestamp, RequestCount, UniqueIPs, ErrorRate, AvgDuration
| order by timestamp desc
```

## DDoS Response Plan

### Immediate Response (0-5 minutes)

1. **Verify Attack**
   ```bash
   # Check Azure metrics
   az monitor metrics list \
       --resource-id <resource-id> \
       --metric DDoSAttackOrNot

   # Check Cloudflare dashboard
   # Security > Events
   ```

2. **Enable Attack Mode**
   ```bash
   # Cloudflare: Enable "Under Attack Mode"
   # Shows CAPTCHA to all visitors
   ```

3. **Scale Up Resources**
   ```bash
   # Azure: Scale out app service
   az appservice plan update \
       --name homeservice-asp \
       --resource-group homeservice-rg \
       --sku P2V2 \
       --number-of-workers 5
   ```

### Short-term Response (5-30 minutes)

1. **Analyze Traffic**
   ```bash
   # Identify attack vectors
   # - IP addresses
   # - User agents
   # - Request patterns
   # - Geographic origin
   ```

2. **Block Malicious Traffic**
   ```bash
   # Cloudflare firewall rule
   (ip.src in {1.2.3.4 5.6.7.8}) or
   (http.user_agent contains "malicious") or
   (ip.geoip.country eq "XX")

   # Action: Block
   ```

3. **Notify Stakeholders**
   - Security team
   - Management
   - Hosting provider (if needed)
   - Customers (if service impacted)

### Long-term Response (30+ minutes)

1. **Implement Permanent Fixes**
   - Update WAF rules
   - Add IP blacklists
   - Adjust rate limits
   - Optimize application

2. **Post-Incident Review**
   - Document attack details
   - Analyze effectiveness
   - Update response plan
   - Improve detection

## Testing DDoS Protection

### Load Testing (Authorized Only)

```bash
# Apache Bench
ab -n 10000 -c 100 https://homeservice.com/api/health

# Artillery.io
npm install -g artillery
artillery quick --count 100 --num 1000 https://homeservice.com/api/health

# Locust
pip install locust
locust -f loadtest.py --host=https://homeservice.com
```

### Verify Protection

1. **Rate Limiting**
   ```bash
   # Should return 429 after threshold
   for i in {1..150}; do
     curl -w "\n%{http_code}\n" https://homeservice.com/api/services
   done
   ```

2. **Security Headers**
   ```bash
   curl -I https://homeservice.com
   # Should see security headers
   ```

3. **DDoS Protection Active**
   ```bash
   # Check Cloudflare protection
   dig homeservice.com
   # Should resolve to Cloudflare IPs
   ```

## Cost Optimization

### Recommended Setup by Budget

**Budget Tier ($0-50/month)**
- Cloudflare Free
- Hostinger built-in DDoS
- Application rate limiting
- **Total: ~$0/month**

**Standard Tier ($50-500/month)**
- Cloudflare Pro ($20)
- Azure App Service Standard (included)
- Application rate limiting
- **Total: ~$20/month**

**Enterprise Tier ($500+/month)**
- Cloudflare Business ($200)
- Azure DDoS Standard ($2,944)
- Azure Front Door ($35+)
- Azure API Management ($50+)
- **Total: ~$3,229/month**

## Best Practices

1. **Always-On Protection**
   - Enable DDoS protection before launch
   - Don't wait for an attack

2. **Multi-Layer Defense**
   - Don't rely on single solution
   - Implement defense in depth

3. **Regular Testing**
   - Test rate limiting monthly
   - Verify alerts work
   - Update response plan

4. **Monitor Continuously**
   - Set up alerts
   - Review logs daily
   - Track metrics

5. **Keep Updated**
   - Update WAF rules
   - Patch vulnerabilities
   - Review security policies

## Resources

- [Azure DDoS Protection Best Practices](https://learn.microsoft.com/en-us/azure/ddos-protection/ddos-best-practices)
- [Cloudflare DDoS Protection](https://www.cloudflare.com/ddos/)
- [Hostinger Security Features](https://www.hostinger.com/tutorials/security)
- [OWASP DDoS Prevention](https://owasp.org/www-community/attacks/Denial_of_Service)

## Emergency Contacts

```
Security Team: security@homeservice.com
DDoS Hotline: +1-xxx-xxx-xxxx
Azure Support: https://portal.azure.com/#create/Microsoft.Support
Cloudflare Support: https://dash.cloudflare.com/support
Hostinger Support: https://www.hostinger.com/contact
```
