{
  description = "nitevault backend dev shell";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs { inherit system; };
      in {
        devShells.default = pkgs.mkShell {
          buildInputs = [
            pkgs.dotnet-sdk_10 # or dotnet-sdk_9, matching your TFM
          ];

          # Prevents dotnet from trying to auto-download SDKs/telemetry noise
          shellHook = ''
            export DOTNET_CLI_TELEMETRY_OPTOUT=1
            export DOTNET_NOLOGO=1
          '';
        };
      });
}