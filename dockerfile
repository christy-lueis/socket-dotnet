#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /usr/src/app

# Copy over the drivers
# MySQL ODBC Ansi
COPY drivers/mysql-connector-odbc-8.0.19-linux-ubuntu19.10-x86-64bit.tar.gz .

RUN apt-get update

# unixODBC
# RUN apt-get install -y unixodbc-bin unixodbc
RUN apt-get install -y unixodbc unixodbc


RUN tar xvzf  mysql-connector-odbc-8.0.19-linux-ubuntu19.10-x86-64bit.tar.gz \
    && ./mysql-connector-odbc-8.0.19-linux-ubuntu19.10-x86-64bit/bin/myodbc-installer -d -a -n "MySQL ODBC 8.0 Unicode Driver" \
        -t "DRIVER=/usr/src/app/mysql-connector-odbc-8.0.19-linux-ubuntu19.10-x86-64bit/lib/libmyodbc8w.so"

#  Make sure links are set
RUN ldconfig
# 4. Use the myodbc-installer to install the driver
RUN ./mysql-connector-odbc-8.0.19-linux-ubuntu19.10-x86-64bit/bin/myodbc-installer -d -a -n "Oracle 11.2" -t "DRIVER=/usr/lib/oracle/11.2/client64/lib/libsqora.so.11.1"

EXPOSE 80
EXPOSE 443


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OneCareAfrica.MicroServices.csproj", "."]
RUN dotnet restore "./OneCareAfrica.MicroServices.csproj"
COPY . .
RUN dotnet build "OneCareAfrica.MicroServices.csproj" -c Release -o /app/build \
    && dotnet publish "OneCareAfrica.MicroServices.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY drivers/mysql-connector-odbc-8.0.19-linux-ubuntu19.10-x86-64bit.tar.gz /usr/src/app/
RUN cd /usr/src/app \
    && apt-get update \
    && apt-get install -y  unixodbc unixodbc libqt5webkit5 libqt5xml5 wkhtmltopdf \
    && tar xvzf  mysql-connector-odbc-8.0.19-linux-ubuntu19.10-x86-64bit.tar.gz \
    && ./mysql-connector-odbc-8.0.19-linux-ubuntu19.10-x86-64bit/bin/myodbc-installer -d -a -n "MySQL ODBC 8.0 Unicode Driver" \
        -t "DRIVER=/usr/src/app/mysql-connector-odbc-8.0.19-linux-ubuntu19.10-x86-64bit/lib/libmyodbc8w.so" \
    && ldconfig \
    && ./mysql-connector-odbc-8.0.19-linux-ubuntu19.10-x86-64bit/bin/myodbc-installer -d -a -n "Oracle 11.2" -t "DRIVER=/usr/lib/oracle/11.2/client64/lib/libsqora.so.11.1" 
    

EXPOSE 80
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/entrypoint.sh .

RUN chmod +x entrypoint.sh \
    && rm -f /app/wkhtmltopdf/linux/x64/wkhtmltopdf && ln -s /usr/bin/wkhtmltopdf /app/wkhtmltopdf/linux/x64/wkhtmltopdf

ENTRYPOINT ["./entrypoint.sh"]
