```
az group create --name saars-wacs-01 --location westus2
az acs create --orchestrator-type=kubernetes --resource-group saars-wacs-01 --name=saars-wacs-01 --agent-count=2 --generate-ssh-keys --windows --admin-username saars --admin-password b7de48de-30a8-47a9-9cec-d1c066a4721c
az acs kubernetes install-cli
az acs kubernetes get-credentials --resource-group=saars-wacs-01 --name=saars-wacs-01

```