var Duracellko;
(function (Duracellko) {
    // Static class providing functionality for Planning Poker application.
    var PlanningPoker = (function () {
        function PlanningPoker() {
        }

        // Shows message box using jQuery.
        PlanningPoker.showMessageBox = function (element) {
            $(element).modal({ backdrop: 'static' });
        };

        // Shows busy indicator using jQuery.
        PlanningPoker.showBusyIndicator = function (element) {
            $(element).modal({ backdrop: 'static', keyboard: false });
        };

        // Hides element using jQuery.
        PlanningPoker.hide = function (element) {
            $(element).modal('hide');
        };

        return PlanningPoker;
    })();

    Duracellko.PlanningPoker = PlanningPoker;
})(Duracellko || (Duracellko = {}));
