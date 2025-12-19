# ZATCA Certificate Testing - Complete Workflow

## Quick Start Guide

### Prerequisites
- Delete the `Output` folder to start fresh: `rm -rf Output`
- Have your ZATCA OTP ready from the sandbox portal (or use default: 123345)
- Credentials are already configured in the scripts

### Step 1: Generate All Certificates

```bash
cd /workspace/Zatca.EInvoice.CLI/TestsData
chmod +x generate-and-verify-certs.sh
./generate-and-verify-certs.sh
```

This will:
- Process ALL `.properties` files in the `Input` folder
- Generate CSR and private keys for each configuration
- Verify each certificate
- Create detailed logs in `Output/<config-name>/`

**Expected Output:**
```
Output/
├── csr-config-example-AR/
│   ├── certificate.csr
│   ├── private.pem
│   ├── config_details.txt
│   ├── generation_output.log
│   ├── verification_result.log
│   └── status.txt
├── csr-config-example-AR-VAT-Group/
├── csr-config-example-EN/
├── csr-config-example-EN-NEW/
└── csr-config-example-EN-VAT-group/
```

### Step 2: Run Complete Compliance Workflow

**For sandbox testing (uses default OTP 123345):**
```bash
chmod +x compliance-workflow.sh

# Test ALL configurations (recommended)
./compliance-workflow.sh --all --env sandbox

# Or test a single configuration
./compliance-workflow.sh --config csr-config-example-EN --env sandbox
```

**For simulation/production (requires fresh OTP):**
```bash
# Get a fresh OTP from: https://sandbox.zatca.gov.sa/IntegrationSandbox
./compliance-workflow.sh --all --otp YOUR_OTP_HERE --env simulation
```

This will perform the complete 5-phase lifecycle for EACH configuration:

**Phase 1: Compliance Certificate (CSID)**
- Request compliance certificate from ZATCA API
- Save certificate, secret, and request ID
- Logs: `compliance/compliance_response.json`, `phase1_summary.txt`

**Phase 2: Compliance Validation**
- Generate simplified and standard test invoices
- Sign each invoice with compliance certificate
- Submit for compliance validation
- Logs: `compliance/test_invoices/` with all invoice files and responses

**Phase 3: Production Certificate (PCSID)**
- Request production certificate
- Save production certificate and secret
- Logs: `production/production_response.json`, `phase3_summary.txt`

**Phase 4: Invoice Submission**
- Generate fresh invoices for production submission
- Sign invoices with production certificate (or compliance cert in sandbox)
- Submit simplified invoices via reporting API (B2C)
- Submit standard invoices via clearance API (B2B)
- Logs: `production/invoice_submissions/`, `phase4_summary.txt`

**Phase 5: Certificate Renewal (Optional)**
- Test certificate renewal API
- Request renewed production certificate
- Logs: `production/renewal/`, `phase5_summary.txt`

### Expected Complete Structure

After running both scripts, each configuration will have:

```
Output/csr-config-example-EN/
├── certificate.csr                 # Original CSR
├── private.pem                     # Private key (KEEP SECURE!)
├── config_details.txt              # Configuration summary
├── generation_output.log           # Generation logs
├── verification_result.log         # Verification logs
├── status.txt                      # PASSED/FAILED
├── workflow_complete.txt           # Final summary
├── compliance/
│   ├── compliance.crt              # Compliance certificate (CSID)
│   ├── secret.txt                  # Compliance secret
│   ├── request_id.txt              # For production cert request
│   ├── compliance_response.json    # Full API response
│   ├── compliance.pfx              # PFX format certificate
│   ├── phase1_summary.txt          # Phase 1 summary
│   ├── phase2_summary.txt          # Phase 2 summary
│   └── test_invoices/
│       ├── simplified_invoice.*    # Simplified invoice files
│       ├── simplified_*.json/txt   # Signing and compliance results
│       ├── standard_invoice.*      # Standard invoice files
│       └── standard_*.json/txt     # Signing and compliance results
└── production/
    ├── production.crt              # Production certificate (PCSID)
    ├── production_secret.txt       # Production secret
    ├── production_response.json    # Full API response
    ├── phase3_summary.txt          # Phase 3 summary
    ├── phase4_summary.txt          # Phase 4 summary
    ├── invoice_submissions/        # Phase 4: Production invoices
    │   ├── simplified_invoice.*    # Simplified invoice (reporting)
    │   ├── simplified_*.json       # Reporting results
    │   ├── standard_invoice.*      # Standard invoice (clearance)
    │   ├── standard_*.json         # Clearance results
    │   └── standard_cleared.xml    # Cleared invoice from ZATCA
    └── renewal/                    # Phase 5: Certificate renewal
        ├── renewed_production.crt  # Renewed certificate
        ├── renewed_production_secret.txt
        ├── renewal_response.json   # Renewal API response
        └── phase5_summary.txt      # Phase 5 summary
```

