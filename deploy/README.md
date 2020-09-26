# Deployment

## Requirements

- Linux or Windows with [WSL](https://docs.microsoft.com/en-us/windows/wsl/install-win10)
- [ansible](https://docs.ansible.com/ansible/latest/installation_guide/intro_installation.html)
- [dotnet](https://dotnet.microsoft.com/download)
- [yarn](https://classic.yarnpkg.com/en/docs/install)
- OPT: `pip install datadog pyyaml packaging`

## Run

- `ansible-galaxy install -r requirements.yml`
- `ansible-playbook -i hosts.ini --vault-password-file secret playbook.yml`
