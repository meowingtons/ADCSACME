# xACME
Add ACME protocol support to a Microsoft PKI (Active Directory Certificate Services)

Get all the benefits of LetsEncrypt or any other CA with ACME support, with an existing Microsoft CA. This ASP.NET Core Web API implements the majority of the ACMEv2 Protocol draft and should support any compliant client. I test it with posh-acme.

Requirements:

- .NET 4.6.1
- One of the following databases
  - MariaDB/MySQL
  - PostgreSQL
  - SQLite
  - MSSQL
- IIS or other reverse proxy
- A Microsoft CA
  - Tested with ADCS on Server 2012r2, Server 2016, and Server 2019
  - Requires an enterprise CA
  - A special certificate template (details outlined below)
