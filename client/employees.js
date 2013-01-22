Meteor.autosubscribe(function() {
	Meteor.subscribe("employees", function() { console.log("recieved employees"); initEmployees(); });
	
});
Template.employees.employee = function () {
	return Employees.find({});
}
Handlebars.registerHelper('avatar_static', function() {
	return this.image || this.handle + "zw.jpg";
});
Handlebars.registerHelper('avatar_animated', function() {
	return this.image || this.handle + "gif.gif";
});

var initEmployees = function() {

	var employeeHandles = Employees.find({}).map(function(e) { return e.handle; }); 

	setDifferentRotationsForQers();

	$('#colleagues .polaroid').mouseenter(resetCinemagraph);
}

function setDifferentRotationsForQers() {
  $('#colleagues').find('.polaroid').each(function () {
    var randomRotation = Math.floor(Math.random() * 21) - 10;
    var polaroid = $(this);
    polaroid.css('-webkit-transform', 'scale(1.0) rotate(' + randomRotation + 'deg)');
    polaroid.css('-webkit-transform', 'scale(1.0) rotate(' + randomRotation + 'deg)');
  });
}

function resetCinemagraph() {
  var $img = $(this).find('.insetshadow img');
  var src = $img.attr('src');
  $img.attr('src', '');
  setTimeout(function () {
    $img.attr('src', src);
  }, 0);
}
