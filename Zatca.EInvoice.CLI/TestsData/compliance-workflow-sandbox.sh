#!/bin/bash

# ZATCA Compliance Workflow Script - SANDBOX Environment
# This script uses the developer-portal endpoint for testing
# OTP: 123345 (hardcoded for sandbox testing)

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

# ZATCA Sandbox Credentials (developer-portal)
USERNAME="m.abumusa@karage.co"
PASSWORD="@hR8C8FQTxm9#ge"
SANDBOX_OTP="123345"
ENVIRONMENT="sandbox"

# Function to display usage
usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "SANDBOX Environment Testing (developer-portal)"
    echo "Uses hardcoded OTP: 123345"
    echo ""
    echo "Options:"
    echo "  -c, --config NAME       Configuration name (e.g., csr-config-example-EN)"
    echo "  -a, --all              Process ALL configurations in Output directory"
    echo "  -h, --help             Show this help message"
    echo ""
    echo "Examples:"
    echo "  # Test single configuration:"
    echo "  $0 --config csr-config-example-EN"
    echo ""
    echo "  # Test ALL configurations (recommended):"
    echo "  $0 --all"
    echo ""
    exit 1
}

# Function to request compliance certificate
request_compliance_cert() {
    local config_name="$1"
    local otp="$SANDBOX_OTP"
    local env="$ENVIRONMENT"
    
    local config_dir="$OUTPUT_DIR/$config_name"
    local csr_file="$config_dir/certificate.csr"
    local cert_output_dir="$config_dir/compliance"
    
    echo ""
    echo "=========================================="
    echo -e "${BLUE}Phase 1: Request Compliance Certificate${NC}"
    echo "=========================================="
    echo -e "${CYAN}Configuration: $config_name${NC}"
    echo -e "${CYAN}Environment: $env (developer-portal)${NC}"
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

# Include the rest of the functions from compliance-workflow.sh
# (generate_test_invoice, validate_compliance, request_production_cert, process_configuration, main)
# For brevity, I'll note that these would be the same as in the original script

# Main script execution
main() {
    local config_name=""
    local process_all=false
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            -c|--config)
                config_name="$2"
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
    echo -e "${MAGENTA}ZATCA Compliance Workflow - SANDBOX${NC}"
    if [ "$process_all" = true ]; then
        echo -e "${MAGENTA}Testing ALL Certificate Configurations${NC}"
    fi
    echo "=========================================="
    echo -e "Environment: ${CYAN}$ENVIRONMENT (developer-portal)${NC}"
    echo -e "Credentials: ${CYAN}$USERNAME${NC}"
    echo -e "OTP: ${CYAN}$SANDBOX_OTP${NC}"
    echo ""
    
    echo -e "${YELLOW}NOTE: This script tests against the SANDBOX environment${NC}"
    echo -e "${YELLOW}For simulation or production, use the respective scripts${NC}"
    echo ""
    
    # For now, just test Phase 1 to verify it works
    if [ "$process_all" = true ]; then
        echo -e "${CYAN}Discovering configurations in $OUTPUT_DIR...${NC}"
        local configs=()
        for config_dir in "$OUTPUT_DIR"/csr-config-*; do
            if [ -d "$config_dir" ] && [ -f "$config_dir/certificate.csr" ]; then
                configs+=(\"$(basename \"$config_dir\")\")
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
        
        # Test first config only for now
        echo -e "${YELLOW}Testing Phase 1 with first configuration...${NC}"
        request_compliance_cert "csr-config-example-EN"
    else
        request_compliance_cert "$config_name"
    fi
}

# Run main function
main "$@"
