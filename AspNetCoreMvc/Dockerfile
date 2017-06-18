FROM microsoft/dotnet:runtime
EXPOSE 60000
WORKDIR /var/www/c4d_aspnetcore_tutorial
COPY publish .
ENTRYPOINT ["dotnet", "C4D.AspNetCore.Tutorial.dll"]