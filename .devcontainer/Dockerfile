FROM mcr.microsoft.com/vscode/devcontainers/dotnet:5.0

RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && nvm install lts/* 2>&1"

RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g cspell" 2>&1
