
document.addEventListener("DOMContentLoaded", function () {
	setupEventsList();
	setupMembershipForm();
	setupEventRegistrationForm();
	setupEventDetailsPage();
});

var archiveApiPath = "/api/events/archived";
var eventDetailsApiPath = "/api/events/details";
var eventsApiPath = "/api/events";

function getArchivedEvents() {
	return window.KUET_API.requestJson(archiveApiPath)
		.then(function (archived) {
			return Array.isArray(archived) ? archived : [];
		})
		.catch(function () {
			return [];
		});
}

function isArchivedEvent(eventName) {
	return getArchivedEvents().then(function (archived) {
		return archived.indexOf(eventName) !== -1;
	});
}

function getEventDetailsOverride(eventName) {
	return window.KUET_API.requestJson(eventDetailsApiPath + "/" + encodeURIComponent(eventName))
		.catch(function () {
			return null;
		});
}

function cloneList(values) {
	return Array.isArray(values) ? values.slice() : [];
}

function mergeEventDetails(baseDetails, overrideDetails, eventName) {
	var merged = {
		title: eventName,
		subtitle: baseDetails.subtitle,
		overview: baseDetails.overview,
		image: baseDetails.image ? {
			src: baseDetails.image.src,
			alt: baseDetails.image.alt
		} : null,
		schedule: cloneList(baseDetails.schedule),
		requirements: cloneList(baseDetails.requirements),
		payment: cloneList(baseDetails.payment)
	};

	if (!overrideDetails) {
		return merged;
	}

	if (overrideDetails.title) {
		merged.title = overrideDetails.title;
	}
	if (overrideDetails.subtitle) {
		merged.subtitle = overrideDetails.subtitle;
	}
	if (overrideDetails.overview) {
		merged.overview = overrideDetails.overview;
	}
	if (overrideDetails.imageSrc || overrideDetails.imageAlt) {
		merged.image = {
			src: overrideDetails.imageSrc || (baseDetails.image ? baseDetails.image.src : ""),
			alt: overrideDetails.imageAlt || (baseDetails.image ? baseDetails.image.alt : "")
		};
	}
	if (Array.isArray(overrideDetails.schedule) && overrideDetails.schedule.length) {
		merged.schedule = cloneList(overrideDetails.schedule);
	}
	if (Array.isArray(overrideDetails.requirements) && overrideDetails.requirements.length) {
		merged.requirements = cloneList(overrideDetails.requirements);
	}
	if (Array.isArray(overrideDetails.payment) && overrideDetails.payment.length) {
		merged.payment = cloneList(overrideDetails.payment);
	}

	return merged;
}

function setupArchivedEvents() {
	setupEventsList();
}

function formatEventDate(value) {
	if (!value) {
		return "Date will be announced";
	}

	var parsed = new Date(value);
	if (isNaN(parsed.getTime())) {
		return "Date will be announced";
	}

	return parsed.toLocaleDateString("en-US", {
		month: "long",
		day: "2-digit",
		year: "numeric"
	});
}

