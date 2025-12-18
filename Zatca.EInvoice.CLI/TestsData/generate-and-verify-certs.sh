#!/bin/bash

# ZATCA Certificate Generation and Verification Script
# This script processes all .properties files in the Input folder,
# generates certificates for each, and verifies them

set -e

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INPUT_DIR="$SCRIPT_DIR/Input"
OUTPUT_DIR="$SCRIPT_DIR/Output"
CLI_DIR="$(dirname "$SCRIPT_DIR")"

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

# Function to parse properties file
parse_property() {
    local file="$1"
    local key="$2"
    grep "^${key}=" "$file" | cut -d'=' -f2-
}

# Function to verify CSR
verify_csr() {
    local csr_file="$1"
    local key_file="$2"
    local config_name="$3"
    
    echo -e "${CYAN}Verifying certificate: $config_name${NC}"
    echo "----------------------------------------"
    
    # Check if files exist
    if [ ! -f "$csr_file" ]; then
        echo -e "${RED}❌ Error: $csr_file not found${NC}"
        return 1
    fi
    
    if [ ! -f "$key_file" ]; then
        echo -e "${RED}❌ Error: $key_file not found${NC}"
        return 1
    fi
    
    # Verify CSR format
    if ! openssl req -in "$csr_file" -noout -text > /dev/null 2>&1; then
        echo -e "${RED}❌ Invalid CSR format${NC}"
        return 1
    fi
    
    # Verify private key format
    if ! openssl ec -in "$key_file" -noout -text > /dev/null 2>&1; then
        echo -e "${RED}❌ Invalid private key format${NC}"
        return 1
    fi
    
    # Verify key pair match
    CSR_PUBKEY=$(openssl req -in "$csr_file" -noout -pubkey)
    KEY_PUBKEY=$(openssl ec -in "$key_file" -pubout 2>/dev/null)
    
    if [ "$CSR_PUBKEY" = "$KEY_PUBKEY" ]; then
        echo -e "${GREEN}✓ Public keys match - CSR and private key are a valid pair${NC}"
    else
        echo -e "${RED}❌ Public keys do NOT match - Invalid key pair${NC}"
        return 1
    fi
    
    # Extract and display subject
    SUBJECT=$(openssl req -in "$csr_file" -noout -subject)
    echo -e "${GREEN}✓ Subject: $SUBJECT${NC}"
    
    # Display key details
    KEY_DETAILS=$(openssl ec -in "$key_file" -noout -text 2>&1 | head -n 2)
    echo -e "${GREEN}✓ Private key: Valid EC key${NC}"
    
    echo -e "${GREEN}✓ Certificate verified successfully${NC}"
    echo ""
    return 0
}

