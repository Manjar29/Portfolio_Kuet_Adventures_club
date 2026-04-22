
document.addEventListener("DOMContentLoaded", function () {
	setupMembershipForm();
	setupEventRegistrationForm();
});

function setupMembershipForm() {
	var form = document.getElementById("membershipForm");
	var status = document.getElementById("formStatus");

	if (!form || !status) {
		return;
	}

	form.addEventListener("submit", function (event) {
		event.preventDefault();

		var fullName = document.getElementById("fullName");
		var memberType = document.getElementById("memberType");
		var department = document.getElementById("department");
		var rollId = document.getElementById("rollId");
		var batch = document.getElementById("batch");
		var mailbox = document.getElementById("mailbox");
		var phoneNumber = document.getElementById("phoneNumber");
		var message = document.getElementById("message");
		var passport = document.querySelector('input[name="passport"]:checked');
		var errorMessage = "";

		if (!fullName.value.trim()) {
			errorMessage = "Please enter your full name.";
		} else if (!memberType.value.trim()) {
			errorMessage = "Please select your member type.";
		} else if (!department.value.trim()) {
			errorMessage = "Please enter your department.";
		} else if (!rollId.value.trim()) {
			errorMessage = "Please enter your roll or ID.";
		} else if (!batch.value.trim()) {
			errorMessage = "Please enter your batch.";
		} else if (!mailbox.value.trim()) {
			errorMessage = "Please enter your mailbox.";
		} else if (!phoneNumber.value.trim()) {
			errorMessage = "Please enter your phone number.";
		} else if (!passport) {
			errorMessage = "Please choose whether you have a valid passport.";
		} else if (!message.value.trim()) {
			errorMessage = "Please write a short note about why you want to join.";
		}

		if (errorMessage) {
			status.textContent = errorMessage;
			status.style.color = "#9a3412";
			return;
		}

		window.location.href = "success.html";
	});
}

function setupEventRegistrationForm() {
	var form = document.getElementById("eventRegisterForm");
	var status = document.getElementById("eventRegisterStatus");

	if (!form || !status) {
		return;
	}

	var eventTitle = document.getElementById("registerEventTitle");
	var eventNameInput = document.getElementById("eventName");
	var query = new URLSearchParams(window.location.search);
	var eventName = query.get("event") || "KUET Adventure Event";

	if (eventTitle) {
		eventTitle.textContent = "Register for " + eventName;
	}
	if (eventNameInput) {
		eventNameInput.value = eventName;
	}

	form.addEventListener("submit", function (event) {
		event.preventDefault();

		var name = document.getElementById("regName");
		var department = document.getElementById("regDepartment");
		var clubId = document.getElementById("regClubId");
		var roll = document.getElementById("regRoll");
		var transaction = document.getElementById("regTxn");
		var errorMessage = "";

		if (!name.value.trim()) {
			errorMessage = "Please enter your name.";
		} else if (!department.value.trim()) {
			errorMessage = "Please enter your department.";
		} else if (!clubId.value.trim()) {
			errorMessage = "Please enter your club ID.";
		} else if (!roll.value.trim()) {
			errorMessage = "Please enter your roll.";
		} else if (!/^bk\d{4}$/i.test(transaction.value.trim())) {
			errorMessage = "Transaction ID format must be bk + last 4 digits (example: bk1298).";
		}

		if (errorMessage) {
			status.textContent = errorMessage;
			status.style.color = "#9a3412";
			return;
		}

		window.location.href = "register-success.html";
	});
}


