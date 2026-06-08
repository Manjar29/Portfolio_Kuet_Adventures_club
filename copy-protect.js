(function () {
	function blockEvent(event) {
		event.preventDefault();
		event.stopPropagation();
		return false;
	}

	function shouldBlockShortcut(event) {
		var key = (event.key || "").toLowerCase();
		var modifierActive = event.ctrlKey || event.metaKey;

		if (event.key === "F12") {
			return true;
		}

		if (modifierActive && (key === "c" || key === "x" || key === "u" || key === "s" || key === "p")) {
			return true;
		}

		if (event.ctrlKey && event.shiftKey && (key === "i" || key === "j" || key === "c")) {
			return true;
		}

		return false;
	}

	function init() {
		["copy", "cut", "contextmenu", "dragstart", "selectstart"].forEach(function (eventName) {
			document.addEventListener(eventName, blockEvent, true);
		});

		document.addEventListener("keydown", function (event) {
			if (shouldBlockShortcut(event)) {
				blockEvent(event);
			}
		}, true);

		// Ensure public pages forward Admin links to the login gate.
		try {
			var current = (window.location.pathname || "").toLowerCase();
			var isPublicPage = current.endsWith("/p29.html") || current.endsWith("p29.html") || current.endsWith("/index.html") || current.endsWith("index.html") || current === "";
			if (isPublicPage) {
				var anchors = document.querySelectorAll('a[href$="admin.html"]');
				anchors.forEach(function (a) {
					a.setAttribute('href', 'AdminLogin.html?returnUrl=admin.html');
				});
			}
		} catch (e) { /* no-op */ }
	}

	if (document.readyState === "loading") {
		document.addEventListener("DOMContentLoaded", init);
		return;
	}

	init();
})();