function setupEventsList() {
	var eventList = document.getElementById("eventList");
	var status = document.getElementById("eventsStatus");

	if (!eventList) {
		return;
	}

	if (status) {
		status.textContent = "Loading events...";
	}

	window.KUET_API.requestJson(eventsApiPath + "?includeArchived=true")
		.then(function (events) {
			if (!Array.isArray(events) || events.length === 0) {
				eventList.innerHTML = "";
				if (status) {
					status.textContent = "No events available right now.";
				}
				return;
			}

			eventList.innerHTML = "";
			for (var i = 0; i < events.length; i++) {
				var item = events[i];
				var card = document.createElement("article");
				card.className = "event-item" + (item.isArchived ? " is-archived" : "");
				card.setAttribute("data-event-name", item.eventName);

				var date = document.createElement("p");
				date.className = "event-date";
				date.textContent = formatEventDate(item.eventDateUtc);

				var heading = document.createElement("h3");
				heading.textContent = item.title || item.eventName;

				var description = document.createElement("p");
				description.textContent = item.shortDescription || item.overview || "Event details are available on the event page.";

				var actions = document.createElement("div");
				actions.className = "event-actions";

				var viewLink = document.createElement("a");
				viewLink.className = "btn event-view-btn";
				viewLink.href = "event.html?event=" + encodeURIComponent(item.eventName);
				viewLink.textContent = "View Event";

				var registerLink = document.createElement("a");
				registerLink.className = "btn btn-primary event-register-btn";
				registerLink.textContent = item.isArchived ? "Archived" : "Register";
				registerLink.href = item.isArchived ? "#" : "register.html?event=" + encodeURIComponent(item.eventName);
				registerLink.classList.toggle("is-disabled", !!item.isArchived);
				registerLink.setAttribute("aria-disabled", item.isArchived ? "true" : "false");

				actions.appendChild(viewLink);
				actions.appendChild(registerLink);

				card.appendChild(date);
				card.appendChild(heading);
				card.appendChild(description);
				card.appendChild(actions);

				if (item.isArchived) {
					var archivedNote = document.createElement("p");
					archivedNote.className = "event-archived-note";
					archivedNote.textContent = item.isExpired ? "Expired: registration deadline has passed." : "Archived by admin.";
					card.appendChild(archivedNote);
				}

				eventList.appendChild(card);
			}

			if (status) {
				status.textContent = "";
			}
		})
		.catch(function (error) {
			eventList.innerHTML = "";
			if (status) {
				status.textContent = "Unable to load events: " + (error && error.message ? error.message : error);
			}
		});
}

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

		var payload = {
			fullName: fullName.value.trim(),
			memberType: memberType.value.trim(),
			department: department.value.trim(),
			rollId: rollId.value.trim(),
			batch: batch.value.trim(),
			mailbox: mailbox.value.trim(),
			phoneNumber: phoneNumber.value.trim(),
			hasPassport: passport.value === "yes",
			message: message.value.trim()
		};

		window.KUET_API.requestApi("/api/memberships", {
			method: "POST",
			headers: {
				"Content-Type": "application/json"
			},
			body: JSON.stringify(payload)
		})
			.then(function (response) {
				if (response.ok) {
					window.location.href = "success.html";
					return;
				}

				// Try to read error details from the response body
				return response.text().then(function (text) {
					var message = "Submit failed (status " + response.status + ")";
					try {
						var json = JSON.parse(text || "{}");
						if (json && json.title) {
							message = json.title + (json.detail ? (": " + json.detail) : "");
						} else if (json && json.errors) {
							// ValidationProblem format
							var all = [];
							Object.keys(json.errors).forEach(function (k) {
								all.push(k + ": " + json.errors[k].join(", "));
							});
							if (all.length) message = all.join("; ");
						} else if (json && json.message) {
							message = json.message;
						}
					} catch (e) {
						// ignore parse errors
					}

					status.textContent = message;
					status.style.color = "#9a3412";
				});
			})
			.catch(function (err) {
				status.textContent = "Could not connect to the ASP.NET API: " + (err && err.message ? err.message : err);
				status.style.color = "#9a3412";
			});
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

		var payload = {
			eventName: eventName,
			fullName: name.value.trim(),
			department: department.value.trim(),
			clubId: clubId.value.trim(),
			roll: roll.value.trim(),
			transactionId: transaction.value.trim()
		};

		window.KUET_API.requestApi("/api/event-registrations", {
			method: "POST",
			headers: {
				"Content-Type": "application/json"
			},
			body: JSON.stringify(payload)
		})
			.then(function (response) {
				if (!response.ok && response.status !== 201) {
					throw new Error("Submit failed (status " + response.status + ").");
				}

				window.location.href = "register-success.html?event=" + encodeURIComponent(eventName);
			})
			.catch(function (err) {
				status.textContent = "Could not connect to the ASP.NET API: " + (err && err.message ? err.message : err);
				status.style.color = "#9a3412";
			});
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
		title: eventName,
		subtitle: "Event details, schedule, requirements, and payment instructions are available below.",
		overview: "Please review each section carefully before registering.",
		schedule: ["Schedule will be announced by the organizing team."],
		requirements: ["Basic participant requirements will be shared by email."],
		payment: ["bKash number: 01700-000000", "Bank account number: 123456789000", "Follow transaction format: bk + last 4 digits."]
	};

	function applyDetailsToView(resolvedDetails, archived) {
		title.textContent = resolvedDetails.title;
		subtitle.textContent = archived ? resolvedDetails.subtitle + " This event is archived and registrations are closed." : resolvedDetails.subtitle;
		overview.textContent = resolvedDetails.overview;
		registerBtn.href = archived ? "#" : "register.html?event=" + encodeURIComponent(eventName);
		registerBtn.textContent = archived ? "Archived Event" : "Register";
		registerBtn.classList.toggle("is-disabled", archived);
		registerBtn.setAttribute("aria-disabled", archived ? "true" : "false");
		registerBtn.setAttribute("tabindex", archived ? "-1" : "0");

		if (resolvedDetails.image && resolvedDetails.image.src) {
			photo.src = resolvedDetails.image.src;
			photo.alt = resolvedDetails.image.alt || resolvedDetails.title;
			photoWrap.hidden = false;
		} else {
			photoWrap.hidden = true;
		}

		renderList(scheduleList, resolvedDetails.schedule);
		renderList(requirementsList, resolvedDetails.requirements);
		renderList(paymentList, resolvedDetails.payment);
	}

	title.textContent = details.title;
	window.KUET_API.requestJson(eventsApiPath + "/by-name/" + encodeURIComponent(eventName))
		.then(function (serverEvent) {
			applyDetailsToView({
				title: serverEvent.title || serverEvent.eventName || eventName,
				subtitle: serverEvent.subtitle || details.subtitle,
				overview: serverEvent.overview || details.overview,
				image: serverEvent.imageSrc ? {
					src: serverEvent.imageSrc,
					alt: serverEvent.imageAlt || serverEvent.title || serverEvent.eventName || eventName
				} : null,
				schedule: Array.isArray(serverEvent.schedule) && serverEvent.schedule.length ? serverEvent.schedule : details.schedule,
				requirements: Array.isArray(serverEvent.requirements) && serverEvent.requirements.length ? serverEvent.requirements : details.requirements,
				payment: Array.isArray(serverEvent.payment) && serverEvent.payment.length ? serverEvent.payment : details.payment
			}, !!serverEvent.isArchived);
		})
		.catch(function () {
			Promise.all([getArchivedEvents(), getEventDetailsOverride(eventName)])
		.then(function (results) {
			var archivedEvents = results[0];
			var overrideDetails = results[1];
			var mergedDetails = mergeEventDetails(details, overrideDetails, eventName);
			var archived = archivedEvents.indexOf(eventName) !== -1;
			applyDetailsToView(mergedDetails, archived);
		})
		.catch(function () {
			applyDetailsToView(details, false);
		});
		});
}

function renderList(target, values) {
	target.innerHTML = "";
	for (var i = 0; i < values.length; i++) {
		var item = document.createElement("li");
		item.textContent = values[i];
		target.appendChild(item);
	}
}


