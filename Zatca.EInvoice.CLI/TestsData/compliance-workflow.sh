#!/bin/bash

# ZATCA Compliance Workflow Script
# This script performs the complete 3-phase certificate lifecycle:
# 1. Request Compliance Certificate (CSID)
# 2. Validate Compliance with test invoices
# 3. Request Production Certificate

set -e

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="$SCRIPT_DIR/Output"
CLI_DIR="$(dirname "$SCRIPT_DIR")"

# ZATCA API Credentials
USERNAME="m.abumusa@karage.co"
PASSWORD="@hR8C8FQTxm9#ge"

# NOTE: For sandbox/simulation testing, try common test OTP
# If this doesn't work, you must get a fresh OTP from ZATCA portal:
# Sandbox portal: https://sandbox.zatca.gov.sa/IntegrationSandbox
DEFAULT_OTP="123345"

# Default environment (sandbox uses developer-portal endpoint)
ENVIRONMENT="sandbox"

# Function to display usage
usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -c, --config NAME       Configuration name (e.g., csr-config-example-EN)"
    echo "  -e, --env ENV          Environment: sandbox|simulation|production (default: sandbox)"
    echo "  -o, --otp OTP          One-Time Password (REQUIRED - get from ZATCA portal)"
    echo "  -a, --all              Process ALL configurations in Output directory"
    echo "  -h, --help             Show this help message"
    echo ""
    echo "Examples:"
    echo "  # Test single configuration (OTP defaults to 123456):"
    echo "  $0 --config csr-config-example-EN"
    echo ""
    echo "  # Test ALL configurations (recommended):"
    echo "  $0 --all"
    echo ""
    echo "  # Custom OTP:"
    echo "  $0 --all --otp 654321"
    echo ""
    exit 1
}

# Function to request compliance certificate
request_compliance_cert() {
    local config_name="$1"
    local otp="$2"
    local env="$3"
    
    local config_dir="$OUTPUT_DIR/$config_name"
    local csr_file="$config_dir/certificate.csr"
    local cert_output_dir="$config_dir/compliance"
    
    echo ""
    echo "=========================================="
    echo -e "${BLUE}Phase 1: Request Compliance Certificate${NC}"
    echo "=========================================="
    echo -e "${CYAN}Configuration: $config_name${NC}"
    echo -e "${CYAN}Environment: $env${NC}"
    echo ""
    
    # Check if CSR exists
    if [ ! -f "$csr_file" ]; then
        echo -e "${RED}❌ Error: CSR not found at $csr_file${NC}"
        echo -e "${YELLOW}Run generate-and-verify-certs.sh first${NC}"
        return 1
    fi
    
    mkdir -p "$cert_output_dir"
    
    # Request compliance certificate using CLI
    echo -e "${YELLOW}Requesting compliance certificate...${NC}"
    
    cd "$CLI_DIR"
    
    if dotnet run --framework net9.0 -- api compliance-cert \
        --csr "$csr_file" \
        --otp "$otp" \
        --env "$env" \
        --output "$cert_output_dir" \
        --json > "$cert_output_dir/compliance_response.json" 2>&1; then
        
        # Parse response - extract JSON from output (skip informational messages)
        local success=$(grep -A 100 '^{' "$cert_output_dir/compliance_response.json" | jq -r '.success' 2>/dev/null || echo "false")
        
        if [ "$success" = "true" ]; then
            local request_id=$(grep -A 100 '^{' "$cert_output_dir/compliance_response.json" | jq -r '.requestId')
            
            # Save request_id for Phase 3
            echo "$request_id" > "$cert_output_dir/request_id.txt"
            
            # Save detailed response info
            cat > "$cert_output_dir/phase1_summary.txt" << EOF
Phase 1: Compliance Certificate Request
Completed: $(date)
Status: SUCCESS
Request ID: $request_id

Files Generated:
- compliance.crt (Compliance CSID Certificate)
- secret.txt (Compliance Secret)
- request_id.txt (Request ID for production cert)
- compliance_response.json (Full API Response)
EOF
            
            echo -e "${GREEN}✓ Compliance certificate received!${NC}"
            echo -e "${GREEN}  Request ID: $request_id${NC}"
            echo -e "${GREEN}  Certificate saved to: $cert_output_dir/compliance.crt${NC}"
            echo -e "${GREEN}  Secret saved to: $cert_output_dir/secret.txt${NC}"
            echo -e "${GREEN}  Request ID saved to: $cert_output_dir/request_id.txt${NC}"
            echo -e "${GREEN}  Summary saved to: $cert_output_dir/phase1_summary.txt${NC}"
            echo ""
            
            return 0
        else
            local error=$(grep -A 100 '^{' "$cert_output_dir/compliance_response.json" | jq -r '.error' 2>/dev/null || echo "Unknown error")
            echo -e "${RED}❌ Failed to get compliance certificate${NC}"
            echo -e "${RED}Error: $error${NC}"
            return 1
        fi
    else
        echo -e "${RED}❌ CLI command failed${NC}"
        return 1
    fi
}

