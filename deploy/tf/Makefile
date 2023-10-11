default: plan

# Default module because we only have one in this repo
module ?= "fllm"

# Example: make deploy module=poc
deploy: fmt docs validate
	cd $(module) && terraform apply

docs:
	cd $(module) && terraform-docs markdown document --output-file README.md --output-mode inject .

fmt: tffmt

# Example: make plan module=poc
plan: fmt docs validate
	cd $(module) && terraform plan

tffmt:
	cd $(module) && terraform fmt -recursive

tfswitch:
	cd $(module) && tfswitch

# Example: make upgrade module=poc
upgrade:
	cd $(module) && terraform init -upgrade

# Example: make validate module=poc
validate:
	cd $(module) && terraform validate

workspaces:
	cd $(module) && \
		terraform workspace list && \
		rm -f .terraform/environment && \
		terraform init