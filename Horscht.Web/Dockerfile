FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
# Setting the work directory for our app
WORKDIR /Horscht

# We copy the .csproj of our app to root and
# restore the dependencies of the project.
COPY ["Directory.Packages.props", "."]
COPY ["Horscht.Web/Horscht.Web.csproj", "Horscht.Web/"]
COPY ["Horscht.Logic/Horscht.Logic.csproj", "Horscht.Logic/"]
COPY ["Horscht.App/Horscht.App.csproj", "Horscht.App/"]
COPY ["Horscht.Contracts/Horscht.Contracts.csproj", "Horscht.Contracts/"]

RUN dotnet restore "./Horscht.Web/Horscht.Web.csproj"

COPY [".", "."]

# We proceed by copying all the contents in
# the main project folder to root and build it
#COPY / .
RUN dotnet build "Horscht.Web/Horscht.Web.csproj" -c Release --no-restore

# Once we're done building, we'll publish the project
# to the publish folder
FROM build-env AS publish
RUN dotnet publish "Horscht.Web/Horscht.Web.csproj" -c Release -o /publish --no-restore --no-build

# We then get the base image for Nginx and set the
# work directory
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# We'll copy all the contents from wwwroot in the publish
# folder into nginx/html for nginx to serve. The destination
# should be the same as what you set in the nginx.conf.
COPY --from=publish /publish/wwwroot /usr/local/webapp/nginx/html
COPY "Horscht.Web/nginx.conf" /etc/nginx/nginx.conf