# Git Hooks Setup

To ensure code quality and consistency across the team, we use Git Hooks (pre-commit) to run linting and build checks automatically.

## Standard Setup (Husky)

We recommend using **Husky** to manage git hooks.

### Prerequisites
- Node.js installed

### Installation

1. Initialize husky:
   ```bash
   npx husky-init && npm install
   ```

2. Add a pre-commit hook to run dotnet build:
   ```bash
   npx husky add .husky/pre-commit "dotnet build"
   ```

## Manual Setup (No Node.js)

If you don't want to use Node.js, you can manually create a pre-commit hook.

1. Create a file named `pre-commit` (no extension) in `.git/hooks/`.
2. Add the following content:
   ```bash
   #!/bin/sh
   echo "Running pre-commit checks..."
   dotnet build
   if [ $? -ne 0 ]; then
     echo "Build failed. Commit aborted."
     exit 1
   fi
   ```
3. Make it executable:
   ```bash
   chmod +x .git/hooks/pre-commit
   ```

## Rules
- Never use `--no-verify` to bypass hooks unless explicitly required for an emergency hotfix.
- All commits must pass the build and linting checks.
