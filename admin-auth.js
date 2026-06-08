(function () {
	var AUTH_SESSION_KEY = "kuetAdminSession";
	var AUTH_COOKIE_NAME = "kuetAdminAuth";

	function getCookie(name) {
		var match = document.cookie.match(new RegExp("(?:^|; )" + name.replace(/[.*+?^${}()|[\]\\]/g, "\\$&") + "=([^;]*)"));
		return match ? decodeURIComponent(match[1]) : "";
	}

	function setCookie(name, value, maxAgeSeconds) {
		var cookie = name + "=" + encodeURIComponent(value) + "; path=/; SameSite=Lax";
		if (typeof maxAgeSeconds === "number") {
			cookie += "; max-age=" + maxAgeSeconds;
		}
		document.cookie = cookie;
	}

	function clearCookie(name) {
		document.cookie = name + "=; path=/; SameSite=Lax; max-age=0";
	}

	function isAuthenticated() {
		try {
			if (sessionStorage.getItem(AUTH_SESSION_KEY) === "1") {
				return true;
			}
			if (getCookie(AUTH_COOKIE_NAME) === "1") {
				return true;
			}
		} catch (error) {
			// ignore storage errors and fall through to unauthenticated
		}
		return false;
	}

	function setAuthenticated(rememberMe) {
		sessionStorage.setItem(AUTH_SESSION_KEY, "1");
		if (rememberMe) {
			setCookie(AUTH_COOKIE_NAME, "1", 60 * 60 * 24 * 7);
		} else {
			setCookie(AUTH_COOKIE_NAME, "1");
		}
	}

	function clearAuthentication() {
		sessionStorage.removeItem(AUTH_SESSION_KEY);
		clearCookie(AUTH_COOKIE_NAME);
	}

	function requireAuth(returnUrl) {
		if (isAuthenticated()) {
			return true;
		}
		var target = encodeURIComponent(returnUrl || (location.pathname.split("/").pop() || "admin.html"));
		location.replace("AdminLogin.html?returnUrl=" + target);
		return false;
	}

	window.KUET_ADMIN_AUTH = {
		isAuthenticated: isAuthenticated,
		setAuthenticated: setAuthenticated,
		clearAuthentication: clearAuthentication,
		requireAuth: requireAuth
	};
})();