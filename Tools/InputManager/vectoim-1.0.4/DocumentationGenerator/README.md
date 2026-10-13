# Documentation Generator Image

This folder contains the Dockerfile for the image used to generate documentation for the VectoInputManager project.

## Description

The Docker image is based on `mcr.microsoft.com/dotnet/sdk:9.0` and includes:

- **Doxygen 1.16.1**: For generating documentation from source code.
- **Graphviz**: For generating interactive svg diagrams in the documentation.
- **PrimaryConstructorConverter**: A tool built from source and used as a filter for Doxygen's input.

## CI/CD Workflow

This image is used in the GitLab CI pipeline (see `.gitlab-ci.yml`) to generate the project documentation. The pipeline performs the following steps using this image:

1. **File Setup**: It copies the source code and configuration files (like `Doxyfile`) from the repository into the container's `/workspace` directory.
2. **Generation**: It runs `doxygen` inside the container. Doxygen processes the files in `/workspace` and uses the pre-installed tools (Graphviz, PrimaryConstructorConverter) to generate the documentation.
3. **Output**: The generated HTML documentation is copied back to the GitLab Pages `public` directory for hosting.

## Build and Publish Instructions

To build and publish the image to the container registry, use the following commands:

```bash
docker login code.europa.eu:4567
docker build -t code.europa.eu:4567/vecto/guests/vecto-sim-harry/vecto-docs:1.1.0 .
docker push code.europa.eu:4567/vecto/guests/vecto-sim-harry/vecto-docs:1.1.0
```