# Function to generate and sign test invoice
generate_test_invoice() {
    local config_name="$1"
    local invoice_type="$2"
    local config_dir="$OUTPUT_DIR/$config_name"
    local compliance_dir="$config_dir/compliance"
    local invoice_dir="$compliance_dir/test_invoices"
    
    mkdir -p "$invoice_dir"
    
    local invoice_file="$invoice_dir/${invoice_type}_invoice.json"
    local xml_file="$invoice_dir/${invoice_type}_invoice.xml"
    local signed_file="$invoice_dir/${invoice_type}_invoice_signed.xml"
    
    echo -e "${YELLOW}Generating $invoice_type invoice...${NC}"
    
    cd "$CLI_DIR"
    
    # Generate sample invoice JSON
    dotnet run --framework net9.0 -- sample invoice \
        --type "$invoice_type" \
        --full \
        --output "$invoice_file" 2>&1 | tee "$invoice_dir/${invoice_type}_json_generation.log"
    
    if [ ! -f "$invoice_file" ]; then
        echo -e "${RED}❌ Failed to generate invoice JSON${NC}"
        return 1
    fi
    
    # Generate XML from JSON
    dotnet run --framework net9.0 -- invoice xml \
        --input "$invoice_file" \
        --output "$xml_file" 2>&1 | tee "$invoice_dir/${invoice_type}_xml_generation.log"
    
    if [ ! -f "$xml_file" ]; then
        echo -e "${RED}❌ Failed to generate XML${NC}"
        return 1
    fi
    
    # Sign the invoice
    local cert_file="$compliance_dir/compliance.crt"
    local key_file="$config_dir/private.pem"
    local pfx_file="$compliance_dir/compliance.pfx"
    
    # Create PFX if it doesn't exist
    if [ ! -f "$pfx_file" ]; then
        if ! openssl pkcs12 -export -out "$pfx_file" -inkey "$key_file" -in "$cert_file" -passout pass: > /dev/null 2>&1; then
            echo -e "${RED}❌ Failed to create PFX certificate${NC}"
            return 1
        fi
    fi
    
    if dotnet run --framework net9.0 -- invoice sign \
        --input "$xml_file" \
        --cert "$pfx_file" \
        --output-xml "$signed_file" \
        --output-hash "$invoice_dir/${invoice_type}_hash.txt" \
        --json 2>&1 | tee "$invoice_dir/${invoice_type}_sign_output.log" > "$invoice_dir/${invoice_type}_sign_result.json"; then
        
        local success=$(grep -A 100 '^{' "$invoice_dir/${invoice_type}_sign_result.json" | jq -r '.success' 2>/dev/null || echo "false")
        
        if [ "$success" = "true" ]; then
            local hash=$(grep -A 100 '^{' "$invoice_dir/${invoice_type}_sign_result.json" | jq -r '.hash')
            local uuid=$(grep -A 100 '^{' "$invoice_dir/${invoice_type}_sign_result.json" | jq -r '.uuid' 2>/dev/null || uuidgen)
            
            # Save for compliance check
            echo "$hash" > "$invoice_dir/${invoice_type}_hash.txt"
            echo "$uuid" > "$invoice_dir/${invoice_type}_uuid.txt"
            
            # Save signing summary
            cat > "$invoice_dir/${invoice_type}_signing_summary.txt" << EOF
Invoice Signing Summary
Type: $invoice_type
Completed: $(date)
Status: SUCCESS

Hash: $hash
UUID: $uuid

Files:
- ${invoice_type}_invoice.json (Input JSON)
- ${invoice_type}_invoice.xml (Unsigned XML)
- ${invoice_type}_invoice_signed.xml (Signed XML)
EOF
            
            echo -e "${GREEN}✓ Invoice signed successfully${NC}"
            echo -e "${GREEN}  Hash: $hash${NC}"
            echo -e "${GREEN}  UUID: $uuid${NC}"
            
            return 0
        else
            echo -e "${RED}❌ Failed to sign invoice${NC}"
            return 1
        fi
    else
        echo -e "${RED}❌ Sign command failed${NC}"
        return 1
    fi
}

