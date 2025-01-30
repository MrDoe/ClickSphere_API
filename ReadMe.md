# ClickSphere API

## Description

ClickSphere API is a middleware application that acts as a bridge between the ClickHouse database and other applications. It is developed using .NET 9 and provides a RESTful interface for querying the database. Compared to the standard HTTP interface, it offers higher level procedures for querying ClickHouse. This API enables seamless integration and interaction with the ClickSphere database in other applications, allowing authenticated users to retrieve data and perform various operations.

Please note that this API is not feature complete and still in development and therefore should not be used in productive environments.

## Installation

To install and run the ClickSphere_API application, follow these steps:

1. Clone the repository: `git clone https://github.com/MrDoe/ClickSphere_API.git`
2. Navigate to the project directory: `cd ClickSphere_API`
3. Install dependencies: `dotnet restore`
4. Build the project: `dotnet build`
5. Edit the configuration file `appsettings.json` to configure your ClickHouse server address, port, user and default database.
6. Execute `dotnet user-secrets init` and `dotnet user-secrets set "ClickHouse:Password" [YourClickHousePassword]`.
7. Run the application: `dotnet run`

## Usage

To use the ClickSphere_API application, follow these steps:

1. Make sure the ClickHouse server and ClickSphere_API is running.
2. Send HTTP requests to the appropriate endpoints to interact with ClickSphere (see the Tests folder for examples).

## Contributing

Contributions are welcome! If you would like to contribute to the ClickSphere API project, please follow these guidelines:

1. Fork the repository.
2. Create a new branch: `git checkout -b my-feature`
3. Make your changes and commit them: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin my-feature`
5. Submit a pull request.

## License

This project is licensed under the [MIT License](LICENSE).

## Contact

If you have any questions or suggestions regarding the ClickSphere API application, please contact Christoph Döllinger at <a href="mailto:christoph.doellinger&#64;med.uni-heidelberg.de">christoph.doellinger&#64;med.uni-heidelberg.de</a>.
