#!/bin/bash

# Set the error handling options
set -eu

# Define some emoji for the messages
check_mark="\xE2\x9C\x85"
cross_mark="\xE2\x9D\x8C"
warning_mark="\xE2\x9A\xA0"
info_mark="\xE2\x84\xB9"

# Define some colors for the messages
green="\033[0;32m"
red="\033[0;31m"
yellow="\033[0;33m"
blue="\033[0;34m"
reset="\033[0m"

# Define a function to parse the command-line options
parse_options() {
    # Use getopt to handle long and short options
    local options
    options=$(getopt -o d --long dry-run -- "$@") || exit 1
    eval set -- "$options"

    # Use a case statement to handle different options
    local dry_run=false # Initialize the dry run flag to false
    while true; do
        case "$1" in
        -d | --dry-run)
            # Set the dry run flag to true
            dry_run=true
            shift
            ;;
        --)
            # End of options
            shift
            break
            ;;
        *)
            # Invalid option
            echo -e "${red}${cross_mark} Usage: $0 [-d|--dry-run] root-folder${reset}"
            exit 1
            ;;
        esac
    done

    # Check for the root folder argument
    if [[ -z $1 ]]; then
        echo -e "${red}${cross_mark} Usage: $0 [-d|--dry-run] root-folder${reset}"
        exit 1
    fi
    local root_folder=$1
    shift

    # Return the dry run flag and the root folder
    echo "$dry_run" "$root_folder"
}

