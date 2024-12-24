# AutoGenerationModels

**AutoGenerationModels** is a tool that automatically generates C# models from SQL Server stored procedures (SP) and table-valued functions (TVF). It simplifies the process of creating model classes, saving developers time by eliminating the need to manually write the code for mapping database query results to C# models.


## Features

- **Generate C# models from stored procedures and table-valued functions**: The tool automatically creates model classes based on query results from the database.
- **Supports multiple SQL data types**: It automatically maps SQL data types to their corresponding C# data types.
- **Easy to use**: Simply input the name of a stored procedure or TVF, and the tool will generate the model for you.

## Installation

To install this tool, you can use the NuGet package manager or the `dotnet` command line.

### Install from NuGet

**Install Globally**
```bash
dotnet tool install --global AutoGenerationModels
```

**Install Locally**

```bash
dotnet new tool-manifest # if you are setting up this repo

dotnet tool install --local AutoGenerationModels
```