# Function to generate certificate from properties file
generate_certificate() {
    local props_file="$1"
    local config_name=$(basename "$props_file" .properties)
    
    echo ""
    echo "=========================================="
    echo -e "${BLUE}Processing: $config_name${NC}"
    echo "=========================================="
    
    # Parse properties
    local common_name=$(parse_property "$props_file" "csr.common.name")
    local serial_number=$(parse_property "$props_file" "csr.serial.number")
    local org_identifier=$(parse_property "$props_file" "csr.organization.identifier")
    local org_unit=$(parse_property "$props_file" "csr.organization.unit.name")
    local org_name=$(parse_property "$props_file" "csr.organization.name")
    local country=$(parse_property "$props_file" "csr.country.name")
    local invoice_type=$(parse_property "$props_file" "csr.invoice.type")
    local address=$(parse_property "$props_file" "csr.location.address")
    local category=$(parse_property "$props_file" "csr.industry.business.category")
    
    # Create output subdirectory for this configuration
    local config_output_dir="$OUTPUT_DIR/$config_name"
    mkdir -p "$config_output_dir"
    
    local csr_file="$config_output_dir/certificate.csr"
    local key_file="$config_output_dir/private.pem"
    
    # Extract solution name, model, and serial from serial.number
    # Format: 1-TST|2-TST|3-ed22f1d8-e6a2-1118-9b58-d9a8f11e445f
    local solution_name=$(echo "$serial_number" | cut -d'|' -f1 | cut -d'-' -f2)
    local model=$(echo "$serial_number" | cut -d'|' -f2 | cut -d'-' -f2)
    local device_serial=$(echo "$serial_number" | cut -d'|' -f3 | cut -d'-' -f2-)
    
    echo -e "${YELLOW}Configuration details:${NC}"
    echo "  Common Name: $common_name"
    echo "  Org ID: $org_identifier"
    echo "  Org Name: $org_name"
    echo "  Org Unit: $org_unit"
    echo "  Country: $country"
    echo "  Invoice Type: $invoice_type"
    echo "  Address: $address"
    echo "  Category: $category"
    echo "  Solution: $solution_name"
    echo "  Model: $model"
    echo "  Device Serial: $device_serial"
    echo ""
    
    # Run the CLI to generate certificate
    echo -e "${YELLOW}Generating certificate...${NC}"
    
    cd "$CLI_DIR"
    
    if dotnet run -- cert generate \
        --org-id "$org_identifier" \
        --solution "$solution_name" \
        --model "$model" \
        --serial "$device_serial" \
        --name "$common_name" \
        --country "$country" \
        --org-name "$org_name" \
        --org-unit "$org_unit" \
        --address "$address" \
        --invoice-type "$invoice_type" \
        --category "$category" \
        --output-dir "$config_output_dir" \
        --csr-file "certificate.csr" \
        --key-file "private.pem"; then
        
        echo -e "${GREEN}✓ Certificate generated successfully${NC}"
        echo ""
        
        # Verify the generated certificate
        if verify_csr "$csr_file" "$key_file" "$config_name"; then
            echo -e "${GREEN}✓✓ $config_name: PASSED${NC}"
            return 0
        else
            echo -e "${RED}✗✗ $config_name: VERIFICATION FAILED${NC}"
            return 1
        fi
    else
        echo -e "${RED}✗ Failed to generate certificate${NC}"
        return 1
    fi
}

# Main script execution
echo "=========================================="
echo "ZATCA Certificate Generation & Verification"
echo "=========================================="
echo ""
echo "Script Directory: $SCRIPT_DIR"
echo "Input Directory: $INPUT_DIR"
echo "Output Directory: $OUTPUT_DIR"
echo "CLI Directory: $CLI_DIR"
echo ""

# Check if CLI project exists
if [ ! -f "$CLI_DIR/Zatca.EInvoice.CLI.csproj" ]; then
    echo -e "${RED}❌ Error: CLI project not found at $CLI_DIR${NC}"
    exit 1
fi

# Counter for statistics
total=0
passed=0
failed=0

# Process all .properties files in Input directory, excluding template
for props_file in "$INPUT_DIR"/*.properties; do
    # Check if file exists (in case no .properties files are found)
    if [ ! -f "$props_file" ]; then
        echo -e "${RED}No .properties files found in $INPUT_DIR${NC}"
        exit 1
    fi
    
    # Get filename
    filename=$(basename "$props_file")
    
    # Skip the template file
    if [[ "$filename" == "csr-config-template.properties" ]]; then
        echo -e "${YELLOW}Skipping template: $filename${NC}"
        continue
    fi
    
    total=$((total + 1))
    
    # Generate and verify certificate
    if generate_certificate "$props_file"; then
        passed=$((passed + 1))
    else
        failed=$((failed + 1))
    fi
done

# Summary
echo ""
echo "=========================================="
echo "Summary"
echo "=========================================="
echo -e "Total configurations: ${BLUE}$total${NC}"
echo -e "Passed: ${GREEN}$passed${NC}"
echo -e "Failed: ${RED}$failed${NC}"
echo ""

if [ $failed -eq 0 ]; then
    echo -e "${GREEN}✓✓ All certificates generated and verified successfully!${NC}"
    exit 0
else
    echo -e "${RED}✗✗ Some certificates failed verification${NC}"
    exit 1
fi