# Define a function to transform the link
transform_link() {
    # Remove the leading path up to and including 'src/' and the trailing '.cs' from the input
    local input=${link#*src/}
    local original_input=$input
    input=${input%.cs}

    # Replace all '/' with '.'
    input=${input//\//.}

    # Replace 'Polly.Core' with 'Polly'
    input=${input//Polly.Core/Polly}

    # Replace .TResult with -1 (docfx does not use Tresult in paths)
    input=${input//.TResult/}

    match=false
    # Check if the link is in the mappings
    for uuid in "${uids[@]}"; do
        if [[ $uuid == "$input" || $uuid =~ ^$input[\`\#] ]]; then
            match=$uuid
            break
        fi
    done

    if [[ $match == false ]]; then
        echo "https://github.com/App-vNext/Polly/tree/main/src/$original_input"
    else
        echo "xref:${match}"
    fi
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

rename_readme() {
    # Check if the dry run option is given
    if [[ $dry_run == true ]]; then
        # Print the readme file renaming message
        echo -e "${yellow}${warning_mark} The following README.md files would be renamed to index.md:${reset}"
        find $root_folder -name "README.md" -exec bash -c 'echo "$0 -> ${0%README.md}index.md"' {} \; | sed "s/^/${info_mark} /"
        echo ""

        # Print the link replacement message
        echo -e "${yellow}${warning_mark} The following links to README.md would be replaced with index.md in all markdown files:${reset}"
        find $root_folder -name "*.md" -type f -exec sed -n 's/README.md/index.md/gp' {} \; | sed "s/^/${info_mark} /"
        echo ""

        # Print the link replacement message
        echo -e "${yellow}${warning_mark} The following references to README.md would be replaced with index.md in all toc.yml files:${reset}"
        find $root_folder -name "toc.yml" -type f -exec sed -n 's/README.md/index.md/gp' {} + | sed "s/^/${info_mark} /"
        echo ""
    else
        # Rename all README.md files to index.md
        find $root_folder -name "README.md" -exec bash -c 'mv "$0" "${0%README.md}index.md"' {} \;

        # Print the readme file renaming message
        echo -e "${green}${check_mark} The following README.md files have been renamed to index.md:${reset}"
        find $root_folder -name "index.md" -exec bash -c 'echo "${0%index.md}README.md -> $0"' {} \; | sed "s/^/${info_mark} /"
        echo ""

        # Replace all links to README.md with index.md in all markdown files
        find $root_folder -name "*.md" -type f -exec sed -i 's/README.md/index.md/g' {} +

        # Print the link replacement message
        echo -e "${green}${check_mark} The following links to README.md have been replaced with index.md in all markdown files:${reset}"
        find $root_folder -name "*.md" -type f -exec sed -n 's/README.md/index.md/gp' {} + | sed "s/^/${info_mark} /"
        echo ""

        # Replace all references to README.md with index.md in all toc.yml files
        find $root_folder -name "toc.yml" -type f -exec sed -i 's/README.md/index.md/g' {} +

        # Print the link replacement message
        echo -e "${green}${check_mark} The following references to README.md have been replaced with index.md in all toc.yml files:${reset}"
        find $root_folder -name "toc.yml" -type f -exec sed -n 's/README.md/index.md/gp' {} + | sed "s/^/${info_mark} /"
        echo ""
    fi
}

transform_links() {
    # Print the link transformation message
    echo -e "${blue}${info_mark} The following links to source code files are transformed to point to the api documentation in all markdown files:${reset}"
    # Find all markdown files in the root folder and its subdirectories, excluding the 'api' directory
    find "$root_folder" -path "$root_folder/api" -prune -o -name '*.md' -print0 | while IFS= read -r -d '' file; do
        # Get the relative path to the root folder
        local relative_path=$(relative_path_to_root "${file#$root_folder/}")

        # Process the file
        while IFS= read -r line; do
            local iterator_line=$line
            # Check if the line contains a link
            while [[ $iterator_line =~ \]\(([^\)]+)\) ]]; do
                # Extract the link
                local link=${BASH_REMATCH[1]}

                # Check if the link contains 'src/' and is a relative path
                if [[ $link == *src/* && $link != http* ]]; then
                    # Transform the link
                    local new_link=$(transform_link "${link}" "${uids}" "${relative_path}")

                    # Replace the link in the new line
                    line=${line/$link/$new_link}

                    # Print the changed link
                    echo -e "${blue}${info_mark} File: $file${reset}"
                    echo -e "${blue}${info_mark} Original link: $link${reset}"
                    echo -e "${blue}${info_mark} New link: $new_link${reset}"
                    echo ""
                fi

                iterator_line=${iterator_line/${BASH_REMATCH[0]}/}
            done

            # Print the new line
            if [[ $dry_run == false ]]; then
                # In normal mode, print the new line to a temporary file
                echo "$line" >>"${file}.tmp"
            fi
        done <"$file"

        # If not in dry run mode, replace the original file with the temporary file
        if [[ $dry_run == false ]]; then
            mv "${file}.tmp" "$file"
        fi
    done
}

# Define a main function to run the script
main() {
    # Parse the command-line options
    local dry_run root_folder
    read -r dry_run root_folder <<<"$(parse_options "$@")"

    if [ ! -d "$root_folder" ]; then
        echo -e "${red}${cross_mark}Error: Root folder does not exist.${reset}"
        exit 1
    fi

    # Check if dry run is enabled
    if [[ $dry_run == true ]]; then
        # Print the dry run mode message
        echo -e "${yellow}${warning_mark} This is a dry run. No changes will be made to the files. The following actions would be performed:${reset}"
        echo ""
    else
        # Print the normal mode message
        echo -e "${green}${check_mark} This is not a dry run. The following changes will be made to the files:${reset}"
        echo ""
    fi

    # Load the mappings
    local uids=()
    while read -r line; do
        line=$(echo "$line" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')
        uids+=("$line")
    done < <(grep -oP '(?<=uid: ).*' "$root_folder/_site/xrefmap.yml")

    # Transform the links in the markdown files
    transform_links "${dry_run}" "${root_folder}" "${uids}"

    # Rename the readme files to index files
    rename_readme "${dry_run}" "${root_folder}"
}

# Call the main function with the arguments
main "$@"
