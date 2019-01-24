# Example usage of containers with MassTransit

These samples show how to use various containers with MassTransit.

Usage includes:

- Consumers
- State Machine Sagas
    + Including custom activities (w/dependencies)

Additional features may also be demonstrated in the future, such as routing slip activities.

The samples use the in-memory transport, to avoid any infrastructure dependencies and can be run straight out of the box.

To see the interaction between the consumers, sagas, etc., type `submit` once the sample is started.

## Requirements

The samples are built using .NET Standard 2.2 and .NET Core App 2.2, so you'll need to have the .NET Core 2.2 SDK installed to run them (use `dotnet run`) to execute).