# Function to validate compliance
validate_compliance() {
    local config_name="$1"
    local env="$2"
    
    local config_dir="$OUTPUT_DIR/$config_name"
    local compliance_dir="$config_dir/compliance"
    local invoice_dir="$compliance_dir/test_invoices"
    
    echo ""
    echo "=========================================="
    echo -e "${BLUE}Phase 2: Compliance Validation${NC}"
    echo "=========================================="
    echo -e "${CYAN}Configuration: $config_name${NC}"
    echo ""
    
    # Check if compliance certificate exists
    local cert_file="$compliance_dir/compliance.crt"
    local secret_file="$compliance_dir/secret.txt"
    
    if [ ! -f "$cert_file" ] || [ ! -f "$secret_file" ]; then
        echo -e "${RED}❌ Compliance certificate not found${NC}"
        return 1
    fi
    
    local secret=$(cat "$secret_file")
    
    # Test invoice types
    local test_types=("simplified" "standard")
    local passed=0
    local failed=0
    
    for invoice_type in "${test_types[@]}"; do
        echo ""
        echo -e "${CYAN}Testing $invoice_type invoice...${NC}"
        
        # Generate and sign invoice
        if ! generate_test_invoice "$config_name" "$invoice_type"; then
            echo -e "${RED}✗ Failed to generate $invoice_type invoice${NC}"
            failed=$((failed + 1))
            continue
        fi
        
        # Submit for compliance check
        local signed_file="$invoice_dir/${invoice_type}_invoice_signed.xml"
        local hash=$(cat "$invoice_dir/${invoice_type}_hash.txt")
        local uuid=$(cat "$invoice_dir/${invoice_type}_uuid.txt")
        
        echo -e "${YELLOW}Submitting for compliance validation...${NC}"
        
        cd "$CLI_DIR"
        
        if dotnet run --framework net9.0 -- api compliance-check \
            --input "$signed_file" \
            --hash "$hash" \
            --uuid "$uuid" \
            --cert "$cert_file" \
            --secret "$secret" \
            --env "$env" \
            --json 2>&1 | tee "$invoice_dir/${invoice_type}_compliance_output.log" > "$invoice_dir/${invoice_type}_compliance_result.json"; then
            
            local success=$(grep -A 100 '^{' "$invoice_dir/${invoice_type}_compliance_result.json" | jq -r '.success' 2>/dev/null || echo "false")
            
            if [ "$success" = "true" ]; then
                local status=$(grep -A 100 '^{' "$invoice_dir/${invoice_type}_compliance_result.json" | jq -r '.status')
                local warnings=$(grep -A 100 '^{' "$invoice_dir/${invoice_type}_compliance_result.json" | jq -r '.warningMessages[]?' 2>/dev/null || echo "None")
                
                # Save compliance check summary
                cat > "$invoice_dir/${invoice_type}_compliance_summary.txt" << EOF
Compliance Check Summary
Type: $invoice_type
Completed: $(date)
Status: PASSED
API Status: $status

Invoice Details:
- Hash: $hash
- UUID: $uuid

Warnings:
$warnings

Full response saved in: ${invoice_type}_compliance_result.json
EOF
                
                echo -e "${GREEN}✓ Compliance check PASSED${NC}"
                echo -e "${GREEN}  Status: $status${NC}"
                passed=$((passed + 1))
            else
                local error=$(grep -A 100 '^{' "$invoice_dir/${invoice_type}_compliance_result.json" | jq -r '.error' 2>/dev/null || echo "Unknown")
                local details=$(grep -A 100 '^{' "$invoice_dir/${invoice_type}_compliance_result.json" | jq -r '.validationResults.errorMessages[]?' 2>/dev/null | head -5)
                
                # Save failure summary
                cat > "$invoice_dir/${invoice_type}_compliance_summary.txt" << EOF
Compliance Check Summary
Type: $invoice_type
Completed: $(date)
Status: FAILED

Error: $error

Validation Errors:
$details

Full response saved in: ${invoice_type}_compliance_result.json
EOF
                
                echo -e "${RED}✗ Compliance check FAILED${NC}"
                echo -e "${RED}  Error: $error${NC}"
                if [ -n "$details" ]; then
                    echo -e "${RED}  Details: $details${NC}"
                fi
                failed=$((failed + 1))
            fi
        else
            echo -e "${RED}✗ Compliance check command failed${NC}"
            failed=$((failed + 1))
        fi
    done
    
    echo ""
    echo -e "Compliance Tests: ${GREEN}$passed passed${NC}, ${RED}$failed failed${NC}"
    
    # Save Phase 2 summary
    cat > "$compliance_dir/phase2_summary.txt" << EOF
Phase 2: Compliance Validation
Completed: $(date)
Configuration: $config_name

Test Results:
- Total Tests: $((passed + failed))
- Passed: $passed
- Failed: $failed

Test Details:
EOF
    
    for invoice_type in "${test_types[@]}"; do
        if [ -f "$invoice_dir/${invoice_type}_compliance_summary.txt" ]; then
            echo "" >> "$compliance_dir/phase2_summary.txt"
            echo "--- $invoice_type Invoice ---" >> "$compliance_dir/phase2_summary.txt"
            cat "$invoice_dir/${invoice_type}_compliance_summary.txt" >> "$compliance_dir/phase2_summary.txt"
        fi
    done
    
    if [ $failed -eq 0 ]; then
        echo -e "${GREEN}✓✓ All compliance tests passed!${NC}"
        echo -e "${GREEN}Summary saved to: $compliance_dir/phase2_summary.txt${NC}"
        return 0
    else
        echo -e "${RED}✗✗ Some compliance tests failed${NC}"
        echo -e "${YELLOW}Details saved to: $compliance_dir/phase2_summary.txt${NC}"
        return 1
    fi
}

