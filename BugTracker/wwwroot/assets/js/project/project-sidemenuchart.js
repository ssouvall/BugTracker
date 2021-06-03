$(function(e){
	
	//________sidemenuproject
	var ctx = document.getElementById( "sidemenuproject" );
	ctx.height="100";

	var myChart = new Chart( ctx, {
		type: 'line',
		data : {
			labels: ['Mon', 'Tues', 'Wed', 'Thurs', 'Fri', 'Sat', 'Sun'],
			datasets: [
			{
				label: "Expenses",
				data: [40, 42, 35, 30, 32, 39, 40],
				backgroundColor: 'rgba(45, 53, 160, 0.04)',
				borderColor: '#3366ff',
				pointBackgroundColor:'#fff',
				pointBorderWidth :2,
				pointRadius :0,
				pointHoverRadius :5,
				borderWidth: 2,
                pointStyle: 'circle',
                pointBorderColor: '#3366ff',
				pointBackgroundColor: '#fff',

			}, {
				label: "Budget",
				data: [30, 35, 35, 37, 40, 48, 50],
				backgroundColor: 'rgba(45, 53, 160, 0.04)',
				borderColor: '#fe7f00',
				pointBackgroundColor:'#fff',
				pointBorderWidth :2,
				pointRadius :0,
				pointHoverRadius :5,
				borderWidth: 2,
				pointStyle: 'circle',
                pointBorderColor: '#fe7f00',
				pointBackgroundColor: '#fff',
			}
		  ]
		},
		options: {
			responsive: true,
       		 maintainAspectRatio: false,
			legend: {
				display: false
			},
			tooltips: {
				enabled: false,
				show: false,
				showContent: true,
				alwaysShowContent: true,
				triggerOn: 'mousemove',
				trigger: 'axis',
				axisPointer:
				{
					label: {
						show: false,
						color: '#728096',
					},
				}
			},

			scales: {
				xAxes: [ {
					gridLines: {
						display: false,
						drawBorder: false,
						color: 'rgba(45, 53, 160, 0.2)',
					},
					ticks: {
						fontColor: '#728096',
						display: false,
					},
					display: true,
					scaleLabel: {
						display: false,
						labelString: 'Month',
						fontColor: '#728096'
					}
				} ],
				yAxes: [ {
					gridLines: {
						color: 'rgba(45, 53, 160, 0.2)',
						zeroLineColor: 'rgba(45, 53, 160, 0.2)',
						drawBorder: false,
						display: false,
					},
					ticks: {
						display: false,
						fontSize: 12,
						fontColor: '#728096',
						padding: 0,
						beginAtZero: true,
						stepSize: 10,
						min: 0,
						max: 50,
					},
				} ]
			},
			title: {
				display: false,
			},
			elements: {
				line: {
					borderWidth: 2
				},
				point: {
					radius: 0,
					hitRadius: 10,
					hoverRadius: 5
				}
			}
		}
	})
	
 });