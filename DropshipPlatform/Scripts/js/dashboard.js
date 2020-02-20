$(document).ready(function() {
    $('.navbar-nav .nav-link').click(function() {
        $('.navbar-nav .nav-link').removeClass('active');
        $(this).addClass('active');
    })

    // Chart Code Start
    var ctx = document.getElementById("line-chart");
    ctx.height = 165;
    new Chart(document.getElementById("line-chart"), {
        type: 'line',
        data: {
            labels: ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"],
            datasets: [{
                data: [86, 114, 106, 106, 107, 111, 133, 221, 783, 2478],
                label: "shipped",
                borderColor: "#3e95cd",
                fill: false
            }, {
                data: [282, 350, 411, 502, 635, 809, 947, 1402, 3700, 5267],
                label: "Unpurchase",
                borderColor: "#8e5ea2",
                fill: false
            }, {
                data: [168, 170, 178, 190, 203, 276, 408, 547, 675, 734],
                label: "Waiting",
                borderColor: "#3cba9f",
                fill: false
            }, {
                data: [40, 20, 10, 16, 24, 38, 74, 167, 508, 784],
                label: "Unpaid",
                borderColor: "#e8c3b9",
                fill: false
            }]
        },
        options: {
            title: {
                display: false,
                text: 'Weekly Order Status Reports'
            }
        }
    });

    new Chart(document.getElementById("doughnut-chart"), {
        type: 'doughnut',
        data: {
            labels: ["Sale Products", "Total Products"],
            datasets: [{
                label: "Population (millions)",
                backgroundColor: ["#3c3c3c"],
                data: [700, 500]
            }]
        },
        options: {
            title: {
                display: false,
                text: ''
            }
        }
    });

    new Chart(document.getElementById("doughnut-chart"), {
        type: 'doughnut',
        data: {
            labels: ["Sale Products", "Total Products"],
            datasets: [{
                label: "",
                backgroundColor: ["#3c3c3c"],
                data: [700, 500]
            }]
        },
        options: {
            title: {
                display: false,
                text: ''
            }
        }
    });


    new Chart(document.getElementById("doughnut-chart-seller"), {
        type: 'doughnut',
        data: {
            labels: ["Total Picked", "Total Sale"],
            datasets: [{
                label: "",
                backgroundColor: ["#3c3c3c"],
                data: [1000, 300]
            }]
        },
        options: {
            title: {
                display: false,
                text: ''
            }
        }
    });

    new Chart(document.getElementById("doughnut-chart-service-status"), {
        type: 'doughnut',
        data: {
            labels: ["Start Service", "Stop Service"],
            datasets: [{
                label: "",
                backgroundColor: ["#3c3c3c"],
                data: [1000, 950]
            }]
        },
        options: {
            title: {
                display: false,
                text: ''
            }
        }
    });

    // Bar chart
    var ctx = document.getElementById("bar-chart");
    ctx.height = 165;
    new Chart(document.getElementById("bar-chart"), {
        type: 'bar',
        data: {
            labels: ["New Users", "Active Users", "Inactive Users"],
            datasets: [{
                label: "",
                backgroundColor: ["#636363", "#e5e5e5", "#000000"],
                data: [2478, 5267, 2734]
            }]
        },
        options: {
            legend: { display: false },
            title: {
                display: true,
                text: 'User Activities Report'
            }
        }
    });

});

window.chartColors = {
    red: 'rgb(255, 99, 132)',
    orange: 'rgb(255, 159, 64)',
    yellow: 'rgb(255, 205, 86)',
    green: 'rgb(75, 192, 192)',
    blue: 'rgb(54, 162, 235)',
    purple: 'rgb(153, 102, 255)',
    grey: 'rgb(201, 203, 207)'
};

window.randomScalingFactor = function() {
    return (Math.random() > 0.5 ? 1.0 : -1.0) * Math.round(Math.random() * 100);
};

var config = {
    type: 'line',
    data: {
        labels: ["January", "February", "March", "April", "May", "June", "July"],
        datasets: [{
            label: "Unfilled",
            fill: false,
            backgroundColor: window.chartColors.blue,
            borderColor: window.chartColors.blue,
            data: [
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor()
            ],
        }, {
            label: "Dashed",
            fill: false,
            backgroundColor: window.chartColors.green,
            borderColor: window.chartColors.green,
            borderDash: [5, 5],
            data: [
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor()
            ],
        }, {
            label: "Filled",
            backgroundColor: window.chartColors.red,
            borderColor: window.chartColors.red,
            data: [
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor(),
                randomScalingFactor()
            ],
            fill: true,
        }]
    },
    options: {
        maintainAspectRatio: false,
        responsive: true,
        legend: {
            position: 'top'
        },
        title: {
            position: 'bottom',
            display: true,
            text: 'Chart.js Resizable Chart'
        },
        tooltips: {
            mode: 'index',
            intersect: false,
        },
        hover: {
            mode: 'nearest',
            intersect: true
        },
        scales: {
            xAxes: [{
                display: true,
                scaleLabel: {
                    display: true,
                    labelString: 'Month'
                }
            }],
            yAxes: [{
                display: true,
                scaleLabel: {
                    display: true,
                    labelString: 'Value'
                }
            }]
        }
    }
};