# Function to request production certificate
request_production_cert() {
    local config_name="$1"
    local env="$2"
    
    local config_dir="$OUTPUT_DIR/$config_name"
    local compliance_dir="$config_dir/compliance"
    local production_dir="$config_dir/production"
    
    echo ""
    echo "=========================================="
    echo -e "${BLUE}Phase 3: Request Production Certificate${NC}"
    echo "=========================================="
    echo -e "${CYAN}Configuration: $config_name${NC}"
    echo ""
    
    # Check if compliance certificate exists
    local cert_file="$compliance_dir/compliance.crt"
    local secret_file="$compliance_dir/secret.txt"
    local request_id_file="$compliance_dir/request_id.txt"
    
    if [ ! -f "$cert_file" ] || [ ! -f "$secret_file" ] || [ ! -f "$request_id_file" ]; then
        echo -e "${RED}❌ Compliance certificate data not found${NC}"
        return 1
    fi
    
    local secret=$(cat "$secret_file")
    local request_id=$(cat "$request_id_file")
    
    mkdir -p "$production_dir"
    
    echo -e "${YELLOW}Requesting production certificate...${NC}"
    echo -e "${CYAN}Request ID: $request_id${NC}"
    
    cd "$CLI_DIR"
    
    if dotnet run --framework net9.0 -- api production-cert \
        --request-id "$request_id" \
        --cert "$cert_file" \
        --secret "$secret" \
        --env "$env" \
        --output "$production_dir" \
        --json > "$production_dir/production_response.json" 2>&1; then
        
        local success=$(grep -A 100 '^{' "$production_dir/production_response.json" | jq -r '.success' 2>/dev/null || echo "false")
        
        if [ "$success" = "true" ]; then
            # Save Phase 3 summary
            cat > "$production_dir/phase3_summary.txt" << EOF
Phase 3: Production Certificate Request
Completed: $(date)
Status: SUCCESS
Request ID Used: $request_id

Files Generated:
- production.crt (Production PCSID Certificate)
- production_secret.txt (Production Secret)
- production_response.json (Full API Response)

NOTE: This certificate should be used for production invoice submission.
EOF
            
            echo -e "${GREEN}✓ Production certificate received!${NC}"
            echo -e "${GREEN}  Certificate saved to: $production_dir/production.crt${NC}"
            echo -e "${GREEN}  Secret saved to: $production_dir/production_secret.txt${NC}"
            echo -e "${GREEN}  Summary saved to: $production_dir/phase3_summary.txt${NC}"
            echo ""
            return 0
        else
            local error=$(grep -A 100 '^{' "$production_dir/production_response.json" | jq -r '.error' 2>/dev/null || echo "Unknown error")
            echo -e "${RED}❌ Failed to get production certificate${NC}"
            echo -e "${RED}Error: $error${NC}"
            return 1
        fi
    else
        echo -e "${RED}❌ CLI command failed${NC}"
        return 1
    fi
}

