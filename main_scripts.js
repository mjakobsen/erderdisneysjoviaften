var today = moment().format("YYYY-MM-DD").toString()
var disneyTitle = "disney sjov";

var isThereDisneyTonight = undefined;

function checkForDisney(response) {
	_.forEach(response.Data[0].Broadcasts, function(value, key, list) {
		if(value.Title.toLowerCase().indexOf(disneyTitle) >= 0) {
			isThereDisneyTonight = {endDate: value.AnnouncedEndTime, startDate: value.AnnouncedStartTime}
		};
	});
};

function setYes(startTime) {
	$("#main").html("Ja der er Disney sjov, og det starter klokken: " + startTime);
}

function setNo() {
	$("#main").html("Desværre der er ikke Disney sjov i dag");
}

function setTooLate(endTime) {
	$("#main").html("Der var Disney sjov i dag, men det er desværre for sent det sluttede klokken: " + endTime);
}

($(document).ready(function(){
	$.ajax({
	  url: "https://www.dr.dk/mu/Schedule/" + today + "%40dr.dk/mas/whatson/channel/DR1",
	}).success(function(response) {
		checkForDisney(response);
		if(isThereDisneyTonight !== undefined) {
			var endDateTime = moment(isThereDisneyTonight.endDate);
			var now = moment();
			if(now.isBefore(endDateTime)){
				var startTime = moment(isThereDisneyTonight.startDate).format("HH:mm");
				setYes(startTime);
			} else {
				var endTime = endDateTime.format("HH:mm");
				setTooLate(endTime);
			}			
		} else {
			setNo();
		}
	});
}));
