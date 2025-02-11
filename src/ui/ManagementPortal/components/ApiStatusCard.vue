<template>
	<div class="api-status-card" role="region">
		<h2>{{ apiName }}</h2>
		<div v-if="loading" class="loading" aria-live="polite">Loading...</div>
		<div v-else-if="error" class="error" aria-live="assertive">{{ error }}</div>
		<div v-else-if="apiStatus">
			<div class="api-detail">
				<p><strong>Name:</strong> {{ apiStatus.name }}</p>
				<p><strong>Description:</strong> {{ description }}</p>
				<p><strong>Instance:</strong> {{ apiStatus.instance }}</p>
				<p><strong>Version:</strong> {{ apiStatus.version }}</p>
				<p><strong>Status:</strong> {{ apiStatus.status }}</p>
				<p v-if="apiStatus.message"><strong>Message:</strong> {{ apiStatus.message }}</p>
			</div>
			<div v-if="apiStatus.subordinate_services" class="subordinate-services">
				<h3>Subordinate Services</h3>
				<ul>
					<li v-for="service in apiStatus.subordinate_services" :key="service.name">
						<p><strong>Name:</strong> {{ service.name }}</p>
						<p><strong>Instance:</strong> {{ service.instance }}</p>
						<p><strong>Version:</strong> {{ service.version }}</p>
						<p><strong>Status:</strong> {{ service.status }}</p>
						<p v-if="service.message"><strong>Message:</strong> {{ service.message }}</p>
					</li>
				</ul>
			</div>
		</div>
		<div v-else>This service does not contain a status endpoint.</div>
		<p class="api-status-card__url"><strong>URL:</strong> {{ apiUrl }}</p>
	</div>
</template>

<script>
export default {
	props: {
		apiName: {
			type: String,
			required: true,
		},
		apiUrl: {
			type: String,
			required: true,
		},
		statusUrl: {
			type: [String, null],
			required: false,
			default: '',
		},
		description: {
			type: String,
			required: false,
			default: '',
		},
	},
	data() {
		return {
			apiStatus: null,
			loading: true,
			error: null,
		};
	},
	async mounted() {
		await this.fetchApiStatus();
	},
	methods: {
		async fetchApiStatus() {
			this.loading = true;
			try {
				if (this.statusUrl) {
					// const response = await $fetch(
					// 	`/api/api-status?url=${encodeURIComponent(this.statusUrl)}`,
					// );
					const response = await $fetch(this.statusUrl);
					if (response.error) {
						this.error = response.error;
					} else {
						this.apiStatus = response;
					}
				}
			} catch (error) {
				console.error('Error fetching API status:', error);
				this.error = `Error fetching API status from ${this.statusUrl}`;
			} finally {
				this.loading = false;
			}
		},
	},
};
</script>

<style scoped>
.api-status-card {
	max-width: 100%;
	border: 1px solid #ddd;
	border-radius: 8px;
	padding: 1.5em;
	margin: 1em;
	box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
	background-color: #fff;
	transition: box-shadow 0.3s ease;
}

.api-status-card:hover {
	box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
}

.api-status-card h2 {
	margin-top: 0;
	margin-bottom: 1em;
	font-size: 1.5em;
	color: #333;
}

.api-status-card .api-detail p,
.api-status-card .subordinate-services p {
	margin: 0.5em 0;
}

.api-status-card .api-detail p strong,
.api-status-card .subordinate-services p strong {
	color: #555;
}

.api-status-card .loading,
.api-status-card .error {
	color: #672525;
	font-weight: normal;
}

.api-status-card .status.ready {
	color: #4caf50;
}

.api-status-card .status.error {
	color: #f44336;
}

.api-status-card .subordinate-services {
	margin-top: 1em;
}

.api-status-card .subordinate-services h3 {
	margin-bottom: 0.5em;
	font-size: 1.2em;
	color: #555;
}

.api-status-card .subordinate-services ul {
	padding-left: 1.5em;
}

.api-status-card .subordinate-services ul li {
	margin-bottom: 1em;
	list-style-type: disc;
}

.api-status-card .api-status-card__url {
	word-break: break-all;
}

@media (max-width: 600px) {
	.api-status-card h2 {
		font-size: 1.2em;
	}

	.error {
		word-break: break-all;
	}
}
</style>
