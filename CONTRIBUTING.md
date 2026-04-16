# Contributing

## How to use this file

Use this file to follow our coding guidelines when submitting to this repo.

## Table of Contents

- [How to use this file](#how-to-use-this-file)
- [First time contributors](#first-time-contributors)
    - [Quick setup](#quick-setup)
    - [Key guidelines](#key-guidelines)

## First time contributors

Welcome! Here's a quick guide to get you started:

### Quick setup

1. Fork and clone the repository
2. Ensure you have .NET 10 SDK or later installed on your machine (check with `dotnet --version`)
3. Build the solution (`dotnet build`)
4. Run tests (`dotnet test`)

### Key guidelines

- Use TDD to write tests where possible (write test first, then implement)
- Add XML docs to all public APIs
- Strictly use Microsoft's naming conventions
- Stricly use Conventional Commits
- Use modern C#, we don't care about .NET Framework (currently we target .NET 10 and C# 14)

----

## Code style

- Stricly
  use [Microsoft's C# naming conventions for identifiers](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)
    - Follow [Microsoft's C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
    - DO NOT use Microsoft's Framework Design Guidelines. They are not idiomatic and outdated.

- Enable nullable on projects:
    - `<Nullable>enable</Nullable>` should be set in the project file, and that new code should use nullable reference types.
    - Make types nullable to indicate optionality.
    - Make types nullable to indicate optionality.
    - If the project links to a Directory.Build.props file, ensure that nullable is enabled globally and not in the project file.
- Avoid primitive obsession.
    - Where a primitive (string, int, bool, double, float etc.) could be replaced with a more expressive type, use a class, struct or record.
    - Only use int for numeric values that have no domain meaning; only use string for string values that have no domain meaning.
    - Where we need to serialize, or for interoperability, you may use primitive types as part of that serialization, instead of writing convertors, for simplicity.
- - Use Responsibility Driven Design
- Focus on the responsibilities that a class has.
- "Responsibility-driven design specifies object behavior before object structure and other implementation considerations are determined. We have found that it minimizes the rework required for major design changes."
- Maximize Abstraction
    - Elide the distinction between data and behavior.
    - Think of responsibilities for “knowing”, “doing”, and “deciding”
- Distribute Behavior
    - Promote a delegated control architecture
    - Make objects smart— give them behaviors, not just data
- Preserve Flexibility
    - Design objects so interior details can be readily changed
- Objects have roles.
    - Common roles are stereotypes: information holder, structurer, service provider, coordinator, controller, interfacer
- Principles
    - Tidy is better than cluttered.
    - Reveal intention; be explicit to support future readers.
    - Prefer simplicity.
    - Do not duplicate knowledge.
    - Avoid having more than one level of indentation in a method.
    - Do not add new types without necessity.
    - There should be one-- and preferably only one --obvious way to do it.
    - If the implementation is hard to explain, it's a bad idea.
    - Keep methods small and focused on a single responsibility
- Follow Beck's "Tidy First" approach by separating structural changes from behavioral changes
    - Separate all changes into two distinct types:
        - STRUCTURAL CHANGES: Rearranging code without changing behavior (renaming, extracting methods, moving code)
        - BEHAVIORAL CHANGES: Adding or modifying actual functionality
    - Never mix structural and behavioral changes in the same commit
    - Always make structural changes first when both are needed
    - Validate structural changes do not alter behavior by running tests before and after
    - Not all of our code follows these conventions.
        - Some of our older code uses older conventions.
        - Follow the boy scout rule, and fix these, as part of your work.
- Default to a class per source file approach, unless one class clearly exists as the details of another.

## Testing

- Use TDD where possible.
- Write developer tests using xUnit.
- Name test methods in the format: When_[condition]_should_[expected_behavior]. Name test classes [behavior]Tests for the behavior being tested across all tests in the file, for example CommandProcessorPostBoxBulkClearAsyncTests.
- Ensure all new features and bug fixes include appropriate test coverage.


## Documentation

- Update or add Documentation comments for all exports from assemblies.
    - To be clear exports: means all public and protected methods of public classes/structs/records/enums.
        - We do not add Documentation comments internal or private classes, or internal methods
    - Documentation are indicated by `///`
    - Documentation comments use XML
    - Documentation comments show up in Intellisense for developers. Bear this in mind when writing comments, as they should be helpful to a developer using the API but not so verbose that a developer would not choose to read it when using intellisense. Use `<remarks>` for notes on implementation or more detailed instructions.
    - They should also be helpful to a developer or LLM reading the code.
    - We provide some guidance on specific elements:
    - Use `<summary>` element to provide an overview of the purpose of the class or method. What behavior or state does it encapsulate? What would you use it for. Use `<paramref>` if you refer to parameters in the summary.
    - Use the `<param>` tag to describe parameters to a constructor or method.
        - Use `<see cref="">` to document the type of the parameter
        - Indicate what the parameter is for, what effect setting it has and if it is optional. If it is optional describe any default value and its impact.
        - The developer should be clear what values they need to provide for the parameter to control desired behavior.
    - Use `<returns>` to indicate the `<see cref="">` of the return type, optionality, and what the value represents.
    - Use `<typeparam>` to indicate the intent of a generic type parameter; document any constraints on the type.
    - Use `<exception>` to document any exceptions that the method call can throw.
    - Use `<value>` to document a property. Like a `<summary>` it should indicate purpose. Like a `<param>` or `<return>` it should use `<see cref="">` to indicate type.

```csharp
/// <summary>
/// Gets or sets the current status.
/// </summary>
/// <value>The current status as a <see cref="string"/>.</value>
public string Status { get; set; }
```

- Use `<remarks>` for advice to developers or LLMs working with the code directly. Include information on how the method is implemented where it is not obvious from the code or significant design decisions have been made. Consider what you would want to know if maintaining this method. Use `<see href="">` if you need to link to external documentation.  This can also be used for more detailed information than could be included in the `<summary>`.
    - Prefer to use good variable and method names to express intent, over inline comments.
        - Use the refactoring "Extract Method To Express Intent" to encapsulate code in a named method that explains intent, over using a comment.
        - Do not add comments for what may be easily inferred from the code.
        - In tests you may use //Arrange, //Act, //Assert.
        - If code has a complex algorithm or non-obvious implementation, prefer to use `/// <remarks>`
- Example:

  ```csharp
  /// <summary>
  /// Sends a message to the specified recipient.
  /// </summary>
  /// <param name="recipient">The recipient's address.</param>
  /// <returns>The message ID.</returns>
  public string SendMessage(string recipient) { ... }
  ```

- Documentation comments should be changed when APIs change.
- Update README.md if it will be out of date with the latest changes.
