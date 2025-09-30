# 基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 9001

## 构建阶段
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#
## 复制 .csproj 文件
#COPY ["../EasyAdmin.Web/EasyAdmin.Web.csproj", "EasyAdmin.Web/"]
#COPY ["../EasyAdmin.Application/EasyAdmin.Application.csproj", "EasyAdmin.Application/"]
#COPY ["../EasyAdmin.Domain/EasyAdmin.Domain.csproj", "EasyAdmin.Domain/"]
#COPY ["../EasyAdmin.Infrastructure/EasyAdmin.Infrastructure.csproj", "EasyAdmin.Infrastructure/"]
#
## 恢复依赖
#RUN dotnet restore "EasyAdmin.Web/EasyAdmin.Web.csproj"
#
## 复制其余文件
#COPY .. .
#WORKDIR "/src/EasyAdmin.Web"
#RUN dotnet build "EasyAdmin.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## 发布阶段
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "EasyAdmin.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## 最终阶段
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "EasyAdmin.Web.dll"]


# 设置环境变量
ENV ASPNETCORE_URLS=http://*:9001

COPY . .

ENTRYPOINT ["dotnet", "EasyAdmin.Web.dll"]