# Function to process single configuration
process_configuration() {
    local config_name="$1"
    local otp="$2"
    local env="$3"
    
    echo ""
    echo "=========================================="
    echo -e "${MAGENTA}Processing Configuration: $config_name${NC}"
    echo "=========================================="
    
    # Phase 1: Request compliance certificate
    if ! request_compliance_cert "$config_name" "$otp" "$env"; then
        echo -e "${RED}✗✗ Failed at Phase 1: Compliance Certificate${NC}"
        return 1
    fi
    
    # Phase 2: Validate compliance
    if ! validate_compliance "$config_name" "$env"; then
        echo -e "${RED}✗✗ Failed at Phase 2: Compliance Validation${NC}"
        return 1
    fi
    
    # Phase 3: Request production certificate
    if ! request_production_cert "$config_name" "$env"; then
        echo -e "${RED}✗✗ Failed at Phase 3: Production Certificate${NC}"
        return 1
    fi
    
    # Save complete workflow summary
    local config_dir="$OUTPUT_DIR/$config_name"
    cat > "$config_dir/workflow_complete.txt" << EOF
========================================
ZATCA Compliance Workflow - COMPLETE
========================================
Configuration: $config_name
Completed: $(date)
Environment: $env

All Phases Completed Successfully:

✓ Phase 1: Compliance Certificate (CSID) - SUCCESS
  Location: $config_dir/compliance/
  
✓ Phase 2: Compliance Validation - SUCCESS
  Location: $config_dir/compliance/test_invoices/
  
✓ Phase 3: Production Certificate (PCSID) - SUCCESS
  Location: $config_dir/production/

Next Steps:
1. Review all generated files and API responses
2. Use production certificate for live invoice submission
3. Keep all certificates and secrets secure

File Structure:
$config_dir/
├── certificate.csr (Original CSR)
├── private.pem (Private Key)
├── compliance/
│   ├── compliance.crt (Compliance Certificate)
│   ├── secret.txt (Compliance Secret)
│   ├── request_id.txt (Request ID)
│   ├── phase1_summary.txt
│   ├── phase2_summary.txt
│   └── test_invoices/ (All test invoice files)
└── production/
    ├── production.crt (Production Certificate)
    ├── production_secret.txt (Production Secret)
    └── phase3_summary.txt
EOF
    
    echo ""
    echo -e "${GREEN}✓✓✓ Configuration $config_name completed successfully!${NC}"
    echo -e "${GREEN}Complete summary saved to: $config_dir/workflow_complete.txt${NC}"
    return 0
}

