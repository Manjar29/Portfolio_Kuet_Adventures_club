(function () {
	function normalizeBaseUrl(baseUrl) {
		return String(baseUrl).replace(/\/+$/, "");
	}

	function unique(values) {
		var seen = Object.create(null);
		var result = [];

		for (var i = 0; i < values.length; i++) {
			var value = values[i];
			if (!seen[value]) {
				seen[value] = true;
				result.push(value);
			}
		}

		return result;
	}

	function getExplicitApiBase() {
		if (typeof window !== "undefined" && window.KUET_API_BASE) {
			return normalizeBaseUrl(window.KUET_API_BASE);
		}

		if (typeof document !== "undefined" && document.querySelector) {
			var meta = document.querySelector('meta[name="kuet-api-base"]');
			if (meta) {
				var content = meta.getAttribute("content");
				if (content) {
					return normalizeBaseUrl(content);
				}
			}
		}

		return "";
	}

	function getApiBaseCandidates() {
		var candidates = [];
		var explicitBase = getExplicitApiBase();
		var currentOrigin = (typeof window !== "undefined" && window.location && window.location.origin) ? normalizeBaseUrl(window.location.origin) : "";

		if (explicitBase && /^https?:\/\//i.test(explicitBase)) {
			candidates.push(explicitBase);
		}

		if (currentOrigin && /^https?:\/\//i.test(currentOrigin)) {
			candidates.push(currentOrigin);
		}

		candidates.push("http://localhost:5136");
		candidates.push("http://127.0.0.1:5136");
		candidates.push("https://localhost:7256");
		candidates.push("https://127.0.0.1:7256");
		candidates.push("http://localhost:5000");
		candidates.push("http://127.0.0.1:5000");
		candidates.push("https://localhost:5000");
		candidates.push("https://127.0.0.1:5000");

		return unique(candidates
			.map(normalizeBaseUrl)
			.filter(function (baseUrl) {
				return /^https?:\/\//i.test(baseUrl);
			}));
	}

	function isJsonResponse(response) {
		var contentType = response.headers && response.headers.get ? response.headers.get("content-type") : "";
		return !!contentType && /json/i.test(contentType);
	}

	function isRetryableResponse(response, options) {
		var method = (options && options.method ? String(options.method) : "GET").toUpperCase();
		if (method === "GET") {
			return response.status === 404 || response.status === 405 || response.status === 501;
		}

		return response.status === 404 || response.status === 405 || response.status === 501;
	}

	function requestFromCandidate(path, options, index, candidates, expectJson) {
			var requestUrl = candidates[index] + path;

			// Diagnostic log to help debug network errors in the browser console
			if (typeof console !== "undefined" && console.debug) {
				console.debug("KUET_API: attempting request to", requestUrl, "options:", options || {});
			}

			return fetch(requestUrl, options).then(function (response) {
				if (response.ok) {
					if (expectJson && !isJsonResponse(response) && index < candidates.length - 1) {
						return requestFromCandidate(path, options, index + 1, candidates, expectJson);
					}

					return response;
				}

				if (index < candidates.length - 1 && isRetryableResponse(response, options)) {
					return requestFromCandidate(path, options, index + 1, candidates, expectJson);
				}

				return response;
			}).catch(function (error) {
				// Log the candidate failure for easier troubleshooting
				if (typeof console !== "undefined" && console.warn) {
					console.warn("KUET_API: request failed for", requestUrl, "error:", error && error.message ? error.message : error);
				}

				if (index < candidates.length - 1) {
					return requestFromCandidate(path, options, index + 1, candidates, expectJson);
				}

				// When all candidates failed, throw a richer error that includes tried hosts
				var tried = candidates.join(", ");
				var message = (error && error.message) ? error.message : String(error);
				var aggregated = new Error("NetworkError: failed to fetch " + path + " from candidates: " + tried + ". Last error: " + message);
				throw aggregated;
			});
	}

	function requestApi(path, options) {
		return requestFromCandidate(path, options, 0, getApiBaseCandidates(), false);
	}

	function requestJson(path, options) {
		return requestFromCandidate(path, options, 0, getApiBaseCandidates(), true).then(function (response) {
			if (!response.ok) {
				throw new Error("Request failed with status " + response.status + ".");
			}

			return response.json();
		});
	}

	window.KUET_API = {
		getApiBaseCandidates: getApiBaseCandidates,
		requestApi: requestApi,
		requestJson: requestJson
	};
})();