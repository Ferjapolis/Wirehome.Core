var app;

(function () {
    app = angular.module('app', []);

    app.factory("translationService", ['$http', createTranslationService]);
    app.factory("controllerProxyService", ['$http', createControllerProxyService]);

})();

(function () {
    Array.prototype.moveItem = function (sourceIndex, direction) {

        var targetIndex;
        if (direction === "up") targetIndex = sourceIndex - 1;
        if (direction === "down") targetIndex = sourceIndex + 1;

        if (targetIndex < 0) {
            return;
        }

        if (targetIndex >= ctrl.Model.length) {
            return;
        }

        var temp = this[sourceIndex];
        this[sourceIndex] = this[targetIndex];
        this[targetIndex] = temp;
    }
})();