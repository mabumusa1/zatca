#!/bin/bash
set -e

export PATH="$PATH:/home/ubuntu/.dotnet/tools"

# Check if SONARQUBE_TOKEN is set
if [[ -z "$SONARQUBE_TOKEN" ]]; then
  echo "Error: SONARQUBE_TOKEN environment variable is not set" >&2
  exit 1
fi

echo "Starting SonarQube analysis..."
dotnet-sonarscanner begin \
  /k:"mabumusa1_zatca" \
  /o:"mabumusa" \
  /d:sonar.host.url="https://sonarcloud.io" \
  /d:sonar.token="$SONARQUBE_TOKEN" \
  /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

echo "Building project..."
dotnet build Zatca.EInvoice.slnx --no-incremental

echo "Running tests with coverage..."
dotnet test Zatca.EInvoice.slnx --no-build \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

echo "Completing analysis..."
dotnet-sonarscanner end /d:sonar.token="$SONARQUBE_TOKEN"

echo "Analysis complete!"
