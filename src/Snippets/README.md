# Snippets

Various code-snippets used in the Polly documentation. Run the following command in the root directory to update the snippets in the docs:

```powershell
dotnet mdsnippets
```

Visit the [MarkdownSnippets][MarkdownSnippets] GitHub repository for more details.

## How to use snippets in Polly documentation

First, locate the relevant `.cs` file where the snippet resides. For instance, `retry.md` refers to snippets found in the `Retry.cs` file.

Next, set up your code snippet. Ideally, use a new method and enclose the section you want to reference between `#region my-snippet` and `#endregion` tags.

```csharp
public static void MySnippet()
{
    #region my-snippet

    // Your code here

    #endregion
}
```

In your Markdown documentation, refer to your code snippet by adding `snippet: my-snippet`.

When you're finished editing, run the `dotnet mdsnippets` command from the root directory of the repository to refresh all the snippets throughout all the Markdown files.

> [!NOTE]
> If you refer to a non-existing snippet (due to typo or unsaved changes) then `dotnet mdsnippets` will warn you.
>
> _Failed: Missing snippets: markdown-path.md: non-existing-snippet_

[MarkdownSnippets]: https://github.com/SimonCropp/MarkdownSnippets
