#!/bin/bash

# Check for the dry run option
if [[ $1 == "--dry-run" ]]; then
    dry_run=true
    shift
fi

# Check for the root folder argument
if [[ -z $1 ]]; then
    echo "Usage: $0 [--dry-run] root-folder"
    exit 1
fi
root_folder=$1
shift

# Define a function to transform the link
transform_link() {
    # Remove the leading path up to and including 'src/' and the trailing '.cs' from the input
    local input=${1#*src/}
    input=${input%.cs}

    # Replace all '/' with '.'
    input=${input//\//.}

    # Replace 'Polly.Core' with 'Polly'
    input=${input//Polly.Core/Polly}

    # Replace .TResult with -1 (docfx does not use Tresult in paths)
    input=${input//.TResult/-1}

    # Prepend 'api/' and append '.html' to the result
    echo "api/${input}.html"
}

# Define a function to generate the relative path to the root folder
relative_path_to_root() {
    # Get the depth of the current file
    local depth=$(tr -dc '/' <<<"$1" | awk '{ print length }')

    # Generate the relative path
    local path=""
    for ((i = 0; i < depth; i++)); do
        path="../${path}"
    done

    echo "$path"
}

create_index_file() {
    # Check for the dry run option
    if [[ $1 == "--dry-run" ]]; then
        dry_run=true
        shift
    fi

    # Check for the root folder argument
    if [[ -z $1 ]]; then
        echo "Usage: $0 [--dry-run] root-folder"
        exit 1
    fi
    root_folder=$1
    shift

    # Set the file path
    local FILE_PATH="$root_folder/index.md"

    # Check if dry run is enabled
    if [[ $dry_run == true ]]; then
        echo "Dry run: The index.md file would be created at $FILE_PATH"
        return 0
    fi

    # Create the file with the specified content
    cat >$FILE_PATH <<EOL
---
redirect_url: readme.html
---
This file will redirect users to readme.html
EOL

    # Print a success message
    echo "The index.md file has been successfully created at $FILE_PATH"
}

function rename_readme() {
  # Check for the root folder argument
  if [[ -z $1 ]]; then
      echo "Usage: $0 [--dry-run] root-folder"
      exit 1
  fi
  root_folder=$1
  shift

  # Rename all README.md files to index.md
  find $root_folder -name "README.md" -exec bash -c 'mv "$0" "${0%README.md}index.md"' {} \;

  # Replace all links to README.md with index.md in all markdown files
  find $root_folder -name "*.md" -type f -exec sed -i 's/README.md/index.md/g' {} +
}

# Find all markdown files in the root folder and its subdirectories, excluding the 'api' directory
find "$root_folder" -path "$root_folder/api" -prune -o -name '*.md' -print0 | while IFS= read -r -d '' file; do
    # Get the relative path to the root folder
    relative_path=$(relative_path_to_root "${file#$root_folder/}")

    # Process the file
    while IFS= read -r line; do
        iterator_line=$line
        # Check if the line contains a link
        while [[ $iterator_line =~ \]\(([^\)]+)\) ]]; do
            # Extract the link
            link=${BASH_REMATCH[1]}

            # Check if the link contains 'src/' and is a relative path
            if [[ $link == *src/* && $link != http* ]]; then
                # Transform the link
                new_link=$(transform_link "$link")

                # Prepend the relative path to the root folder
                new_link="${relative_path}${new_link}"

                # Replace the link in the new line
                line=${line/$link/$new_link}

                # Print the changed link in dry run mode
                echo "File: $file"
                echo "Original link: $link"
                echo "New link: $new_link"
                echo ""
            fi

            iterator_line=${iterator_line/${BASH_REMATCH[0]}/}
        done

        # Print the new line
        if [[ ! $dry_run ]]; then
            # In normal mode, print the new line to a temporary file
            echo "$line" >>"${file}.tmp"
        fi
    done <"$file"

    # If not in dry run mode, replace the original file with the temporary file
    if [[ ! $dry_run ]]; then
        mv "${file}.tmp" "$file"
    fi
done

create_index_file "$root_folder"

# Check if the dry run option is given
if [[ $dry_run ]]; then
  # Print what the function would do without actually doing it
  echo "This is a dry run. The following actions would be performed:"
  find $root_folder -name "README.md" -exec echo "Rename {} to ${}/index.md" \;
  find $root_folder -name "*.md" -exec grep -l "README.md" {} \; | xargs echo "Replace links to README.md with index.md in"
else
  # Call the function
  rename_readme "$root_folder"
fi
