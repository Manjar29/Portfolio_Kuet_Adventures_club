
document.addEventListener("DOMContentLoaded", function () {
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
});


