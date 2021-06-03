( function ( $ ) {
	"use strict";

	/*-----Statistics-----*/
	var myCanvas = document.getElementById("statistics");

	var myCanvasContext = myCanvas.getContext("2d");
	var gradientStroke1 = myCanvasContext.createLinearGradient(0, 180, 0, 280);
	gradientStroke1.addColorStop(0, '#f5f6f8');
	gradientStroke1.addColorStop(1, 'rgba(246, 247, 249, .05)');

	myCanvas.height="350";
    var myChart = new Chart( myCanvas, {
		type: 'line',
		data: {
			labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
			datasets: [{
				label: 'on going',
				data: [12, 20, 23, 18, 26, 25, 29.9, 25.5, 23, 25, 27, 18],
				backgroundColor: 'transparent',
				borderWidth: 3,
				borderColor: '#3366ff',
				hoverBorderColor: '#3366ff',
			},{
				label: 'Completed',
				data: [15, 17, 9.2, 20, 23, 17, 10, 25.2, 25, 18, 22, 20],
				backgroundColor: 'transparent',
				borderWidth: 3,
				borderColor: '#fe7f00',
				hoverBorderColor: '#fe7f00',

			}, {

			    label: '',
				data: [18, 18, 15, 23, 20, 16, 22, 18, 20, 24, 15, 22],
				backgroundColor: gradientStroke1,
				borderWidth: 3,
				borderColor:'#f5f6f8',
			}
		  ]
		},
		options: {
			responsive: true,
			maintainAspectRatio: false,
			layout: {
				padding: {
					left: 0,
					right: 0,
					top: 0,
					bottom: 0
				}
			},
			tooltips: {
				enabled: false,
			},
			scales: {
				yAxes: [{
					gridLines: {
						display: true,
						drawBorder: false,
						zeroLineColor: 'rgba(142, 156, 173,0.1)',
						color: "rgba(142, 156, 173,0.1)",
					},
					scaleLabel: {
						display: false,
					},
					ticks: {
						min: 5,
						stepSize: 5,
						max: 30,
						fontColor: "#8492a6",
					},
				}],
				xAxes: [{
					ticks: {
						fontColor: "#8492a6",
					},
					gridLines: {
						color: "rgba(142, 156, 173,0.1)",
						display: false
					},

				}]
			},
			legend: {
				display: false
			},
			elements: {
				point: {
					radius: 0
				}
			}
		}
	});
	/*-----Statistics-----*/

	/*----- Advancedtask ------*/
	var options = {
		series: [74, 35],
		chart: {
			height:260,
			type: 'donut',
		},
		dataLabels: {
			enabled: false
		},

		legend: {
			show: false,
		},
		 stroke: {
			show: true,
			width:0
		},
		plotOptions: {
		pie: {
			donut: {
				size: '80%',
				background: 'transparent',
				labels: {
					show: true,
					name: {
						show: true,
						fontSize: '29px',
						color:'#6c6f9a',
						offsetY: -10
					},
					value: {
						show: true,
						fontSize: '26px',
						color: undefined,
						offsetY: 16,
						formatter: function (val) {
							return val + "%"
						}
					},
					total: {
						show: true,
						showAlways: false,
						label: 'Total Employees',
						fontSize: '22px',
						fontWeight: 600,
						color: '#373d3f',
						// formatter: function (w) {
						//   return w.globals.seriesTotals.reduce((a, b) => {
						// 	return a + b
						//   }, 0)
						// }
					  }

				}
			}
		}
		},
		responsive: [{
			breakpoint: 480,
			options: {
				legend: {
					show: false,
				}
			}
		}],
		labels: ["Male","Female"],
		colors: ['#3366ff', '#fe7f00'],
	};
	var chart = new ApexCharts(document.querySelector("#employee"), options);
	chart.render();

	/* Data Table */
	$('#projecttable').DataTable({
		"paging":   false,
		searching: false,
		"info": false
	});
	/* End Data Table */

	//______calendar
    $('.custom-calendar').pignoseCalendar({
		disabledDates:[
			'2021-01-20'
		],
		format: 'YYY-MM-DD'
	});

})( jQuery );

