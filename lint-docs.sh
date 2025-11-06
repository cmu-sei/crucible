#!/bin/bash

DOCS_DIRECTORY=/workspaces/crucible-docs

# --- Color codes ---
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[1;34m'
BOLD='\033[1m'
RESET='\033[0m'

# --- Vale Lint ---
echo -e "\n${BLUE}${BOLD}## Linting ${DOCS_DIRECTORY} using Vale${RESET}\n"

vale "$DOCS_DIRECTORY" --config "$DOCS_DIRECTORY/.vale.ini"
vale_exit=$?

# --- Markdownlint Lint ---
echo -e "\n${BLUE}${BOLD}## Linting ${DOCS_DIRECTORY} using markdownlint-cli2${RESET}\n"

markdownlint-cli2 "$DOCS_DIRECTORY/**/*.md" --config "$DOCS_DIRECTORY/.markdownlint-cli2.yaml"
markdownlint_exit=$?

# --- Results summary ---
echo -e "\n${BOLD}=============================="
echo -e "        LINTING SUMMARY"
echo -e "==============================${RESET}\n"

if [[ $vale_exit -eq 0 && $markdownlint_exit -eq 0 ]]; then
    echo -e "${GREEN}✅ No errors were found with vale or markdownlint-cli2.${RESET}"
else
    if [[ $vale_exit -ne 0 ]]; then
        echo -e "${RED}❌ Vale exited with status code ${vale_exit}.${RESET}"
        echo -e "   ${YELLOW}Review output and fix errors.${RESET}"
    else
        echo -e "${GREEN}✔ Vale passed successfully.${RESET}"
    fi

    if [[ $markdownlint_exit -ne 0 ]]; then
        echo -e "${RED}❌ markdownlint-cli2 exited with status code ${markdownlint_exit}.${RESET}"
        echo -e "   ${YELLOW}Review output and fix errors.${RESET}"
    else
        echo -e "${GREEN}✔ markdownlint-cli2 passed successfully.${RESET}"
    fi
fi

echo -e ""
