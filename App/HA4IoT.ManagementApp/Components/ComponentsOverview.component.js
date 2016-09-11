(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, $http) {

        ctrl = this;

        ctrl.Model = [];

        ctrl.moveArea = function (area, direction) {
            var sourceIndex = ctrl.Model.indexOf(area);
            ctrl.Model.moveItem(sourceIndex, direction);
        }

        ctrl.loadDemoData = function () {

            $http.get("Areas/DemoData.json").then(function (response) {
                ctrl.loadAreas(response.data);
            });
        }

        ctrl.loadAreas = function (source) {

            var areas = [];
            $.each(source.Areas, function (id, area) {

                var row = {
                    Id: id,
                    Caption: area.Settings.AppSettings.Caption,
                    SortValue: area.Settings.AppSettings.SortValue,
                    Image: area.Settings.AppSettings.Image
                };

                areas.push(row);
            });

            areas = areas.sort(function (a, b) {
                return a.SortValue - b.SortValue;
            });

            ctrl.Model = areas;
        }

        ctrl.loadDemoData();
    }

    module.component("components", {
        templateUrl: "Components/ComponentsOverview.component.html",
        controllerAs: "coCtrl",
        controller: ["controllerProxyService", "$http", createController]
    });

})();