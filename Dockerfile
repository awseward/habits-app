FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
COPY deploy /
WORKDIR /HabitsApp
EXPOSE 8085
CMD ["dotnet", "HabitsApp.dll"]
