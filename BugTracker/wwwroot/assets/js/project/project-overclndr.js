$(function(e){
	
	//________ Datepicker
	$( '.fc-datepicker').datepicker({
		dateFormat: "dd M yy",
		monthNamesShort: [ "Jan", "Feb", "Mar", "Apr", "Maj", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec" ],
		zIndex: 999998,
	});
	
	//________ Datepicker
	$( '.fc-datepicker1').datepicker({
		dateFormat: "dd M yy",
		monthNamesShort: [ "Jan", "Feb", "Mar", "Apr", "Maj", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec" ],
		zIndex: 1,
	});
	
	//________ Select2
	$('.select2').select2({
		minimumResultsForSearch: Infinity,
		width:"100%"
	});

	//______summernote
	$('.summernote').summernote({
		placeholder: '',
		tabsize: 1,
		height: 300
	});

 });

//________ FullCalendar
document.addEventListener('DOMContentLoaded', function() {
	var calendarEl = document.getElementById('calendar1');
	var calendar = new FullCalendar.Calendar(calendarEl, {
	   headerToolbar: {
		left: 'prev',
		center: 'title',
		right: 'next'
	  },
	  navLinks: true, // can click day/week names to navigate views
	  businessHours: true, // display business hours
	  editable: true,
	  selectable: true,
	  selectMirror: true,
	  droppable: true, // this allows things to be dropped onto the calendar
	  drop: function(arg) {
		// is the "remove after drop" checkbox checked?
		if (document.getElementById('drop-remove').checked) {
		  // if so, remove the element from the "Draggable Events" list
		  arg.draggedEl.parentNode.removeChild(arg.draggedEl);
		}
	  },
	  select: function(arg) {
		var title = prompt('Event Title:');
		if (title) {
		  calendar.addEvent({
			title: title,
			start: arg.start,
			end: arg.end,
			allDay: arg.allDay
		  })
		}
		calendar.unselect()
	  },
	  eventClick: function(arg) {
		if (confirm('Are you sure you want to delete this event?')) {
		  arg.event.remove()
		}
	  },
	  editable: true,
	  dayMaxEvents: true, // allow "more" link when too many events
	  eventRender: function (event, element) {
		debugger;
		if ((event.description).toString() == "Halfday"){
			element.find(".fc-event-time").after($("<span class=\"fc-event-icons\"></span>").html("<i class='fe fe-view'></i> "));
		}
	  },
	  events: [
		{
		  title: 'Faith Harris',
		  start: '2021-02-12',
		  display: 'rgba(1, 195, 83, 0.15)',
		  color:'rgba(1, 195, 83, 0.15)',
		},
		{
			title: 'Austin Bell',
			start: '2021-01-01',
			display: 'rgba(71, 84, 242, 0.15)',
			color:'rgba(71, 84, 242, 0.15)',
		},
		{
			title: 'Maria Bower',
			start: '2021-04-11',
			display: 'rgba(255, 173, 0, 0.15)',
			color:'rgba(255, 173, 0, 0.15)',
		},
		{
			title: 'Peter Hill',
			start: '2021-03-11',
			display: 'rgba(1, 195, 83, 0.15)',
			color:'rgba(1, 195, 83, 0.15)',
		},
		{
			title: 'Victoria Lyman',
			start: '2021-02-02',
			display: 'rgba(1, 195, 83, 0.15)',
			color:'rgba(1, 195, 83, 0.15)',
		},
		{
			title: 'Adam Quinn',
			start: '2021-01-21',
			display: 'rgba(255, 173, 0, 0.15)',
			color:'rgba(255, 173, 0, 0.15)',
		},
		{
			title: 'Melanie Coleman',
			start: '2021-01-23',
			display: 'rgba(71, 84, 242, 0.15)',
			color:'rgba(71, 84, 242, 0.15)',
		},
		{
			title: 'Adam Quinn',
			start: '2021-03-13',
			display: 'rgba(1, 195, 83, 0.15)',
			color:'rgba(1, 195, 83, 0.15)',
		},
	  ]
	});
	calendar.render();
});	