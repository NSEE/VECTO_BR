FROM mcr.microsoft.com/dotnet/sdk:9.0

# Download and install Doxygen 1.16.1 and Graphviz dependencies
RUN apt-get update && \
    apt-get install -y wget graphviz && \
    wget https://www.doxygen.nl/files/doxygen-1.16.1.linux.bin.tar.gz && \
    tar -xzf doxygen-1.16.1.linux.bin.tar.gz && \
    chmod +x doxygen-1.16.1/bin/* && \
    cp doxygen-1.16.1/bin/* /usr/local/bin/ && \
    rm -rf doxygen-1.16.1* && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /workspace

# Download and build PrimaryConstructorConverter from GitHub
RUN wget https://github.com/hakakou/PrimaryConstructorConverter/archive/refs/tags/1.0.0.tar.gz && \
    tar -xzf 1.0.0.tar.gz && \
    rm 1.0.0.tar.gz

WORKDIR /workspace/PrimaryConstructorConverter-1.0.0/PrimaryConstructorConverter
RUN dotnet publish -c Release -o /workspace/Tools/PrimaryConstructorConverter
