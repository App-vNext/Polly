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

    # Prepend 'api/' and append '.html' to the result
    echo "/api/${input}.html"
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
  cat > $FILE_PATH <<EOL
---
redirect_url: readme.html
---
This file will redirect users to readme.html
EOL

  # Print a success message
  echo "The index.md file has been successfully created at $FILE_PATH"
}

# Find all markdown files in the root folder and its subdirectories, excluding the 'api' directory
find "$root_folder" -path "$root_folder/api" -prune -o -name '*.md' -print0 | while IFS= read -r -d '' file; do
    # Process the file
    while IFS= read -r line; do
        # Check if the line contains a link
        if [[ $line =~ \]\(([^\)]+)\) ]]; then
            # Extract the link
            link=${BASH_REMATCH[1]}

            # Check if the link contains 'src/' and is a relative path
            if [[ $link == *src/* && $link != http* ]]; then
                # Transform the link
                new_link=$(transform_link "$link")

                # Replace the link in the line
                line=${line/$link/$new_link}

                # Print the changed link in dry run mode
                # if [[ $dry_run ]]; then
                echo "File: $file"
                echo "Original link: $link"
                echo "New link: $new_link"
                echo ""
                # fi
            fi
        fi

        # Print the line
        if [[ ! $dry_run ]]; then
            # In normal mode, print the line to a temporary file
            echo "$line" >> "${file}.tmp"
        fi
    done < "$file"

    # If not in dry run mode, replace the original file with the temporary file
    if [[ ! $dry_run ]]; then
        mv "${file}.tmp" "$file"
    fi
done

