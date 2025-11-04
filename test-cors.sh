#!/bin/bash

# NOL Car Rental API - CORS Test Script
# Usage: bash test-cors.sh

API_URL="https://nolrental.runasp.net/api/admin/cars?sortBy=createdAt&sortOrder=desc&page=1&pageSize=10"

echo "🌐 NOL Car Rental API - CORS Test"
echo "=================================="
echo ""
echo "Testing URL: $API_URL"
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test 1: Simple GET with CORS
echo "🧪 Test 1: Simple GET Request (CORS)"
echo "------------------------------------"
response=$(curl -s -w "\n%{http_code}" -H "Origin: https://example.com" "$API_URL")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" == "200" ]; then
    echo -e "${GREEN}✅ SUCCESS${NC}"
    echo "Status Code: $http_code"
    echo "Response (first 300 chars):"
    echo "$body" | head -c 300
    echo "..."
else
    echo -e "${RED}❌ FAILED${NC}"
    echo "Status Code: $http_code"
    echo "Response: $body"
fi
echo ""
echo ""

# Test 2: Check CORS Headers
echo "🧪 Test 2: Check CORS Headers"
echo "------------------------------"
cors_headers=$(curl -s -I -H "Origin: https://example.com" "$API_URL" | grep -i "access-control")

if [ -n "$cors_headers" ]; then
    echo -e "${GREEN}✅ CORS Headers Present${NC}"
    echo "$cors_headers"
else
    echo -e "${RED}❌ No CORS Headers Found${NC}"
fi
echo ""
echo ""

# Test 3: OPTIONS Preflight Request
echo "🧪 Test 3: OPTIONS Preflight Request"
echo "-------------------------------------"
options_response=$(curl -s -I -X OPTIONS \
    -H "Origin: https://example.com" \
    -H "Access-Control-Request-Method: GET" \
    -H "Access-Control-Request-Headers: Content-Type, Authorization" \
    "$API_URL")

echo "$options_response" | head -n 1
cors_options=$(echo "$options_response" | grep -i "access-control")

if [ -n "$cors_options" ]; then
    echo -e "${GREEN}✅ Preflight Successful${NC}"
    echo "$cors_options"
else
    echo -e "${RED}❌ Preflight Failed${NC}"
fi
echo ""
echo ""

# Test 4: GET with Custom Headers
echo "🧪 Test 4: GET with Custom Headers"
echo "-----------------------------------"
custom_response=$(curl -s -w "\n%{http_code}" \
    -H "Origin: https://example.com" \
    -H "Content-Type: application/json" \
    -H "Accept-Language: ar" \
    "$API_URL")

custom_code=$(echo "$custom_response" | tail -n1)
custom_body=$(echo "$custom_response" | sed '$d')

if [ "$custom_code" == "200" ]; then
    echo -e "${GREEN}✅ SUCCESS with Custom Headers${NC}"
    echo "Status Code: $custom_code"
    echo "Headers sent: Content-Type, Accept-Language"
else
    echo -e "${RED}❌ FAILED${NC}"
    echo "Status Code: $custom_code"
fi
echo ""
echo ""

# Test 5: Detailed CORS Header Check
echo "🧪 Test 5: Detailed CORS Header Analysis"
echo "-----------------------------------------"
full_headers=$(curl -s -I -H "Origin: https://example.com" "$API_URL")

echo "Checking for required CORS headers:"
echo ""

if echo "$full_headers" | grep -qi "access-control-allow-origin"; then
    echo -e "${GREEN}✅${NC} Access-Control-Allow-Origin: $(echo "$full_headers" | grep -i "access-control-allow-origin" | cut -d: -f2-)"
else
    echo -e "${RED}❌${NC} Access-Control-Allow-Origin: NOT FOUND"
fi

if echo "$full_headers" | grep -qi "access-control-allow-methods"; then
    echo -e "${GREEN}✅${NC} Access-Control-Allow-Methods: $(echo "$full_headers" | grep -i "access-control-allow-methods" | cut -d: -f2-)"
else
    echo -e "${YELLOW}⚠️${NC}  Access-Control-Allow-Methods: NOT FOUND"
fi

if echo "$full_headers" | grep -qi "access-control-allow-headers"; then
    echo -e "${GREEN}✅${NC} Access-Control-Allow-Headers: $(echo "$full_headers" | grep -i "access-control-allow-headers" | cut -d: -f2-)"
else
    echo -e "${YELLOW}⚠️${NC}  Access-Control-Allow-Headers: NOT FOUND"
fi

if echo "$full_headers" | grep -qi "access-control-max-age"; then
    echo -e "${GREEN}✅${NC} Access-Control-Max-Age: $(echo "$full_headers" | grep -i "access-control-max-age" | cut -d: -f2-)"
else
    echo -e "${YELLOW}⚠️${NC}  Access-Control-Max-Age: NOT FOUND"
fi

echo ""
echo ""

# Summary
echo "📊 Summary"
echo "=========="
if [ "$http_code" == "200" ] && [ -n "$cors_headers" ]; then
    echo -e "${GREEN}✅ CORS is working correctly!${NC}"
    echo ""
    echo "Your API accepts cross-origin requests from any domain."
    echo "You can now connect from your frontend application."
else
    echo -e "${RED}❌ CORS may not be fully configured${NC}"
    echo ""
    echo "Check the following:"
    echo "1. Verify CORS is configured in Program.cs"
    echo "2. Verify CORS headers in web.config"
    echo "3. Ensure app.UseCors() is called in Program.cs"
    echo "4. Check if the API is accessible"
fi

echo ""
echo "🔗 Test in browser: file://$(pwd)/test-cors.html"
echo "📊 Check logs: https://seq-production-43df.up.railway.app/"
echo ""