## Credentials

- **Username:** m.abumusa@karage.co
- **Password:** @hR8C8FQTxm9#ge
- **Sandbox Portal:** https://sandbox.zatca.gov.sa/IntegrationSandbox
- **Default Sandbox OTP:** 123345 (may work, get fresh OTP if it fails)

## What Gets Tested

The scripts will test **ALL** 5 certificate configurations:

| Config | Language | Org ID | VAT Group |
|--------|----------|--------|-----------|
| csr-config-example-EN | English | 399999999900003 | No |
| csr-config-example-EN-NEW | English | 310000000000013 | No |
| csr-config-example-AR | Arabic | 399999999900003 | No |
| csr-config-example-EN-VAT-group | English | 399999999910003 | Yes |
| csr-config-example-AR-VAT-Group | Arabic | 399999999910003 | Yes |

Each configuration goes through:
- Phase 0: Certificate generation and verification
- Phase 1: Compliance certificate request (CSID)
- Phase 2: Simplified and standard invoice compliance checks
- Phase 3: Production certificate request (PCSID)
- Phase 4: Invoice submission (clearance + reporting)
- Phase 5: Certificate renewal test

## Sandbox Limitations

When testing in sandbox mode, you may see these expected behaviors:
- **Simplified invoice reporting** may show "sandbox certificate limitation" warning
- **Certificate renewal** may fail with 404 (endpoint not available in sandbox)
- These are normal in sandbox - the actual API flow works correctly

## Troubleshooting

### Check Logs
All API responses and outputs are saved. Check:
- `generation_output.log` - Certificate generation
- `*_response.json` - API responses
- `*_summary.txt` files - Human-readable summaries
- `*_output.log` files - Complete command outputs

### Common Issues

**No configurations found:**
```bash
# Make sure you're in the right directory
cd /workspace/Zatca.EInvoice.CLI/TestsData
ls -la Input/*.properties
```

**OTP expired:**
- Get a fresh OTP from the sandbox portal
- OTPs are time-sensitive

**Certificate already exists:**
- Delete Output folder: `rm -rf Output`
- Run scripts again

### View Results Summary

```bash
# Check all configuration statuses
find Output -name "status.txt" -exec echo {} \; -exec cat {} \;

# Check all phase summaries
find Output -name "*_summary.txt" -exec echo "=== {} ===" \; -exec cat {} \;

# Check workflow completion
find Output -name "workflow_complete.txt" -exec cat {} \;

# Count completed workflows
find Output -name "workflow_complete.txt" | wc -l
```

## Success Indicators

- All 5 configurations complete with `workflow_complete.txt`
- Phase 1-4 summaries show SUCCESS
- Compliance checks pass for both simplified and standard invoices
- Clearance returns status: CLEARED for standard invoices
- No unexpected errors in `*_response.json` files

## Next Steps

After successful completion:
1. Review all generated summaries in each config folder
2. Verify API responses are successful
3. Production certificates are ready for live use
4. Keep all secrets and private keys secure
5. Deploy production certificates to your systems
