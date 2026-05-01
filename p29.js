
document.addEventListener("DOMContentLoaded", function () {
	setupMembershipForm();
	setupEventRegistrationForm();
	setupEventDetailsPage();
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

function setupEventDetailsPage() {
	var title = document.getElementById("eventDetailTitle");
	var subtitle = document.getElementById("eventDetailSubtitle");
	var overview = document.getElementById("eventOverview");
	var scheduleList = document.getElementById("eventSchedule");
	var requirementsList = document.getElementById("eventRequirements");
	var paymentList = document.getElementById("eventPayment");
	var registerBtn = document.getElementById("detailRegisterBtn");
	var photoWrap = document.getElementById("eventDetailPhotoWrap");
	var photo = document.getElementById("eventDetailPhoto");

	if (!title || !subtitle || !overview || !scheduleList || !requirementsList || !paymentList || !registerBtn || !photoWrap || !photo) {
		return;
	}

	var query = new URLSearchParams(window.location.search);
	var eventName = query.get("event") || "KUET Adventure Event";

	var eventData = {
		"Sundarbans Eco Exploration": {
			subtitle: "Mangrove ecosystem learning camp with guided exploration and nature safety orientation.",
			overview: "A one-day eco-focused expedition for observation, awareness, and team-based field learning.",
			image: {
				src: "sundarban.webp",
				alt: "Sundarbans nature scene for KUET Adventure Club event"
			},
			schedule: [
				"Reporting at KUET gate: 5:30 AM",
				"Departure by bus: 6:00 AM",
				"Guided exploration and workshop: 10:00 AM - 3:00 PM",
				"Return to campus: 9:00 PM"
			],
			requirements: [
				"Student ID and club ID",
				"Comfortable trekking shoes",
				"Reusable water bottle",
				"Basic personal medicine"
			],
			payment: [
				"Registration fee: 800 BDT",
				"bKash number: 01712-345678",
				"Bank account number: 123456789012",
				"Payment method: bKash (send money) or bank transfer",
				"Transaction format: bk + last 4 digits (example: bk1298)",
				"Use that transaction ID in registration form"
			]
		},
		"KUET to Bagerhat Cycling Run": {
			subtitle: "Long-distance group cycling event focusing endurance, road discipline, and hydration planning.",
			overview: "A 70 km controlled route ride with mentor checkpoints and pace groups.",
			image: {
				src: "cycling.webp",
				alt: "KUET Adventure Club cycling event photo"
			},
			schedule: [
				"Bike check and briefing: 5:00 AM",
				"Ride start: 5:45 AM",
				"Checkpoint breaks every 20 km",
				"Expected return: 2:00 PM"
			],
			requirements: [
				"Helmet and front-back lights",
				"Roadworthy cycle with brakes",
				"Two water bottles",
				"Emergency contact number"
			],
			payment: [
				"Registration fee: 500 BDT",
				"bKash number: 01718-112233",
				"Bank account number: 123456789013",
				"Payment method: bKash (merchant) or bank transfer",
				"Transaction format: bk + last 4 digits (example: bk4455)",
				"Provide transaction ID during registration"
			]
		},
		"Adventure Bootcamp 3.0": {
			subtitle: "Two-day intensive bootcamp with team challenges, map reading, and survival practice.",
			overview: "Hands-on field training to improve leadership, planning, and outdoor emergency response.",
			image: {
				src: "shelter.webp",
				alt: "Adventure Bootcamp shelter building photo"
			},
			schedule: [
				"Day 1 reporting: 8:00 AM",
				"Shelter and knot workshops: Day 1",
				"Night camp drills: Day 1 evening",
				"Final challenge and wrap-up: Day 2"
			],
			requirements: [
				"Sleeping bag and light backpack",
				"Torch and power bank",
				"Personal utensils",
				"Sports shoes and extra clothing"
			],
			payment: [
				"Registration fee: 1200 BDT",
				"bKash number: 01722-334455",
				"Bank account number: ibbl 123456789014",
				"Payment method: bKash (send money) or bank transfer",
				"Transaction format: bk + last 4 digits (example: bk7721)",
				"Submit valid transaction ID in the registration form"
			]
		}
	};

	var details = eventData[eventName] || {
		subtitle: "Event details, schedule, requirements, and payment instructions are available below.",
		overview: "Please review each section carefully before registering.",
		schedule: ["Schedule will be announced by the organizing team."],
		requirements: ["Basic participant requirements will be shared by email."],
		payment: ["bKash number: 01700-000000", "Bank account number: 123456789000", "Follow transaction format: bk + last 4 digits."]
	};

	title.textContent = eventName;
	subtitle.textContent = details.subtitle;
	overview.textContent = details.overview;
	registerBtn.href = "register.html?event=" + encodeURIComponent(eventName);

	if (details.image) {
		photo.src = details.image.src;
		photo.alt = details.image.alt;
		photoWrap.hidden = false;
	} else {
		photoWrap.hidden = true;
	}

	renderList(scheduleList, details.schedule);
	renderList(requirementsList, details.requirements);
	renderList(paymentList, details.payment);
}

function renderList(target, values) {
	target.innerHTML = "";
	for (var i = 0; i < values.length; i++) {
		var item = document.createElement("li");
		item.textContent = values[i];
		target.appendChild(item);
	}
}


