FROM mcr.microsoft.com/dotnet/sdk:10.0

# The base image already has an 'ubuntu' user with UID 1000
# Just add sudo access for it
RUN apt-get update \
    && apt-get install -y sudo \
    && echo "ubuntu ALL=(root) NOPASSWD:ALL" > /etc/sudoers.d/ubuntu \
    && chmod 0440 /etc/sudoers.d/ubuntu

USER ubuntu
