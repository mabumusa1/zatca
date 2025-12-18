# ZATCA Certificate Testing - Complete Workflow

## Quick Start Guide

### Prerequisites
- Delete the `Output` folder to start fresh: `rm -rf Output`
- Have your ZATCA OTP ready from the sandbox portal
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
└── csr-config-example-EN-VAT-group/
```

### Step 2: Run Complete Compliance Workflow

**IMPORTANT**: Get a fresh OTP from the ZATCA portal before running this step:
- **Sandbox Portal**: https://sandbox.zatca.gov.sa/IntegrationSandbox
- Login with credentials (see below)
- Generate a new OTP (they expire quickly)

```bash
chmod +x compliance-workflow.sh

# Test ALL configurations (recommended) - REPLACE YOUR_OTP with actual OTP from portal
./compliance-workflow.sh --all --otp YOUR_OTP_HERE --env simulation

# Or test a single configuration
./compliance-workflow.sh --config csr-config-example-EN --otp YOUR_OTP_HERE --env simulation
```

**Note**: Use `--env simulation` for testing. Sandbox and simulation endpoints behave differently.

This will perform the complete 3-phase lifecycle for EACH configuration:

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
│   ├── phase1_summary.txt          # Phase 1 summary
│   ├── phase2_summary.txt          # Phase 2 summary
│   ├── compliance.pfx              # PFX format certificate
│   └── test_invoices/
│       ├── simplified_invoice.json
│       ├── simplified_invoice.xml
│       ├── simplified_invoice_signed.xml
│       ├── simplified_hash.txt
│       ├── simplified_uuid.txt
│       ├── simplified_compliance_result.json
│       ├── simplified_compliance_summary.txt
│       ├── standard_invoice.json
│       ├── standard_invoice.xml
│       ├── standard_invoice_signed.xml
│       ├── standard_hash.txt
│       ├── standard_uuid.txt
│       ├── standard_compliance_result.json
│       └── standard_compliance_summary.txt
└── production/
    ├── production.crt              # Production certificate (PCSID)
    ├── production_secret.txt       # Production secret
    ├── production_response.json    # Full API response
    └── phase3_summary.txt          # Phase 3 summary
```

## Credentials

- **Username:** m.abumusa@karage.co
- **Password:** @hR8C8FQTxm9#ge
- **Sandbox Portal:** https://sandbox.zatca.gov.sa/IntegrationSandbox

## What Gets Tested

The scripts will test **ALL** certificate configurations:

1. **csr-config-example-EN** - English configuration
2. **csr-config-example-AR** - Arabic configuration
3. **csr-config-example-EN-VAT-group** - English VAT group
4. **csr-config-example-AR-VAT-Group** - Arabic VAT group

Each configuration goes through:
- ✓ Certificate generation
- ✓ Certificate verification
- ✓ Compliance certificate request
- ✓ Simplified invoice compliance check
- ✓ Standard invoice compliance check
- ✓ Production certificate request

## Troubleshooting

### Check Logs
All API responses and outputs are saved. Check:
- `generation_output.log` - Certificate generation
- `compliance_response.json` - API responses
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
```

## Success Indicators

✅ **Script completed successfully** - All tests passed
✅ **Files generated** - Check Output folder structure
✅ **API responses saved** - All JSON files present
✅ **Certificates created** - .crt and .pem files exist
✅ **Summary files** - Human-readable summaries available

## Next Steps

After successful completion:
1. Review all generated summaries in each config folder
2. Verify API responses are successful
3. Production certificates are ready for live use
4. Keep all secrets and private keys secure
5. Deploy production certificates to your systems