# Main script execution
main() {
    local config_name=""
    local otp=""
    local process_all=false
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            -c|--config)
                config_name="$2"
                shift 2
                ;;
            -e|--env)
                ENVIRONMENT="$2"
                shift 2
                ;;
            -o|--otp)
                otp="$2"
                shift 2
                ;;
            -a|--all)
                process_all=true
                shift
                ;;
            -h|--help)
                usage
                ;;
            *)
                echo -e "${RED}Unknown option: $1${NC}"
                usage
                ;;
        esac
    done
    
    # Use default OTP if not provided
    if [ -z "$otp" ]; then
        if [ -n "$DEFAULT_OTP" ]; then
            otp="$DEFAULT_OTP"
            echo -e "${YELLOW}ℹ Using default test OTP: $otp${NC}"
            echo -e "${YELLOW}  If this fails, get a fresh OTP from: https://sandbox.zatca.gov.sa/IntegrationSandbox${NC}"
            echo ""
        else
            echo -e "${RED}❌ Error: OTP is required${NC}"
            echo -e "${YELLOW}Get your OTP from: https://sandbox.zatca.gov.sa/IntegrationSandbox${NC}"
            echo ""
            usage
        fi
    fi
    
    # Validate inputs
    if [ "$process_all" = false ] && [ -z "$config_name" ]; then
        echo -e "${RED}❌ Error: Configuration name is required (or use --all)${NC}"
        usage
    fi
    
    # Check dependencies
    if ! command -v jq &> /dev/null; then
        echo -e "${RED}❌ Error: jq is required but not installed${NC}"
        echo -e "${YELLOW}Install with: apt-get install jq${NC}"
        exit 1
    fi
    
    if [ ! -f "$CLI_DIR/Zatca.EInvoice.CLI.csproj" ]; then
        echo -e "${RED}❌ Error: CLI project not found at $CLI_DIR${NC}"
        exit 1
    fi
    
    # Display banner
    echo "=========================================="
    echo -e "${MAGENTA}ZATCA Compliance Workflow${NC}"
    if [ "$process_all" = true ]; then
        echo -e "${MAGENTA}Testing ALL Certificate Configurations${NC}"
    fi
    echo "=========================================="
    echo -e "Environment: ${CYAN}$ENVIRONMENT${NC}"
    echo -e "Credentials: ${CYAN}$USERNAME${NC}"
    echo -e "OTP: ${CYAN}$otp${NC}"
    echo ""
    
    # Process configurations
    local total=0
    local passed=0
    local failed=0
    
    if [ "$process_all" = true ]; then
        # First, discover all configurations
        echo -e "${CYAN}Discovering configurations in $OUTPUT_DIR...${NC}"
        local configs=()
        for config_dir in "$OUTPUT_DIR"/csr-config-*; do
            if [ -d "$config_dir" ] && [ -f "$config_dir/certificate.csr" ]; then
                configs+=("$(basename "$config_dir")")
            fi
        done
        
        if [ ${#configs[@]} -eq 0 ]; then
            echo -e "${RED}❌ No configurations found in $OUTPUT_DIR${NC}"
            echo -e "${YELLOW}Run generate-and-verify-certs.sh first${NC}"
            exit 1
        fi
        
        echo -e "${GREEN}Found ${#configs[@]} configuration(s) to test:${NC}"
        for config in "${configs[@]}"; do
            echo -e "  - $config"
        done
        echo ""
        
        # Process all configurations
        for config_name in "${configs[@]}"; do
            total=$((total + 1))
            
            if process_configuration "$config_name" "$otp" "$ENVIRONMENT"; then
                passed=$((passed + 1))
            else
                failed=$((failed + 1))
                echo -e "${YELLOW}⚠ Continuing with remaining configurations...${NC}"
                echo ""
            fi
        done
    else
        # Process single configuration
        total=1
        if process_configuration "$config_name" "$otp" "$ENVIRONMENT"; then
            passed=1
        else
            failed=1
        fi
    fi
    
    # Final summary
    echo ""
    echo "=========================================="
    echo -e "${MAGENTA}Final Summary${NC}"
    echo "=========================================="
    echo -e "Total configurations: ${BLUE}$total${NC}"
    echo -e "Passed: ${GREEN}$passed${NC}"
    echo -e "Failed: ${RED}$failed${NC}"
    echo ""
    
    if [ $failed -eq 0 ]; then
        echo -e "${GREEN}✓✓✓ All workflows completed successfully!${NC}"
        exit 0
    else
        echo -e "${RED}✗✗✗ Some workflows failed${NC}"
        exit 1
    fi
}

# Run main function
main "$@"
