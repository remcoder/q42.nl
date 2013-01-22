//per meteor best practices
//http://andrewscala.com/meteor/

Meteor.startup(function() {
	var collections = ['employees'];
	_.each(collections, function(collection) {
		_.each(['insert', 'update', 'remove'], function(method) {
			Meteor.default_server.method_handlers['/' + collection + '/' + method] = function() {};
		});
	});
});
