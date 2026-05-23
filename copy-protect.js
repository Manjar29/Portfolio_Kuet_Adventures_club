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
	}

	if (document.readyState === "loading") {
		document.addEventListener("DOMContentLoaded", init);
		return;
	}

	init();
})();
