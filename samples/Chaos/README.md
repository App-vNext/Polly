# Chaos Example

This example demonstrates how to use new [chaos engineering](https://www.pollydocs.org/chaos) tools in Polly to inject chaos into HTTP client communication.
The HTTP client communicates with `https://jsonplaceholder.typicode.com/todos` endpoint.

To test the application:

- Run the app using `dotnet run` command.
- Access the root endpoint `https://localhost:62683` and refresh it multiple times.
- Observe the logs in out console window. You should see chaos injection and also mitigation of chaos by resilience strategies.
