$services = @{
    "version" = "0.8.3"
    "sourceRegistry" = "ghcr.io/solliancenet/foundationallm"
    "destRegistry" = "acrfllm.azurecr.io"
    "destUsername" = "acrfllm"
    "destPassword" = "<ACR Password>"
    "service_matrix" = @(
        "agent-hub-api"
        "authorization-api"
        "chat-ui"
        "core-api"
        "core-job"
        "data-source-hub-api"
        "gatekeeper-api"
        "gatekeeper-integration-api"
        "gateway-api"
        "gateway-adapter-api"
        "langchain-api"
        "management-api"
        "management-ui"
        "orchestration-api"
        "prompt-hub-api"
        "semantic-kernel-api"
        "state-api"
        "vectorization-api"
        "vectorization-job"
    )
}


foreach ($service in $($services.service_matrix)) {
    $srcChartUrl = "$($services.sourceRegistry)/helm/$service"
    $destChartUrl = "$($services.destRegistry)/helm"
    docker logout
    helm pull oci://$srcChartUrl --version $services.version
    echo $services.destPassword | docker login $services.destRegistry -u $services.destUsername --password-stdin
    helm push "$service-$($services.version).tgz" oci://$destChartUrl

    $imageWithTag = "$($service):$($services.version)"
    $srcImageUrl = "$($services.sourceRegistry)/$imageWithTag"
    $destImageUrl = "$($services.destRegistry)/$imageWithTag"
    docker logout
    docker pull $srcImageUrl
    docker tag $srcImageUrl $destImageUrl
    echo $services.destPassword | docker login $services.destRegistry -u $services.destUsername --password-stdin
    docker push $destImageUrl
}

docker logout
docker pull registry.k8s.io/ingress-nginx/controller:v1.11.3@sha256:d56f135b6462cfc476447cfe564b83a45e8bb7da2774963b00d12161112270b7
docker tag "registry.k8s.io/ingress-nginx/controller:v1.11.3@sha256:d56f135b6462cfc476447cfe564b83a45e8bb7da2774963b00d12161112270b7" "$($services.destRegistry)/ingress-nginx/controller:v1.11.3"
echo $services.destPassword | docker login $services.destRegistry -u $services.destUsername --password-stdin
docker push "$($services.destRegistry)/ingress-nginx/controller:v1.11.3"


docker logout
docker pull registry.k8s.io/ingress-nginx/kube-webhook-certgen:v1.4.4@sha256:a9f03b34a3cbfbb26d103a14046ab2c5130a80c3d69d526ff8063d2b37b9fd3f
docker tag "registry.k8s.io/ingress-nginx/kube-webhook-certgen:v1.4.4@sha256:a9f03b34a3cbfbb26d103a14046ab2c5130a80c3d69d526ff8063d2b37b9fd3f" "$($services.destRegistry)/ingress-nginx/kube-webhook-certgen:v1.4.4"
echo $services.destPassword | docker login $services.destRegistry -u $services.destUsername --password-stdin
docker push "$($services.destRegistry)/ingress-nginx/kube-webhook-certgen:v1.4.4"

docker logout
docker pull registry.k8s.io/ingress-nginx/opentelemetry-1.25.3:v20240813-b933310d@sha256:f7604ac0547ed64d79b98d92133234e66c2c8aade3c1f4809fed5eec1fb7f922
docker tag "registry.k8s.io/ingress-nginx/opentelemetry-1.25.3:v20240813-b933310d@sha256:f7604ac0547ed64d79b98d92133234e66c2c8aade3c1f4809fed5eec1fb7f922" "$($services.destRegistry)/ingress-nginx/opentelemetry-1.25.3:v20240813-b933310d"
echo $services.destPassword | docker login $services.destRegistry -u $services.destUsername --password-stdin
docker push "$($services.destRegistry)/ingress-nginx/opentelemetry-1.25.3:v20240813-b933310d"

docker logout
docker pull registry.k8s.io/defaultbackend-amd64:1.5
docker tag "registry.k8s.io/defaultbackend-amd64:1.5" "$($services.destRegistry)/defaultbackend-amd64:1.5"
echo $services.destPassword | docker login $services.destRegistry -u $services.destUsername --password-stdin
docker push "$($services.destRegistry)/defaultbackend-amd64:1.5"