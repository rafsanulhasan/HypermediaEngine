# Project: HypermediaEngine

## Commands
- dotnet build : Build Project or Solution
- dotnet run : Run Project
- dotnet test : Run tests
- dotnet stryker : Run Mutation Tests

## Architecture
- Middlewares
- Dependency Injection
- Endpoint Filters / Result Filters

## Conventions
- Use Explicit Type declarations with Tartet typed new expression or collection expression. (e.g FileStream stream = new(), List<int> intList = [];)
  - Exception 1: Stream stream = new FileStream()
  - Exception 2: IEnumerable<int> intStream = new List<int>()
- Prefer Async Disposal over Sync Disposal. (e.g. await using FileStream stream = new FileStream()). 
- Return shape is always { data, error }
- Never expose stack traces to the client
- Use the logger module, not console.log

## Watch out for
- Run test cases after building every feature using `dotnet test`
- Run Mutation tests after running all tests using `dotnet stryker`