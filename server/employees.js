var _kickAssQ42Folk = [
 { name: "Alexander Overvoorde", handle:"alexander", github:"", image: "anonymous.jpg"},
 { name: "Arian van Gend", handle:"arian", github:""},
 { name: "Bas Warmerdam", handle:"bas", github:""},
 { name: "Benjamin de Jager", handle:"benjamin", github:""},
 { name: "Bob van Oorschot", handle:"bob", github:""},
 { name: "Chris Waalberg", handle:"chris", github:""},
 { name: "Christiaan Hees", handle:"christiaan", github:""},
 { name: "Cihan Duruer", handle:"cihan", github:""},
 { name: "Coen Bijpost", handle:"coen", github:""},
 { name: "Cynthia Wijntje", handle:"cynthia", github:""},
 { name: "Elaine Oliver", handle:"elaine", github:""},
 { name: "Frank Raterink", handle:"frank", github:""},
 { name: "Herman Banken", handle:"herman", github:""},
 { name: "Ivo de Kler", handle:"ivo", github:""},
 { name: "Jaap Taal", handle:"jaap", github:""},
 { name: "Jan Willem Maneschijn", handle:"janwillem", github:""},
 { name: "Jasper Kaizer", handle:"jasper", github:""},
 { name: "Jelle Visser", handle:"jelle", github:""},
 { name: "Jeroen Gijsman", handle:"jeroen", github:""},
 { name: "Johan Huijkman", handle:"johan", github:""},
 { name: "Kamil Afsar", handle:"kamil", github:""},
 { name: "Kars Veling", handle:"kars", github:""},
 { name: "Katja Hollaar", handle:"katja", github:""},
 { name: "Korjan van Wieringen", handle:"korjan", github:""},
 { name: "Leonard Punt", handle:"leonard", github:""},
 { name: "Lukas van Driel", handle:"lukas", github:""},
 { name: "Marcel Duin", handle:"marcel", github:""},
 { name: "Mark van Straten", handle:"mark", github:""},
 { name: "Martijn Laarman", handle:"martijnl", github:""},
 { name: "Martijn van Steenbergen", handle:"martijn", github:""},
 { name: "Martin Kool", handle:"martin", github:""},
 { name: "Michiel Post", handle:"michiel", github:""},
 { name: "Paul Visschers", handle:"paul", github:""},
 { name: "Rahul Choudhury", handle:"rahul", github:""},
 { name: "Remco Veldkamp", handle:"remco", github:""},
 { name: "Richard Lems", handle:"richard", github:""},
 { name: "Roelf-Jan de Vries", handle:"roelfjan", github:""},
 { name: "Sander de Vos", handle:"sander", github:""},
 { name: "Sanjay Sheombar", handle:"sanjay", github:""},
 { name: "Sjoerd Visscher", handle:"sjoerd", github:""},
 { name: "Stef Brooijmans", handle:"stef", github:""},
 { name: "Suzanne Waalberg", handle:"suzanne", github:""},
 { name: "Tim Logtenberg", handle:"timl", github:""},
 { name: "Tim van Deursen", handle:"timd", github:""},
 { name: "Tim van Steenis", handle:"tims", github:""},
 { name: "Thijs van der Meulen", handle:"thijs", github:""},
 { name: "Tom Lokhorst", handle:"tom", github:""},
 { name: "Wilbert Mekenkamp", handle:"wilbert", github:""}
];

//Get an ACL going on, clients should not get to insert q folk.
Employees.allow({
	insert: function () {
		return false;
	}
});

//minimongo has no support for mongodb's upserts :(
console.log("trying to insert q42 peeps");
var inserts = 0, updates = 0;
_.each(_kickAssQ42Folk, function(e) {
	var count = Employees.find({handle: e.handle}).count()
	if (count === 0) {
		Employees.insert(e);
		inserts++;
	}
	else {
		Employees.update({hanle: e.handle}, e, {set: e });
		updates++;
	}
});
console.log("Inserted " + inserts + " and udated " + updates + " q peeps");

var employeeHandles = _.map(_kickAssQ42Folk, function(e) { return e.handle;  });

//Delete employees whos handles are no longer there:
var employeeCountBefore = Employees.find({}).count();
Employees.remove({ handle: { $nin: employeeHandles } });

var employeeCountAfter = Employees.find({}).count();
console.log("Deleted " + Math.max(0, employeeCountBefore - employeeCountAfter) + " q peeps");

Meteor.publish("employees", function () {
	return Employees.find({}); 
});

Meteor.publish("employeeHandles", function () {
	return employeeHandles;
});
//client 

//var x = new Meteor.Collection("Employees")
//undefined
//Meteor.subscribe("employees");
//Object
//var c = x.find({})
//undefined
//c.count()
//45
